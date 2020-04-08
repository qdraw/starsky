using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Helpers;
using starskycore.Helpers;
using starskytest.FakeCreateAn;

namespace starskytest.Helpers
{
	[TestClass]
	public class ExtensionRolesHelperTest
	{
		
        [TestMethod]
        public void Files_ExtensionThumbSupportedList_TiffMp4MovXMPCheck()
        {
            Assert.AreEqual(false,ExtensionRolesHelper.IsExtensionThumbnailSupported("file.tiff"));
            Assert.AreEqual(false,ExtensionRolesHelper.IsExtensionThumbnailSupported("file.mp4"));
            Assert.AreEqual(false,ExtensionRolesHelper.IsExtensionThumbnailSupported("file.mov"));
            Assert.AreEqual(false,ExtensionRolesHelper.IsExtensionThumbnailSupported("file.xmp"));
        }
		
		[TestMethod]
		public void Files_ExtensionThumbSupportedList_JpgCheck()
		{
			Assert.AreEqual(true,ExtensionRolesHelper.IsExtensionThumbnailSupported("file.jpg"));
			Assert.AreEqual(true,ExtensionRolesHelper.IsExtensionThumbnailSupported("file.bmp"));
		}

		[TestMethod]
		public void Files_ExtensionThumbSupportedList_null()
		{
			Assert.AreEqual(false,ExtensionRolesHelper.IsExtensionThumbnailSupported(null));
			// equal or less then three chars
			Assert.AreEqual(false,ExtensionRolesHelper.IsExtensionThumbnailSupported("nul"));
		}

		[TestMethod]
		public void Files_ExtensionThumbSupportedList_FolderName()
		{
			Assert.AreEqual(false,ExtensionRolesHelper.IsExtensionThumbnailSupported("Some Foldername"));
		}

		[TestMethod]
        public void Files_ExtensionSyncSupportedList_TiffCheck()
        {
            var extensionSyncSupportedList = ExtensionRolesHelper.ExtensionSyncSupportedList;
            Assert.AreEqual(true,extensionSyncSupportedList.Contains("tiff"));
            Assert.AreEqual(true,extensionSyncSupportedList.Contains("jpg"));

        }

        [TestMethod]
        public void Files_GetImageFormat_png_Test()
        {
            var fileType = ExtensionRolesHelper.GetImageFormat(new byte[] {137, 80, 78, 71});
            Assert.AreEqual(fileType,ExtensionRolesHelper.ImageFormat.png);
        }
        
        [TestMethod]
        public void GetImageFormat_Png_Test()
        {
	        var newImage = CreateAnPng.Bytes.Take(15).ToArray();
	        var result = ExtensionRolesHelper.GetImageFormat(newImage);
	        Assert.AreEqual(ExtensionRolesHelper.ImageFormat.png,result);
        }
        
        [TestMethod]
        public void Files_GetImageFormat_jpeg2_Test()
        {
            var fileType = ExtensionRolesHelper.GetImageFormat(new byte[] {255, 216, 255, 225});
            Assert.AreEqual(fileType,ExtensionRolesHelper.ImageFormat.jpg);
        }
        
        [TestMethod]
        public void GetImageFormat_Jpeg_Test()
        {
	        var newImage = CreateAnImage.Bytes.Take(15).ToArray();
	        var result = ExtensionRolesHelper.GetImageFormat(newImage);
	        Assert.AreEqual(ExtensionRolesHelper.ImageFormat.jpg,result);
        }
        
        [TestMethod]
        public void Files_GetImageFormat_tiff2_Test()
        {
            var fileType = ExtensionRolesHelper.GetImageFormat(new byte[] {77, 77, 42});
            Assert.AreEqual(fileType,ExtensionRolesHelper.ImageFormat.tiff);
        }
        
        [TestMethod]
        public void Files_GetImageFormat_tiff3_Test()
        {
            var fileType = ExtensionRolesHelper.GetImageFormat(new byte[] {77, 77, 0});
            Assert.AreEqual(fileType,ExtensionRolesHelper.ImageFormat.tiff);
        }

        [TestMethod]
        public void Files_GetImageFormat_bmp_Test()
        {
            byte[] bmBytes = Encoding.ASCII.GetBytes("BM");
            var fileType = ExtensionRolesHelper.GetImageFormat(bmBytes);
            Assert.AreEqual(fileType,ExtensionRolesHelper.ImageFormat.bmp);
        }
        
        [TestMethod]
        public void Files_GetImageFormat_gif_Test()
        {
            byte[] bmBytes = Encoding.ASCII.GetBytes("GIF");
            var fileType = ExtensionRolesHelper.GetImageFormat(bmBytes);
            Assert.AreEqual(fileType,ExtensionRolesHelper.ImageFormat.gif);
        }
        
        [TestMethod]
        public void Files_GetImageFormat_xmp_Test()
        {
            byte[] bmBytes = Encoding.ASCII.GetBytes("<x:xmpmeta");
            var fileType = ExtensionRolesHelper.GetImageFormat(bmBytes);
            Assert.AreEqual(fileType,ExtensionRolesHelper.ImageFormat.xmp);
        }

		[TestMethod]
		public void ExtensionRolesHelperTest_IsExtensionForceXmp_Positive()
		{
			var result = ExtensionRolesHelper.IsExtensionForceXmp("/test.arw");
			Assert.AreEqual(true,result);
		}
		
		[TestMethod]
		public void ExtensionRolesHelperTest_IsExtensionForceXmp_Negative()
		{
			var result = ExtensionRolesHelper.IsExtensionForceXmp("/test.jpg");
			Assert.AreEqual(false,result);
		}

		[TestMethod]
		public void ExtensionRolesHelperTest_ReplaceExtensionWithXmp_arw()
		{
			var result = ExtensionRolesHelper.ReplaceExtensionWithXmp("/test.arw");
			Assert.AreEqual("/test.xmp",result);
		}
		
		[TestMethod]
		public void ExtensionRolesHelperTest_ReplaceExtensionWithXmp_tiff()
		{
			var result = ExtensionRolesHelper.ReplaceExtensionWithXmp("/test.tiff");
			Assert.AreEqual("/test.xmp",result);
		}

		[TestMethod]
		public void ExtensionRolesHelperTest_ReplaceExtensionWithXmp_fail()
		{
			var result = ExtensionRolesHelper.ReplaceExtensionWithXmp("/test.so");
			Assert.AreEqual(string.Empty,result);
		}
		
		[TestMethod]
		public void Files_GetImageFormat_h264_Test()
		{
			var fileType = ExtensionRolesHelper.GetImageFormat(new byte[] { 00,  00,  00,  20,  102, 116, 121, 112});
			Assert.AreEqual(ExtensionRolesHelper.ImageFormat.mp4,fileType);
		}

		[TestMethod]
		public void GetImageFormat_QuickTimeMp4_Test()
		{
			var newImage = CreateAnQuickTimeMp4.Bytes.Take(15).ToArray();
			var result = ExtensionRolesHelper.GetImageFormat(newImage);
			Assert.AreEqual(ExtensionRolesHelper.ImageFormat.mp4,result);
		}

		[TestMethod]
		public void StringToByteArrayTest()
		{
			Assert.AreEqual(119, ExtensionRolesHelper.StringToByteArray("77")[0]);
		}

		[TestMethod]
		public void MapFileTypesToExtension_fileWithNoExtension()
		{
			var result = ExtensionRolesHelper.MapFileTypesToExtension("no_ext");
			Assert.AreEqual(ExtensionRolesHelper.ImageFormat.unknown, result);
		}
		
		[TestMethod]
        public void MapFileTypesToExtension_fileWithNonExistingExtension()
        {
        	var result = ExtensionRolesHelper.MapFileTypesToExtension("non.ext");
        	Assert.AreEqual(ExtensionRolesHelper.ImageFormat.unknown, result);
        }
        
        [TestMethod]
        public void MapFileTypesToExtension_Null()
        {
	        var result = ExtensionRolesHelper.MapFileTypesToExtension(null);
	        Assert.AreEqual(ExtensionRolesHelper.ImageFormat.unknown, result);
        }

        [TestMethod]
        public void IsExtensionExifToolSupported_Null()
        {
	        var result = ExtensionRolesHelper.IsExtensionExifToolSupported(null);
	        Assert.IsFalse(result);
        }
        
        [TestMethod]
        public void IsExtensionExifToolSupported_fileWithNoExtension()
        {
	        var result = ExtensionRolesHelper.IsExtensionExifToolSupported("no_ext");
	        Assert.IsFalse(result);
        }
	}
}
