using System.IO;
using System.Linq;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace starskytest.starsky.foundation.thumbnailmeta.ServicesPreviewSize;

[TestClass]
public class Test
{
	[TestMethod]
	public void TestThumbnailSize()
	{
		var directories = ImageMetadataReader.ReadMetadata("/Users/dion/data/git/starsky/starsky/starskytest/FakeCreateAn/CreateAnImageLargePreview/20241112_110839_DSC02741.jpg");
		var exifSubIfd = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();

		if ( exifSubIfd != null && exifSubIfd.ContainsTag(0x2001) ) // 0x2001 = PreviewImage
		{
			var previewData = exifSubIfd.GetByteArray(0x2001);
			File.WriteAllBytes("preview.jpg", previewData);
		}
	}
}
