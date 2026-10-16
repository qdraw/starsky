using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Enums;
using starsky.foundation.thumbnailmeta.Services;
using starskytest.FakeCreateAn;

namespace starskytest.starsky.foundation.thumbnailmeta.Services;

[TestClass]
public class MetaThumbnailImageProcessorFactoryTests
{
	[TestMethod]
	public void Create_IsSkia()
	{
		Assert.IsInstanceOfType<SkiaMetaThumbnailImageProcessor>(
			new MetaThumbnailImageProcessorFactory().Create());
	}

	[TestMethod]
	[DataRow(ThumbnailImageFormat.jpg)]
	[DataRow(ThumbnailImageFormat.webp)]
	public void CropResizeRotate_Rotated(ThumbnailImageFormat format)
	{
		using var input = new MemoryStream(CreateAnSyntheticImage.Jpeg(160, 120));
		using var output = new MemoryStream();

		new MetaThumbnailImageProcessorFactory().Create().CropResizeRotate(input, 4000, 3000,
			ImageRotation.Rotation.Rotate90Cw, format, output);

		using var image = ImageCompare.Decode(output.ToArray());
		// rotated 90 degrees: the long side is now the height
		Assert.IsGreaterThan(image.Width, image.Height);
	}
}
