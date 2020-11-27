using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.import.Services;
using starsky.foundation.database.Helpers;
using starsky.foundation.database.Import;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.database.Query;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Services;
using starsky.foundation.storage.Services;
using starskycore.Models;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;
using starskytest.Models;

namespace starskytest.starsky.feature.import.Services
{
	[TestClass]
	public class ImportTestInMemoryDb
	{
		private readonly IQuery _query;
		private readonly AppSettings _appSettings;
		private readonly FakeIStorage _iStorageFake;
		private readonly IConsole _console;
		private readonly IImportQuery _importQuery;
		private readonly string _exampleHash;

		public ImportTestInMemoryDb()
		{
			var provider = new ServiceCollection()
				.AddMemoryCache();

			_appSettings = new AppSettings{
				DatabaseType = AppSettings.DatabaseTypeList.InMemoryDatabase, 
				Verbose = true
			};

			provider.AddSingleton(_appSettings);

			new SetupDatabaseTypes(_appSettings, provider).BuilderDb();
			provider.AddScoped<IQuery,Query>();
			provider.AddScoped<IImportQuery, ImportQuery>();

			var serviceProvider = provider.BuildServiceProvider();
			
			_query = serviceProvider.GetRequiredService<IQuery>();
			_importQuery = serviceProvider.GetRequiredService<IImportQuery>();

			_console = new ConsoleWrapper();
			
			_iStorageFake = new FakeIStorage(
				new List<string>{"/"},
				new List<string>{"/test.jpg","/color_class_winner.jpg"},
				new List<byte[]>{CreateAnImage.Bytes, CreateAnImageColorClass.Bytes}
			);
			
			_exampleHash = new FileHash(_iStorageFake).GetHashCode("/test.jpg").Key;

		}
		
		[TestMethod]
		public async Task Importer_Gpx()
		{
			var storage = new FakeIStorage(
				new List<string>{"/"}, 
				new List<string>{"/test.gpx"},
				new List<byte[]>{CreateAnGpx.Bytes});
			
			var importService = new Import(new FakeSelectorStorage(storage), _appSettings, new FakeIImportQuery(null),
				new FakeExifTool(storage, _appSettings),_query,_console);
			var expectedFilePath = ImportTest.GetExpectedFilePath(storage, _appSettings, "/test.gpx");

			var result = await importService.Importer(new List<string> {"/test.gpx"},
				new ImportSettingsModel());

			var getResult = await _query.GetObjectByFilePathAsync(expectedFilePath);
			Assert.AreEqual(expectedFilePath,getResult.FilePath);
			Assert.AreEqual(ImportStatus.Ok, result[0].Status);
			
			await _query.RemoveItemAsync(getResult);
		}
		
		[TestMethod]
		public async Task Importer_OverwriteStructure_HappyFlow()
		{
			var importService = new Import(new FakeSelectorStorage(_iStorageFake), 
				_appSettings, new FakeIImportQuery(null),
				new FakeExifTool(_iStorageFake, _appSettings),_query, _console);
			
			var result = await importService.Importer(new List<string> {"/test.jpg"},
				new ImportSettingsModel{
					Structure = "/yyyy/MM/yyyy_MM_dd*/_yyyyMMdd_HHmmss.ext"
				});
			
			var expectedFilePath = ImportTest.GetExpectedFilePath(_iStorageFake, new AppSettings
			{
				Structure = "/yyyy/MM/yyyy_MM_dd*/_yyyyMMdd_HHmmss.ext"
			}, "/test.jpg");
			
			Assert.AreEqual(expectedFilePath,result[0].FilePath);
			var queryResult = await _query.GetObjectByFilePathAsync(expectedFilePath);
			
			Assert.AreEqual(expectedFilePath,queryResult.FilePath);

			_iStorageFake.FileDelete(expectedFilePath);
			await _query.RemoveItemAsync(queryResult);
		}
		
		
		[TestMethod]
		public async Task Importer_HappyFlow_ItShouldAddTo_ImportDb()
		{
			var importService = new Import(new FakeSelectorStorage(_iStorageFake), 
				_appSettings, new FakeIImportQuery(null),
				new FakeExifTool(_iStorageFake, _appSettings),_query, _console);
			
			var result = await importService.Importer(new List<string> {"/test.jpg"},
				new ImportSettingsModel{
					Structure = "/yyyy/MM/yyyy_MM_dd*/_yyyyMMdd_HHmmss.ext"
				});
			
			var isHashInImportDb = await _importQuery.IsHashInImportDbAsync(_exampleHash);
			Assert.IsTrue(isHashInImportDb);
			
			var expectedFilePath = ImportTest.GetExpectedFilePath(_iStorageFake, new AppSettings
			{
				Structure = "/yyyy/MM/yyyy_MM_dd*/_yyyyMMdd_HHmmss.ext"
			}, "/test.jpg");
			
			var queryResult = await _query.GetObjectByFilePathAsync(expectedFilePath);
			
			_iStorageFake.FileDelete(expectedFilePath);
			await _query.RemoveItemAsync(queryResult);
		}
		
		[TestMethod]
		public async Task Importer_OverwriteColorClass()
		{
			var importService = new Import(new FakeSelectorStorage(_iStorageFake), _appSettings, new FakeIImportQuery(null),
				new FakeExifTool(_iStorageFake, _appSettings),_query, _console);

			var expectedFilePath = ImportTest.GetExpectedFilePath(_iStorageFake, _appSettings, "/test.jpg");
			var result = await importService.Importer(new List<string> {"/test.jpg"},
				new ImportSettingsModel{
					ColorClass = 5
				});
			
			Assert.AreEqual(expectedFilePath,result.FirstOrDefault().FilePath);
			var queryResult = await _query.GetObjectByFilePathAsync(expectedFilePath);
			
			Assert.AreEqual(expectedFilePath,queryResult.FilePath);
			Assert.AreEqual(ColorClassParser.Color.Typical,queryResult.ColorClass);

			_iStorageFake.FileDelete(expectedFilePath);
			await _query.RemoveItemAsync(queryResult);
		}
		
		[TestMethod]
		public async Task Importer_ToDefaultFolderStructure_default_HappyFlow()
		{
			var importService = new Import(new FakeSelectorStorage(_iStorageFake), _appSettings, new FakeIImportQuery(null),
				new FakeExifTool(_iStorageFake, _appSettings),_query,_console);

			var expectedFilePath = ImportTest.GetExpectedFilePath(_iStorageFake, _appSettings, "/test.jpg");
			var result = await importService.Importer(new List<string> {"/test.jpg"},
				new ImportSettingsModel());
			
			Assert.AreEqual(expectedFilePath,result[0].FilePath);
			var queryResult = await _query.GetObjectByFilePathAsync(expectedFilePath);
			Assert.AreEqual(expectedFilePath,queryResult.FilePath);

			_iStorageFake.FileDelete(expectedFilePath);
			await _query.RemoveItemAsync(queryResult);
		}
	}
}
