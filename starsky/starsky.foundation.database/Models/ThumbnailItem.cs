using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using starsky.foundation.platform.Enums;

namespace starsky.foundation.database.Models;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public class ThumbnailItem
{
	public ThumbnailItem()
	{
		// used be EF Core
		FileHash = string.Empty;
	}
	
	public ThumbnailItem(string fileHash, bool? tinyMeta, bool? small,
		bool? large, bool? extraLarge, string? reasons = null)
	{
		FileHash = fileHash;
		if ( tinyMeta != null ) TinyMeta = tinyMeta;
		if ( small != null ) Small = small;
		if ( large != null ) Large = large;
		if ( extraLarge != null ) ExtraLarge = extraLarge;
		if ( reasons != null ) Reasons = reasons;
	}
	
	[Key]
	[Column(TypeName = "varchar(190)")]
	[MaxLength(190)]
	[Required]
	public string FileHash { get; set; }

	/// <summary>
	/// 150px, null is to-do, false is error, true, is done
	/// </summary>
	public bool? TinyMeta { get; set; }

	/// <summary>
	/// 300px, null is to-do, false is error, true, is done
	/// </summary>
	public bool? Small { get; set; }

	/// <summary>
	/// 1000px, null is to-do, false is error, true, is done
	/// </summary>
	public bool? Large { get; set; }

	/// <summary>
	/// 2000px, null is to-do, false is error, true, is done
	/// </summary>
	public bool? ExtraLarge { get; set; }

	/// <summary>
	/// Private field to avoid null issues
	/// </summary>
	private string ReasonsPrivate { get; set; } = string.Empty;
	
	/// <summary>
	/// When something went wrong add message here
	/// </summary>
	public string? Reasons {
		get => ReasonsPrivate;
		set
		{
			if ( value == null )
			{
				return;
			}
			ReasonsPrivate = value;
		} 
	}
}
