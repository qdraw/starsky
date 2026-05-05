using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace starsky.foundation.database.Models;

public enum QueueItemStatus
{
	Pending = 0,
	Processing = 1,
	Done = 2,
	Failed = 3
}

public sealed class QueueItem
{
	[Key]
	[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public int Id { get; set; }

	[MaxLength(64)] [Required] public string QueueName { get; set; } = string.Empty;

	[Column(TypeName = "varchar(36)")]
	[MaxLength(36)]
	public Guid JobId { get; set; }

	[MaxLength(150)] [Required] public string JobType { get; set; } = string.Empty;

	[MaxLength(1024)] public string? MetaData { get; set; }

	[MaxLength(512)] public string? TraceParentId { get; set; }

	public int PriorityLane { get; set; }

	[MaxLength(4096)] public string? PayloadJson { get; set; }

	[ConcurrencyCheck] public QueueItemStatus Status { get; set; } = QueueItemStatus.Pending;

	public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

	public DateTime? ClaimedAtUtc { get; set; }

	public DateTime? ProcessedAtUtc { get; set; }
}
