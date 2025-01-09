using System;
using System.IO;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using starsky.foundation.platform.Enums;

namespace starsky.foundation.thumbnailgeneration.GenerationFactory.ImageSharp;

internal static class SaveThumbnailImageFormatHelper
{
	/// <summary>
	///     Used in ResizeThumbnailToStream to save based on the input settings
	/// </summary>
	/// <param name="image">Rgba32 image</param>
	/// <param name="imageFormat">Files ImageFormat</param>
	/// <param name="outputStream">input stream to save</param>
	internal static Task SaveThumbnailImageFormat(Image image,
		ThumbnailImageFormat imageFormat,
		MemoryStream outputStream)
	{
		ArgumentNullException.ThrowIfNull(outputStream);

		return SaveThumbnailImageFormatInternal(image, imageFormat, outputStream);
	}

	/// <summary>
	///     Private: use => SaveThumbnailImageFormat
	///     Used in ResizeThumbnailToStream to save based on the input settings
	/// </summary>
	/// <param name="image">Rgba32 image</param>
	/// <param name="imageFormat">Files ImageFormat</param>
	/// <param name="outputStream">input stream to save</param>
	private static async Task SaveThumbnailImageFormatInternal(Image image,
		ThumbnailImageFormat imageFormat,
		MemoryStream outputStream)
	{
		switch ( imageFormat )
		{
			case ThumbnailImageFormat.png:
				await image.SaveAsync(outputStream,
					new PngEncoder
					{
						ColorType = PngColorType.Rgb,
						CompressionLevel = PngCompressionLevel.BestSpeed,
						SkipMetadata = true,
						TransparentColorMode = PngTransparentColorMode.Clear
					});
				return;
			case ThumbnailImageFormat.webp:
				await image.SaveAsync(outputStream,
					new WebpEncoder { Quality = 80, EntropyPasses = 1, SkipMetadata = true });
				return;
			default:
				await image.SaveAsync(outputStream, new JpegEncoder { Quality = 90 });
				break;
		}
	}
}
