using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using SixLabors.ImageSharp.Processing;

namespace starsky.foundation.thumbnailgeneration.GenerationFactory.ImageSharp;

internal static class ImageSharpImageResizeHelper
{
	internal static void ImageSharpImageResize(Image image, int width, bool removeExif)
	{
		// Preserve EXIF metadata if requested
		if ( image.Metadata.ExifProfile != null && !removeExif )
		{
			image.Metadata.ExifProfile.SetValue(ExifTag.Software, "Starsky");
		}

		if ( image.Metadata.ExifProfile != null && removeExif )
		{
			image.Metadata.ExifProfile = null;
			image.Metadata.IccProfile = null;
		}

		var height = 0;
		if ( image.Height >= image.Width )
		{
			height = width;
			width = 0;
		}

		// AutoOrient automatically reads and applies EXIF orientation tag to the image pixels,
		// ensuring portrait and other rotated images display correctly.
		// Supports EXIF orientation values 1-8 (all standard EXIF rotation modes).
		image.Mutate(x => x.AutoOrient());
		image.Mutate(x => x
			.Resize(width, height, KnownResamplers.Lanczos3)
		);
	}
}
