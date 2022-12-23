using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.import.Models;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Extensions;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Storage;
using starskytest.FakeCreateAn;

namespace starskytest.starsky.foundation.database.Models
{
	[TestClass]
	public sealed class ImportIndexItemTest
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
			services.ConfigurePoCo<AppSettings>(configuration.GetSection("App"));
			// build the service
			var serviceProvider = services.BuildServiceProvider();
			// get the service
			_appSettings = serviceProvider.GetRequiredService<AppSettings>();
		}

		[TestMethod]
		public void ImportIndexItemRemoveEscapedCharactersTest()
		{
			var structuredFileName = "yyyyMMdd_HHmmss_\\d.ext";
			var result = ImportIndexItem.RemoveEscapedCharacters(structuredFileName);
			Assert.AreEqual("yyyyMMdd_HHmmss_.ext",result);
		}

		[TestMethod]
		public void ParseDateTimeFromFileName_Null()
		{
			var importItem = new ImportIndexItem {SourceFullFilePath = null};
			var dateTime = importItem.ParseDateTimeFromFileName();
			Assert.AreEqual(new DateTime(), dateTime);
		}

		[TestMethod]
		public void ParseDateTimeFromFileName_Test()
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
				out var answerDateTime);
            
			Assert.AreEqual(answerDateTime,input.DateTime);
		}

		[TestMethod]
		public void ParseDateTimeFromFileNameWithSpaces_Test()
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
				out var answerDateTime);
            
			Assert.AreEqual(answerDateTime,input.DateTime);
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
				out var answerDateTime);
            
			Assert.AreEqual(answerDateTime,input.DateTime);
		}

        
		[TestMethod]
		public void ImportIndexItemParse_ParseDateTimeFromFileName_AppendixUsedInConfig()
		{

			_appSettings.Structure = "/yyyyMMdd_HHmmss_\\d\\e\\f\\g.ext";
            
			var input = new ImportIndexItem(_appSettings)
			{
				SourceFullFilePath = Path.DirectorySeparatorChar + "20180726_194523.jpg"
			};
            
			input.ParseDateTimeFromFileName();
            
			DateTime.TryParseExact(
				"20180726_194523", 
				"yyyyMMdd_HHmmss",
				CultureInfo.InvariantCulture, 
				DateTimeStyles.None, 
				out var answerDateTime);
            
			Assert.AreEqual(answerDateTime,input.DateTime);
		}
		
		[TestMethod]
		public void ImportIndexItemParse_Structure_Fallback()
		{
			_appSettings.Structure = null;
			var input = new ImportIndexItem(_appSettings){SourceFullFilePath = ".jpg"};
			var result = input.ParseDateTimeFromFileName();
			Assert.AreEqual(result,new DateTime());
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
            
			Regex pattern = new Regex("-|_| |;|\\.|:", RegexOptions.None, TimeSpan.FromMilliseconds(100));
			var output = pattern.Replace("2018-02-03 18.47.35.jpg",string.Empty);
            
			DateTime.TryParseExact(
				"20180203_184735", 
				"yyyyMMdd_HHmmss",
				CultureInfo.InvariantCulture, 
				DateTimeStyles.None, 
				out var answerDateTime);
            
			Assert.AreEqual(answerDateTime,input.DateTime);
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
		[ExpectedException(typeof(ArgumentNullException))]
		public void ImportIndexItem_CtorRequest_SearchSubDirInDirectory()
		{
			new ImportIndexItem().SearchSubDirInDirectory(null, null);
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
				out var answerDateTime);
            
			// Check if those overwrite is accepted
			Assert.AreEqual(answerDateTime,input.DateTime);
                   
			new StorageHostFullPathFilesystem().FileDelete(createAnImageNoExif.FullFilePathWithDate);
		}

		[TestMethod]
		public void ImportFileSettingsModel_DefaultsToIgnore_Test()
		{
			var importSettings = new ImportSettingsModel {ColorClass = 999};
			Assert.AreEqual(-1,importSettings.ColorClass);
		}
	}
}
