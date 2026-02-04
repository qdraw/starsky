using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Thumbnails;
using starsky.foundation.thumbnailgeneration.Models;

namespace starskytest.starsky.foundation.thumbnailgeneration.Models;

[TestClass]
public class GenerationResultModelTest
{
	[TestMethod]
	public void GenerationResultModel_SetSizeWithPixelValue()
	{
		var model = new GenerationResultModel
		{
			SizeInPixels = 300,
			// Size is set by the pixel value
			FileHash = "test",
			ImageFormat = ThumbnailImageFormat.unknown
		};

		Assert.AreEqual(300, model.SizeInPixels);
		Assert.AreEqual(ThumbnailSize.Small, model.Size);
	}
}
