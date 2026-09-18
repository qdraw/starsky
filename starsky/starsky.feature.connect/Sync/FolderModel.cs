using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Microsoft.EntityFrameworkCore;
using starsky.foundation.connect.Protocol.Generated;
using starsky.foundation.connect.Sync;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models;

namespace starsky.feature.connect.Sync;

/// <summary>
/// Manages the per-folder index state in the shared EF Core database.
/// Handles full/delta index exchanges and builds ClusterConfig Device entries.
/// </summary>
public sealed class FolderModel
{
	private readonly ApplicationDbContext _db;

	public FolderModel(ApplicationDbContext db)
	{
		_db = db;
	}

	/// <summary>
	/// Replaces all index entries for the folder with the supplied file list.
	/// Resets sequence counter and clears block info.
	/// </summary>
	public async Task ApplyIndexAsync(
		string folder,
		IEnumerable<FileInfo> files,
		CancellationToken ct = default)
	{
		await using var tx = await _db.Database.BeginTransactionAsync(ct);

		// Clear existing entries
		var existingMetas = _db.ConnectFileMetas.Where(m => m.Folder == folder);
		_db.ConnectFileMetas.RemoveRange(existingMetas);

		var existingBlocks = _db.ConnectBlockInfos.Where(b => b.Folder == folder);
		_db.ConnectBlockInfos.RemoveRange(existingBlocks);

		await _db.SaveChangesAsync(ct);

		var folderMeta = await _db.ConnectFolderMetas.FindAsync([folder], ct)
		                 ?? new ConnectFolderMeta { Folder = folder, IndexId = (long)((ulong)Random.Shared.NextInt64()) };

		var sequence = folderMeta.Sequence;
		foreach ( var fi in files )
		{
			sequence++;
			_db.ConnectFileMetas.Add(ToMeta(folder, fi, sequence));
			AddBlockInfos(folder, fi);
		}

		folderMeta.Sequence = sequence;
		_db.ConnectFolderMetas.Update(folderMeta);
		await _db.SaveChangesAsync(ct);
		await tx.CommitAsync(ct);
	}

	/// <summary>
	/// Upserts index entries (delta exchange: only entries changed since last sync).
	/// </summary>
	public async Task ApplyIndexUpdateAsync(
		string folder,
		IEnumerable<FileInfo> files,
		CancellationToken ct = default)
	{
		var folderMeta = await _db.ConnectFolderMetas.FindAsync([folder], ct)
		                 ?? new ConnectFolderMeta { Folder = folder, IndexId = (long)((ulong)Random.Shared.NextInt64()) };

		var sequence = folderMeta.Sequence;
		foreach ( var fi in files )
		{
			var existing = await _db.ConnectFileMetas
				.FirstOrDefaultAsync(m => m.Folder == folder && m.Name == fi.Name, ct);

			sequence++;
			if ( existing is null )
			{
				_db.ConnectFileMetas.Add(ToMeta(folder, fi, sequence));
			}
			else
			{
				UpdateMeta(existing, fi, sequence);
				_db.ConnectFileMetas.Update(existing);
			}

			// Replace block info for this file
			var oldBlocks = _db.ConnectBlockInfos.Where(b => b.Folder == folder && b.Name == fi.Name);
			_db.ConnectBlockInfos.RemoveRange(oldBlocks);
			AddBlockInfos(folder, fi);
		}

		folderMeta.Sequence = sequence;
		_db.ConnectFolderMetas.Update(folderMeta);
		await _db.SaveChangesAsync(ct);
	}

	/// <summary>
	/// Builds the Device entry for our own device to include in ClusterConfig.
	/// </summary>
	public async Task<(long IndexId, long MaxSequence)> GetFolderStateAsync(
		string folder,
		CancellationToken ct = default)
	{
		var meta = await _db.ConnectFolderMetas.FindAsync([folder], ct);
		return meta is null ? (0L, 0L) : (meta.IndexId, meta.Sequence);
	}

	/// <summary>
	/// Returns the last-known IndexId and MaxSequence for a remote peer, or (0,0) if unknown.
	/// </summary>
	public async Task<(long IndexId, long MaxSequence)> GetPeerStateAsync(
		string folder,
		byte[] deviceId,
		CancellationToken ct = default)
	{
		var records = await _db.ConnectDeviceIndexes
			.Where(d => d.Folder == folder)
			.ToListAsync(ct);
		var record = records.FirstOrDefault(d => d.DeviceId.SequenceEqual(deviceId));
		return record is null ? (0L, 0L) : (record.IndexId, record.MaxSequence);
	}

	/// <summary>
	/// Determines whether we need to send a full Index or a delta IndexUpdate
	/// to the remote device, based on the IndexId and MaxSequence it advertised.
	/// </summary>
	public async Task<IndexNeed> NeedsDeltaOrFullAsync(
		string folder,
		byte[] remoteDeviceId,
		long remoteIndexId,
		long remoteMaxSeq,
		CancellationToken ct = default)
	{
		var meta = await _db.ConnectFolderMetas.FindAsync([folder], ct);
		if ( meta is null || meta.IndexId == 0 ) return IndexNeed.Full;

		// Look up what we know about this peer's stored sequence for us
		var deviceRecord = await _db.ConnectDeviceIndexes
			.FirstOrDefaultAsync(d => d.Folder == folder && d.DeviceId == remoteDeviceId, ct);

		if ( remoteIndexId != 0 && remoteIndexId == (deviceRecord?.IndexId ?? 0) )
		{
			return new IndexNeed(false, remoteMaxSeq);
		}

		return IndexNeed.Full;
	}

	/// <summary>
	/// Returns files with Sequence > <paramref name="fromSeq"/>, ordered by sequence.
	/// </summary>
	public async Task<List<ConnectFileMeta>> GetFilesForUpdateAsync(
		string folder,
		long fromSeq,
		CancellationToken ct = default)
	{
		return await _db.ConnectFileMetas
			.Where(m => m.Folder == folder && m.Sequence > fromSeq)
			.OrderBy(m => m.Sequence)
			.ToListAsync(ct);
	}

	/// <summary>
	/// Returns files with Sequence > <paramref name="fromSeq"/> together with their block infos.
	/// </summary>
	public async Task<List<(ConnectFileMeta Meta, List<ConnectBlockInfo> Blocks)>> GetFilesWithBlocksForUpdateAsync(
		string folder,
		long fromSeq,
		CancellationToken ct = default)
	{
		var metas = await _db.ConnectFileMetas
			.Where(m => m.Folder == folder && m.Sequence > fromSeq)
			.OrderBy(m => m.Sequence)
			.ToListAsync(ct);

		var names = metas.Select(m => m.Name).ToHashSet();
		var allBlocks = await _db.ConnectBlockInfos
			.Where(b => b.Folder == folder && names.Contains(b.Name))
			.ToListAsync(ct);

		var byName = allBlocks.GroupBy(b => b.Name)
			.ToDictionary(g => g.Key, g => g.OrderBy(b => b.Offset).ToList());

		return metas
			.Select(m => (m, byName.TryGetValue(m.Name, out var blocks) ? blocks : []))
			.ToList();
	}

	/// <summary>
	/// Records what a remote device has told us its max sequence is for us.
	/// </summary>
	public async Task RecordPeerStateAsync(
		string folder,
		byte[] deviceId,
		long indexId,
		long maxSequence,
		CancellationToken ct = default)
	{
		var existing = await _db.ConnectDeviceIndexes
			.FirstOrDefaultAsync(d => d.Folder == folder && d.DeviceId == deviceId, ct);

		if ( existing is null )
		{
			_db.ConnectDeviceIndexes.Add(new ConnectDeviceIndex
			{
				Folder = folder,
				DeviceId = deviceId,
				IndexId = indexId,
				MaxSequence = maxSequence,
			});
		}
		else
		{
			existing.IndexId = indexId;
			existing.MaxSequence = maxSequence;
			_db.ConnectDeviceIndexes.Update(existing);
		}

		await _db.SaveChangesAsync(ct);
	}

	// ── Private helpers ───────────────────────────────────────────────────────

	private static ConnectFileMeta ToMeta(string folder, FileInfo fi, long sequence) =>
		new()
		{
			Folder = folder,
			Name = fi.Name,
			Version = fi.Version?.ToByteArray() ?? [],
			Sequence = sequence,
			BlockSize = fi.BlockSize == 0 ? 128 * 1024 : fi.BlockSize,
			Deleted = fi.Deleted,
			Invalid = fi.Invalid,
			NoPermissions = fi.NoPermissions,
			ModifiedBy = ( long )fi.ModifiedBy,
			SymlinkTarget = fi.SymlinkTarget ?? string.Empty,
		};

	private static void UpdateMeta(ConnectFileMeta meta, FileInfo fi, long sequence)
	{
		meta.Version = fi.Version?.ToByteArray() ?? [];
		meta.Sequence = sequence;
		meta.BlockSize = fi.BlockSize == 0 ? 128 * 1024 : fi.BlockSize;
		meta.Deleted = fi.Deleted;
		meta.Invalid = fi.Invalid;
		meta.NoPermissions = fi.NoPermissions;
		meta.ModifiedBy = ( long )fi.ModifiedBy;
		meta.SymlinkTarget = fi.SymlinkTarget ?? string.Empty;
	}

	private void AddBlockInfos(string folder, FileInfo fi)
	{
		foreach ( var block in fi.Blocks )
		{
			_db.ConnectBlockInfos.Add(new ConnectBlockInfo
			{
				Folder = folder,
				Name = fi.Name,
				Offset = block.Offset,
				Size = block.Size,
				Hash = block.Hash.ToByteArray(),
			});
		}
	}
}

public readonly struct IndexNeed
{
	public static readonly IndexNeed Full = new(true, 0);

	public IndexNeed(bool requiresFull, long fromSequence)
	{
		RequiresFull = requiresFull;
		FromSequence = fromSequence;
	}

	public bool RequiresFull { get; }
	public long FromSequence { get; }
}
