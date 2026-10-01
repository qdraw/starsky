using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace starsky.foundation.database.Models;

public sealed class ConnectFileMeta
{
	[Key]
	[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public int Id { get; set; }

	/// <summary>
	/// Syncthing folder ID (e.g. "default"). Soft-join to FileIndexItem — no FK constraint
	/// because BEP may learn about a file before Starsky's scanner has processed it.
	/// </summary>
	[MaxLength(190)]
	public string Folder { get; set; } = string.Empty;

	/// <summary>
	/// File path relative to the folder root, using '/' separators (BEP convention).
	/// </summary>
	[MaxLength(380)]
	public string Name { get; set; } = string.Empty;

	/// <summary>
	/// Serialised BEP Vector protobuf (list of device-id/counter pairs forming a Lamport clock).
	/// </summary>
	public byte[] Version { get; set; } = [];

	/// <summary>
	/// Monotonically increasing index sequence; incremented on every local change.
	/// Used for delta index exchange with peers.
	/// </summary>
	public long Sequence { get; set; }

	/// <summary>
	/// Block size in bytes used to split this file; a power of two in [128 KiB, 16 MiB].
	/// </summary>
	public int BlockSize { get; set; }

	/// <summary>
	/// True when the file has been deleted on the device that owns this record.
	/// Deleted entries must be kept so peers can learn about the deletion.
	/// </summary>
	public bool Deleted { get; set; }

	/// <summary>
	/// True when the file is present but cannot be synced (e.g. permission error).
	/// </summary>
	public bool Invalid { get; set; }

	/// <summary>
	/// Unix permission bits (e.g. 0644). Zero when not set or not applicable.
	/// </summary>
	public int Permissions { get; set; }

	/// <summary>
	/// True when permission bits should not be synced.
	/// </summary>
	public bool NoPermissions { get; set; }

	/// <summary>
	/// Short device ID (first 64 bits of the raw device hash) of the device that last
	/// modified this file. Stored as the BEP uint64 value.
	/// </summary>
	public long ModifiedBy { get; set; }

	/// <summary>
	/// Symlink target path. Empty for non-symlink entries.
	/// </summary>
	[MaxLength(380)]
	public string SymlinkTarget { get; set; } = string.Empty;
}
