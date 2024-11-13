using System.Collections.Generic;
using System.Linq;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.Jpeg;

namespace starsky.foundation.thumbnailmeta.Helpers;

public static class GetImageSize
{
	public static (int, int) GetSize(List<Directory> allExifItems)
	{
		var jpegTags = allExifItems.OfType<JpegDirectory>().FirstOrDefault()?.Tags;

		var heightPixels = jpegTags?.FirstOrDefault(p => p.Type == JpegDirectory.TagImageHeight)
			?.Description;
		var widthPixels = jpegTags?.FirstOrDefault(p => p.Type == JpegDirectory.TagImageWidth)
			?.Description;

		if ( !string.IsNullOrEmpty(heightPixels) && !string.IsNullOrEmpty(widthPixels) )
		{
			return ParseStringGetSize(widthPixels, heightPixels);
		}

		foreach ( var subIfdDirectory in allExifItems.OfType<ExifSubIfdDirectory>()
			         .Select(p => p.Tags) )
		{
			var ifdHeight = subIfdDirectory
				.FirstOrDefault(p => p.Type == ExifDirectoryBase.TagExifImageHeight)
				?.Description;
			var ifdWidth = subIfdDirectory.FirstOrDefault(p => p.Type ==
			                                                   ExifDirectoryBase.TagExifImageWidth)
				?.Description;
			if ( ifdWidth == null || ifdHeight == null )
			{
				continue;
			}

			heightPixels = ifdHeight;
			widthPixels = ifdWidth;
		}

		return ParseStringGetSize(widthPixels, heightPixels);
	}

	private static (int, int) ParseStringGetSize(string? widthPixels, string? heightPixels)
	{
		var heightParseResult = int.TryParse(heightPixels?.Replace(
			" pixels", string.Empty), out var height);

		var widthParseResult =
			int.TryParse(widthPixels?.Replace(" pixels", string.Empty), out var width);
		if ( !heightParseResult || !widthParseResult )
		{
			return ( 0, 0 );
		}

		return ( width, height );
	}
}
