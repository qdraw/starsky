using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Enums;
using starsky.foundation.thumbnailgeneration.GenerationFactory.ImageSharp;

namespace starskytest.starsky.foundation.thumbnailgeneration.GenerationFactory.ImageSharp;

[TestClass]
public class SaveThumbnailImageFormatHelperTests
{
	[TestMethod]
	public async Task ResizeThumbnailImageFormat_NullInput()
	{
		await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () =>
			await SaveThumbnailImageFormatHelper.SaveThumbnailImageFormat(null!,
				ThumbnailImageFormat.jpg, null!));
	}
}
