using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using starsky.foundation.connect.Sync;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models;

namespace starsky.feature.connect.Sync;

/// <summary>
/// Periodically scans configured folders, computes block-level SHA-256 hashes,
/// and keeps ConnectFileMeta / ConnectBlockInfo up to date.
/// Also performs incremental re-scans when ISyncLocalChangeSource fires.
/// </summary>
public sealed class Scanner : IHostedService, IDisposable
{
	private static readonly TimeSpan FullScanInterval = TimeSpan.FromHours(1);

	private readonly ApplicationDbContext _db;
	private readonly IReadOnlyList<string> _folderRoots;
	private readonly ISyncLocalChangeSource _changeSource;
	private readonly FolderModel _folderModel;
	private readonly ILogger<Scanner> _logger;
	private CancellationTokenSource? _cts;

	public Scanner(
		ApplicationDbContext db,
		IReadOnlyList<string> folderRoots,
		ISyncLocalChangeSource changeSource,
		FolderModel folderModel,
		ILogger<Scanner> logger)
	{
		_db = db;
		_folderRoots = folderRoots;
		_changeSource = changeSource;
		_folderModel = folderModel;
		_logger = logger;
	}

	public Task StartAsync(CancellationToken cancellationToken)
	{
		_cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
		_changeSource.LocalFileChanged += OnLocalFileChanged;
		_ = Task.Run(() => ScanLoopAsync(_cts.Token), _cts.Token);
		return Task.CompletedTask;
	}

	public Task StopAsync(CancellationToken cancellationToken)
	{
		_changeSource.LocalFileChanged -= OnLocalFileChanged;
		_cts?.Cancel();
		return Task.CompletedTask;
	}

	private async Task ScanLoopAsync(CancellationToken ct)
	{
		while ( !ct.IsCancellationRequested )
		{
			await ScanAllFoldersAsync(ct);
			try
			{
				await Task.Delay(FullScanInterval, ct);
			}
			catch ( OperationCanceledException ) { break; }
		}
	}

	private void OnLocalFileChanged(object? sender, SyncLocalChangeEventArgs args)
	{
		// Fire-and-forget incremental scan for the changed file
		_ = Task.Run(async () =>
		{
			try
			{
				var folder = FindFolderRoot(args.FullPath);
				if ( folder is null ) return;
				await ScanFileAsync(folder.Value.FolderId, folder.Value.Root, args.FullPath, CancellationToken.None);
			}
			catch ( Exception ex )
			{
				_logger.LogWarning(ex, "Incremental scan failed for {Path}.", args.FullPath);
			}
		});
	}

	private async Task ScanAllFoldersAsync(CancellationToken ct)
	{
		foreach ( var root in _folderRoots )
		{
			if ( ct.IsCancellationRequested ) break;
			var folderId = Path.GetFileName(root.TrimEnd(Path.DirectorySeparatorChar));
			try
			{
				await ScanFolderAsync(folderId, root, ct);
			}
			catch ( Exception ex )
			{
				_logger.LogWarning(ex, "Scan failed for folder {Root}.", root);
			}
		}
	}

	private async Task ScanFolderAsync(string folderId, string root, CancellationToken ct)
	{
		_logger.LogDebug("Scanning folder {FolderId}.", folderId);

		// Use FileIndexItem records already in the DB for this folder root
		var fileItems = await _db.FileIndex
			.Where(f => f.ParentDirectory != null && f.ParentDirectory.StartsWith("/" + folderId))
			.Where(f => f.IsDirectory != true)
			.ToListAsync(ct);

		foreach ( var item in fileItems )
		{
			if ( ct.IsCancellationRequested ) break;
			var fullPath = Path.Combine(root, item.FilePath?.TrimStart('/') ?? string.Empty);
			if ( !File.Exists(fullPath) ) continue;

			try
			{
				await ScanFileItemAsync(folderId, root, fullPath, item, ct);
			}
			catch ( Exception ex )
			{
				_logger.LogDebug(ex, "Could not scan {Path}.", fullPath);
			}
		}
	}

	private async Task ScanFileAsync(string folderId, string root, string fullPath, CancellationToken ct)
	{
		if ( !File.Exists(fullPath) )
		{
			// Mark as deleted
			var relativePath = fullPath[root.Length..].TrimStart(Path.DirectorySeparatorChar)
				.Replace(Path.DirectorySeparatorChar, '/');
			var meta = await _db.ConnectFileMetas
				.FirstOrDefaultAsync(m => m.Folder == folderId && m.Name == relativePath, ct);
			if ( meta is not null )
			{
				meta.Deleted = true;
				_db.ConnectFileMetas.Update(meta);
				await _db.SaveChangesAsync(ct);
			}

			return;
		}

		await ScanFileItemAsync(folderId, root, fullPath, null, ct);
	}

	private async Task ScanFileItemAsync(
		string folderId,
		string root,
		string fullPath,
		FileIndexItem? existingItem,
		CancellationToken ct)
	{
		var relativePath = fullPath[root.Length..].TrimStart(Path.DirectorySeparatorChar)
			.Replace(Path.DirectorySeparatorChar, '/');

		var info = new System.IO.FileInfo(fullPath);
		var blockSize = BlockSizing.SelectBlockSize(info.Length);
		var blocks = await ComputeBlocksAsync(fullPath, blockSize, ct);

		// Upsert ConnectBlockInfo
		var existing = _db.ConnectBlockInfos.Where(b => b.Folder == folderId && b.Name == relativePath);
		_db.ConnectBlockInfos.RemoveRange(existing);

		foreach ( var (offset, size, hash) in blocks )
		{
			_db.ConnectBlockInfos.Add(new ConnectBlockInfo
			{
				Folder = folderId,
				Name = relativePath,
				Offset = offset,
				Size = size,
				Hash = hash,
			});
		}

		// Upsert ConnectFileMeta
		var meta = await _db.ConnectFileMetas
			.FirstOrDefaultAsync(m => m.Folder == folderId && m.Name == relativePath, ct);

		var folderMeta = await _db.ConnectFolderMetas.FindAsync([folderId], ct)
		                 ?? new ConnectFolderMeta { Folder = folderId, IndexId = (long)((ulong)Random.Shared.NextInt64()) };

		folderMeta.Sequence++;

		if ( meta is null )
		{
			_db.ConnectFileMetas.Add(new ConnectFileMeta
			{
				Folder = folderId,
				Name = relativePath,
				BlockSize = blockSize,
				Sequence = folderMeta.Sequence,
			});
		}
		else
		{
			meta.BlockSize = blockSize;
			meta.Sequence = folderMeta.Sequence;
			meta.Deleted = false;
			_db.ConnectFileMetas.Update(meta);
		}

		_db.ConnectFolderMetas.Update(folderMeta);
		await _db.SaveChangesAsync(ct);
	}

	private static async Task<List<(long Offset, int Size, byte[] Hash)>> ComputeBlocksAsync(
		string path,
		int blockSize,
		CancellationToken ct)
	{
		var result = new List<(long, int, byte[])>();
		await using var stream = File.OpenRead(path);
		var buffer = new byte[blockSize];
		long offset = 0;

		while ( true )
		{
			var read = await stream.ReadAsync(buffer.AsMemory(0, blockSize), ct);
			if ( read == 0 ) break;

			var hash = SHA256.HashData(buffer.AsSpan(0, read));
			result.Add((offset, read, hash));
			offset += read;
		}

		return result;
	}

	private (string FolderId, string Root)? FindFolderRoot(string fullPath)
	{
		foreach ( var root in _folderRoots )
		{
			if ( fullPath.StartsWith(root, StringComparison.OrdinalIgnoreCase) )
			{
				return (Path.GetFileName(root.TrimEnd(Path.DirectorySeparatorChar)), root);
			}
		}

		return null;
	}

	/// <summary>
	/// Triggers an immediate incremental scan of a single file path.
	/// The folder root is determined from the configured <see cref="_folderRoots"/>.
	/// </summary>
	public Task ScanPathAsync(string fullPath, CancellationToken ct = default)
	{
		var folder = FindFolderRoot(fullPath);
		if ( folder is null )
		{
			return Task.CompletedTask;
		}

		return ScanFileAsync(folder.Value.FolderId, folder.Value.Root, fullPath, ct);
	}

	/// <summary>
	/// Scans all files under <paramref name="root"/> directly from the filesystem,
	/// without relying on <see cref="starsky.foundation.database.Models.FileIndexItem"/> records.
	/// Suitable for integration tests and bootstrapping new folders.
	/// </summary>
	public async Task ScanFolderDirectAsync(string folderId, string root, CancellationToken ct = default)
	{
		foreach ( var fullPath in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories) )
		{
			if ( ct.IsCancellationRequested )
			{
				break;
			}

			if ( fullPath.Contains(TempDirName, StringComparison.Ordinal) )
			{
				continue;
			}

			if ( Path.GetFileName(fullPath) == ".stfolder" )
			{
				continue;
			}

			try
			{
				await ScanFileItemAsync(folderId, root, fullPath, null, ct);
			}
			catch ( Exception ex )
			{
				_logger.LogWarning(ex, "Error scanning {Path}.", fullPath);
			}
		}
	}

	private const string TempDirName = ".starskyconnect_tmp";

	public void Dispose() => _cts?.Dispose();
}
