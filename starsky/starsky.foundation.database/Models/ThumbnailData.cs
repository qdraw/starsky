using System.ComponentModel.DataAnnotations;

namespace starsky.foundation.database.Models;

public class ThumbnailData
{
	[Key]
	public string FileHash { get; set; }

	/// <summary>
	/// 150px
	/// </summary>
	public bool TinyMeta { get; set; } = false;

	/// <summary>
	/// 300px
	/// </summary>
	public bool Small { get; set; } = false;

	/// <summary>
	/// 1000px
	/// </summary>
	public bool Large { get; set; } = false;

	/// <summary>
	/// 2000px
	/// </summary>
	public bool ExtraLarge { get; set; } = false;

	public string? Errors { get; set; }
}
