using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Middleware;
using starsky.Models;

namespace starskytests.Models
{
    [TestClass]
    public class ImportIndexItemTest
    {
        private readonly AppSettings _appSettings;

        public ImportIndexItemTest()
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
        public void ImportIndexItemParseFileNameTest()
        {
            var createAnImage = new CreateAnImage();

            _appSettings.Structure = "/yyyy/MM/yyyy_MM_dd/yyyyMMdd_HHmmss.ext";

            var importItem = new ImportIndexItem(_appSettings);
            importItem.SourceFullFilePath = createAnImage.FullFilePath;

            var fileName = importItem.ParseFileName();
            Assert.AreEqual("00010101_000000.jpg", fileName);
        }

        [TestMethod]
        public void ImportIndexItemParseSubfoldersTest()
        {
            var createAnImage = new CreateAnImage();
            _appSettings.Structure = "/yyyy/MM/yyyy_MM_dd/yyyyMMdd_HHmmss.ext";
            var importItem = new ImportIndexItem(_appSettings);
            importItem.SourceFullFilePath = createAnImage.FullFilePath;
            var s = importItem.ParseSubfolders(false);
            Assert.AreEqual("/0001/01/0001_01_01/",s);
        }

        [TestMethod]
        public void ImportIndexItemParseSubfolders_TRslashABC_Test()
        {
            _appSettings.Structure = "/\\t\\r/\\a\\b\\c/test";

            var createAnImage = new CreateAnImage();
            var importItem = new ImportIndexItem(_appSettings);

            importItem.SourceFullFilePath = createAnImage.FullFilePath;
            var s = importItem.ParseSubfolders(false);
            Assert.AreEqual("/tr/abc/",s);
        }
        
        [TestMethod]
        public void ImportIndexItemParseSubfolders_Tzzz_slashABC_Test()
        {
            _appSettings.Structure = "/\\t\\z/\\a\\b\\c/test";
            var createAnImage = new CreateAnImage();
            var importItem = new ImportIndexItem(_appSettings);

            importItem.SourceFullFilePath = createAnImage.FullFilePath;
            _appSettings.StorageFolder = createAnImage.BasePath;
            var s = importItem.ParseSubfolders(false);
            Assert.AreEqual("/tz/abc/",s);
        }
        
        [TestMethod]
        public void ImportIndexItemParse_filenamebase_filename_Test()
        {
            _appSettings.Structure = "/\\t\\z/\\a\\b{filenamebase}/{filenamebase}.ext";

            var createAnImage = new CreateAnImage();
            var importItem = new ImportIndexItem(_appSettings);
            importItem.SourceFullFilePath = createAnImage.FullFilePath;
            _appSettings.StorageFolder = createAnImage.BasePath;
            var fileName = importItem.ParseFileName();
            Assert.AreEqual(createAnImage.DbPath.Replace("/",string.Empty),fileName);
        }
        
        [TestMethod]
        public void ImportIndexItemParse_filenamebase_subfolder_Test()
        {
            _appSettings.Structure = "/{filenamebase}/{filenamebase}.ext";
            var createAnImage = new CreateAnImage();
            var importItem = new ImportIndexItem(_appSettings);

            importItem.SourceFullFilePath = createAnImage.FullFilePath;
            _appSettings.StorageFolder = createAnImage.BasePath;
            var subfolders = importItem.ParseSubfolders(false);
            Assert.AreEqual("/" + createAnImage.DbPath.Replace("/",string.Empty).Replace(".jpg",string.Empty) + "/",subfolders);
        }

        [TestMethod]
        [ExpectedException(typeof(FileNotFoundException))]
        public void ImportIndexItemParse_FileNotExist_Test()
        {
            _appSettings.Structure = "/yyyyMMdd_HHmmss.ext";
            var input = new ImportIndexItem(_appSettings)
            {
                SourceFullFilePath = Path.DirectorySeparatorChar + "20180101_011223.jpg"
            };

            input.ParseFileName();
            // ExpectedException
        }

        [TestMethod]
        public void ImportIndexItemParse_ParseDateTimeFromFileName_Test()
        {

            _appSettings.Structure = "/yyyyMMdd_HHmmss.ext";
            
            var input = new ImportIndexItem(_appSettings)
            {
                SourceFullFilePath = Path.DirectorySeparatorChar + "20180101_011223.jpg"
            };
            
            input.ParseDateTimeFromFileName();
            
            DateTime.TryParseExact(
                "20180101_011223", 
                "yyyyMMdd_HHmmss",
                CultureInfo.InvariantCulture, 
                DateTimeStyles.None, 
                out var anserDateTime);
            
            Assert.AreEqual(anserDateTime,input.DateTime);
        }
        
        [TestMethod]
        public void ImportIndexItemParse_ParseDateTimeFromFileNameWithExtraFileNameBase_Test()
        {

            _appSettings.Structure = "/yyyyMMdd_HHmmss_{filenamebase}.ext";
            
            var input = new ImportIndexItem(_appSettings)
            {
                SourceFullFilePath = Path.DirectorySeparatorChar + "20180101_011223_2018-07-26 19.45.23.jpg"
            };
            
            input.ParseDateTimeFromFileName();
            
            DateTime.TryParseExact(
                "20180101_011223", 
                "yyyyMMdd_HHmmss",
                CultureInfo.InvariantCulture, 
                DateTimeStyles.None, 
                out var anserDateTime);
            
            Assert.AreEqual(anserDateTime,input.DateTime);
        }

        
        [TestMethod]
        public void ImportIndexItemParse_ParseDateTimeFromFileName_WithExtraDotsInName_Test()
        {

            _appSettings.Structure = "/yyyyMMdd_HHmmss.ext";

            var input = new ImportIndexItem(_appSettings)
            {
                SourceFullFilePath = Path.DirectorySeparatorChar + "2018-02-03 18.47.35.jpg"
            };
            
            input.ParseDateTimeFromFileName();
            
            Regex pattern = new Regex("-|_| |;|\\.|:");
            var output = pattern.Replace("2018-02-03 18.47.35.jpg",string.Empty);
            
            DateTime.TryParseExact(
                "20180203_184735", 
                "yyyyMMdd_HHmmss",
                CultureInfo.InvariantCulture, 
                DateTimeStyles.None, 
                out var anserDateTime);
            
            Assert.AreEqual(anserDateTime,input.DateTime);
        }
        
        
    }
}