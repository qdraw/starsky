using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.import.Models;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Extensions;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Storage;
using starsky.foundation.storage.Structure.Helpers;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

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
			var dict = new Dictionary<string, string?>
			{
				{ "App:StorageFolder", newImage.BasePath }, { "App:Verbose", "true" }
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

			Assert.AreEqual(answerDateTime, input.DateTime);
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

			_appSettings.Structure = null!;
			// Go to the default structure setting 
			_appSettings.StorageFolder = createAnImage.BasePath;

			// Use a strange structure setting to overwrite
			var input = new ImportIndexItem(_appSettings)
			{
				SourceFullFilePath = createAnImageNoExif.FullFilePathWithDate,
				Structure = "/HHmmss_yyyyMMdd.ext"
			};

			input.ParseDateTimeFromFileName();

			DateTime.TryParseExact(
				"20120101_123300",
				"yyyyMMdd_HHmmss",
				CultureInfo.InvariantCulture,
				DateTimeStyles.None,
				out var answerDateTime);

			// Check if those overwrite is accepted
			Assert.AreEqual(answerDateTime, input.DateTime);

			new StorageHostFullPathFilesystem(new FakeIWebLogger()).FileDelete(createAnImageNoExif
				.FullFilePathWithDate);
		}

		[TestMethod]
		public void ImportFileSettingsModel_DefaultsToIgnore_Test()
		{
			var importSettings = new ImportSettingsModel { ColorClass = 999 };
			Assert.AreEqual(-1, importSettings.ColorClass);
		}
	}
}
