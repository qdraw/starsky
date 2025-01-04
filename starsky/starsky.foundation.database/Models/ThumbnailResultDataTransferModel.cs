using System;
using starsky.foundation.platform.Thumbnails;

namespace starsky.foundation.database.Models;

public class ThumbnailResultDataTransferModel
{
	public ThumbnailResultDataTransferModel(string fileHash, bool? tinyMeta = null,
		bool? small = null, bool? large = null, bool? extraLarge = null)
	{
		FileHash = fileHash;
		if ( tinyMeta != null )
		{
			TinyMeta = tinyMeta;
		}

		if ( small != null )
		{
			Small = small;
		}

		if ( large != null )
		{
			Large = large;
		}

		if ( extraLarge != null )
		{
			ExtraLarge = extraLarge;
		}
	}

	public string? FileHash { get; set; }

	/// <summary>
	///     150px, null is to-do, false is error, true, is done
	/// </summary>
	public bool? TinyMeta { get; set; }

	/// <summary>
	///     300px, null is to-do, false is error, true, is done
	/// </summary>
	public bool? Small { get; set; }

	/// <summary>
	///     1000px, null is to-do, false is error, true, is done
	/// </summary>
	public bool? Large { get; set; }

	/// <summary>
	///     2000px, null is to-do, false is error, true, is done
	/// </summary>
	public bool? ExtraLarge { get; set; }

	/// <summary>
	///     When something went wrong add message here
	/// </summary>
	public string? Reasons { get; set; }

	/// <summary>
	///     Null is to-do
	///     True is done
	///     False is Failed
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
}
