using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.native.PreviewImageNative.Helpers;

namespace starskytest.starsky.foundation.native.PreviewImageNative.Helpers;

[TestClass]
public class ShellThumbnailExtractionWindowsTest
{
	[TestMethod]
	public void GenerateThumbnail()
	{
		ShellThumbnailExtractionWindows.GenerateThumbnail(
			"C:\\Users\\dion.vanvelde\\Pictures\\20220820_041825_DSC00426-20220820_041832_DSC00432_pano_def.jpg",
			"C:\\test.jpg", 512, 512);
	}
}
