using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using starsky.foundation.connect.Protocol.Generated;

namespace starsky.feature.connect.Sync;

/// <summary>
/// Content-addressed temporary storage for in-progress file downloads.
/// Blocks are written to a temp directory and assembled into the final file
/// when all blocks have been received.
/// </summary>
public sealed class BlockStore : IDisposable
{
	private const string TempDirName = ".starskyconnect_tmp";

	// (folder, name) → set of received block offsets
	private readonly ConcurrentDictionary<(string, string), HashSet<long>> _received = new();
	private readonly ConcurrentDictionary<(string, string), SemaphoreSlim> _locks = new();

	private string TempPath(string folder, string name) =>
		Path.Combine(folder, TempDirName, name.Replace('/', Path.DirectorySeparatorChar) + ".part");

	public async Task WriteBlockAsync(
		string folder,
		string name,
		long offset,
		ReadOnlyMemory<byte> data,
		CancellationToken ct = default)
	{
		var key = (folder, name);
		var sem = _locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
		await sem.WaitAsync(ct);
		try
		{
			var tempPath = TempPath(folder, name);
			Directory.CreateDirectory(Path.GetDirectoryName(tempPath)!);

			await using var stream = new FileStream(
				tempPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
			stream.Seek(offset, SeekOrigin.Begin);
			await stream.WriteAsync(data, ct);

			var received = _received.GetOrAdd(key, _ => []);
			received.Add(offset);
		}
		finally
		{
			sem.Release();
		}
	}

	public async Task<ReadOnlyMemory<byte>?> ReadBlockAsync(
		string folder,
		string name,
		long offset,
		int size,
		CancellationToken ct = default)
	{
		var tempPath = TempPath(folder, name);
		if ( !File.Exists(tempPath) ) return null;

		var buffer = new byte[size];
		await using var stream = new FileStream(
			tempPath, FileMode.Open, FileAccess.Read, FileShare.Read);
		stream.Seek(offset, SeekOrigin.Begin);
		var read = await stream.ReadAsync(buffer, ct);
		if ( read != size ) return null;
		return buffer.AsMemory();
	}

	public bool IsComplete(string folder, string name, IReadOnlyList<BlockInfo> blocks)
	{
		if ( blocks.Count == 0 ) return true;
		var key = (folder, name);
		if ( !_received.TryGetValue(key, out var received) ) return false;
		return blocks.All(b => received.Contains(b.Offset));
	}

	public async Task AssembleFileAsync(
		string folder,
		string name,
		string destPath,
		CancellationToken ct = default)
	{
		var tempPath = TempPath(folder, name);
		var destDir = Path.GetDirectoryName(destPath);
		if ( destDir is not null )
		{
			Directory.CreateDirectory(destDir);
		}

		// Move temp file to destination
		if ( File.Exists(destPath) )
		{
			File.Delete(destPath);
		}

		File.Move(tempPath, destPath);
	}

	public void Forget(string folder, string name)
	{
		var key = (folder, name);
		_received.TryRemove(key, out _);
		if ( _locks.TryRemove(key, out var sem) )
		{
			sem.Dispose();
		}

		var tempPath = TempPath(folder, name);
		try { File.Delete(tempPath); } catch { /* best effort */ }
	}

	public void Dispose()
	{
		foreach ( var sem in _locks.Values )
		{
			sem.Dispose();
		}
	}
}
