using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Helpers;
using starskycore.Helpers;

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
        public void Files_GetImageFormat_jpeg2_Test()
        {
            var fileType = ExtensionRolesHelper.GetImageFormat(new byte[] {255, 216, 255, 225});
            Assert.AreEqual(fileType,ExtensionRolesHelper.ImageFormat.jpg);
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
		
//		[TestMethod]
//		public void Files_GetImageFormat_h264_Test()
//		{
//			var fileType = ExtensionRolesHelper.GetImageFormat(new byte[] { 00,  00,  00,  20,  66,  74,  79,  70,  69,  7});
//			Assert.AreEqual(ExtensionRolesHelper.ImageFormat.h264,fileType);
//		}
        
	}
}
