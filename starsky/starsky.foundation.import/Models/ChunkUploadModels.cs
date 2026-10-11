using System;
using System.Collections.Generic;

namespace starsky.foundation.import.Models;

public sealed class ChunkUploadSessionModel
{
	public string UploadId { get; set; } = string.Empty;
	public string FileName { get; set; } = string.Empty;
	public string ParentDirectory { get; set; } = string.Empty;
	public int TotalChunks { get; set; }
	public long TotalSize { get; set; }
	public DateTime CreatedAt { get; set; }
	public DateTime ExpiresAt { get; set; }
	public long ReceivedBytes { get; set; }
	public Dictionary<int, byte[]> Chunks { get; set; } = [];
}

public sealed class ChunkUploadStatusModel
{
	public string UploadId { get; set; } = string.Empty;
	public string FileName { get; set; } = string.Empty;
	public string ParentDirectory { get; set; } = string.Empty;
	public int TotalChunks { get; set; }
	public int ReceivedChunks { get; set; }
	public long TotalSize { get; set; }
	public long ReceivedBytes { get; set; }
	public bool IsComplete { get; set; }
	public DateTime ExpiresAt { get; set; }
}

public sealed class ChunkUploadInitResultModel
{
	public string UploadId { get; set; } = string.Empty;
	public DateTime ExpiresAt { get; set; }
}

