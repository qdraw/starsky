using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Thumbnails;
using starsky.foundation.thumbnailgeneration.GenerationFactory.Generators;

namespace starskytest.starsky.foundation.thumbnailgeneration.GenerationFactory.Generators;

[TestClass]
public class NotSupportedFallbackThumbnailGeneratorTests
{
	[TestMethod]
	public async Task GenerateThumbnail_ReturnsEmptyList()
	{
		// Arrange
		var generator = new NotSupportedFallbackThumbnailGenerator();
		const string singleSubPath = "test.jpg";
		const string fileHash = "hash";
		const ThumbnailImageFormat imageFormat = ThumbnailImageFormat.jpg;
		var thumbnailSizes = new List<ThumbnailSize> { ThumbnailSize.Small, ThumbnailSize.Large };

		// Act
		var result =
			await generator.GenerateThumbnail(singleSubPath, fileHash, imageFormat, thumbnailSizes);

		// Assert
		Assert.IsNotNull(result);
		Assert.IsFalse(result.Any());
	}
}
