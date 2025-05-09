using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace starskytest.starsky.foundation.thumbnailmeta.ServicesPreviewSize;

[TestClass]
public class Test2
{
	[TestMethod]
	[Timeout(1000)] // Timeout set to avoid the endless loop
	public void TestThumbnailSize()
	{
		const string filePath =
			"/Users/dion/data/fotobieb/2024/11/2024_11_11_d glow eindhoven/20241111_185241_DSC00914.jpg"; // Update to your file path
		QuickLookThumbnail.GenerateThumbnail(filePath);
	}
}
