using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starskycore.Helpers;
using starskycore.Middleware;
using starskycore.Models;
using starskytest.FakeCreateAn;

namespace starskytest.Helpers
{
    [TestClass]
    public class FilesTest
    {
        private readonly AppSettings _appSettings;

        public FilesTest()
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
                { "App:ThumbnailTempFolder", newImage.BasePath },
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
        public void Files_IsFolderOrFileTest()
        {
            var newImage = new CreateAnImage();
            // Testing base folder of Image, and Image it self
            
            Assert.AreEqual(FolderOrFileModel.FolderOrFileTypeList.Folder, FilesHelper.IsFolderOrFile(newImage.BasePath));
            Assert.AreEqual(FolderOrFileModel.FolderOrFileTypeList.File,FilesHelper.IsFolderOrFile(newImage.FullFilePath));
        }

        [TestMethod]
        public void Files_GetFilesInDirectoryTest1()
        {
            // Used for JPEG files
            var newImage = new CreateAnImage();
            _appSettings.ThumbnailTempFolder = newImage.BasePath;
            _appSettings.StorageFolder = newImage.BasePath;
            var filesInFolder = FilesHelper.GetFilesInDirectory(newImage.BasePath);
            Assert.AreEqual(filesInFolder.Any(),true);
        }



        
    }
}
