using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starskycore.Helpers;
using starskycore.Middleware;
using starskycore.Models;
using starskytests.FakeCreateAn;

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
        public void ImportIndexItemParse_FileNameWithAppendix_Test()
        {
            var createAnImage = new CreateAnImageNoExif();

            _appSettings.Structure = "/yyyy/MM/yyyy_MM_dd/yyyyMMdd_HHmmss_\\d.ext"; // <<<----

            var importItem = new ImportIndexItem(_appSettings);
            importItem.SourceFullFilePath = createAnImage.FullFilePathWithDate;
	        importItem.ParseDateTimeFromFileName();
	        
            var fileName = importItem.ParseFileName(false);
            Assert.AreEqual("00010101_000000_d.jpg", fileName);
            
            FilesHelper.DeleteFile(importItem.SourceFullFilePath);
        }
	    
	    [TestMethod]
	    public void ImportIndexItemParse_FileNameWithAppendixInFileName_Test()
	    {
		    var createAnImage = new CreateAnImageNoExif();

		    var filPathWithAppendix = Path.Join(createAnImage.BasePath,"2018.01.01 02.02.02-test.jpg");
		    if(!File.Exists(filPathWithAppendix)) File.Move(createAnImage.FullFilePathWithDate,filPathWithAppendix);
		    
		    _appSettings.Structure = "/yyyyMMdd_HHmmss_\\d.ext"; // <<<----

		    var importItem = new ImportIndexItem(_appSettings);
		    importItem.SourceFullFilePath = filPathWithAppendix;
		    importItem.ParseDateTimeFromFileName();

		    var fileName = importItem.ParseFileName(false);
		    Assert.AreEqual("20180101_020202_d.jpg", fileName);
            
		    FilesHelper.DeleteFile(filPathWithAppendix);
	    }

	    [TestMethod]
	    public void ImportIndexItemRemoveEscapedCharactersTest()
	    {
		    var structuredFileName = "yyyyMMdd_HHmmss_\\d.ext";
		    var result = new ImportIndexItem(new AppSettings()).RemoveEscapedCharacters(structuredFileName);
			Assert.AreEqual("yyyyMMdd_HHmmss_.ext",result);
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
            _appSettings.Structure = "/\\t\\r/\\a\\b\\c/\\t\\e\\s\\t.ext";
            // file.ext is ignored but required
            
            var createAnImage = new CreateAnImage();
            var importItem = new ImportIndexItem(_appSettings);

            importItem.SourceFullFilePath = createAnImage.FullFilePath;
            var s = importItem.ParseSubfolders(false);
            Assert.AreEqual("/tr/abc/",s);
        }
        
        [TestMethod]
        public void ImportIndexItemParseSubfolders_Tzzz_slashABC_Test()
        {
            _appSettings.Structure = "/\\t\\z/\\a\\b\\c/test.ext";
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
	    public void ImportIndexItemParse_xuxuxuxu_subfolder_Test()
	    {
		    _appSettings.Structure = "/xuxuxuxu_ssHHmm.ext";

		    var createAnImage = new CreateAnImage();
		    var importItem = new ImportIndexItem(_appSettings);

		    importItem.SourceFullFilePath = createAnImage.FullFilePath;
		    _appSettings.StorageFolder = createAnImage.BasePath;
		    var subfolders = importItem.ParseSubfolders(false);
		    Assert.AreNotEqual(subfolders,"/cs");
		    Assert.AreEqual(subfolders,string.Empty);
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
        public void ImportIndexItemParse_ParseDateTimeFromFileNameWithSpaces_Test()
        {
            var input = new ImportIndexItem(new AppSettings())
            {
                SourceFullFilePath = Path.DirectorySeparatorChar + "2018 08 20 19 03 00.jpg"
            };
            
            input.ParseDateTimeFromFileName();

            DateTime.TryParseExact(
                "20180820_190300", 
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
                SourceFullFilePath = Path.DirectorySeparatorChar + "2018-07-26 19.45.23.jpg"
            };
            
            input.ParseDateTimeFromFileName();
            
            DateTime.TryParseExact(
                "20180726_194523", 
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

        [TestMethod]
        public void ImportIndexItem_CtorRequest_ColorClass()
        {
            var context = new DefaultHttpContext();
            context.Request.Headers["ColorClass"] = "1";
            var model = new ImportSettingsModel(context.Request);
            Assert.AreEqual(1, model.ColorClass);
            
        }
        
        [TestMethod]
        public void ImportIndexItemParse_OverWriteStructureFeature_Test()
        {
            var createAnImageNoExif = new CreateAnImageNoExif();
            var createAnImage = new CreateAnImage();

            _appSettings.Structure = null;
            // Go to the default structure setting 
            _appSettings.StorageFolder = createAnImage.BasePath;
    
            // Use a strange structure setting to overwrite
            var input = new ImportIndexItem(_appSettings)
            {
                SourceFullFilePath = createAnImageNoExif.FullFilePathWithDate,
                Structure =  "/HHmmss_yyyyMMdd.ext"
            };

            input.ParseDateTimeFromFileName();
            
            DateTime.TryParseExact(
                "20120101_123300", 
                "yyyyMMdd_HHmmss",
                CultureInfo.InvariantCulture, 
                DateTimeStyles.None, 
                out var anserDateTime);
            
            // Check if those overwite is accepted
            Assert.AreEqual(anserDateTime,input.DateTime);
                   
            FilesHelper.DeleteFile(createAnImageNoExif.FullFilePathWithDate);
        }

        [TestMethod]
        public void ImportFileSettingsModel_DefaultsToZero_Test()
        {
            var importSettings = new ImportSettingsModel {ColorClass = 999};
            Assert.AreEqual(0,importSettings.ColorClass);
        }

        



    }
}
