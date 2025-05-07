using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Thumbnails;

namespace starskytest.starsky.foundation.platform.Thumbnails;

[TestClass]
public class ThumbnailSizesTests
{
	[TestMethod]
	public void GetLargeToSmallSizes_WithExtraLarge()
	{
		// Arrange
		var expectedSizes = new List<ThumbnailSize>
		{
			ThumbnailSize.ExtraLarge, ThumbnailSize.Large, ThumbnailSize.Small
		};

		// Act
		var result = ThumbnailSizes.GetLargeToSmallSizes(false);

		// Assert
		CollectionAssert.AreEqual(expectedSizes, result);
	}

	[TestMethod]
	public void GetLargeToSmallSizes_WithoutExtraLarge()
	{
		// Arrange
		var expectedSizes = new List<ThumbnailSize> { ThumbnailSize.Large, ThumbnailSize.Small };

		// Act
		var result = ThumbnailSizes.GetLargeToSmallSizes(true);

		// Assert
		CollectionAssert.AreEqual(expectedSizes, result);
	}
}
