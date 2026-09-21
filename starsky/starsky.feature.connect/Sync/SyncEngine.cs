using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using starsky.foundation.connect.Protocol;
using starsky.foundation.connect.Protocol.Generated;
using starsky.foundation.connect.Sync;
using starsky.foundation.connect.Transport;
using starsky.foundation.database.Models;
using BepFileInfo = starsky.foundation.connect.Protocol.Generated.FileInfo;
using BepIndex = starsky.foundation.connect.Protocol.Generated.Index;

namespace starsky.feature.connect.Sync;

/// <summary>
/// Top-level sync orchestrator. Implements the BEP connection state machine:
/// stateInitial (only ClusterConfig accepted) → stateReady (all messages accepted).
/// Wires ConnectionManager events to FolderModel, BlockStore, and Downloader.
/// </summary>
public sealed class SyncEngine : IHostedService, IDisposable
{
	private enum ConnectionState { Initial, Ready }

	private readonly ConnectionManager _connectionManager;
	private readonly FolderModel _folderModel;
	private readonly BlockStore _blockStore;
	private readonly Downloader _downloader;
	private readonly ConflictResolver _conflictResolver;
	private readonly IReadOnlyDictionary<string, string> _folderRoots; // folderId → local root path
	private readonly byte[] _localDeviceIdBytes;
	private readonly string _localDeviceId;
	private readonly ILogger<SyncEngine> _logger;

	private readonly ConcurrentDictionary<string, ConnectionState> _states = new();

	/// <summary>
	/// Optional callback invoked after a file has been downloaded and assembled to disk.
	/// Arguments are (folderId, relativeName).
	/// </summary>
	public Func<string, string, Task>? OnFileDownloaded { get; set; }

	public SyncEngine(
		ConnectionManager connectionManager,
		FolderModel folderModel,
		BlockStore blockStore,
		Downloader downloader,
		ConflictResolver conflictResolver,
		IReadOnlyDictionary<string, string> folderRoots,
		byte[] localDeviceIdBytes,
		string localDeviceId,
		ILogger<SyncEngine> logger)
	{
		_connectionManager = connectionManager;
		_folderModel = folderModel;
		_blockStore = blockStore;
		_downloader = downloader;
		_conflictResolver = conflictResolver;
		_folderRoots = folderRoots;
		_localDeviceIdBytes = localDeviceIdBytes;
		_localDeviceId = localDeviceId;
		_logger = logger;
	}

	public async Task StartAsync(CancellationToken cancellationToken)
	{
		_connectionManager.OnPeerConnected = OnPeerConnectedAsync;
		_connectionManager.OnMessageReceived = OnMessageReceivedAsync;
		_connectionManager.OnPeerDisconnected = OnPeerDisconnectedAsync;

		await _connectionManager.StartAsync(cancellationToken);
	}

	public async Task StopAsync(CancellationToken cancellationToken)
	{
		await _connectionManager.StopAsync(cancellationToken);
	}

	// ── ConnectionManager callbacks ───────────────────────────────────────────

	private async Task OnPeerConnectedAsync(string deviceId, Hello hello, CancellationToken ct)
	{
		_states[deviceId] = ConnectionState.Initial;
		_logger.LogInformation("BEP handshake with {DeviceId} ({Name}).", deviceId, hello.DeviceName);

		// Send our ClusterConfig immediately
		await SendClusterConfigAsync(deviceId, ct);
	}

	private async Task OnMessageReceivedAsync(string deviceId, BepMessage msg, CancellationToken ct)
	{
		var state = _states.GetOrAdd(deviceId, _ => ConnectionState.Initial);

		if ( state == ConnectionState.Initial )
		{
			if ( msg.Header.Type != MessageType.ClusterConfig )
			{
				_logger.LogWarning("Received {Type} before ClusterConfig from {DeviceId}, closing.",
					msg.Header.Type, deviceId);
				// In a real implementation, we'd close the connection here
				return;
			}

			_states[deviceId] = ConnectionState.Ready;
		}

		switch ( msg.Header.Type )
		{
			case MessageType.ClusterConfig:
				await HandleClusterConfigAsync(deviceId, msg.ParseBody(ClusterConfig.Parser), ct);
				break;

			case MessageType.Index:
				await HandleIndexAsync(deviceId, msg.ParseBody(BepIndex.Parser), ct);
				break;

			case MessageType.IndexUpdate:
				await HandleIndexUpdateAsync(deviceId, msg.ParseBody(IndexUpdate.Parser), ct);
				break;

			case MessageType.Request:
				// Serve block requests from peer without blocking the read loop
				_ = Task.Run(() => HandleRequestAsync(deviceId, msg.ParseBody(Request.Parser), ct), ct);
				break;

			case MessageType.Response:
				_downloader.ReceiveResponse(msg.ParseBody(Response.Parser));
				break;

			case MessageType.Ping:
				// Receipt of any message resets the receive-timeout timer; no explicit reply needed
				break;

			case MessageType.Close:
				var close = msg.ParseBody(Close.Parser);
				_logger.LogInformation("Peer {DeviceId} closed: {Reason}.", deviceId, close.Reason);
				_states.TryRemove(deviceId, out _);
				break;

			default:
				_logger.LogDebug("Unhandled message type {Type} from {DeviceId}.", msg.Header.Type, deviceId);
				break;
		}
	}

	private Task OnPeerDisconnectedAsync(string deviceId, Exception? reason, CancellationToken ct)
	{
		_states.TryRemove(deviceId, out _);
		_logger.LogInformation("Peer {DeviceId} disconnected: {Reason}.", deviceId, reason?.Message);
		return Task.CompletedTask;
	}

	// ── Message handlers ──────────────────────────────────────────────────────

	private async Task HandleClusterConfigAsync(string deviceId, ClusterConfig config, CancellationToken ct)
	{
		foreach ( var folder in config.Folders )
		{
			if ( !_folderRoots.ContainsKey(folder.Id) ) continue;

			// Find the Device record that represents the remote peer's view of us
			var ourRecord = folder.Devices.FirstOrDefault(d =>
				ByteString.CopyFrom(_localDeviceIdBytes).Equals(d.Id));

			if ( ourRecord is null ) continue;

			await _folderModel.RecordPeerStateAsync(
				folder.Id,
				GetDeviceIdBytes(deviceId),
				ourRecord.IndexId != 0 ? ( long )ourRecord.IndexId : 0,
				ourRecord.MaxSequence,
				ct);

			// Determine if we need a full Index or delta IndexUpdate
			var need = await _folderModel.NeedsDeltaOrFullAsync(
				folder.Id,
				GetDeviceIdBytes(deviceId),
				( long )ourRecord.IndexId,
				ourRecord.MaxSequence,
				ct);

			await SendIndexAsync(deviceId, folder.Id, need, ct);
		}
	}

	private async Task HandleIndexAsync(string deviceId, BepIndex index, CancellationToken ct)
	{
		if ( !ValidateFileInfoList(deviceId, index.Files) ) return;
		await _folderModel.ApplyIndexAsync(index.Folder, index.Files, ct);

		// Schedule downloads for files we don't have
		_ = Task.Run(() => ScheduleDownloadsAsync(deviceId, index.Folder, index.Files, ct), ct);
	}

	private async Task HandleIndexUpdateAsync(string deviceId, IndexUpdate update, CancellationToken ct)
	{
		if ( !ValidateFileInfoList(deviceId, update.Files) ) return;
		await _folderModel.ApplyIndexUpdateAsync(update.Folder, update.Files, ct);

		_ = Task.Run(() => ScheduleDownloadsAsync(deviceId, update.Folder, update.Files, ct), ct);
	}

	private async Task HandleRequestAsync(string deviceId, Request request, CancellationToken ct)
	{
		// Try block store first (in-progress uploads), then fall back to the real file on disk
		var data = await _blockStore.ReadBlockAsync(
			request.Folder, request.Name, request.Offset, request.Size, ct);

		if ( data is null && _folderRoots.TryGetValue(request.Folder, out var root) )
		{
			data = await ReadBlockFromDiskAsync(root, request.Name, request.Offset, request.Size, ct);
		}

		Response response;
		if ( data is null )
		{
			response = new Response { Id = request.Id, Code = ErrorCode.NoSuchFile };
		}
		else
		{
			response = new Response
			{
				Id = request.Id,
				Data = ByteString.CopyFrom(data.Value.Span),
				Code = ErrorCode.NoError,
			};
		}

		await _connectionManager.SendAsync(deviceId, response, MessageType.Response, compress: true, ct);
	}

	private static async Task<ReadOnlyMemory<byte>?> ReadBlockFromDiskAsync(
		string root, string name, long offset, int size, CancellationToken ct)
	{
		var path = Path.Combine(root, name.Replace('/', Path.DirectorySeparatorChar));
		if ( !File.Exists(path) ) return null;

		var buffer = new byte[size];
		await using var stream = File.OpenRead(path);
		stream.Seek(offset, SeekOrigin.Begin);
		var read = await stream.ReadAsync(buffer.AsMemory(0, size), ct);
		if ( read == 0 ) return null;
		return buffer.AsMemory(0, read);
	}

	// ── Outbound index exchange ───────────────────────────────────────────────

	private async Task SendClusterConfigAsync(string deviceId, CancellationToken ct)
	{
		var config = new ClusterConfig();
		var peerIdBytes = GetDeviceIdBytes(deviceId);

		foreach ( var folderId in _folderRoots.Keys )
		{
			var (indexId, maxSeq) = await _folderModel.GetFolderStateAsync(folderId, ct);
			var (peerIndexId, peerMaxSeq) = await _folderModel.GetPeerStateAsync(folderId, peerIdBytes, ct);

			var folder = new Folder { Id = folderId, Label = folderId };
			// BEP spec: ClusterConfig must list all devices sharing the folder.
			folder.Devices.Add(new Device
			{
				Id = ByteString.CopyFrom(_localDeviceIdBytes),
				IndexId = ( ulong )indexId,
				MaxSequence = maxSeq,
			});
			folder.Devices.Add(new Device
			{
				Id = ByteString.CopyFrom(peerIdBytes),
				IndexId = ( ulong )peerIndexId,
				MaxSequence = peerMaxSeq,
			});
			config.Folders.Add(folder);
		}

		await _connectionManager.SendAsync(deviceId, config, MessageType.ClusterConfig, ct: ct);
	}

	private async Task SendIndexAsync(
		string deviceId,
		string folderId,
		IndexNeed need,
		CancellationToken ct)
	{
		if ( need.RequiresFull )
		{
			var allFiles = await _folderModel.GetFilesWithBlocksForUpdateAsync(folderId, 0, ct);
			var index = new BepIndex { Folder = folderId };
			index.Files.AddRange(allFiles.Select(pair => ToFileInfo(pair.Meta, pair.Blocks)));
			await _connectionManager.SendAsync(deviceId, index, MessageType.Index, compress: true, ct);
		}
		else
		{
			var deltaFiles = await _folderModel.GetFilesWithBlocksForUpdateAsync(folderId, need.FromSequence, ct);
			if ( deltaFiles.Count == 0 ) return;

			var update = new IndexUpdate { Folder = folderId };
			update.Files.AddRange(deltaFiles.Select(pair => ToFileInfo(pair.Meta, pair.Blocks)));
			await _connectionManager.SendAsync(deviceId, update, MessageType.IndexUpdate, compress: true, ct);
		}
	}

	// ── Download scheduling ───────────────────────────────────────────────────

	private async Task ScheduleDownloadsAsync(
		string deviceId,
		string folder,
		IEnumerable<BepFileInfo> files,
		CancellationToken ct)
	{
		if ( !_folderRoots.TryGetValue(folder, out var root) ) return;

		foreach ( var fi in files )
		{
			if ( ct.IsCancellationRequested ) return;
			if ( fi.Deleted || fi.Invalid || fi.Blocks.Count == 0 ) continue;

			try
			{
				// Pass root (filesystem path) to downloader so BlockStore temp files land in the right place.
				var ok = await _downloader.DownloadFileAsync(deviceId, folder, root, fi.Name, fi.Blocks, ct);

				if ( ok )
				{
					var destPath = Path.Combine(root, fi.Name.Replace('/', Path.DirectorySeparatorChar));
					await _blockStore.AssembleFileAsync(root, fi.Name, destPath, ct);
					if ( !OperatingSystem.IsWindows() && fi.Permissions != 0 )
					{
						// Restore unix permission bits (rwxrwxrwx) sent by the peer.
						// Skipped on Windows because UnixFileMode has no effect there.
						File.SetUnixFileMode(destPath, ( UnixFileMode )( fi.Permissions & 0x1FF ));
					}
					_blockStore.Forget(root, fi.Name);
					_logger.LogInformation("Downloaded and assembled {Folder}/{Name}.", folder, fi.Name);

					if ( OnFileDownloaded is not null )
					{
						await OnFileDownloaded(folder, fi.Name);
					}
				}
			}
			catch ( Exception ex ) when ( ex is not OperationCanceledException )
			{
				_logger.LogError(ex, "Error downloading {Folder}/{Name}.", folder, fi.Name);
			}
		}
	}

	// ── Index consistency validation ──────────────────────────────────────────

	private bool ValidateFileInfoList(string deviceId, IEnumerable<BepFileInfo> files)
	{
		foreach ( var fi in files )
		{
			if ( string.IsNullOrEmpty(fi.Name) )
			{
				_logger.LogWarning("Received FileInfo with empty name from {DeviceId}.", deviceId);
				return false;
			}

			if ( fi.Name.Contains('\\', StringComparison.Ordinal) )
			{
				_logger.LogWarning("FileInfo name contains backslash from {DeviceId}: {Name}.", deviceId, fi.Name);
				return false;
			}

			if ( fi.Deleted && fi.Blocks.Count > 0 )
			{
				_logger.LogWarning("Deleted FileInfo has non-empty Blocks from {DeviceId}: {Name}.", deviceId, fi.Name);
				return false;
			}

			if ( ( fi.Type == FileInfoType.Directory || fi.Type == FileInfoType.Symlink ) && fi.Blocks.Count > 0 )
			{
				_logger.LogWarning("Directory/symlink has Blocks from {DeviceId}: {Name}.", deviceId, fi.Name);
				return false;
			}
		}

		return true;
	}

	// ── Helpers ───────────────────────────────────────────────────────────────

	private static BepFileInfo ToFileInfo(ConnectFileMeta meta, IReadOnlyList<ConnectBlockInfo> blocks)
	{
		var fi = new BepFileInfo
		{
			Name = meta.Name,
			Sequence = meta.Sequence,
			BlockSize = meta.BlockSize,
			Permissions = ( uint )meta.Permissions,
			Deleted = meta.Deleted,
			Invalid = meta.Invalid,
			NoPermissions = meta.NoPermissions,
			ModifiedBy = ( ulong )meta.ModifiedBy,
			SymlinkTarget = meta.SymlinkTarget,
		};

		if ( meta.Version is { Length: > 0 } )
		{
			fi.Version = Vector.Parser.ParseFrom(meta.Version);
		}

		foreach ( var b in blocks )
		{
			fi.Blocks.Add(new BlockInfo
			{
				Offset = b.Offset,
				Size = b.Size,
				Hash = ByteString.CopyFrom(b.Hash),
			});
		}

		return fi;
	}

	private static byte[] GetDeviceIdBytes(string normalizedDeviceId)
	{
		// The 56-char normalized ID has Luhn check chars at positions 13, 27, 41, 55.
		// Strip them to recover the 52 pure Base32 chars that encode the 32-byte hash.
		return starsky.foundation.connect.Crypto.Base32.Decode(
			StripLuhnChars(normalizedDeviceId));
	}

	private static string StripLuhnChars(string s)
	{
		// 56-char normalized form: 4 blocks of 14 chars (13 data + 1 Luhn at index 13, 27, 41, 55)
		var buf = new System.Text.StringBuilder(52);
		for ( var i = 0; i < s.Length && buf.Length < 52; i++ )
		{
			if ( i % 14 == 13 ) continue; // every 14th char is a Luhn checksum
			buf.Append(s[i]);
		}

		return buf.ToString();
	}

	public void Dispose()
	{
		_blockStore.Dispose();
		_downloader.Dispose();
	}
}
