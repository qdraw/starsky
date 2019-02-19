using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
        
	}
}
