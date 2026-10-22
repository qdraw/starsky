using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace starsky.foundation.database.Models;

/// <summary>
/// Per-block SHA-256 hashes for a single file, as required by BEP v1 block-level delta transfer.
/// FileIndexItem.FileHash covers the whole file; this table stores individual block hashes.
/// </summary>
public sealed class ConnectBlockInfo
{
	[Key]
	[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public int Id { get; set; }

	/// <summary>Syncthing folder ID.</summary>
	[MaxLength(190)]
	public string Folder { get; set; } = string.Empty;

	/// <summary>File path relative to folder root, '/' separators.</summary>
	[MaxLength(380)]
	public string Name { get; set; } = string.Empty;

	/// <summary>Byte offset of this block within the file.</summary>
	public long Offset { get; set; }

	/// <summary>Block size in bytes (may be smaller than ConnectFileMeta.BlockSize for the last block).</summary>
	public int Size { get; set; }

	/// <summary>SHA-256 hash of the block data (32 bytes).</summary>
	public byte[] Hash { get; set; } = [];
}
