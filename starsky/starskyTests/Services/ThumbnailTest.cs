using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Helpers;
using starsky.Middleware;
using starsky.Models;
using starsky.Services;

namespace starskytests.Services
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
        public void CreateAndRenamteThumbTest()
        {

            var newImage = new CreateAnImage();
            _appSettings.ThumbnailTempFolder = newImage.BasePath;;

            var hashString = FileHash.GetHashCode(newImage.FullFilePath);

            // Delete if exist, to optimize test
            var thumbnailPAth = Path.Combine(newImage.BasePath, hashString + ".jpg");
            if (File.Exists(thumbnailPAth))
            {
                File.Delete(thumbnailPAth);
            }

            // Create an thumbnail based on the image
            new Thumbnail(_appSettings).CreateThumb(newImage.DbPath);
            Assert.AreEqual(true,File.Exists(thumbnailPAth));

            // Test Rename feature and delete if passed
            new Thumbnail(_appSettings).RenameThumb(hashString, "AAAAA");
            var thumbnailApAth = Path.Combine(newImage.BasePath, "AAAAA" + ".jpg");
            if (File.Exists(thumbnailApAth))
            {
                File.Delete(thumbnailApAth);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(FileNotFoundException))]
        public void ThumbnailCreateThumbnailNullTest()
        {
            new Thumbnail(_appSettings).CreateThumb(new FileIndexItem());
        }
        
        [TestMethod]
        public void ThumbnailCreateThumbnailNotFoundTest()
        {
            var newImage = new CreateAnImage();
            _appSettings.ThumbnailTempFolder = newImage.BasePath;;
            new Thumbnail(_appSettings).CreateThumb(new FileIndexItem{FileHash = "t",FileName = "t",FilePath = "/"});
        }

        [TestMethod]
        [ExpectedException(typeof(FileNotFoundException))]
        public void ThumbnailCreateThumb_FileIndexItem_ThumbnailTempFolderNull_Test()
        {
            _appSettings.ThumbnailTempFolder = null;
            new Thumbnail(_appSettings).CreateThumb(new FileIndexItem());
        }

        [TestMethod]
        [ExpectedException(typeof(FileNotFoundException))]
        public void ThumbnailRenameThumb_DirectInput_ThumbnailTempFolderNull_Test()
        {
            _appSettings.ThumbnailTempFolder = null;
            new Thumbnail(_appSettings).RenameThumb(null, null);
        }
        
        [TestMethod]
        public void ThumbnailRenameThumb_DirectInput_nonexistingOldHash_Test()
        {
            // Should not crash
            var newImage = new CreateAnImage();
            _appSettings.ThumbnailTempFolder = newImage.BasePath;;
            new Thumbnail(_appSettings).RenameThumb(null, "ThumbnailRenameThumb_nonexistingOldHash_Test");
        }
        
        [TestMethod]
        public void ThumbnailRenameThumb_DirectInput_nonexistingNewHash_Test()
        {
            // For testing:    if File.Exists(newThumbPath)
            var newImage = new CreateAnImage();
            _appSettings.ThumbnailTempFolder = newImage.BasePath;;
            var dbPathWithoutExtAndSlash = newImage.DbPath.Replace(".jpg", string.Empty).Replace("/", string.Empty);
            new Thumbnail(_appSettings).RenameThumb(dbPathWithoutExtAndSlash,dbPathWithoutExtAndSlash);
        }
        
        [TestMethod]
        public void ThumbnailByDirectoryTest()
        {

            var createAnImage = new CreateAnImage();
            _appSettings.ThumbnailTempFolder = createAnImage.BasePath;;
            _appSettings.StorageFolder = createAnImage.BasePath;

            var hashString = FileHash.GetHashCode(createAnImage.FullFilePath);

            // Delete if exist, to optimize test
            var thumbnailPath = Path.Combine(createAnImage.BasePath, hashString + ".jpg");
            if (File.Exists(thumbnailPath))
            {
                File.Delete(thumbnailPath);
            }
            
            // Create an thumbnail based on the image
            new ThumbnailByDirectory(_appSettings,null).CreateThumb(createAnImage.BasePath);
            
            Assert.AreEqual(true,File.Exists(thumbnailPath));
            
            File.Delete(thumbnailPath);

        }

        [TestMethod]
        public void Thumbnail_ResizeThumbnailToStream_JPEG_Test()
        {
            var newImage = new CreateAnImage();

            var thumb = new Thumbnail(_appSettings, null).ResizeThumbnailToStream(newImage.FullFilePath, 1, 1, 75, true,
                Files.ImageFormat.jpg);
            Assert.AreEqual(true,thumb.CanRead);
        }
        
        [TestMethod]
        public void Thumbnail_ResizeThumbnailToStream_PNG_Test()
        {
            var newImage = new CreateAnImage();

            var thumb = new Thumbnail(_appSettings, null).ResizeThumbnailToStream(newImage.FullFilePath, 1, 1, 75, false,
                Files.ImageFormat.png);
            Assert.AreEqual(true,thumb.CanRead);
        }


    }
}