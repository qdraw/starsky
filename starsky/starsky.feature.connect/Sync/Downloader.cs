using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using starsky.foundation.connect.Protocol;
using starsky.foundation.connect.Protocol.Generated;
using starsky.foundation.connect.Transport;

namespace starsky.feature.connect.Sync;

/// <summary>
/// Schedules block download Requests to peers and correlates Responses.
/// Limits concurrency to 128 outstanding requests (matching Syncthing's default).
/// </summary>
public sealed class Downloader : IDisposable
{
	private readonly ConnectionManager _connectionManager;
	private readonly BlockStore _blockStore;
	private readonly ILogger<Downloader> _logger;

	private readonly SemaphoreSlim _requestSlots = new(128, 128);
	private readonly ConcurrentDictionary<int, TaskCompletionSource<Response>> _pending = new();
	private int _nextRequestId;

	public Downloader(
		ConnectionManager connectionManager,
		BlockStore blockStore,
		ILogger<Downloader> logger)
	{
		_connectionManager = connectionManager;
		_blockStore = blockStore;
		_logger = logger;
	}

	/// <summary>
	/// Called by SyncEngine when a Response message arrives from a peer.
	/// Resolves the pending TaskCompletionSource for that request ID.
	/// </summary>
	public void ReceiveResponse(Response response)
	{
		if ( _pending.TryRemove(response.Id, out var tcs) )
		{
			tcs.TrySetResult(response);
		}
	}

	/// <summary>
	/// Downloads all blocks for a file from the given peer and writes them to BlockStore.
	/// Returns true if all blocks were successfully received.
	/// </summary>
	public async Task<bool> DownloadFileAsync(
		string deviceId,
		string folder,
		string name,
		System.Collections.Generic.IReadOnlyList<BlockInfo> blocks,
		CancellationToken ct = default)
	{
		var tasks = new Task<bool>[blocks.Count];
		for ( var i = 0; i < blocks.Count; i++ )
		{
			var block = blocks[i];
			var blockIndex = i;
			tasks[i] = DownloadBlockAsync(deviceId, folder, name, block, blockIndex, ct);
		}

		var results = await Task.WhenAll(tasks);
		return Array.TrueForAll(results, r => r);
	}

	private async Task<bool> DownloadBlockAsync(
		string deviceId,
		string folder,
		string name,
		BlockInfo block,
		int blockNo,
		CancellationToken ct)
	{
		await _requestSlots.WaitAsync(ct);
		try
		{
			var id = Interlocked.Increment(ref _nextRequestId);
			var tcs = new TaskCompletionSource<Response>(TaskCreationOptions.RunContinuationsAsynchronously);
			_pending[id] = tcs;

			var request = new Request
			{
				Id = id,
				Folder = folder,
				Name = name,
				Offset = block.Offset,
				Size = block.Size,
				Hash = block.Hash,
				BlockNo = blockNo,
			};

			await _connectionManager.SendAsync(deviceId, request, MessageType.Request, compress: false, ct);

			using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
			timeoutCts.CancelAfter(TimeSpan.FromSeconds(60));

			Response response;
			try
			{
				response = await tcs.Task.WaitAsync(timeoutCts.Token);
			}
			catch ( OperationCanceledException )
			{
				_pending.TryRemove(id, out _);
				return false;
			}

			if ( response.Code != ErrorCode.NoError || response.Data.IsEmpty )
			{
				_logger.LogWarning("Block request failed: {Code} for {Folder}/{Name}@{Offset}.",
					response.Code, folder, name, block.Offset);
				return false;
			}

			await _blockStore.WriteBlockAsync(folder, name, block.Offset, response.Data.ToByteArray(), ct);
			return true;
		}
		finally
		{
			_requestSlots.Release();
		}
	}

	public void Dispose()
	{
		_requestSlots.Dispose();
		foreach ( var tcs in _pending.Values )
		{
			tcs.TrySetCanceled();
		}
	}
}
