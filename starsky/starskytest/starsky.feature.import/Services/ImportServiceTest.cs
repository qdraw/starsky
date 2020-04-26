using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.import.Services;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starsky.foundation.readmeta.Services;
using starsky.foundation.storage.Services;
using starskycore.Models;
using starskytest.FakeMocks;
using starskytest.Models;

namespace starskytest.starsky.feature.import.Services
{
	[TestClass]
	public class ImportServiceTest
	{
		private readonly FakeIStorage _iStorageFake;
		private readonly string _exampleHash;
		private readonly FakeIStorage _iStorageDirectoryRecursive;

		public ImportServiceTest()
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
			var importService = new Import(new FakeSelectorStorage(_iStorageDirectoryRecursive), appSettings, new FakeIImportQuery(null),
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
			var importService = new Import(new FakeSelectorStorage(_iStorageDirectoryRecursive), appSettings, new FakeIImportQuery(null),
				new FakeExifTool(_iStorageDirectoryRecursive, appSettings), null);
			
			var result = await importService.Preflight(new List<string> {"/test"},
				new ImportSettingsModel{RecursiveDirectory = false});
			
			Assert.IsNotNull(result.FirstOrDefault());
			Assert.AreEqual(1,result.Count);

			Assert.AreEqual(ImportStatus.Ok, result[0].Status);

			// "/layer0.jpg",
			Assert.AreEqual("/test/layer1.jpg", result[0].SourceFullFilePath);
		}

		private string GetExpectedFilePath(AppSettings appSettings, string inputFileFullPath, int index = 0)
		{
			var fileIndexItem = new ReadMeta(_iStorageFake).ReadExifAndXmpFromFile(inputFileFullPath);
			var importIndexItem = new ImportIndexItem(appSettings)
			{
				FileIndexItem = fileIndexItem,
				DateTime = fileIndexItem.DateTime,
				SourceFullFilePath = inputFileFullPath
			};
			
			importIndexItem.FileIndexItem.FileName = importIndexItem.ParseFileName(ExtensionRolesHelper.ImageFormat.jpg,false);
			importIndexItem.FileIndexItem.ParentDirectory = importIndexItem.ParseSubfolders(false);
			return Import.AppendIndexerToFilePath(importIndexItem.FileIndexItem.FilePath, index) ;
		}

		[TestMethod]
		public async Task Importer_ToDefaultFolderStructure_default()
		{
			var appSettings = new AppSettings();
			var importService = new Import(new FakeSelectorStorage(_iStorageFake), appSettings, new FakeIImportQuery(null),
				new FakeExifTool(_iStorageFake, appSettings), new FakeIQuery());

			var expectedFilePath = GetExpectedFilePath(appSettings, "/test.jpg");
			var result = await importService.Importer(new List<string> {"/test.jpg"},
				new ImportSettingsModel());
			
			Assert.AreEqual(expectedFilePath,result.FirstOrDefault().FilePath);
		}

		[TestMethod]
		[ExpectedException(typeof(ApplicationException))]
		public async Task Importer_Over50Times()
		{

			var appSettings = new AppSettings();
			var path = GetExpectedFilePath(appSettings, "/test.jpg");
			await _iStorageFake.WriteStreamAsync(
				new MemoryStream(FakeCreateAn.CreateAnImage.Bytes), path
			);
			
			var importService = new Import(new FakeSelectorStorage(_iStorageFake), appSettings, new FakeIImportQuery(null),
				new FakeExifTool(_iStorageFake, appSettings), new FakeIQuery());
			
			importService.MaxTryGetDestinationPath = 1;
			
			var result = await importService.Importer(new List<string> {"/test.jpg"},
				new ImportSettingsModel());
		}
	}
}
