using System.ComponentModel.DataAnnotations;

namespace starsky.foundation.database.Models;

/// <summary>
/// Per-folder metadata for BEP v1 delta index exchange.
/// Each Syncthing folder has one row here.
/// </summary>
public sealed class ConnectFolderMeta
{
	/// <summary>Syncthing folder ID — the primary key.</summary>
	[Key]
	[MaxLength(190)]
	public string Folder { get; set; } = string.Empty;

	/// <summary>
	/// 64-bit random value generated once per folder and persisted.
	/// Must be regenerated if the index is reset.
	/// Announced to peers in ClusterConfig so they know whether to send full Index or delta.
	/// </summary>
	public long IndexId { get; set; }

	/// <summary>
	/// Current local sequence counter. Incremented on every index update.
	/// Peers use this to request only files with sequence > their known max.
	/// </summary>
	public long Sequence { get; set; }
}
