using starsky.foundation.import.Models;

namespace starsky.foundation.import.Interfaces;

public interface IChunkUploadSessionStore
{
	ChunkUploadInitResultModel Create(string fileName, string parentDirectory,
		int totalChunks, long totalSize);
	bool AddChunk(string uploadId, int chunkIndex, byte[] chunkData, out string errorMessage);
	ChunkUploadStatusModel? GetStatus(string uploadId);
	bool TryAssemble(string uploadId, out byte[] payload, out string errorMessage);
	bool Delete(string uploadId);
}

