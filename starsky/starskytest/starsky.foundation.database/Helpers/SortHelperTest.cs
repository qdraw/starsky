using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Helpers;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;

namespace starskytest.starsky.foundation.database.Helpers
{
	[TestClass]
	public class SortHelperTest
	{
		[TestMethod]
		public void ImageFormatOrder()
		{
			var exampleList = new List<FileIndexItem>
			{
				new FileIndexItem("/test3.mp4")
				{
					ImageFormat = ExtensionRolesHelper.ImageFormat.mp4
				},
				new FileIndexItem("/test3.gpx")
				{
					ImageFormat = ExtensionRolesHelper.ImageFormat.gpx
				},
				new FileIndexItem("/test3.jpg")
				{
					ImageFormat = ExtensionRolesHelper.ImageFormat.jpg
				},
				new FileIndexItem("/test.jpg")
				{
					ImageFormat = ExtensionRolesHelper.ImageFormat.notfound
				},
				new FileIndexItem("/test.xmp")
				{
					ImageFormat = ExtensionRolesHelper.ImageFormat.xmp
				},
				new FileIndexItem("/test.png")
				{
					ImageFormat = ExtensionRolesHelper.ImageFormat.png
				},
				new FileIndexItem("/test2.jpg")
				{
					ImageFormat = ExtensionRolesHelper.ImageFormat.unknown
				},
				new FileIndexItem("/test.bmp")
				{
					ImageFormat = ExtensionRolesHelper.ImageFormat.bmp
				},
				new FileIndexItem("/test2.jp4")
				{
					ImageFormat = ExtensionRolesHelper.ImageFormat.gif
				},
				new FileIndexItem("/test.tiff")
				{
					ImageFormat = ExtensionRolesHelper.ImageFormat.tiff
				}
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
	}
}
