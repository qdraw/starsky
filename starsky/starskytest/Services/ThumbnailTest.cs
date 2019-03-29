using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starskycore.Helpers;
using starskycore.Middleware;
using starskycore.Models;
using starskycore.Services;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;
using starskytest.Models;

namespace starskytest.Services
{
    [TestClass]
    public class ThumbnailTest
    {
        private readonly AppSettings _appSettings;

        public ThumbnailTest()
        {
            // Add a dependency injection feature
            var services = new ServiceCollection();
            // Inject Config helper
            services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
            // random config
            var newImage = new CreateAnImage();
            var dict = new Dictionary<string, string>
            {
                { "App:StorageFolder", newImage.BasePath },
                { "App:Verbose", "true" }
            };
            // Start using dependency injection
            var builder = new ConfigurationBuilder();  
            // Add random config to dependency injection
            builder.AddInMemoryCollection(dict);
            // build config
            var configuration = builder.Build();
            // inject config as object to a service
            services.ConfigurePoco<AppSettings>(configuration.GetSection("App"));
            // build the service
            var serviceProvider = services.BuildServiceProvider();
            // get the service
            _appSettings = serviceProvider.GetRequiredService<AppSettings>();
        }
        
        [TestMethod]
        public void CreateAndRenameThumbTest()
        {

            var createAnImage = new CreateAnImage();
            _appSettings.ThumbnailTempFolder = createAnImage.BasePath;;

	        var iStorage = new StorageSubPathFilesystem(_appSettings);
	        var fileHashCode = new FileHash(iStorage).GetHashCode(createAnImage.DbPath);

	        // for some magic this is different
	        var fileHashCode1 = FileHashStatic.GetHashCode(createAnImage.FullFilePath);
	        
            // Delete if exist, to optimize test
            var thumbnailPAth = Path.Combine(createAnImage.BasePath, fileHashCode1 + ".jpg");
            if (File.Exists(thumbnailPAth))
            {
                File.Delete(thumbnailPAth);
            }

	        // todo: fix this test
//            // Create an thumbnail based on the image
//            new Thumbnail(_appSettings).CreateThumb(createAnImage.DbPath);
	        
            Assert.AreEqual(true,File.Exists(thumbnailPAth));

            // Test Rename feature and delete if passed
	        new StorageSubPathFilesystem(new AppSettings
	        {
		        ThumbnailTempFolder = createAnImage.BasePath
	        }).ThumbnailMove(fileHashCode, "AAAAA"); 
	        
            var thumbnailApAth = Path.Combine(createAnImage.BasePath, "AAAAA" + ".jpg");
            if (File.Exists(thumbnailApAth))
            {
                File.Delete(thumbnailApAth);
            }
        }

        [TestMethod]
        public void CreateAndRename_byobject_ThumbTest()
        {
            // object does not exist or return values

            var newImage = new CreateAnImage();
            _appSettings.ThumbnailTempFolder = newImage.BasePath;;

            var fileIndexItemList = new List<FileIndexItem>
            {
                new FileIndexItem
                {
                    FileHash = "000",
                    FileName = newImage.FileName,
                    ParentDirectory = "/"
                }
            };
	        // todo: fix this test
//	        new StorageHostFullPathFilesystem()

//            new Thumbnail(_appSettings,null).RenameThumb(fileIndexItemList);
        }

        [TestMethod]
        [ExpectedException(typeof(FileNotFoundException))]
        public void ThumbnailCreateThumbnailNullTest()
        {
            new Thumbnail(new FakeIStorage(),new FakeExifTool()).CreateThumb("/,","404");
        }
        
        [TestMethod]
        public void ThumbnailCreateThumbnailNotFoundTest()
        {
            var newImage = new CreateAnImage();
            _appSettings.ThumbnailTempFolder = newImage.BasePath;
	        throw new NotImplementedException();

//            new Thumbnail(new FakeIStorage(),new FakeExifTool() ).CreateThumb(new FileIndexItem{FileHash = "t",FileName = "t",FilePath = "/"});
        }

        [TestMethod]
        [ExpectedException(typeof(FileNotFoundException))]
        public void ThumbnailCreateThumb_FileIndexItem_ThumbnailTempFolderNull_Test()
        {
	        throw new NotImplementedException();

//            _appSettings.ThumbnailTempFolder = null;
//            new Thumbnail(new FakeIStorage(),new FakeExifTool() ).CreateThumb(new FileIndexItem());
        }

        [TestMethod]
        [ExpectedException(typeof(FileNotFoundException))]
        public void ThumbnailRenameThumb_DirectInput_ThumbnailTempFolderNull_Test()
        {
	        throw new NotImplementedException();
//            _appSettings.ThumbnailTempFolder = null;
//            new Thumbnail(_appSettings).RenameThumb(null, null);
        }
        
        [TestMethod]
        public void ThumbnailRenameThumb_DirectInput_nonexistingOldHash_Test()
        {
	        throw new NotImplementedException();

//            // Should not crash
//            var newImage = new CreateAnImage();
//            _appSettings.ThumbnailTempFolder = newImage.BasePath;;
//            new Thumbnail(_appSettings).RenameThumb(null, "ThumbnailRenameThumb_nonexistingOldHash_Test");
        }
        
        [TestMethod]
        public void ThumbnailRenameThumb_DirectInput_nonexistingNewHash_Test()
        {
	        throw new NotImplementedException();

//            // For testing:    if File.Exists(newThumbPath)
//            var newImage = new CreateAnImage();
//            _appSettings.ThumbnailTempFolder = newImage.BasePath;;
//            var dbPathWithoutExtAndSlash = newImage.DbPath.Replace(".jpg", string.Empty).Replace("/", string.Empty);
//            new Thumbnail(_appSettings).RenameThumb(dbPathWithoutExtAndSlash,dbPathWithoutExtAndSlash);
        }
        
        [TestMethod]
        public void ThumbnailByDirectoryTest()
        {
	        throw new NotImplementedException();

//            var createAnImage = new CreateAnImage();
//            _appSettings.ThumbnailTempFolder = createAnImage.BasePath;;
//            _appSettings.StorageFolder = createAnImage.BasePath;
//
//	        var iStorage = new StorageSubPathFilesystem(_appSettings);
//	        
//	        // For some magic this is different
////	        var fileHashCode = new FileHash(iStorage).GetHashCode(createAnImage.DbPath);
//	        var fileHashCode = FileHashStatic.GetHashCode(createAnImage.FullFilePath);
//	        
//            // Delete if exist, to optimize test
//            var thumbnailPath = Path.Combine(createAnImage.BasePath, fileHashCode + ".jpg");
//            if (File.Exists(thumbnailPath))
//            {
//                File.Delete(thumbnailPath);
//            }
//            
//            // Create an thumbnail based on the image
//            new ThumbnailByDirectory(_appSettings,null).CreateThumb(createAnImage.BasePath);
//            
//            Assert.AreEqual(true,File.Exists(thumbnailPath));
//            
//            File.Delete(thumbnailPath);

        }
	    
	    
	    [TestMethod]
	    public void ThumbnailByDirectory_Check_HashTest()
	    {
		    throw new NotImplementedException();

//		    var createAnImage = new CreateAnImage();
//		    _appSettings.ThumbnailTempFolder = createAnImage.BasePath;;
//		    _appSettings.StorageFolder = createAnImage.BasePath;
//
//
//		    var listOfFileIndexItems = new List<FileIndexItem> {
//			    new FileIndexItem
//				{
//					FileName = createAnImage.FileName,
//					ParentDirectory = "/",
//				}
//		    };
//		    // Create an thumbnail in base64 based on the image
//		    var base64DataUriList = new ThumbnailByDirectory(_appSettings,null).ToBase64DataUriList(listOfFileIndexItems);
//		    Assert.AreEqual(true,base64DataUriList.FirstOrDefault().Contains("data:image"));
	    }

        [TestMethod]
        public void Thumbnail_ResizeThumbnailToStream_JPEG_Test()
        {

            var newImage = new CreateAnImage();

            var thumb = new Thumbnail(new StorageHostFullPathFilesystem(), new FakeExifTool()).ResizeThumbnail(newImage.FullFilePath, 1, 1, 75, true,
	            ExtensionRolesHelper.ImageFormat.jpg);
            Assert.AreEqual(true,thumb.CanRead);
        }
        
        [TestMethod]
        public void Thumbnail_ResizeThumbnailToStream_PNG_Test()
        {
            var newImage = new CreateAnImage();

	        var thumb = new Thumbnail(new StorageHostFullPathFilesystem(), new FakeExifTool()).ResizeThumbnail(newImage.FullFilePath, 1, 1, 75, true,
		        ExtensionRolesHelper.ImageFormat.png);
            Assert.AreEqual(true,thumb.CanRead);
        }


    }
}
