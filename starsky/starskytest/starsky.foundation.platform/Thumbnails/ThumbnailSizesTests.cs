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
			ThumbnailSize.ExtraLarge,
			ThumbnailSize.Large,
			ThumbnailSize.Small,
			ThumbnailSize.TinyMeta
		};

		// Act
		var result = ThumbnailSizes.GetLargeToSmallSizes();

		// Assert
		CollectionAssert.AreEqual(expectedSizes, result);
	}

	[TestMethod]
	public void GetLargeToSmallSizes_WithoutExtraLarge()
	{
		// Arrange
		var expectedSizes = new List<ThumbnailSize>
		{
			ThumbnailSize.Large, ThumbnailSize.Small, ThumbnailSize.TinyMeta
		};

		// Act
		var result = ThumbnailSizes.GetLargeToSmallSizes(ThumbnailGenerationType.SkipExtraLarge);

		// Assert
		CollectionAssert.AreEqual(expectedSizes, result);
	}

	[TestMethod]
	public void GetLargeToSmallSizes_OnlySmall()
	{
		// Arrange
		var expectedSizes = new List<ThumbnailSize> { ThumbnailSize.Small, ThumbnailSize.TinyMeta };

		// Act
		var result = ThumbnailSizes.GetLargeToSmallSizes(ThumbnailGenerationType.SmallOnly);

		// Assert
		CollectionAssert.AreEqual(expectedSizes, result);
	}
}
