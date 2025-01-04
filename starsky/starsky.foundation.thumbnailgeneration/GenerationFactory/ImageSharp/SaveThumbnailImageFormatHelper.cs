using System;
using System.IO;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using starsky.foundation.platform.Helpers;

namespace starsky.foundation.thumbnailgeneration.GenerationFactory.ImageSharp;

public static class SaveThumbnailImageFormatHelper
{
	/// <summary>
	///     Used in ResizeThumbnailToStream to save based on the input settings
	/// </summary>
	/// <param name="image">Rgba32 image</param>
	/// <param name="imageFormat">Files ImageFormat</param>
	/// <param name="outputStream">input stream to save</param>
	internal static Task SaveThumbnailImageFormat(Image image,
		ExtensionRolesHelper.ImageFormat imageFormat,
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
		ExtensionRolesHelper.ImageFormat imageFormat,
		MemoryStream outputStream)
	{
		switch ( imageFormat )
		{
			case ExtensionRolesHelper.ImageFormat.png:
				await image.SaveAsync(outputStream,
					new PngEncoder
					{
						ColorType = PngColorType.Rgb,
						CompressionLevel = PngCompressionLevel.BestSpeed,
						SkipMetadata = true,
						TransparentColorMode = PngTransparentColorMode.Clear
					});
				return;
			case ExtensionRolesHelper.ImageFormat.webp:
				await image.SaveAsync(outputStream,
					new WebpEncoder { Quality = 80, EntropyPasses = 1, SkipMetadata = true });
				return;
			default:
				await image.SaveAsync(outputStream, new JpegEncoder { Quality = 90 });
				break;
		}
	}
}
