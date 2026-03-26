using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail.TiffEmbeded;

namespace starskytest.starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail.Models;

[TestClass]
public class RawFlavorHelperTests
{
	[TestMethod]
	[DataRow("/some/path/image.arw", "SonyArw")]
	[DataRow("/some/path/image.ARW", "SonyArw")]
	[DataRow("image.cr2", "CanonCr2")]
	[DataRow("IMAGE.CR2", "CanonCr2")]
	[DataRow("/abs/path/to/file.dng", "Unknown")]
	[DataRow("/abs/path/to/file.nef", "Unknown")]
	[DataRow("/noextension", "Unknown")]
	[DataRow(".hiddenfile", "Unknown")]
	[DataRow("/tricky.name.with.dots.arw", "SonyArw")]
	public void GetRawFlavorFromPath_RecognizesExtensions(string path, string expectedName)
	{
		var result = RawFlavorHelper.GetRawFlavorFromPath(path);
		var expected = (RawFlavor)System.Enum.Parse(typeof(RawFlavor), expectedName);
		Assert.AreEqual(expected, result);
	}
}
