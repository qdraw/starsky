//using System;
//using System.IO;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using starsky.Models;
//using starsky.Services;
//
//namespace starskytests
//{
//    [TestClass]
//    public class ThumbnailTest
//    {
//        [TestMethod]
//        public void CreateAndRenamteThumbTest()
//        {
//
//            var newImage = new CreateAnImage();
//            AppSettingsProvider.ThumbnailTempFolder = newImage.BasePath;
//            AppSettingsProvider.BasePath = newImage.BasePath;
//
//            var hashString = FileHash.GetHashCode(newImage.FullFilePath);
//
//            // Delete if exist, to optimize test
//            var thumbnailPAth = Path.Combine(newImage.BasePath, hashString + ".jpg");
//            if (File.Exists(thumbnailPAth))
//            {
//                File.Delete(thumbnailPAth);
//            }
//
//            // Create an thumbnail based on the image
//            Thumbnail.CreateThumb(newImage.DbPath);
//            Assert.AreEqual(true,File.Exists(thumbnailPAth));
//
//            // Test Rename feature and delete if passed
//            new Thumbnail().RenameThumb(hashString, "AAAAA");
//            var thumbnailApAth = Path.Combine(newImage.BasePath, "AAAAA" + ".jpg");
//            if (File.Exists(thumbnailApAth))
//            {
//                File.Delete(thumbnailApAth);
//            }
//        }
//
//        [TestMethod]
//        [ExpectedException(typeof(FileNotFoundException))]
//        public void ThumbnailCreateThumbnailNullTest()
//        {
//            Thumbnail.CreateThumb(new FileIndexItem());
//        }
//        
//        [TestMethod]
//        public void ThumbnailCreateThumbnailNotFoundTest()
//        {
//            var newImage = new CreateAnImage();
//            AppSettingsProvider.ThumbnailTempFolder = newImage.BasePath;
//            Thumbnail.CreateThumb(new FileIndexItem{FileHash = "t",FileName = "t",FilePath = "/"});
//        }
//
//        [TestMethod]
//        [ExpectedException(typeof(FileNotFoundException))]
//        public void ThumbnailCreateThumb_FileIndexItem_ThumbnailTempFolderNull_Test()
//        {
//            AppSettingsProvider.ThumbnailTempFolder = null;
//            Thumbnail.CreateThumb(new FileIndexItem());
//        }
//
//        [TestMethod]
//        [ExpectedException(typeof(FileNotFoundException))]
//        public void ThumbnailRenameThumb_DirectInput_ThumbnailTempFolderNull_Test()
//        {
//            AppSettingsProvider.ThumbnailTempFolder = null;
//            new Thumbnail().RenameThumb(null, null);
//        }
//        
//        [TestMethod]
//        public void ThumbnailRenameThumb_DirectInput_nonexistingOldHash_Test()
//        {
//            // Should not crash
//            var newImage = new CreateAnImage();
//            AppSettingsProvider.ThumbnailTempFolder = newImage.BasePath;
//            new Thumbnail().RenameThumb(null, "ThumbnailRenameThumb_nonexistingOldHash_Test");
//        }
//        
//        [TestMethod]
//        public void ThumbnailRenameThumb_DirectInput_nonexistingNewHash_Test()
//        {
//            // For testing:    if File.Exists(newThumbPath)
//            var newImage = new CreateAnImage();
//            AppSettingsProvider.ThumbnailTempFolder = newImage.BasePath;
//            var dbPathWithoutExtAndSlash = newImage.DbPath.Replace(".jpg", string.Empty).Replace("/", string.Empty);
//            new Thumbnail().RenameThumb(dbPathWithoutExtAndSlash,dbPathWithoutExtAndSlash);
//        }
//        
//        [TestMethod]
//        public void ThumbnailByDirectoryTest()
//        {
//
//            var newImage = new CreateAnImage();
//            AppSettingsProvider.ThumbnailTempFolder = newImage.BasePath;
//            AppSettingsProvider.BasePath = newImage.BasePath;
//
//            var hashString = FileHash.GetHashCode(newImage.FullFilePath);
//
//            // Delete if exist, to optimize test
//            var thumbnailPath = Path.Combine(newImage.BasePath, hashString + ".jpg");
//            if (File.Exists(thumbnailPath))
//            {
//                File.Delete(thumbnailPath);
//            }
//        
//            // Create an thumbnail based on the image
//            ThumbnailByDirectory.CreateThumb();
//            
//            Assert.AreEqual(true,File.Exists(thumbnailPath));
//            
//            File.Delete(thumbnailPath);
//
//        }
//
//        
//        
//        
//        
//    }
//}