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
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Services;
using starsky.foundation.readmeta.Services;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Services;
using starskycore.Models;
using starskytest.FakeCreateAn;
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
		private IConsole _console;

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
			
			_console = new FakeConsoleWrapper(new List<string>());
		}

		[TestMethod]
		public async Task Preflight_SingleImage_HappyFlow()
		{
			var appSettings = new AppSettings{Verbose = true};
			var importService = new Import(new FakeSelectorStorage(_iStorageFake), appSettings, new FakeIImportQuery(null),
				new FakeExifTool(_iStorageFake, appSettings), null, _console);

			var result = await importService.Preflight(
				new List<string> {"/test.jpg"},
				new ImportSettingsModel());
			
			Assert.IsNotNull(result.FirstOrDefault());
			Assert.AreEqual(ImportStatus.Ok, result.FirstOrDefault().Status);
			
			Assert.IsNotNull(result.FirstOrDefault().FileIndexItem);
			Assert.IsNotNull(result.FirstOrDefault().FileIndexItem.FilePath);
		}

		[TestMethod]
		public async Task Importer_EmptyDirectory()
		{
			var appSettings = new AppSettings{Verbose = true};
			var storage = new FakeIStorage(new List<string>{"/"});
			var importService = new Import(new FakeSelectorStorage(storage), appSettings, new FakeIImportQuery(null),
				new FakeExifTool(storage, appSettings), null, _console);

			var result = await importService.Importer(
				new List<string> {"/"},
				new ImportSettingsModel());

			Assert.IsNotNull(result);
			Assert.IsTrue(!result.Any());
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
				new FakeExifTool(_iStorageFake, appSettings), null, _console);
			
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
			var appSettings = new AppSettings{Verbose = true};
			var storage = new FakeIStorage(
				new List<string>{"/"},
				new List<string>{"/test.jpg"},
				new List<byte[]>{new byte[0]}
			);
			var importService = new Import(new FakeSelectorStorage(storage), appSettings, 
				new FakeIImportQuery(null),
				new FakeExifTool(_iStorageFake, appSettings), null, _console);
			
			var result = await importService.Preflight(
				new List<string> {"/test.jpg"},
				new ImportSettingsModel());
			
			Assert.IsNotNull(result.FirstOrDefault());
			Assert.AreEqual(ImportStatus.FileError, result.FirstOrDefault().Status);
		}
		
		[TestMethod]
		public async Task Preflight_SingleImage_WrongExtension()
		{
			var appSettings = new AppSettings{Verbose = true};
			
			var storage = new FakeIStorage(
				new List<string>{"/"},
				new List<string>{"/test.unknown"},
				new List<byte[]>{FakeCreateAn.CreateAnImage.Bytes}
			);
			
			var importService = new Import(new FakeSelectorStorage(storage), 
				appSettings,
				new FakeIImportQuery(null),
				new FakeExifTool(storage, appSettings), null, _console);

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

			new FakeExifTool(_iStorageFake, appSettings), null, _console);

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
				new FakeExifTool(_iStorageFake, appSettings), null, _console);

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
				new FakeExifTool(_iStorageFake, appSettings), null, _console);
			
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
				new FakeExifTool(_iStorageFake, appSettings), null, _console);
			
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
				new FakeExifTool(_iStorageDirectoryRecursive, appSettings), null,  _console);
			
			var importIndexItems = await importService.Preflight(
				new List<string> {"/"},
				new ImportSettingsModel
				{
					RecursiveDirectory = true, 
					IndexMode = false
				});
			
			Assert.IsNotNull(importIndexItems.FirstOrDefault());

			foreach ( var item in importIndexItems )
			{
				Console.WriteLine(item.FilePath);
			}
			
			Assert.AreEqual(3,importIndexItems.Count);

			Assert.AreEqual(ImportStatus.Ok, importIndexItems[0].Status);
			Assert.AreEqual(ImportStatus.Ok, importIndexItems[1].Status);
			Assert.AreEqual(ImportStatus.Ok, importIndexItems[2].Status);

			// "/layer0.jpg","/test/layer1.jpg", "/test/test/layer2.jpg" (order is random)
			Assert.IsTrue(importIndexItems.Any(p => p.SourceFullFilePath == "/layer0.jpg"));
			Assert.IsTrue(importIndexItems.Any(p => p.SourceFullFilePath == "/test/layer1.jpg"));
			Assert.IsTrue(importIndexItems.Any(p => p.SourceFullFilePath == "/test/test/layer2.jpg"));
		}
		
		[TestMethod]
		public async Task Preflight_DirectoryNonRecursive()
		{
			var appSettings = new AppSettings();
			var importService = new Import(new FakeSelectorStorage(_iStorageDirectoryRecursive), appSettings, 
			new FakeIImportQuery(null),
				new FakeExifTool(_iStorageDirectoryRecursive, appSettings), null,  _console);
			
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
				FilenamesHelper.GetFileExtensionWithoutDot(fileIndexItem.FileName));
			importIndexItem.FileIndexItem.FileName = structureService.ParseFileName(
				fileIndexItem.DateTime, fileIndexItem.FileCollectionName,
				FilenamesHelper.GetFileExtensionWithoutDot(fileIndexItem.FileName));
			
			var result = Import.AppendIndexerToFilePath(
				importIndexItem.FileIndexItem.ParentDirectory,
				importIndexItem.FileIndexItem.FileName,
				index);
			return result;
		}

		[TestMethod]
		public async Task Importer_ToDefaultFolderStructure_default_HappyFlow()
		{
			var appSettings = new AppSettings();
			var query = new FakeIQuery();
			var importService = new Import(new FakeSelectorStorage(_iStorageFake), appSettings, new FakeIImportQuery(null),
				new FakeExifTool(_iStorageFake, appSettings),query,_console);

			var expectedFilePath = GetExpectedFilePath(_iStorageFake, appSettings, "/test.jpg");
			var result = await importService.Importer(new List<string> {"/test.jpg"},
				new ImportSettingsModel());
			
			Assert.AreEqual(expectedFilePath,result.FirstOrDefault().FilePath);
			Assert.AreEqual(expectedFilePath,query.GetObjectByFilePath(expectedFilePath).FilePath);

			_iStorageFake.FileDelete(expectedFilePath);
		}
		
		[TestMethod]
		public async Task Importer_DeleteAfter()
		{
			var appSettings = new AppSettings{Verbose = true};
			var query = new FakeIQuery();
			var storage = new FakeIStorage(
				new List<string>{"/"}, 
				new List<string>{"/test.jpg"},
				new List<byte[]>{FakeCreateAn.CreateAnImage.Bytes});
			
			var importService = new Import(new FakeSelectorStorage(storage), appSettings, new FakeIImportQuery(null),
				new FakeExifTool(storage, appSettings),query,_console);

			var result = await importService.Importer(new List<string> {"/test.jpg"},
				new ImportSettingsModel{DeleteAfter = true});
			
			Assert.AreEqual(ImportStatus.Ok, result.FirstOrDefault().Status);			
			Assert.IsFalse(storage.ExistFile("/test.jpg"));			
		}

		[TestMethod]
		public async Task Importer_Gpx()
		{
			var appSettings = new AppSettings{Verbose = true};
			var query = new FakeIQuery();
			var storage = new FakeIStorage(
				new List<string>{"/"}, 
				new List<string>{"/test.gpx"},
				new List<byte[]>{CreateAnGpx.Bytes});
			
			var importService = new Import(new FakeSelectorStorage(storage), appSettings, new FakeIImportQuery(null),
				new FakeExifTool(storage, appSettings),query,_console);

			var result = await importService.Importer(new List<string> {"/test.gpx"},
				new ImportSettingsModel());
			
			var expectedFilePath = GetExpectedFilePath(storage, appSettings, "/test.gpx");
			Assert.AreEqual(expectedFilePath,query.GetObjectByFilePath(expectedFilePath).FilePath);
			Assert.AreEqual(ImportStatus.Ok, result.FirstOrDefault().Status);			
		}
		
		[TestMethod]
		[ExpectedException(typeof(IndexOutOfRangeException))]
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
				new FakeExifTool(storage, appSettings), new FakeIQuery(), _console)
			{
				MaxTryGetDestinationPath = 0
			};

			var result = await importService.Importer(new List<string> {"/test.jpg"},
				new ImportSettingsModel());
			// System.ApplicationException
			Assert.AreEqual(ImportStatus.FileError,result.FirstOrDefault().Status);
		}
		
		[TestMethod]
		public async Task Importer_DuplicateFileName()
		{
			var appSettings = new AppSettings();
			
			var storage = new FakeIStorage(
				new List<string>{"/", "/2018", "/2018/04","/2018/04/2018_04_22"}, 
				new List<string>{"/test.jpg","/2018/04/2018_04_22/20180422_161454_test.jpg"},
				new List<byte[]>{FakeCreateAn.CreateAnImage.Bytes, new byte[0]});

			var importService = new Import(new FakeSelectorStorage(storage), appSettings,
				new FakeIImportQuery(null),
				new FakeExifTool(storage, appSettings), new FakeIQuery(), _console);

			var result = await importService.Importer(new List<string> {"/test.jpg"},
				new ImportSettingsModel());
			
			Assert.AreEqual(ImportStatus.Ok,result.FirstOrDefault().Status);

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
				new FakeExifTool(_iStorageFake, appSettings),query, _console);

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
		public async Task Importer_CheckIfAddToDatabaseTime()
		{
			var appSettings = new AppSettings();
			var query = new FakeIQuery();
			var importService = new Import(new FakeSelectorStorage(_iStorageFake), appSettings, new FakeIImportQuery(null),
				new FakeExifTool(_iStorageFake, appSettings),query, _console);
			
			var result = await importService.Importer(new List<string> {"/test.jpg"},
				new ImportSettingsModel());
			
			// AddToDatabase is Used by the importer History agent

			Assert.IsTrue(result.FirstOrDefault().FileIndexItem.AddToDatabase >= DateTime.UtcNow.AddMinutes(-10));
			Assert.IsTrue(result.FirstOrDefault().AddToDatabase >= DateTime.UtcNow.AddMinutes(-10));
		}

		[TestMethod]
		public async Task Importer_OverwriteStructure_HappyFlow()
		{
			var appSettings = new AppSettings();
			var query = new FakeIQuery();
			var importService = new Import(new FakeSelectorStorage(_iStorageFake), appSettings, new FakeIImportQuery(null),
				new FakeExifTool(_iStorageFake, appSettings),query, _console);
			
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
				new FakeExifTool(_iStorageFake, appSettings),query, _console);
			
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
				new FakeExifTool(storage, appSettings),query, _console);
		
			await importService.Importer(new List<string> {"/test.jpg"},
				new ImportSettingsModel());

			Assert.IsTrue(storage.ExistFolder("/"));
			Assert.IsTrue(storage.ExistFolder("/2018"));
			Assert.IsTrue(storage.ExistFolder("/2018/04"));
			Assert.IsTrue(storage.ExistFolder("/2018/04/2018_04_22"));
		}
		
		[TestMethod]
		public async Task Importer_AreParentFoldersCreated_Home_Database()
		{
			var appSettings = new AppSettings();
			var query = new FakeIQuery();
			var storage = new FakeIStorage(
				new List<string>{"/"},
				new List<string>{"/test.jpg"},
				new List<byte[]>{FakeCreateAn.CreateAnImage.Bytes}
			);
			var importService = new Import(new FakeSelectorStorage(storage), appSettings, new FakeIImportQuery(null),
				new FakeExifTool(storage, appSettings),query, _console);
		
			await importService.Importer(new List<string> {"/test.jpg"},
				new ImportSettingsModel());

			// Home is created at first
			Assert.IsNotNull(query.GetObjectByFilePath("/"));
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
				new FakeExifTool(storage, appSettings),query, _console);
		
			await importService.Importer(new List<string> {"/test.jpg"},
				new ImportSettingsModel());

			// Home is not created in the loop
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
				new FakeExifTool(storage, appSettings),query, _console);
			
			var result = await importService.Importer(new List<string> {"/test.jpg"},
				new ImportSettingsModel());
			
			// remove it due we have one example
			await importQuery.RemoveAsync(result.FirstOrDefault().FileHash);
			
			await importService.Importer(new List<string> {"/test.jpg"},
				new ImportSettingsModel());

			var items = storage.GetDirectories("/");
		}


		[TestMethod]
		public void Preflight_Predict_Duplicates()
		{
			var appSettings = new AppSettings
			{
				Structure = "/yyyy/yyyyMMdd_HHmmss_\\d.ext"
			};
			var query = new FakeIQuery();
			var importQuery = new FakeIImportQuery(null);
			var storage = new FakeIStorage(
				new List<string>{"/","/0001","/2020"},
				new List<string>{"/test.jpg","/0001/00010101_000000_d.png", 
					"/0001/00010101_000000_d_2.png", "/2020/20200501_120000_1.png"},
				new List<byte[]>{new byte[0], new byte[0], new byte[0], new byte[0]}
			);
			var importService = new Import(new FakeSelectorStorage(storage), appSettings,importQuery,
				new FakeExifTool(storage, appSettings),query, _console);
			
			var duplicatesExampleList = new List<ImportIndexItem>
			{
				new ImportIndexItem
				{
					FilePath = "/0001/00010101_000000_d.png",
					Status = ImportStatus.Ok,
					FileIndexItem = new FileIndexItem("/0001/00010101_000000_d.png")
				},
				new ImportIndexItem
				{
					Status = ImportStatus.Ok,
					FilePath = "/0001/00010101_000000_d.png",
					FileIndexItem = new FileIndexItem("/0001/00010101_000000_d.png")
				},
				new ImportIndexItem
				{
					Status = ImportStatus.Ok,
					FilePath = "/2020/20200501_120000_d.png",
					FileIndexItem = new FileIndexItem("/2020/20200501_120000_d.png")
				},
				new ImportIndexItem
				{
					Status = ImportStatus.Ok,
					FilePath = "/2020/20200501_120000_d.png",
					FileIndexItem = new FileIndexItem("/2020/20200501_120000_d.png")
				}
			};

			var directoriesContent = importService.ParentFoldersDictionary(duplicatesExampleList);
			var result = importService.CheckForDuplicateNaming(duplicatesExampleList,directoriesContent);

			var fileIndexItemFilePathList = result.Select(x => x.FileIndexItem.FilePath).ToList();
			Assert.AreEqual(4,fileIndexItemFilePathList.Count);
			Assert.AreEqual("/0001/00010101_000000_d_1.png", fileIndexItemFilePathList[0]);
			Assert.AreEqual("/0001/00010101_000000_d_3.png", fileIndexItemFilePathList[1]);
			Assert.AreEqual("/2020/20200501_120000_d.png", fileIndexItemFilePathList[2]);
			Assert.AreEqual("/2020/20200501_120000_d_1.png", fileIndexItemFilePathList[3]);
		}

	}
}
