using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using SixLabors.ImageSharp.PixelFormats;
using starsky.foundation.thumbnailgeneration.GenerationFactory.ImageSharp;

namespace starskytest.starsky.foundation.thumbnailgeneration.GenerationFactory.ImageSharp;

[TestClass]
public class ImageSharpImageResizeHelperTests
{
	[TestMethod]
	public void ImageSharpImageResize_HeightGreaterThanOrEqualToWidth()
	{
		using var image = new Image<Rgba32>(100, 200); // Height > Width
		ImageSharpImageResizeHelper.ImageSharpImageResize(image, 50, false);

		Assert.AreEqual(50, image.Height);
		Assert.AreEqual(25, image.Width);
	}

	[TestMethod]
	public void ImageSharpImageResize_WidthGreaterThanHeight()
	{
		using var image = new Image<Rgba32>(200, 100); // Width > Height
		ImageSharpImageResizeHelper.ImageSharpImageResize(image, 50, false);

		Assert.AreEqual(50, image.Width);
		Assert.AreEqual(25, image.Height);
	}

	[TestMethod]
	public void ImageSharpImageResize_RemoveExif()
	{
		using var image = new Image<Rgba32>(100, 200);
		image.Metadata.ExifProfile = new ExifProfile();
		ImageSharpImageResizeHelper.ImageSharpImageResize(image, 50, true);

		Assert.IsNull(image.Metadata.ExifProfile);
		Assert.IsNull(image.Metadata.IccProfile);
	}

	[TestMethod]
	public void ImageSharpImageResize_KeepExif()
	{
		using var image = new Image<Rgba32>(100, 200);
		image.Metadata.ExifProfile = new ExifProfile();
		ImageSharpImageResizeHelper.ImageSharpImageResize(image, 50, false);

		Assert.IsNotNull(image.Metadata.ExifProfile);
		Assert.AreEqual("Starsky",
			image.Metadata.ExifProfile.Values.FirstOrDefault(
				p => p.Tag == ExifTag.Software)?.GetValue());
	}
}
