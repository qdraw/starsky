using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace starsky.foundation.database.Models;

public class ThumbnailItem
{
	public ThumbnailItem(string? fileHash = null)
	{
		FileHash = fileHash!;
	}
	
	[Key]
	[Column(TypeName = "varchar(190)")]
	[MaxLength(190)]
	[Required]
	public string FileHash { get; set; }

	/// <summary>
	/// 150px, null is to-do, false is error, true, is done
	/// </summary>
	public bool? TinyMeta { get; set; } = null;

	/// <summary>
	/// 300px, null is to-do, false is error, true, is done
	/// </summary>
	public bool? Small { get; set; } = null;

	/// <summary>
	/// 1000px, null is to-do, false is error, true, is done
	/// </summary>
	public bool? Large { get; set; } = null;

	/// <summary>
	/// 2000px, null is to-do, false is error, true, is done
	/// </summary>
	public bool? ExtraLarge { get; set; } = null;

	
	/// <summary>
	/// When something went wrong add message here
	/// </summary>
	public string? Reasons { get; set; }
}
