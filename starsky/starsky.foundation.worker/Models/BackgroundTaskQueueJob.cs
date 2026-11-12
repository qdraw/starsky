using System;
using starsky.foundation.platform.Tenancy;

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

	/// <summary>
	///     The tenant that queued the job (the ambient tenant at creation), restored when the job
	///     runs. The queues are shared by all tenants
	/// </summary>
	public string TenantId { get; init; } = TenantAmbient.Current;
}
