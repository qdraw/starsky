using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace starsky.foundation.database.Models;

/// <summary>
/// Tracks the index state of each remote device per folder.
/// Used to determine whether to send a full Index or only an IndexUpdate on reconnect.
/// </summary>
public sealed class ConnectDeviceIndex
{
	[Key]
	[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public int Id { get; set; }

	/// <summary>Syncthing folder ID.</summary>
	[MaxLength(190)]
	public string Folder { get; set; } = string.Empty;

	/// <summary>Raw 32-byte device ID (SHA-256 of the peer's certificate).</summary>
	[MaxLength(32)]
	public byte[] DeviceId { get; set; } = [];

	/// <summary>
	/// The last sequence number we sent to this peer for this folder.
	/// On reconnect, we only need to send files with sequence > MaxSequence.
	/// </summary>
	public long MaxSequence { get; set; }

	/// <summary>
	/// The IndexId we sent to this peer. If it differs from ConnectFolderMeta.IndexId,
	/// a full Index must be sent instead of a delta update.
	/// </summary>
	public long IndexId { get; set; }
}
