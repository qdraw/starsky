using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using SixLabors.ImageSharp.Processing;

namespace starsky.foundation.thumbnailgeneration.GenerationFactory.ImageSharp;

public static class ImageSharpImageResizeHelper
{
	public static void ImageSharpImageResize(Image image, int width, bool removeExif)
	{
		// Add original rotation to the image as json
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

		image.Mutate(x => x.AutoOrient());
		image.Mutate(x => x
			.Resize(width, height, KnownResamplers.Lanczos3)
		);
	}
}
