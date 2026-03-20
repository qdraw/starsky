using System;

namespace starsky.foundation.worker.Models;

[Serializable]
public sealed class BackgroundTaskQueueJob
{
	public Guid JobId { get; init; } = Guid.NewGuid();
	public string? MetaData { get; init; }
	public string? TraceParentId { get; init; }
	public int PriorityLane { get; init; }
	public DateTime CreatedAtUtc { get; init; } = DateTime.UtcNow;
	public string? JobType { get; init; }
	public string? PayloadJson { get; init; }
}
