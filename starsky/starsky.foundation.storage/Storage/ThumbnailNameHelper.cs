using System;
using System.Globalization;
using System.Text.RegularExpressions;
using starsky.foundation.platform.Enums;

namespace starsky.foundation.storage.Storage;

/// <summary>
///     ThumbnailNamesHelper
/// </summary>
public static partial class ThumbnailNameHelper
{
	/// <summary>
	///     Without TinyMeta
	/// </summary>
	public static readonly ThumbnailSize[] GeneratedThumbnailSizes = new[]
	{
		ThumbnailSize.ExtraLarge, ThumbnailSize.Small, ThumbnailSize.Large
	};

	public static readonly ThumbnailSize[] SecondGeneratedThumbnailSizes = new[]
	{
		ThumbnailSize.Small, ThumbnailSize
			.Large //  <- will be false when skipExtraLarge = true, its already created 
	};

	public static readonly ThumbnailSize[] AllThumbnailSizes = new[]
	{
		ThumbnailSize.TinyMeta, ThumbnailSize.ExtraLarge, ThumbnailSize.Small,
		ThumbnailSize.Large
	};

	public static int GetSize(ThumbnailSize size)
	{
		switch ( size )
		{
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

	public static ThumbnailSize GetSize(string fileName)
	{
		var fileNameWithoutExtension =
			fileName.Replace(".jpg", string.Empty);

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

	public static string Combine(string fileHash, int size)
	{
		return Combine(fileHash, GetSize(size));
	}

	public static string Combine(string fileHash, ThumbnailSize size,
		bool appendExtension = false)
	{
		if ( appendExtension )
		{
			return fileHash + GetAppend(size) + ".jpg";
		}

		return fileHash + GetAppend(size);
	}

	private static string GetAppend(ThumbnailSize size)
	{
		switch ( size )
		{
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

	[GeneratedRegex("@\\d+", RegexOptions.None, 1000)]
	private static partial Regex AtDigitRegex();

	public static string RemoveSuffix(string? thumbnailOutputHash)
	{
		return thumbnailOutputHash == null
			? string.Empty
			: AtDigitRegex().Replace(thumbnailOutputHash, string.Empty);
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
