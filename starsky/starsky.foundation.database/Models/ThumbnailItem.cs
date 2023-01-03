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
	
	/// <summary>
	/// 
	/// </summary>
	/// <param name="fileHash"></param>
	/// <param name="thumbnailSize"></param>
	/// <param name="setStatus"></param>
	public ThumbnailItem(string fileHash, ThumbnailSize? thumbnailSize = null, bool? setStatus = null )
	{
		FileHash = fileHash;
		Change(thumbnailSize, setStatus);
	}

	/// <summary>
	/// Null is to-do
	/// True is done
	/// False is Failed
	/// </summary>
	/// <param name="thumbnailSize">The size</param>
	/// <param name="setStatus">Null is to-do |  True is done | False is Failed</param>
	/// <exception cref="ArgumentOutOfRangeException"></exception>
	public void Change(ThumbnailSize? thumbnailSize = null, bool? setStatus = null)
	{
		switch ( thumbnailSize )
		{
			case ThumbnailSize.TinyMeta:
				TinyMeta = setStatus;
				break;
			case ThumbnailSize.Small:
				Small = setStatus;
				break;
			case ThumbnailSize.Large:
				Large = setStatus;
				break;
			case ThumbnailSize.ExtraLarge:
				ExtraLarge = setStatus;
				break;
			case null:
				break;
			default:
				throw new ArgumentOutOfRangeException(nameof(thumbnailSize), thumbnailSize, null);
		}
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
	/// When something went wrong add message here
	/// </summary>
	public string? Reasons { get; set; }
}