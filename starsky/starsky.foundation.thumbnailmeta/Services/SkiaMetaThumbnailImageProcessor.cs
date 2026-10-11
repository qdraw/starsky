using System;
using System.IO;
using SkiaSharp;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Enums;
using starsky.foundation.thumbnailgeneration.GenerationFactory.Skia;
using starsky.foundation.thumbnailmeta.Helpers;
using starsky.foundation.thumbnailmeta.Interfaces;

namespace starsky.foundation.thumbnailmeta.Services;

public sealed class SkiaMetaThumbnailImageProcessor : IMetaThumbnailImageProcessor
{
	/// <summary>
	///     Matches the quality of the previous (ImageSharp) output
	/// </summary>
	private const int Quality = 75;

	public void CropResizeRotate(Stream thumbnail, int sourceWidth, int sourceHeight,
		ImageRotation.Rotation rotation, ThumbnailImageFormat imageFormat, Stream output)
	{
		using var smallImage = SkiaImageHelper.Decode(thumbnail);

		var result = NewImageSize.NewImageSizeCalc(smallImage.Width,
			smallImage.Height, sourceWidth, sourceHeight);

		using var cropped = SkiaImageHelper.Crop(smallImage, result.DestX,
			result.DestY, result.DestWidth, result.DestHeight);

		var larger = ( int ) Math.Round(result.DestWidth * 1.2, 0);
		using var resized = SkiaImageHelper.Resize(cropped, larger, 0);
		using var rotated = SkiaImageHelper.Rotate(resized, rotation.ToDegrees());

		SkiaImageHelper.Encode(rotated, imageFormat == ThumbnailImageFormat.webp
			? SKEncodedImageFormat.Webp
			: SKEncodedImageFormat.Jpeg, Quality, output);
	}
}
