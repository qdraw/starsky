using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Helpers;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;

namespace starskytest.starsky.foundation.database.Helpers;

[TestClass]
public sealed class SortHelperTest
{
	[TestMethod]
	public void ImageFormatOrder()
	{
		var exampleList = new List<FileIndexItem>
		{
			new("/test3.mp4") { ImageFormat = ExtensionRolesHelper.ImageFormat.mp4 },
			new("/test3.gpx") { ImageFormat = ExtensionRolesHelper.ImageFormat.gpx },
			new("/test3.jpg") { ImageFormat = ExtensionRolesHelper.ImageFormat.jpg },
			new("/test.jpg") { ImageFormat = ExtensionRolesHelper.ImageFormat.notfound },
			new("/test.xmp") { ImageFormat = ExtensionRolesHelper.ImageFormat.xmp },
			new("/test.png") { ImageFormat = ExtensionRolesHelper.ImageFormat.png },
			new("/test2.jpg") { ImageFormat = ExtensionRolesHelper.ImageFormat.unknown },
			new("/test.bmp") { ImageFormat = ExtensionRolesHelper.ImageFormat.bmp },
			new("/test2.jp4") { ImageFormat = ExtensionRolesHelper.ImageFormat.gif },
			new("/test.tiff") { ImageFormat = ExtensionRolesHelper.ImageFormat.tiff }
		};
		var result = SortHelper.Helper(exampleList, SortType.ImageFormat);
		var extensionList = result.Select(p => p.ImageFormat).ToList();

		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.notfound, extensionList[0]);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.unknown, extensionList[1]);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.jpg, extensionList[2]);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.tiff, extensionList[3]);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.bmp, extensionList[4]);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.gif, extensionList[5]);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.png, extensionList[6]);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.xmp, extensionList[7]);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.gpx, extensionList[8]);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.mp4, extensionList[9]);
	}

	[TestMethod]
	public void ImageFormatOrder_ThenByFileName()
	{
		var exampleList = new List<FileIndexItem>
		{
			new("/test3.mp4") { ImageFormat = ExtensionRolesHelper.ImageFormat.mp4 },
			new("/test3.gpx") { ImageFormat = ExtensionRolesHelper.ImageFormat.gpx },
			new("/test3.jpg") { ImageFormat = ExtensionRolesHelper.ImageFormat.jpg },
			new("/test.jpg") { ImageFormat = ExtensionRolesHelper.ImageFormat.notfound },
			new("/test.xmp") { ImageFormat = ExtensionRolesHelper.ImageFormat.xmp },
			new("/test.png") { ImageFormat = ExtensionRolesHelper.ImageFormat.png },
			new("/test2.jpg") { ImageFormat = ExtensionRolesHelper.ImageFormat.unknown },
			new("/test.bmp") { ImageFormat = ExtensionRolesHelper.ImageFormat.bmp },
			new("/test2.jp4") { ImageFormat = ExtensionRolesHelper.ImageFormat.gif },
			new("/test.tiff") { ImageFormat = ExtensionRolesHelper.ImageFormat.tiff }
		};

		var result = SortHelper.Helper(exampleList, SortType.ImageFormat).ToList();
		var extensionList = result.Select(p => p.ImageFormat).ToList();
		var fileNameList = result.Select(p => p.FileName).ToList();

		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.notfound, extensionList[0]);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.unknown, extensionList[1]);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.jpg, extensionList[2]);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.tiff, extensionList[3]);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.bmp, extensionList[4]);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.gif, extensionList[5]);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.png, extensionList[6]);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.xmp, extensionList[7]);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.gpx, extensionList[8]);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.mp4, extensionList[9]);

		Assert.AreEqual("test.jpg", fileNameList[0]);
		Assert.AreEqual("test2.jpg", fileNameList[1]);
		Assert.AreEqual("test3.jpg", fileNameList[2]);
	}
}
