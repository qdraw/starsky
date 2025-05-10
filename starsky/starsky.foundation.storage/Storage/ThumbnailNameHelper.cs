using System;
using System.Globalization;
using System.Text.RegularExpressions;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Thumbnails;

namespace starsky.foundation.storage.Storage;

/// <summary>
///     ThumbnailNamesHelper
/// </summary>
public static partial class ThumbnailNameHelper
{
	/// <summary>
	///     Without TinyMeta
	/// </summary>
	public static readonly ThumbnailSize[] GeneratedThumbnailSizes =
	[
		ThumbnailSize.ExtraLarge, ThumbnailSize.Small, ThumbnailSize.Large
	];

	public static readonly ThumbnailSize[] AllThumbnailSizes =
	[
		ThumbnailSize.TinyMeta, ThumbnailSize.ExtraLarge, ThumbnailSize.Small,
		ThumbnailSize.Large
	];

	public static int GetSize(ThumbnailSize size)
	{
		switch ( size )
		{
			case ThumbnailSize.TinyIcon:
				return 4;
			case ThumbnailSize.TinyMeta:
				return 150;
			case ThumbnailSize.Small:
				return 300;
			case ThumbnailSize.Large:
				return 1000;
			case ThumbnailSize.ExtraLarge:
				return 2000;
			default:
				throw new ArgumentOutOfRangeException(nameof(size), size, null);
		}
	}

	public static ThumbnailSize GetSize(int size)
	{
		switch ( size )
		{
			case 4:
				return ThumbnailSize.TinyIcon;
			case 150:
				return ThumbnailSize.TinyMeta;
			case 300:
				return ThumbnailSize.Small;
			case 1000:
				return ThumbnailSize.Large;
			case 2000:
				return ThumbnailSize.ExtraLarge;
			default:
				return ThumbnailSize.Unknown;
		}
	}

	public static ThumbnailSize GetSize(string fileName,
		ThumbnailImageFormat imageFormat)
	{
		var fileNameWithoutExtension =
			fileName.Replace($".{imageFormat}", string.Empty);

		var afterAtString = Regex.Match(fileNameWithoutExtension, "@\\d+",
				RegexOptions.None, TimeSpan.FromMilliseconds(200))
			.Value.Replace("@", string.Empty);

		if ( fileNameWithoutExtension.Replace($"@{afterAtString}", string.Empty).Length != 26 )
		{
			return ThumbnailSize.Unknown;
		}

		if ( string.IsNullOrEmpty(afterAtString) )
		{
			return ThumbnailSize.Large;
		}

		int.TryParse(afterAtString, NumberStyles.Number,
			CultureInfo.InvariantCulture, out var afterAt);
		return GetSize(afterAt);
	}

	public static int Width(this ThumbnailSize size)
	{
		return GetSize(size);
	}

	public static string Combine(string fileHash, int size,
		ThumbnailImageFormat imageFormat)
	{
		return Combine(fileHash, GetSize(size), imageFormat);
	}

	public static string Combine(string fileHash, ThumbnailSize size,
		ThumbnailImageFormat imageFormat)
	{
		return fileHash + GetAppend(size) + "." + imageFormat;
	}

	private static string GetAppend(ThumbnailSize size)
	{
		switch ( size )
		{
			case ThumbnailSize.TinyIcon:
				return "@4";
			case ThumbnailSize.TinyMeta:
				return "@meta";
			case ThumbnailSize.Small:
				return "@300";
			case ThumbnailSize.Large:
				return string.Empty;
			case ThumbnailSize.ExtraLarge:
				return "@2000";
			default:
				throw new ArgumentOutOfRangeException(nameof(size), size, null);
		}
	}

	/// <summary>
	///     Replace at and digit with empty string
	///     @\d+.?\w{0,4}
	/// </summary>
	/// <returns></returns>
	[GeneratedRegex(@"@\d+.?\w{0,4}", RegexOptions.None, 1000)]
	private static partial Regex AtDigitAndExtensionRegex();

	/// <summary>
	///     Remove @2000 or @2000.jpg
	/// </summary>
	/// <param name="thumbnailOutputHash">input hash with extension</param>
	/// <returns>fileHash without suffix and or extension</returns>
	public static string RemoveSuffix(string? thumbnailOutputHash)
	{
		return thumbnailOutputHash == null
			? string.Empty
			: AtDigitAndExtensionRegex().Replace(thumbnailOutputHash, string.Empty);
	}

	/// <summary>
	///     ThumbnailName
	///     Regex.IsMatch (pre compiled regex)
	/// </summary>
	/// <returns>Regex object</returns>
	[GeneratedRegex(
		"^[a-zA-Z0-9_-]+$",
		RegexOptions.None,
		100)]
	private static partial Regex ThumbnailNameRegex();

	public static bool ValidateThumbnailName(string thumbnailName)
	{
		return ThumbnailNameRegex().IsMatch(thumbnailName);
	}
}
