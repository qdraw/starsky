using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.import.Services;
using starsky.foundation.database.Import;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starsky.foundation.readmeta.Services;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Services;
using starskycore.Models;
using starskytest.FakeMocks;
using starskytest.Models;

namespace starskytest.starsky.feature.import.Services
{
	[TestClass]
	public class ImportTest
	{
		private readonly FakeIStorage _iStorageFake;
		private readonly string _exampleHash;
		private readonly FakeIStorage _iStorageDirectoryRecursive;

		public ImportTest()
		{
			_iStorageFake = new FakeIStorage(
				new List<string>{"/"},
				new List<string>{"/test.jpg"},
				new List<byte[]>{FakeCreateAn.CreateAnImage.Bytes}
				);
			_exampleHash = new FileHash(_iStorageFake).GetHashCode("/test.jpg").Key;
			
			_iStorageDirectoryRecursive = new FakeIStorage(
				new List<string>{"/", "/test", "/test/test"},
				new List<string>{"/layer0.jpg","/test/layer1.jpg", "/test/test/layer2.jpg"},
				new List<byte[]>{
					FakeCreateAn.CreateAnImage.Bytes,
					FakeCreateAn.CreateAnImage.Bytes, 
					FakeCreateAn.CreateAnImage.Bytes}
			);
		}

		[TestMethod]
		public async Task Preflight_SingleImage_HappyFlow()
		{
			var appSettings = new AppSettings();
			var importService = new Import(new FakeSelectorStorage(_iStorageFake), appSettings, new FakeIImportQuery(null),
				new FakeExifTool(_iStorageFake, appSettings), null);

			var result = await importService.Preflight(
				new List<string> {"/test.jpg"},
				new ImportSettingsModel());
			
			Assert.IsNotNull(result.FirstOrDefault());
			Assert.AreEqual(ImportStatus.Ok, result.FirstOrDefault().Status);
			
			Assert.IsNotNull(result.FirstOrDefault().FileIndexItem);
			Assert.IsNotNull(result.FirstOrDefault().FileIndexItem.FilePath);
		}
		
		
		[TestMethod]
		public async Task Preflight_SingleImage_DateGetByFileNameNoExif()
		{
			var appSettings = new AppSettings();
			var storage = new FakeIStorage(
				new List<string>{"/"},
				new List<string>{"/2020-04-27 11:07:00.jpg"},
				new List<byte[]>{FakeCreateAn.CreateAnImageNoExif.Bytes}
			);
			var importService = new Import(new FakeSelectorStorage(storage), appSettings, new FakeIImportQuery(null),
				new FakeExifTool(_iStorageFake, appSettings), null);
			
			var result = await importService.Preflight(
				new List<string> {"/2020-04-27 11:07:00.jpg"},
				new ImportSettingsModel());
			
			Assert.IsNotNull(result.FirstOrDefault());
			Assert.AreEqual(new DateTime(2020,04,27,11,07,00), 
				result.FirstOrDefault().DateTime);

			Assert.AreEqual(importService.MessageDateTimeBasedOnFilename,
				result.FirstOrDefault().FileIndexItem.Description);
		}
		
		[TestMethod]
		public async Task Preflight_SingleImage_FileType_NotSupported()
		{
			var appSettings = new AppSettings();
			var storage = new FakeIStorage(
				new List<string>{"/"},
				new List<string>{"/test.jpg"},
				new List<byte[]>{new byte[0]}
			);
			var importService = new Import(new FakeSelectorStorage(storage), appSettings, new FakeIImportQuery(null),
				new FakeExifTool(_iStorageFake, appSettings), null);
			
			var result = await importService.Preflight(
				new List<string> {"/test.jpg"},
				new ImportSettingsModel());
			
			Assert.IsNotNull(result.FirstOrDefault());
			Assert.AreEqual(ImportStatus.FileError, result.FirstOrDefault().Status);
		}
		
		[TestMethod]
		public async Task Preflight_SingleImage_WrongExtension()
		{
			var appSettings = new AppSettings();
			
			var storage = new FakeIStorage(
				new List<string>{"/"},
				new List<string>{"/test.unknown"},
				new List<byte[]>{FakeCreateAn.CreateAnImage.Bytes}
			);
			
			var importService = new Import(new FakeSelectorStorage(storage), appSettings, new FakeIImportQuery(null),
				new FakeExifTool(storage, appSettings), null);

			var result = await importService.Preflight(
				new List<string> {"/test.unknown"},
				new ImportSettingsModel());
			
			Assert.IsNotNull(result.FirstOrDefault());
			Assert.AreEqual(ImportStatus.FileError, result.FirstOrDefault().Status);
		}

		[TestMethod]
		public async Task Preflight_SingleImage_HashAlreadyInImportDb()
		{
			var appSettings = new AppSettings();
			var importService = new Import(new FakeSelectorStorage(_iStorageFake), appSettings,
				new FakeIImportQuery(new List<string>{_exampleHash}),

			new FakeExifTool(_iStorageFake, appSettings), null);

			var result = await importService.Preflight(new List<string> {"/test.jpg"},
				new ImportSettingsModel());
			
			Assert.IsNotNull(result.FirstOrDefault());
			Assert.AreEqual(ImportStatus.IgnoredAlreadyImported, result.FirstOrDefault().Status);
		}
		
		[TestMethod]
		public async Task Preflight_SingleImage_Ignore_HashAlreadyInImportDb()
		{
			var appSettings = new AppSettings();
			var importService = new Import(new FakeSelectorStorage(_iStorageFake), appSettings,
				new FakeIImportQuery(new List<string>{_exampleHash}),
				new FakeExifTool(_iStorageFake, appSettings), null);

			var result = await importService.Preflight(new List<string> {"/test.jpg"},
				new ImportSettingsModel
				{
					IndexMode = false
				});
			
			Assert.IsNotNull(result.FirstOrDefault());
			Assert.AreEqual(ImportStatus.Ok, result.FirstOrDefault().Status);
		}
		
		[TestMethod]
		public async Task Preflight_SingleImage_NonExist()
		{
			var appSettings = new AppSettings();
			var importService = new Import(new FakeSelectorStorage(_iStorageFake), appSettings, new FakeIImportQuery(null),
				new FakeExifTool(_iStorageFake, appSettings), null);
			
			var result = await importService.Preflight(new List<string> {"/non-exist.jpg"},
				new ImportSettingsModel());
			
			Assert.IsNotNull(result.FirstOrDefault());
			Assert.AreEqual(ImportStatus.NotFound, result.FirstOrDefault().Status);
		}
		
		[TestMethod]
		public async Task Preflight_SingleImage_NonExistDirectory()
		{
			var appSettings = new AppSettings();
			var importService = new Import(new FakeSelectorStorage(_iStorageFake), appSettings, new FakeIImportQuery(null),
				new FakeExifTool(_iStorageFake, appSettings), null);
			
			var result = await importService.Preflight(new List<string> {"/non-exist"},
				new ImportSettingsModel());
			
			Assert.IsNotNull(result.FirstOrDefault());
			Assert.AreEqual(ImportStatus.NotFound, result.FirstOrDefault().Status);
		}

		[TestMethod]
		public async Task Preflight_DirectoryRecursive()
		{
			var appSettings = new AppSettings();
			var importService = new Import(
				new FakeSelectorStorage(_iStorageDirectoryRecursive), 
				appSettings, new FakeIImportQuery(null),
				new FakeExifTool(_iStorageDirectoryRecursive, appSettings), null);
			
			var result = await importService.Preflight(new List<string> {"/"},
				new ImportSettingsModel{RecursiveDirectory = true});
			
			Assert.IsNotNull(result.FirstOrDefault());
			Assert.AreEqual(3,result.Count);

			Assert.AreEqual(ImportStatus.Ok, result[0].Status);
			Assert.AreEqual(ImportStatus.Ok, result[1].Status);
			Assert.AreEqual(ImportStatus.Ok, result[2].Status);

			// "/layer0.jpg","/test/layer1.jpg", "/test/test/layer2.jpg"
			Assert.AreEqual("/layer0.jpg", result[0].SourceFullFilePath);
			Assert.AreEqual("/test/layer1.jpg", result[1].SourceFullFilePath);
			Assert.AreEqual("/test/test/layer2.jpg", result[2].SourceFullFilePath);
		}
		
		[TestMethod]
		public async Task Preflight_DirectoryNonRecursive()
		{
			var appSettings = new AppSettings();
			var importService = new Import(new FakeSelectorStorage(_iStorageDirectoryRecursive), appSettings, 
			new FakeIImportQuery(null),
				new FakeExifTool(_iStorageDirectoryRecursive, appSettings), null);
			
			var result = await importService.Preflight(new List<string> {"/test"},
				new ImportSettingsModel{RecursiveDirectory = false});
			
			Assert.IsNotNull(result.FirstOrDefault());
			Assert.AreEqual(1,result.Count);

			Assert.AreEqual(ImportStatus.Ok, result[0].Status);

			// "/layer0.jpg",
			Assert.AreEqual("/test/layer1.jpg", result[0].SourceFullFilePath);
		}

		[TestMethod]
		public void AppendIndexerToFilePath_default()
		{
			var result = Import.AppendIndexerToFilePath("/test/", "test.jpg", 0);
			Assert.AreEqual("/test/test.jpg",result);
		}
		
		[TestMethod]
		public void AppendIndexerToFilePath_minus10()
		{
			var result = Import.AppendIndexerToFilePath("/test/", "test.jpg", -10);
			Assert.AreEqual("/test/test.jpg",result);
		}
		
		[TestMethod]
		public void AppendIndexerToFilePath_5()
		{
			var result = Import.AppendIndexerToFilePath("/test", "test.jpg", 5);
			Assert.AreEqual("/test/test_5.jpg",result);
		}

		/// <summary>
		/// Helper to get the expected file path
		/// </summary>
		/// <param name="storage">use this storage provider</param>
		/// <param name="appSettings">structure config</param>
		/// <param name="inputFileFullPath">subPath style </param>
		/// <param name="index">number</param>
		/// <returns>expected result</returns>
		private string GetExpectedFilePath(IStorage storage, AppSettings appSettings, string inputFileFullPath, int index = 0)
		{
			var fileIndexItem = new ReadMeta(_iStorageFake).ReadExifAndXmpFromFile(inputFileFullPath);
			var importIndexItem = new ImportIndexItem(appSettings)
			{
				FileIndexItem = fileIndexItem,
				DateTime = fileIndexItem.DateTime,
				SourceFullFilePath = inputFileFullPath
			};

			var structureService = new StructureService(storage, appSettings.Structure);
			importIndexItem.FileIndexItem.ParentDirectory = structureService.ParseSubfolders(
				fileIndexItem.DateTime, fileIndexItem.FileCollectionName,
				fileIndexItem.ImageFormat);
			importIndexItem.FileIndexItem.FileName = structureService.ParseFileName(
				fileIndexItem.DateTime, fileIndexItem.FileCollectionName,
				fileIndexItem.ImageFormat);
			
			return Import.AppendIndexerToFilePath(
				importIndexItem.FileIndexItem.ParentDirectory,
				importIndexItem.FileIndexItem.FileName, 
				index) ;
		}

		[TestMethod]
		public async Task Importer_ToDefaultFolderStructure_default_HappyFlow()
		{
			var appSettings = new AppSettings();
			var query = new FakeIQuery();
			var importService = new Import(new FakeSelectorStorage(_iStorageFake), appSettings, new FakeIImportQuery(null),
				new FakeExifTool(_iStorageFake, appSettings),query);

			var expectedFilePath = GetExpectedFilePath(_iStorageFake, appSettings, "/test.jpg");
			var result = await importService.Importer(new List<string> {"/test.jpg"},
				new ImportSettingsModel());
			
			Assert.AreEqual(expectedFilePath,result.FirstOrDefault().FilePath);
			Assert.AreEqual(expectedFilePath,query.GetObjectByFilePath(expectedFilePath).FilePath);

			_iStorageFake.FileDelete(expectedFilePath);
		}

		[TestMethod]
		[ExpectedException(typeof(ApplicationException))]
		public async Task Importer_Over100Times()
		{
			var appSettings = new AppSettings();
			var storage =new FakeIStorage();
			// write source file
			await storage.WriteStreamAsync(
				new MemoryStream(FakeCreateAn.CreateAnImage.Bytes), "/test.jpg"
			);
			// write  /2018/04/2018_04_22/20180422_161454_test.jpg
			var path = GetExpectedFilePath(storage, appSettings, "/test.jpg");
			await storage.WriteStreamAsync(
				new MemoryStream(FakeCreateAn.CreateAnImage.Bytes), path
			);
			var importService = new Import(new FakeSelectorStorage(storage), appSettings,
				new FakeIImportQuery(null),
				new FakeExifTool(storage, appSettings), new FakeIQuery())
			{
				MaxTryGetDestinationPath = 1
			};

			await importService.Importer(new List<string> {"/test.jpg"},
				new ImportSettingsModel());

			// ExpectedException
		}
		
		[TestMethod]
		public async Task Importer_DuplicateFileName()
		{
			var appSettings = new AppSettings();
			
			var storage =new FakeIStorage();
			// write source file
			await storage.WriteStreamAsync(
				new MemoryStream(FakeCreateAn.CreateAnImage.Bytes), "/test.jpg"
			);
			// write  /2018/04/2018_04_22/20180422_161454_test.jpg
			var path = GetExpectedFilePath(storage, appSettings, "/test.jpg");
			await storage.WriteStreamAsync(
				new MemoryStream(FakeCreateAn.CreateAnImage.Bytes), path
			);

			var importService = new Import(new FakeSelectorStorage(storage), appSettings,
				new FakeIImportQuery(null),
				new FakeExifTool(storage, appSettings), new FakeIQuery());

			var result = await importService.Importer(new List<string> {"/test.jpg"},
				new ImportSettingsModel());
			
			// get something like  /2018/04/2018_04_22/20180422_161454_test_1.jpg
			var expectedFilePath = GetExpectedFilePath(storage, appSettings, "/test.jpg", 1);
			Assert.AreEqual(expectedFilePath,result.FirstOrDefault().FilePath);
		}

		
		[TestMethod]
		public async Task Importer_OverwriteColorClass()
		{
			var appSettings = new AppSettings();
			var query = new FakeIQuery();
			var importService = new Import(new FakeSelectorStorage(_iStorageFake), appSettings, new FakeIImportQuery(null),
				new FakeExifTool(_iStorageFake, appSettings),query);

			var expectedFilePath = GetExpectedFilePath(_iStorageFake, appSettings, "/test.jpg");
			var result = await importService.Importer(new List<string> {"/test.jpg"},
				new ImportSettingsModel{
					ColorClass = 5
				});
			
			Assert.AreEqual(expectedFilePath,result.FirstOrDefault().FilePath);
			var queryResult = query.GetObjectByFilePath(expectedFilePath);
			
			Assert.AreEqual(expectedFilePath,queryResult.FilePath);
			Assert.AreEqual(ColorClassParser.Color.Typical,queryResult.ColorClass);

			_iStorageFake.FileDelete(expectedFilePath);
		}
		
		[TestMethod]
		public async Task Importer_OverwriteStructure_HappyFlow()
		{
			var appSettings = new AppSettings();
			var query = new FakeIQuery();
			var importService = new Import(new FakeSelectorStorage(_iStorageFake), appSettings, new FakeIImportQuery(null),
				new FakeExifTool(_iStorageFake, appSettings),query);
			
			var result = await importService.Importer(new List<string> {"/test.jpg"},
				new ImportSettingsModel{
					Structure = "/yyyy/MM/yyyy_MM_dd*/_yyyyMMdd_HHmmss.ext"
				});
			
			var expectedFilePath = GetExpectedFilePath(_iStorageFake, new AppSettings
			{
				Structure = "/yyyy/MM/yyyy_MM_dd*/_yyyyMMdd_HHmmss.ext"
			}, "/test.jpg");
			
			Assert.AreEqual(expectedFilePath,result.FirstOrDefault().FilePath);
			var queryResult = query.GetObjectByFilePath(expectedFilePath);
			
			Assert.AreEqual(expectedFilePath,queryResult.FilePath);

			_iStorageFake.FileDelete(expectedFilePath);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public async Task Importer_OverwriteStructure_Exception()
		{
			var appSettings = new AppSettings();
			var query = new FakeIQuery();
			var importService = new Import(new FakeSelectorStorage(_iStorageFake), appSettings, new FakeIImportQuery(null),
				new FakeExifTool(_iStorageFake, appSettings),query);
			
			await importService.Importer(new List<string> {"/test.jpg"},
				new ImportSettingsModel{
					Structure = "/.ext"
				});
			// ExpectedException
		}

		
		[TestMethod]
		public async Task Importer_AreParentFoldersCreated_Storage()
		{
			var appSettings = new AppSettings();
			var query = new FakeIQuery();
			var storage = new FakeIStorage(
				new List<string>{"/"},
				new List<string>{"/test.jpg"},
				new List<byte[]>{FakeCreateAn.CreateAnImage.Bytes}
			);
			var importService = new Import(new FakeSelectorStorage(storage), appSettings, new FakeIImportQuery(null),
				new FakeExifTool(storage, appSettings),query);
		
			await importService.Importer(new List<string> {"/test.jpg"},
				new ImportSettingsModel());

			Assert.IsTrue(storage.ExistFolder("/"));
			Assert.IsTrue(storage.ExistFolder("/2018"));
			Assert.IsTrue(storage.ExistFolder("/2018/04"));
			Assert.IsTrue(storage.ExistFolder("/2018/04/2018_04_22"));
		}
		
		[TestMethod]
		public async Task Importer_AreParentFoldersCreated_Database()
		{
			var appSettings = new AppSettings();
			var query = new FakeIQuery();
			var storage = new FakeIStorage(
				new List<string>{"/"},
				new List<string>{"/test.jpg"},
				new List<byte[]>{FakeCreateAn.CreateAnImage.Bytes}
			);
			var importService = new Import(new FakeSelectorStorage(storage), appSettings, new FakeIImportQuery(null),
				new FakeExifTool(storage, appSettings),query);
		
			await importService.Importer(new List<string> {"/test.jpg"},
				new ImportSettingsModel());

			Assert.IsNotNull(query.GetObjectByFilePath("/"));
			Assert.IsNotNull(query.GetObjectByFilePath("/2018"));
			Assert.IsNotNull(query.GetObjectByFilePath("/2018/04"));
			Assert.IsNotNull(query.GetObjectByFilePath("/2018/04/2018_04_22"));
		}

		[TestMethod]
		public async Task Importer_AreParentFoldersCreated_MultipleInputs()
		{
			var appSettings = new AppSettings();
			var query = new FakeIQuery();
			var importQuery = new FakeIImportQuery(null);
			var storage = new FakeIStorage(
				new List<string>{"/"},
				new List<string>{"/test.jpg"},
				new List<byte[]>{FakeCreateAn.CreateAnImage.Bytes}
			);
			
			var importService = new Import(new FakeSelectorStorage(storage), appSettings,importQuery,
				new FakeExifTool(storage, appSettings),query);
			
			var result = await importService.Importer(new List<string> {"/test.jpg"},
				new ImportSettingsModel());
			
			// remove it due we have one example
			await importQuery.RemoveAsync(result.FirstOrDefault().FileHash);
			
			await importService.Importer(new List<string> {"/test.jpg"},
				new ImportSettingsModel());

			var items = storage.GetDirectories("/");
		}

	}
}
