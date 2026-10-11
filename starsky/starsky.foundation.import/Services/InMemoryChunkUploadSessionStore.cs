using System;
using System.Collections.Concurrent;
using System.Linq;
using starsky.foundation.import.Interfaces;
using starsky.foundation.import.Models;
using starsky.foundation.injection;

namespace starsky.foundation.import.Services;

[Service(typeof(IChunkUploadSessionStore), InjectionLifetime = InjectionLifetime.Singleton)]
public sealed class InMemoryChunkUploadSessionStore : IChunkUploadSessionStore
{
	private static readonly TimeSpan SessionTtl = TimeSpan.FromHours(1);
	private readonly ConcurrentDictionary<string, ChunkUploadSessionModel> _sessions = new();

	public ChunkUploadInitResultModel Create(string fileName, string parentDirectory,
		int totalChunks, long totalSize)
	{
		ClearExpired();
		var now = DateTime.UtcNow;
		var uploadId = Guid.NewGuid().ToString("N");
		var session = new ChunkUploadSessionModel
		{
			UploadId = uploadId,
			FileName = fileName,
			ParentDirectory = parentDirectory,
			TotalChunks = totalChunks,
			TotalSize = totalSize,
			CreatedAt = now,
			ExpiresAt = now.Add(SessionTtl),
			ReceivedBytes = 0
		};
		_sessions[uploadId] = session;

		return new ChunkUploadInitResultModel
		{
			UploadId = uploadId,
			ExpiresAt = session.ExpiresAt
		};
	}

	public bool AddChunk(string uploadId, int chunkIndex, byte[] chunkData, out string errorMessage)
	{
		errorMessage = string.Empty;
		ClearExpired();
		if ( !_sessions.TryGetValue(uploadId, out var session) )
		{
			errorMessage = "upload session not found";
			return false;
		}

		if ( chunkIndex < 0 || chunkIndex >= session.TotalChunks )
		{
			errorMessage = "invalid chunk index";
			return false;
		}

		lock ( session )
		{
			if ( session.Chunks.ContainsKey(chunkIndex) )
			{
				errorMessage = "chunk already uploaded";
				return false;
			}

			session.Chunks[chunkIndex] = chunkData;
			session.ReceivedBytes += chunkData.LongLength;
			session.ExpiresAt = DateTime.UtcNow.Add(SessionTtl);
		}

		return true;
	}

	public ChunkUploadStatusModel? GetStatus(string uploadId)
	{
		ClearExpired();
		if ( !_sessions.TryGetValue(uploadId, out var session) )
		{
			return null;
		}

		lock ( session )
		{
			return new ChunkUploadStatusModel
			{
				UploadId = session.UploadId,
				FileName = session.FileName,
				ParentDirectory = session.ParentDirectory,
				TotalChunks = session.TotalChunks,
				ReceivedChunks = session.Chunks.Count,
				TotalSize = session.TotalSize,
				ReceivedBytes = session.ReceivedBytes,
				IsComplete = session.Chunks.Count == session.TotalChunks &&
				             session.ReceivedBytes == session.TotalSize,
				ExpiresAt = session.ExpiresAt
			};
		}
	}

	public bool TryAssemble(string uploadId, out byte[] payload, out string errorMessage)
	{
		payload = [];
		errorMessage = string.Empty;
		ClearExpired();
		if ( !_sessions.TryGetValue(uploadId, out var session) )
		{
			errorMessage = "upload session not found";
			return false;
		}

		lock ( session )
		{
			if ( session.Chunks.Count != session.TotalChunks )
			{
				errorMessage = "missing chunks";
				return false;
			}

			if ( session.ReceivedBytes != session.TotalSize )
			{
				errorMessage = "total size mismatch";
				return false;
			}

			if ( session.TotalSize > int.MaxValue )
			{
				errorMessage = "payload too large for in-memory assembly";
				return false;
			}

			var buffer = new byte[(int)session.TotalSize];
			var offset = 0;
			foreach ( var chunk in session.Chunks.OrderBy(p => p.Key).Select(p => p.Value) )
			{
				Buffer.BlockCopy(chunk, 0, buffer, offset, chunk.Length);
				offset += chunk.Length;
			}

			payload = buffer;
			return true;
		}
	}

	public bool Delete(string uploadId)
	{
		return _sessions.TryRemove(uploadId, out _);
	}

	private void ClearExpired()
	{
		var now = DateTime.UtcNow;
		foreach ( var item in _sessions.Where(p => p.Value.ExpiresAt <= now).ToList() )
		{
			_sessions.TryRemove(item.Key, out _);
		}
	}
}


