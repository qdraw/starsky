using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Helpers;
using starsky.Middleware;
using starsky.Models;
using starsky.Services;

namespace starskytests
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
            
            Assert.AreEqual(FolderOrFileModel.FolderOrFileTypeList.Folder, Files.IsFolderOrFile(newImage.BasePath));
            Assert.AreEqual(FolderOrFileModel.FolderOrFileTypeList.File,Files.IsFolderOrFile(newImage.FullFilePath));
        }

        [TestMethod]
        public void Files_GetAllFilesDirectoryTest()
        {
            // Assumes that
            //     ~/.nuget/packages/microsoft.testplatform.testhost/15.6.0/lib/netstandard1.5/
            // has subfolders
            
            // Used For subfolders
            var newImage = new CreateAnImage();
            var filesInFolder = Files.GetAllFilesDirectory(newImage.BasePath);
            Assert.AreEqual(true,filesInFolder.Any());
            
        }

        [TestMethod]
        public void Files_GetFilesInDirectoryTest1()
        {
            // Used for JPEG files
            var newImage = new CreateAnImage();
            _appSettings.ThumbnailTempFolder = newImage.BasePath;
            _appSettings.StorageFolder = newImage.BasePath;
            var filesInFolder = Files.GetFilesInDirectory(newImage.BasePath);
            Assert.AreEqual(filesInFolder.Any(),true);
        }

        [TestMethod]
        public void Files_GetFilesRecrusiveTest()
        {            
            var path = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + Path.DirectorySeparatorChar;

            var content = Files.GetFilesRecrusive(path);

            Console.WriteLine("count => "+ content.Count());

            // Gives a list of the content in the temp folder.
            Assert.AreEqual(true, content.Count() >= 2);            

        }

        [TestMethod]
        public void Files_ExtensionThumbSupportedList_TiffMp4MovXMPCheck()
        {
            var extensionThumbSupportedList = Files.ExtensionThumbSupportedList;
            Assert.AreEqual(false,extensionThumbSupportedList.Contains("tiff"));
            Assert.AreEqual(false,extensionThumbSupportedList.Contains("mp4"));
            Assert.AreEqual(false,extensionThumbSupportedList.Contains("mov"));
            Assert.AreEqual(false,extensionThumbSupportedList.Contains("xmp"));
        }
        
        [TestMethod]
        public void Files_ExtensionSyncSupportedList_TiffCheck()
        {
            var extensionSyncSupportedList = Files.ExtensionSyncSupportedList;
            Assert.AreEqual(true,extensionSyncSupportedList.Contains("tiff"));
            Assert.AreEqual(true,extensionSyncSupportedList.Contains("jpg"));

        }
    }
}