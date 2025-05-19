using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.import.Models;
using starsky.feature.import.Services;
using starsky.foundation.database.Models;
using starsky.foundation.geo.ReverseGeoCode;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Services;
using starsky.foundation.readmeta.Services;
using starsky.foundation.storage.Helpers;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Models;
using starsky.foundation.storage.Services;
using starsky.foundation.storage.Storage;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.import.Services;

/// <summary>
///     ImportTest.cs / ImportServiceTest
/// </summary>
[TestClass]
public sealed class ImportTest
{
	private readonly IConsole _console;
	private readonly string _exampleHash;
	private readonly FakeIStorage _iStorageDirectoryRecursive;
	private readonly FakeIStorage _iStorageFake;

	/// <summary>
	///     Also known as ImportServiceTest (Also check the InMemoryDb version)
	/// </summary>
	public ImportTest()
	{
		_iStorageFake = new FakeIStorage(
			new List<string> { "/" },
			new List<string> { "/test.jpg", "/color_class_winner.jpg" },
			new List<byte[]>
			{
				CreateAnImage.Bytes.ToArray(), CreateAnImageColorClass.Bytes.ToArray()
			}
		);
		_exampleHash = new FileHash(_iStorageFake, new FakeIWebLogger()).GetHashCode("/test.jpg")
			.Key;

		_iStorageDirectoryRecursive = new FakeIStorage(
			new List<string> { "/", "/test", "/test/test" },
			new List<string> { "/layer0.jpg", "/test/layer1.jpg", "/test/test/layer2.jpg" },
			new List<byte[]>
			{
				CreateAnImage.Bytes.ToArray(),
				CreateAnImage.Bytes.ToArray(),
				CreateAnImage.Bytes.ToArray()
			}
		);

		_console = new FakeConsoleWrapper(new List<string>());
	}

	[TestMethod]
	public async Task Preflight_SingleImage_HappyFlow()
	{
		var appSettings = new AppSettings { Verbose = true };
		var importService = new Import(new FakeSelectorStorage(_iStorageFake), appSettings,
			new FakeIImportQuery(),
			new FakeExifTool(_iStorageFake, appSettings), null!, _console,
			new FakeIMetaExifThumbnailService(), new FakeIWebLogger(),
			new FakeIThumbnailQuery(), new FakeIReverseGeoCodeService(), new FakeMemoryCache());

		var result = await importService.Preflight(
			new List<string> { "/test.jpg" },
			new ImportSettingsModel());

		Assert.IsNotNull(result.FirstOrDefault());
		Assert.AreEqual(ImportStatus.Ok, result.FirstOrDefault()?.Status);

		Assert.IsNotNull(result.FirstOrDefault()?.FileIndexItem);
		Assert.IsNotNull(result.FirstOrDefault()?.FileIndexItem?.FilePath);
		Assert.AreNotEqual(0, result.FirstOrDefault()?.FileIndexItem?.Size);
	}

	[TestMethod]
	public async Task Preflight_SingleImage_Ignore()
	{
		var appSettings = new AppSettings
		{
			Verbose = true, ImportIgnore = new List<string> { "test" }
		};
		var importService = new Import(new FakeSelectorStorage(_iStorageFake), appSettings,
			new FakeIImportQuery(),
			new FakeExifTool(_iStorageFake, appSettings), null!,
			_console, new FakeIMetaExifThumbnailService(), new FakeIWebLogger(),
			new FakeIThumbnailQuery(), new FakeIReverseGeoCodeService(), new FakeMemoryCache());

		var result = await importService.Preflight(
			new List<string> { "/test.jpg" },
			new ImportSettingsModel());

		Assert.IsNotNull(result.FirstOrDefault());
		Assert.AreEqual(ImportStatus.Ignore, result.FirstOrDefault()?.Status);

		Assert.IsNull(result.FirstOrDefault()?.FileIndexItem);
	}

	[TestMethod]
	public async Task Preflight_SingleImage_Ignore_ColorClassOverwrite()
	{
		var appSettings = new AppSettings { Verbose = true };
		var importService = new Import(new FakeSelectorStorage(_iStorageFake), appSettings,
			new FakeIImportQuery(),
			new FakeExifTool(_iStorageFake, appSettings), null!,
			_console, new FakeIMetaExifThumbnailService(), new FakeIWebLogger(),
			new FakeIThumbnailQuery(), new FakeIReverseGeoCodeService(), new FakeMemoryCache());

		var result = await importService.Preflight(
			new List<string> { "/color_class_winner.jpg" },
			new ImportSettingsModel());

		Assert.IsNotNull(result.FirstOrDefault());
		Assert.AreEqual(ImportStatus.Ok, result.FirstOrDefault()?.Status);

		Assert.IsNotNull(result.FirstOrDefault()?.FileIndexItem);
		Assert.IsNotNull(result.FirstOrDefault()?.FileIndexItem?.FilePath);
		Assert.AreNotEqual(0, result.FirstOrDefault()?.FileIndexItem?.Size);
		Assert.AreEqual(ColorClassParser.Color.Winner,
			result.FirstOrDefault()?.FileIndexItem?.ColorClass);
	}

	[TestMethod]
	public async Task Preflight_SingleImage_ForceOverWrite_ColorClassOverwrite()
	{
		var appSettings = new AppSettings { Verbose = true };
		var importService = new Import(new FakeSelectorStorage(_iStorageFake), appSettings,
			new FakeIImportQuery(),
			new FakeExifTool(_iStorageFake, appSettings),
			null!, _console, new FakeIMetaExifThumbnailService(),
			new FakeIWebLogger(), new FakeIThumbnailQuery(), new FakeIReverseGeoCodeService(),
			new FakeMemoryCache());

		var result = await importService.Preflight(
			new List<string> { "/color_class_winner.jpg" }, // <- in this test we change it
			new ImportSettingsModel
			{
				ColorClass = 5 // <- - - - - - - - - - - - -
			});

		Assert.IsNotNull(result.FirstOrDefault());
		Assert.AreEqual(ImportStatus.Ok, result.FirstOrDefault()?.Status);

		Assert.IsNotNull(result.FirstOrDefault()?.FileIndexItem);
		Assert.IsNotNull(result.FirstOrDefault()?.FileIndexItem?.FilePath);
		Assert.AreNotEqual(0, result.FirstOrDefault()?.FileIndexItem?.Size);
		Assert.AreEqual(ColorClassParser.Color.Typical,
			result.FirstOrDefault()?.FileIndexItem?.ColorClass);
	}

	[TestMethod]
	public async Task Preflight_SingleImage_DateGetByFileNameNoExif()
	{
		var appSettings = new AppSettings();
		var storage = new FakeIStorage(
			new List<string> { "/" },
			new List<string> { "/2020-04-27 11:07:00.jpg" },
			new List<byte[]> { CreateAnImageNoExif.Bytes.ToArray() }
		);
		var importService = new Import(new FakeSelectorStorage(storage), appSettings,
			new FakeIImportQuery(),
			new FakeExifTool(_iStorageFake, appSettings), null!, _console,
			new FakeIMetaExifThumbnailService(), new FakeIWebLogger(),
			new FakeIThumbnailQuery(), new FakeIReverseGeoCodeService(), new FakeMemoryCache());

		var result = await importService.Preflight(
			new List<string> { "/2020-04-27 11:07:00.jpg" },
			new ImportSettingsModel());

		Assert.IsNotNull(result.FirstOrDefault());
		Assert.AreEqual(new DateTime(2020, 04, 27, 11, 07, 00, DateTimeKind.Local),
			result.FirstOrDefault()?.DateTime);

		Assert.AreEqual(ObjectCreateIndexItemService.MessageDateTimeBasedOnFilename,
			result.FirstOrDefault()?.FileIndexItem?.Description);
	}

	[TestMethod]
	public async Task Preflight_SingleImage_FileType_NotSupported()
	{
		var appSettings = new AppSettings { Verbose = true };
		var storage = new FakeIStorage(
			new List<string> { "/" },
			new List<string> { "/test.jpg" },
			new List<byte[]> { Array.Empty<byte>() }
		);
		var importService = new Import(new FakeSelectorStorage(storage), appSettings,
			new FakeIImportQuery(),
			new FakeExifTool(_iStorageFake, appSettings), null!, _console,
			new FakeIMetaExifThumbnailService(), new FakeIWebLogger(),
			new FakeIThumbnailQuery(), new FakeIReverseGeoCodeService(), new FakeMemoryCache());

		var result = await importService.Preflight(
			new List<string> { "/test.jpg" },
			new ImportSettingsModel());

		Assert.IsNotNull(result.FirstOrDefault());
		Assert.AreEqual(ImportStatus.FileError, result.FirstOrDefault()?.Status);
	}

	[TestMethod]
	public async Task Preflight_SingleImage_WrongExtension()
	{
		var appSettings = new AppSettings { Verbose = true };

		var storage = new FakeIStorage(
			new List<string> { "/" },
			new List<string> { "/test.unknown" },
			new List<byte[]> { CreateAnImage.Bytes.ToArray() }
		);

		var importService = new Import(new FakeSelectorStorage(storage),
			appSettings,
			new FakeIImportQuery(),
			new FakeExifTool(storage, appSettings), null!,
			_console, new FakeIMetaExifThumbnailService(), new WebLogger(),
			new FakeIThumbnailQuery(), new FakeIReverseGeoCodeService(), new FakeMemoryCache());

		var result = await importService.Preflight(
			new List<string> { "/test.unknown" },
			new ImportSettingsModel());

		Assert.IsNotNull(result.FirstOrDefault());
		Assert.AreEqual(ImportStatus.FileError, result.FirstOrDefault()?.Status);
	}

	[TestMethod]
	public async Task Preflight_SingleImage_HashAlreadyInImportDb()
	{
		// Exist already
		var appSettings = new AppSettings { Verbose = true };
		var importService = new Import(new FakeSelectorStorage(_iStorageFake), appSettings,
			new FakeIImportQuery(new List<string> { _exampleHash }),
			new FakeExifTool(_iStorageFake, appSettings), null!, _console,
			new FakeIMetaExifThumbnailService(), new FakeIWebLogger(),
			new FakeIThumbnailQuery(), new FakeIReverseGeoCodeService(), new FakeMemoryCache());

		var result = await importService.Preflight(new List<string> { "/test.jpg" },
			new ImportSettingsModel());

		Assert.IsNotNull(result.FirstOrDefault());
		Assert.AreEqual(ImportStatus.IgnoredAlreadyImported, result.FirstOrDefault()?.Status);
	}

	[TestMethod]
	public async Task Preflight_SingleImage_Ignore_HashAlreadyInImportDb()
	{
		var appSettings = new AppSettings();
		var importService = new Import(new FakeSelectorStorage(_iStorageFake), appSettings,
			new FakeIImportQuery(new List<string> { _exampleHash }),
			new FakeExifTool(_iStorageFake, appSettings), null!, _console,
			new FakeIMetaExifThumbnailService(), new FakeIWebLogger(),
			new FakeIThumbnailQuery(), new FakeIReverseGeoCodeService(), new FakeMemoryCache());

		var result = await importService.Preflight(new List<string> { "/test.jpg" },
			new ImportSettingsModel { IndexMode = false });

		Assert.IsNotNull(result.FirstOrDefault());
		Assert.AreEqual(ImportStatus.Ok, result.FirstOrDefault()?.Status);
	}

	[TestMethod]
	public async Task Preflight_SingleImage_NonExist()
	{
		var appSettings = new AppSettings { Verbose = true };
		var importService = new Import(new FakeSelectorStorage(_iStorageFake),
			appSettings, new FakeIImportQuery(),
			new FakeExifTool(_iStorageFake, appSettings), null!, _console,
			new FakeIMetaExifThumbnailService(), new FakeIWebLogger(),
			new FakeIThumbnailQuery(), new FakeIReverseGeoCodeService(), new FakeMemoryCache());

		var result = await importService.Preflight(new List<string> { "/non-exist.jpg" },
			new ImportSettingsModel());

		Assert.IsNotNull(result.FirstOrDefault());
		Assert.AreEqual(ImportStatus.NotFound, result.FirstOrDefault()?.Status);
	}

	[TestMethod]
	public async Task Preflight_SingleImage_NonExistDirectory()
	{
		var appSettings = new AppSettings();
		var importService = new Import(new FakeSelectorStorage(_iStorageFake),
			appSettings, new FakeIImportQuery(),
			new FakeExifTool(_iStorageFake, appSettings), null!, _console,
			new FakeIMetaExifThumbnailService(), new FakeIWebLogger(),
			new FakeIThumbnailQuery(), new FakeIReverseGeoCodeService(), new FakeMemoryCache());

		var result = await importService.Preflight(new List<string> { "/non-exist" },
			new ImportSettingsModel());

		Assert.IsNotNull(result.FirstOrDefault());
		Assert.AreEqual(ImportStatus.NotFound, result.FirstOrDefault()?.Status);
	}

	[TestMethod]
	public async Task Preflight_DirectoryRecursive()
	{
		var appSettings = new AppSettings();
		var importService = new Import(
			new FakeSelectorStorage(_iStorageDirectoryRecursive),
			appSettings, new FakeIImportQuery(),
			new FakeExifTool(_iStorageDirectoryRecursive, appSettings),
			null!, _console, new FakeIMetaExifThumbnailService(), new FakeIWebLogger(),
			new FakeIThumbnailQuery(), new FakeIReverseGeoCodeService(), new FakeMemoryCache());

		var importIndexItems = await importService.Preflight(
			new List<string> { "/" },
			new ImportSettingsModel { RecursiveDirectory = true, IndexMode = false });

		Assert.IsNotNull(importIndexItems.FirstOrDefault());

		foreach ( var item in importIndexItems )
		{
			Console.WriteLine("import ~ " + item.FilePath);
		}

		Assert.AreEqual(3, importIndexItems.Count);

		Assert.AreEqual(ImportStatus.Ok, importIndexItems[0].Status);
		Assert.AreEqual(ImportStatus.Ok, importIndexItems[1].Status);
		Assert.AreEqual(ImportStatus.Ok, importIndexItems[2].Status);

		// "/layer0.jpg","/test/layer1.jpg", "/test/test/layer2.jpg" (order is random)
		Assert.IsTrue(importIndexItems.Exists(p => p.SourceFullFilePath == "/layer0.jpg"));
		Assert.IsTrue(importIndexItems.Exists(p => p.SourceFullFilePath == "/test/layer1.jpg"));
		Assert.IsTrue(importIndexItems.Exists(p =>
			p.SourceFullFilePath == "/test/test/layer2.jpg"));
	}

	[TestMethod]
	public async Task Preflight_DirectoryNonRecursive()
	{
		var appSettings = new AppSettings();
		var importService = new Import(new FakeSelectorStorage(_iStorageDirectoryRecursive),
			appSettings,
			new FakeIImportQuery(),
			new FakeExifTool(_iStorageDirectoryRecursive, appSettings),
			null!, _console, new FakeIMetaExifThumbnailService(), new FakeIWebLogger(),
			new FakeIThumbnailQuery(), new FakeIReverseGeoCodeService(), new FakeMemoryCache());

		var result = await importService.Preflight(new List<string> { "/test" },
			new ImportSettingsModel { RecursiveDirectory = false });

		Assert.IsNotNull(result.FirstOrDefault());
		Assert.AreEqual(1, result.Count);

		Assert.AreEqual(ImportStatus.Ok, result[0].Status);

		// "/layer0.jpg",
		Assert.AreEqual("/test/layer1.jpg", result[0].SourceFullFilePath);
	}

	[TestMethod]
	public void AppendIndexerToFilePath_default()
	{
		var result = Import.AppendIndexerToFilePath("/test/", "test.jpg", 0);
		Assert.AreEqual("/test/test.jpg", result);
	}

	[TestMethod]
	public void AppendIndexerToFilePath_minus10()
	{
		var result = Import.AppendIndexerToFilePath("/test/", "test.jpg", -10);
		Assert.AreEqual("/test/test.jpg", result);
	}

	[TestMethod]
	public void AppendIndexerToFilePath_5()
	{
		var result = Import.AppendIndexerToFilePath("/test", "test.jpg", 5);
		Assert.AreEqual("/test/test_5.jpg", result);
	}

	/// <summary>
	///     Helper to get the expected file path
	/// </summary>
	/// <param name="storage">use this storage provider</param>
	/// <param name="appSettings">structure config</param>
	/// <param name="inputFileFullPath">subPath style </param>
	/// <param name="index">number</param>
	/// <returns>expected result</returns>
	public static async Task<string> GetExpectedFilePathAsync(IStorage storage,
		AppSettings appSettings, string inputFileFullPath, int index = 0)
	{
		var fileIndexItem = await new ReadMeta(storage, appSettings,
			null, new FakeIWebLogger()).ReadExifAndXmpFromFileAsync(inputFileFullPath);
		var importIndexItem = new ImportIndexItem(appSettings)
		{
			FileIndexItem = fileIndexItem,
			DateTime = fileIndexItem!.DateTime,
			SourceFullFilePath = inputFileFullPath
		};

		var structureService = new StructureService(storage, appSettings.Structure);
		importIndexItem.FileIndexItem!.ParentDirectory = structureService.ParseSubfolders(
			fileIndexItem.DateTime, fileIndexItem.FileCollectionName!,
			FilenamesHelper.GetFileExtensionWithoutDot(fileIndexItem.FileName!));
		importIndexItem.FileIndexItem.FileName = structureService.ParseFileName(
			fileIndexItem.DateTime, fileIndexItem.FileCollectionName!,
			FilenamesHelper.GetFileExtensionWithoutDot(fileIndexItem.FileName!));

		var result = Import.AppendIndexerToFilePath(
			importIndexItem.FileIndexItem.ParentDirectory!,
			importIndexItem.FileIndexItem.FileName!,
			index);
		return result;
	}

	[TestMethod]
	public async Task Importer_DeleteAfter()
	{
		var appSettings = new AppSettings { Verbose = true };
		var query = new FakeIQuery();
		var storage = new FakeIStorage(
			["/"],
			["/test.jpg"],
			new List<byte[]> { CreateAnImage.Bytes.ToArray() });

		var importService = new Import(new FakeSelectorStorage(storage), appSettings,
			new FakeIImportQuery(),
			new FakeExifTool(storage, appSettings), query, _console,
			new FakeIMetaExifThumbnailService(), new FakeIWebLogger(),
			new FakeIThumbnailQuery(), new FakeIReverseGeoCodeService(), new FakeMemoryCache());

		var result = await importService.Importer(new List<string> { "/test.jpg" },
			new ImportSettingsModel { DeleteAfter = true });

		Assert.AreEqual(ImportStatus.Ok, result.FirstOrDefault()?.Status);
		Assert.IsFalse(storage.ExistFile("/test.jpg"));
	}

	private static AppSettings SetupReverseGeoCodeServiceData()
	{
		var appSettings = new AppSettings
		{
			DependenciesFolder =
				Path.Combine(new CreateAnImage().BasePath, "tmp-dependencies-import-test")
		};


		// create a temp folder
		if ( !new StorageHostFullPathFilesystem(new FakeIWebLogger()).ExistFolder(appSettings
			    .DependenciesFolder) )
		{
			new StorageHostFullPathFilesystem(new FakeIWebLogger()).CreateDirectory(appSettings
				.DependenciesFolder);
		}

		// We mock the data to avoid http request during tests

		// Mockup data to: 
		// map the city
		const string mockCities1000 =
			"2747351\t\'s-Hertogenbosch\t\'s-Hertogenbosch\t\'s Bosch,\'s-Hertogenbosch,Bois-le-Duc,Bolduque,Boscoducale,De Bosk,Den Bosch,Hertogenbosch,Herzogenbusch,Khertogenbos,Oeteldonk,Silva Ducis,Хертогенбос,’s-Hertogenbosch\t51.69917\t5.30417\tP\tPPLA\tNL\t\t06\t0796\t\t\t134520\t\t7\tEurope/Amsterdam\t2017-10-17\r\n" +
			"6693230\tVilla Santa Rita\tVilla Santa Rita\t\t-34.61082\t-58.481\tP\tPPLX\tAR\t\t07\t02011\t\t\t34000\t\t25\tAmerica/Argentina/Buenos_Aires\t2017-05-08\r\n" +
			"3713678\tBuenos Aires\tBuenos Aires\tBuenos Aires\t8.63146\t-79.94775\tP\tPPLA3\tPA\t\t13\t\t\t\t496\t\t232\tAmerica/Panama\t2017-08-16\r\n" +
			"3713682\tBuenos Aires\tBuenos Aires\tBuenos Aires\t8.41384\t-81.4844\tP\tPPLA2\tPA\t\t12\t\t\t\t400\t\t336\tAmerica/Panama\t2017-08-16\r\n" +
			"6691831\tVatican City\tVatican City\tCitta del Vaticano,Città del Vaticano,Ciudad del Vaticano,Etat de la Cite du Vatican,Staat Vatikanstadt,Staat der Vatikanstadt,Vatican,Vatican City,Vatican City State,Vaticano,Vatikan,Vatikanas,Vatikanstaden,Vatikanstadt,batikan,batikan si,État de la Cité du Vatican,Ватикан,바티칸,바티칸 시\t41.90268\t12.45414\tP\tPPLC\tVA\tIT\t\t\t\t\t829\t55\t61\tEurope/Vatican\t2018-08-17\n" +
			"2747704\tSchalkhaar1\tSchalkhaar\tSchalkhaap\t52.26833\t6.19444\tP\tPPL\tNL\t\t15\t0150\t\t\t4610\t\t9\tEurope/Amsterdam\t2017-10-17\n";

		new StorageHostFullPathFilesystem(new FakeIWebLogger()).WriteStream(
			StringToStreamHelper.StringToStream(mockCities1000),
			Path.Combine(appSettings.DependenciesFolder, "cities1000.txt"));

		// Mockup data to:
		// map the state and country

		const string admin1CodesAscii = "NL.07\tNorth Holland\tNorth Holland\t2749879\r\n" +
		                                "NL.06\tNorth Brabant\tNorth Brabant\t2749990\r\n" +
		                                "NL.05\tLimburg\tLimburg\t2751596\r\n" +
		                                "NL.03\tGelderland\tGelderland\t2755634\r\n" +
		                                "AR.07\tBuenos Aires F.D.\tBuenos Aires F.D.\t3433955\r\n";

		new StorageHostFullPathFilesystem(new FakeIWebLogger()).WriteStream(
			StringToStreamHelper.StringToStream(admin1CodesAscii),
			Path.Combine(appSettings.DependenciesFolder, "admin1CodesASCII.txt"));
		return appSettings;
	}

	[TestMethod]
	public async Task Importer_ReverseGeoCode()
	{
		var appSettings = SetupReverseGeoCodeServiceData();
		var query = new FakeIQuery();
		var storage = new FakeIStorage(
			["/"],
			["/test.jpg"],
			new List<byte[]> { CreateAnImage.Bytes.ToArray() });

		var importService = new Import(new FakeSelectorStorage(storage), appSettings,
			new FakeIImportQuery(),
			new FakeExifTool(storage, appSettings), query, _console,
			new FakeIMetaExifThumbnailService(), new FakeIWebLogger(),
			new FakeIThumbnailQuery(),
			new ReverseGeoCodeService(appSettings, new FakeIGeoFileDownload(),
				new FakeIWebLogger()), new FakeMemoryCache());

		var result = await importService.Importer(new List<string> { "/test.jpg" },
			new ImportSettingsModel { ReverseGeoCode = true });

		// Expect
		Assert.AreEqual(ImportStatus.Ok, result.FirstOrDefault()?.Status);
		var fileIndexItem = result.FirstOrDefault()?.FileIndexItem;
		Assert.IsTrue(storage.ExistFile("/test.jpg"));
		Assert.IsNotNull(fileIndexItem);
		Assert.AreEqual("Schalkhaar", fileIndexItem.LocationCity);
		Assert.AreEqual("NLD", fileIndexItem.LocationCountryCode);
		Assert.AreEqual("Netherlands", fileIndexItem.LocationCountry);
	}

	[TestMethod]
	public async Task Importer_EmptyDirectory()
	{
		var appSettings = new AppSettings { Verbose = true };
		var storage = new FakeIStorage(["/"]);
		var importService = new Import(new FakeSelectorStorage(storage), appSettings,
			new FakeIImportQuery(),
			new FakeExifTool(storage, appSettings), null!, _console,
			new FakeIMetaExifThumbnailService(), new FakeIWebLogger(),
			new FakeIThumbnailQuery(), new FakeIReverseGeoCodeService(), new FakeMemoryCache());

		var result = await importService.Importer(
			new List<string> { "/" },
			new ImportSettingsModel());

		Assert.IsNotNull(result);
		Assert.AreEqual(0, result.Count);
	}

	[TestMethod]
	public async Task Importer_Xmp_WhenImportingAFileThatAlreadyHasAnXmpSidecarFile()
	{
		var appSettings = new AppSettings { Verbose = true };
		var query = new FakeIQuery();
		var storage = new FakeIStorage(
			new List<string> { "/" },
			new List<string> { "/test.dng", "/test.xmp" },
			new List<byte[]> { CreateAnPng.Bytes.ToArray(), CreateAnXmp.Bytes.ToArray() });

		var importService = new Import(new FakeSelectorStorage(storage), appSettings,
			new FakeIImportQuery(),
			new FakeExifTool(storage, appSettings), query, _console,
			new FakeIMetaExifThumbnailService(), new FakeIWebLogger(),
			new FakeIThumbnailQuery(), new FakeIReverseGeoCodeService(), new FakeMemoryCache());
		var expectedFilePath = await GetExpectedFilePathAsync(storage, appSettings,
			"/test.dng");

		var result = await importService.Importer(new List<string> { "/test.dng" },
			new ImportSettingsModel());

		Assert.AreEqual(expectedFilePath, result[0].FileIndexItem?.FilePath);
		Assert.AreEqual(ImportStatus.Ok, result[0].Status);
		// Apple is read from XMP
		Assert.AreEqual("Apple", result[0].FileIndexItem?.Make);
	}

	[TestMethod]
	public async Task Importer_Xmp_CheckIfSidecarExtensionsFilled()
	{
		// File already exist before importing
		// WhenImportingAFileThatAlreadyHasAnXmpSidecarFile
		var appSettings = new AppSettings { Verbose = true, ExifToolImportXmpCreate = true };
		var query = new FakeIQuery();
		var storage = new FakeIStorage(
			new List<string> { "/" },
			new List<string> { "/test.dng", "/test.xmp" },
			new List<byte[]> { CreateAnPng.Bytes.ToArray(), CreateAnXmp.Bytes.ToArray() });

		var importService = new Import(new FakeSelectorStorage(storage), appSettings,
			new FakeIImportQuery(),
			new FakeExifTool(storage, appSettings), query,
			_console, new FakeIMetaExifThumbnailService(), new FakeIWebLogger(),
			new FakeIThumbnailQuery(), new FakeIReverseGeoCodeService(), new FakeMemoryCache());

		var result = await importService.Importer(new List<string> { "/test.dng" },
			new ImportSettingsModel());

		Assert.AreEqual(1, result.Count);
		Assert.AreEqual(1, result[0].FileIndexItem?.SidecarExtensionsList.Count);

		var sidecarExtList = result[0].FileIndexItem?.SidecarExtensionsList.ToList();
		Assert.AreEqual("xmp", sidecarExtList?[0]);
	}

	[TestMethod]
	public async Task Importer_Xmp_NotOverWriteExistingFile()
	{
		// WhenImportingAFileThatAlreadyHasAnXmpSidecarFile
		// When importing just copy the xmp file and keep it, not create a new one
		var appSettings = new AppSettings { Verbose = true, ExifToolImportXmpCreate = true };

		var query = new FakeIQuery();
		var storage = new FakeIStorage(
			new List<string> { "/" },
			new List<string> { "/test.dng", "/test.xmp" },
			new List<byte[]> { CreateAnPng.Bytes.ToArray(), CreateAnXmp.Bytes.ToArray() });

		var importService = new Import(new FakeSelectorStorage(storage),
			appSettings, new FakeIImportQuery(), new FakeExifTool(storage, appSettings), query,
			_console, new FakeIMetaExifThumbnailService(), new FakeIWebLogger(),
			new FakeIThumbnailQuery(), new FakeIReverseGeoCodeService(), new FakeMemoryCache());

		var result = await importService.Importer(new List<string> { "/test.dng" },
			new ImportSettingsModel { ReverseGeoCode = true });

		Assert.AreEqual(1, result.Count);
		var xmpExpectedFilePath = ( await GetExpectedFilePathAsync(storage, appSettings,
			"/test.dng") ).Replace(".dng", ".xmp");

		var xmpReadStream = storage.ReadStream(xmpExpectedFilePath);

		var xmpStreamLength = xmpReadStream.Length;
		var toStringAsync = await StreamToStringHelper.StreamToStringAsync(xmpReadStream);

		Assert.AreEqual(CreateAnXmp.Bytes.Length, xmpStreamLength);
		Assert.IsTrue(toStringAsync.Contains("<tiff:Make>Apple</tiff:Make>"));
	}

	[TestMethod]
	public async Task Importer_XmpIsCreatedDuringImport()
	{
		// xmp is created during import
		var appSettings = new AppSettings { Verbose = true };
		var query = new FakeIQuery();
		var storage = new FakeIStorage(
			new List<string> { "/" },
			new List<string> { "/test.dng" },
			new List<byte[]> { CreateAnPng.Bytes.ToArray() });

		var importService = new Import(new FakeSelectorStorage(storage), appSettings,
			new FakeIImportQuery(),
			new FakeExifTool(storage, appSettings), query,
			_console, new FakeIMetaExifThumbnailService(), new FakeIWebLogger(),
			new FakeIThumbnailQuery(), new FakeIReverseGeoCodeService(), new FakeMemoryCache());

		await importService.Importer(new List<string> { "/test.dng" },
			new ImportSettingsModel());

		var expectedFilePath = ( await GetExpectedFilePathAsync(storage, appSettings,
			"/test.dng") ).Replace(".dng", ".xmp");

		Assert.IsTrue(storage.ExistFile(expectedFilePath));

		var stream = storage.ReadStream(expectedFilePath);
		var toStringAsync = await StreamToStringHelper.StreamToStringAsync(stream);

		Assert.AreEqual(FakeExifTool.XmpInjection, toStringAsync);
	}

	[TestMethod]
	public async Task Importer_Over100Times()
	{
		var appSettings = new AppSettings();
		var storage = new FakeIStorage();
		// write source file
		await storage.WriteStreamAsync(
			new MemoryStream(CreateAnImage.Bytes.ToArray()), "/test.jpg"
		);
		// write  /2018/04/2018_04_22/20180422_161454_test.jpg
		var path = await GetExpectedFilePathAsync(storage, appSettings, "/test.jpg");
		await storage.WriteStreamAsync(
			new MemoryStream(CreateAnImage.Bytes.ToArray()), path
		);
		var importService = new Import(new FakeSelectorStorage(storage), appSettings,
			new FakeIImportQuery(),
			new FakeExifTool(storage, appSettings), new FakeIQuery(), _console,
			new FakeIMetaExifThumbnailService(), new FakeIWebLogger(),
			new FakeIThumbnailQuery(), new FakeIReverseGeoCodeService(), new FakeMemoryCache())
		{
			MaxTryGetDestinationPath = 0
		};

		// used to be:[ExpectedException(typeof(AggregateException))]
		await Assert.ThrowsExactlyAsync<AggregateException>(async () =>
			await importService.Importer(new List<string> { "/test.jpg" },
				new ImportSettingsModel()));
	}

	[TestMethod]
	public async Task Importer_DuplicateFileName()
	{
		var appSettings = new AppSettings();

		var storage = new FakeIStorage(
			new List<string> { "/", "/2018", "/2018/04", "/2018/04/2018_04_22" },
			new List<string> { "/test.jpg", "/2018/04/2018_04_22/20180422_161454_test.jpg" },
			new List<byte[]>
			{
				CreateAnImage.Bytes.ToArray(), Array.Empty<byte>()
			}); // instead of new byte[0]

		var importService = new Import(new FakeSelectorStorage(storage), appSettings,
			new FakeIImportQuery(),
			new FakeExifTool(storage, appSettings), new FakeIQuery(), _console,
			new FakeIMetaExifThumbnailService(), new FakeIWebLogger(),
			new FakeIThumbnailQuery(), new FakeIReverseGeoCodeService(), new FakeMemoryCache());

		var result = await importService.Importer(new List<string> { "/test.jpg" },
			new ImportSettingsModel());

		Assert.AreEqual(ImportStatus.Ok, result[0].Status);

		// get something like  /2018/04/2018_04_22/20180422_161454_test_1.jpg
		var expectedFilePath =
			await GetExpectedFilePathAsync(storage, appSettings, "/test.jpg", 1);
		Assert.AreEqual(expectedFilePath, result[0].FilePath);
	}

	[TestMethod]
	public async Task Importer_CheckIfAddToDatabaseTime()
	{
		var appSettings = new AppSettings();
		var query = new FakeIQuery();
		var importService = new Import(new FakeSelectorStorage(_iStorageFake),
			appSettings, new FakeIImportQuery(),
			new FakeExifTool(_iStorageFake, appSettings), query, _console,
			new FakeIMetaExifThumbnailService(), new FakeIWebLogger(),
			new FakeIThumbnailQuery(), new FakeIReverseGeoCodeService(), new FakeMemoryCache());

		var result = await importService.Importer(new List<string> { "/test.jpg" },
			new ImportSettingsModel());

		// AddToDatabase is Used by the importer History agent

		Assert.IsTrue(result[0].FileIndexItem?.AddToDatabase >=
		              DateTime.UtcNow.AddMinutes(-10));
		Assert.IsTrue(result[0].AddToDatabase >= DateTime.UtcNow.AddMinutes(-10));
	}

	[TestMethod]
	public async Task Importer_OverwriteStructure_ArgumentException()
	{
		var appSettings = new AppSettings();
		var query = new FakeIQuery();
		var importService = new Import(new FakeSelectorStorage(_iStorageFake), appSettings,
			new FakeIImportQuery(),
			new FakeExifTool(_iStorageFake, appSettings), query, _console,
			new FakeIMetaExifThumbnailService(), new FakeIWebLogger(),
			new FakeIThumbnailQuery(), new FakeIReverseGeoCodeService(), new FakeMemoryCache());

		await Assert.ThrowsExactlyAsync<ArgumentException>(async () =>
			await importService.Importer(new List<string> { "/test.jpg" },
				new ImportSettingsModel { Structure = "/.ext" }));
	}

	[TestMethod]
	public async Task Importer_IOException_FileFailsWritingToSubPath()
	{
		var appSettings = new AppSettings { Verbose = true };

		var subPathStorage = new FakeIStorage(new AggregateException(new IOException()));
		var fakeImportQuery = new FakeIImportQuery();
		var fakeDbQuery = new FakeIQuery();

		var importService = new Import(new FakeSelectorStorageByType(
				subPathStorage,
				null!, _iStorageFake, null!
			), appSettings, fakeImportQuery,
			new FakeExifTool(_iStorageFake, appSettings), fakeDbQuery, _console,
			new FakeIMetaExifThumbnailService(), new FakeIWebLogger(),
			new FakeIThumbnailQuery(), new FakeIReverseGeoCodeService(), new FakeMemoryCache());

		var result = await importService.Importer(
			new ImportIndexItem
			{
				SourceFullFilePath = "/test.jpg",
				FilePath = "/test.jpg",
				FileHash = "hash73845934893459",
				Status = ImportStatus.Ok,
				FileIndexItem = new FileIndexItem { FilePath = "/test.jpg" }
			},
			new ImportSettingsModel());

		Assert.IsFalse(subPathStorage.ExistFile("/test.jpg"));
		Assert.IsFalse(await fakeImportQuery.IsHashInImportDbAsync("hash73845934893459"));
		Assert.AreEqual(ImportStatus.FileError, result.Status);
	}

	[TestMethod]
	public async Task Importer_AreParentFoldersCreated_Storage()
	{
		var appSettings = new AppSettings();
		var query = new FakeIQuery();
		var storage = new FakeIStorage(
			new List<string> { "/" },
			new List<string> { "/test.jpg" },
			new List<byte[]> { CreateAnImage.Bytes.ToArray() }
		);
		var importService = new Import(new FakeSelectorStorage(storage), appSettings,
			new FakeIImportQuery(),
			new FakeExifTool(storage, appSettings), query, _console,
			new FakeIMetaExifThumbnailService(), new FakeIWebLogger(),
			new FakeIThumbnailQuery(), new FakeIReverseGeoCodeService(), new FakeMemoryCache());

		await importService.Importer(new List<string> { "/test.jpg" },
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
			new List<string> { "/" },
			new List<string> { "/test.jpg" },
			new List<byte[]> { CreateAnImage.Bytes.ToArray() }
		);
		var importService = new Import(new FakeSelectorStorage(storage), appSettings,
			new FakeIImportQuery(),
			new FakeExifTool(storage, appSettings), query, _console,
			new FakeIMetaExifThumbnailService(), new FakeIWebLogger(),
			new FakeIThumbnailQuery(), new FakeIReverseGeoCodeService(), new FakeMemoryCache());

		await importService.Importer(new List<string> { "/test.jpg" },
			new ImportSettingsModel());

		// Home is created at first
		Assert.IsNotNull(await query.GetObjectByFilePathAsync("/"));
	}

	[TestMethod]
	public async Task Importer_AreParentFoldersCreated_Database()
	{
		var appSettings = new AppSettings();
		var query = new FakeIQuery();
		var storage = new FakeIStorage(
			new List<string> { "/" },
			new List<string> { "/test.jpg" },
			new List<byte[]> { CreateAnImage.Bytes.ToArray() }
		);
		var importService = new Import(new FakeSelectorStorage(storage), appSettings,
			new FakeIImportQuery(),
			new FakeExifTool(storage, appSettings), query, _console,
			new FakeIMetaExifThumbnailService(), new FakeIWebLogger(),
			new FakeIThumbnailQuery(), new FakeIReverseGeoCodeService(), new FakeMemoryCache());

		await importService.Importer(new List<string> { "/test.jpg" },
			new ImportSettingsModel());

		// Home is not created in the loop
		Assert.IsNotNull(await query.GetObjectByFilePathAsync("/2018"));
		Assert.IsNotNull(await query.GetObjectByFilePathAsync("/2018/04"));
		Assert.IsNotNull(await query.GetObjectByFilePathAsync("/2018/04/2018_04_22"));
	}

	[TestMethod]
	public async Task Importer_AreParentFoldersCreated_MultipleInputs()
	{
		var appSettings = new AppSettings();
		var query = new FakeIQuery();
		var importQuery = new FakeIImportQuery();
		var storage = new FakeIStorage(
			new List<string> { "/" },
			new List<string> { "/test.jpg" },
			new List<byte[]> { CreateAnImage.Bytes.ToArray() }
		);

		var importService = new Import(new FakeSelectorStorage(storage), appSettings,
			importQuery,
			new FakeExifTool(storage, appSettings), query, _console,
			new FakeIMetaExifThumbnailService(), new FakeIWebLogger(),
			new FakeIThumbnailQuery(), new FakeIReverseGeoCodeService(), new FakeMemoryCache());

		var result = await importService.Importer(new List<string> { "/test.jpg" },
			new ImportSettingsModel());

		// remove it due we have one example
		await importQuery.RemoveAsync(result[0].FileHash!);

		await importService.Importer(new List<string> { "/test.jpg" },
			new ImportSettingsModel());

		Assert.IsNotNull(result);
	}

	[TestMethod]
	public async Task Importer_ShouldNotUpdateQuery_IndexModeFalse()
	{
		var appSettings = new AppSettings { Verbose = true };
		var storage = new FakeIStorage(
			new List<string> { "/" },
			new List<string> { "/test.jpg" },
			new List<byte[]> { CreateAnImage.Bytes.ToArray() }
		);

		var importService = new Import(new FakeSelectorStorage(storage), appSettings, null!,
			new FakeExifTool(storage, appSettings), null!, _console,
			new FakeIMetaExifThumbnailService(), new FakeIWebLogger(),
			new FakeIThumbnailQuery(), new FakeIReverseGeoCodeService(), new FakeMemoryCache());

		var result = await importService.Importer(new List<string> { "/test.jpg" },
			new ImportSettingsModel { IndexMode = false });

		Assert.AreEqual(ImportStatus.Ok, result[0].Status);
	}

	[TestMethod]
	public void Preflight_Predict_Duplicates()
	{
		var appSettings = new AppSettings { Structure = "/yyyy/yyyyMMdd_HHmmss_\\d.ext" };
		var query = new FakeIQuery();
		var importQuery = new FakeIImportQuery();
		var storage = new FakeIStorage(
			new List<string> { "/", "/0001", "/2020" },
			new List<string>
			{
				"/test.jpg",
				"/0001/00010101_000000_d.png",
				"/0001/00010101_000000_d_2.png",
				"/2020/20200501_120000_1.png"
			},
			new List<byte[]>
			{
				Array.Empty<byte>(),
				Array.Empty<byte>(),
				Array.Empty<byte>(),
				Array.Empty<byte>()
			} // instead of new byte[0]
		);
		var importService = new Import(new FakeSelectorStorage(storage), appSettings,
			importQuery,
			new FakeExifTool(storage, appSettings), query, _console,
			new FakeIMetaExifThumbnailService(), new FakeIWebLogger(),
			new FakeIThumbnailQuery(), new FakeIReverseGeoCodeService(), new FakeMemoryCache());

		var duplicatesExampleList = new List<ImportIndexItem>
		{
			new()
			{
				FilePath = "/0001/00010101_000000_d.png",
				Status = ImportStatus.Ok,
				FileIndexItem = new FileIndexItem("/0001/00010101_000000_d.png")
			},
			new()
			{
				Status = ImportStatus.Ok,
				FilePath = "/0001/00010101_000000_d.png",
				FileIndexItem = new FileIndexItem("/0001/00010101_000000_d.png")
			},
			new()
			{
				Status = ImportStatus.Ok,
				FilePath = "/2020/20200501_120000_d.png",
				FileIndexItem = new FileIndexItem("/2020/20200501_120000_d.png")
			},
			new()
			{
				Status = ImportStatus.Ok,
				FilePath = "/2020/20200501_120000_d.png",
				FileIndexItem = new FileIndexItem("/2020/20200501_120000_d.png")
			}
		};

		var directoriesContent = importService.ParentFoldersDictionary(duplicatesExampleList);
		var result =
			importService.CheckForDuplicateNaming(duplicatesExampleList, directoriesContent);

		var fileIndexItemFilePathList = result.Select(x => x.FileIndexItem?.FilePath).ToList();
		Assert.AreEqual(4, fileIndexItemFilePathList.Count);
		Assert.AreEqual("/0001/00010101_000000_d_1.png", fileIndexItemFilePathList[0]);
		Assert.AreEqual("/0001/00010101_000000_d_3.png", fileIndexItemFilePathList[1]);
		Assert.AreEqual("/2020/20200501_120000_d.png", fileIndexItemFilePathList[2]);
		Assert.AreEqual("/2020/20200501_120000_d_1.png", fileIndexItemFilePathList[3]);
	}

	[TestMethod]
	public void Preflight_Predict_Duplicates_MissingFileIndexObject()
	{
		var appSettings = new AppSettings { Structure = "/yyyy/yyyyMMdd_HHmmss_\\d.ext" };
		var importQuery = new FakeIImportQuery();
		var query = new FakeIQuery();

		var storage = new FakeIStorage(
			new List<string> { "/", "/0001", "/2020" },
			new List<string>
			{
				"/test.jpg",
				"/0001/00010101_000000_d.png",
				"/0001/00010101_000000_d_2.png",
				"/2020/20200501_120000_1.png"
			},
			new List<byte[]>
			{
				Array.Empty<byte>(),
				Array.Empty<byte>(),
				Array.Empty<byte>(),
				Array.Empty<byte>()
			}
			// instead of new byte[0]
		);

		var importService = new Import(new FakeSelectorStorage(storage), appSettings,
			importQuery,
			new FakeExifTool(storage, appSettings), query, _console,
			new FakeIMetaExifThumbnailService(), new FakeIWebLogger(),
			new FakeIThumbnailQuery(), new FakeIReverseGeoCodeService(), new FakeMemoryCache());

		var duplicatesExampleList = new List<ImportIndexItem>
		{
			new() { FilePath = "/0001/00010101_000000_d.png", Status = ImportStatus.Ok },
			new() { Status = ImportStatus.Ok, FilePath = "/0001/00010101_000000_d.png" },
			new() { Status = ImportStatus.Ok, FilePath = "/2020/20200501_120000_d.png" },
			new() { Status = ImportStatus.Ok, FilePath = "/2020/20200501_120000_d.png" }
		};

		var fileIndexItemFilePathList =
			importService.CheckForDuplicateNaming(duplicatesExampleList, null!);
		Assert.AreEqual(4, fileIndexItemFilePathList.Count);
	}

	[TestMethod]
	public async Task InternalImporter_IgnoreWrongInput()
	{
		var importService = new Import(new FakeSelectorStorage(_iStorageFake),
			new AppSettings(),
			new FakeIImportQuery(), new FakeExifTool(_iStorageFake, new AppSettings()),
			new FakeIQuery(), _console, new FakeIMetaExifThumbnailService(),
			new FakeIWebLogger(), new FakeIThumbnailQuery(), new FakeIReverseGeoCodeService(),
			new FakeMemoryCache());

		var result = await importService.Importer(
			new ImportIndexItem { Status = ImportStatus.FileError },
			new ImportSettingsModel());
		Assert.AreEqual(ImportStatus.FileError, result.Status);
	}

	[TestMethod]
	public async Task CreateMataThumbnail_SkipWhenAppSettings()
	{
		var importService = new Import(new FakeSelectorStorage(_iStorageFake),
			new AppSettings { MetaThumbnailOnImport = false },
			new FakeIImportQuery(), new FakeExifTool(_iStorageFake, new AppSettings()),
			new FakeIQuery(), _console, new FakeIMetaExifThumbnailService(),
			new FakeIWebLogger(), new FakeIThumbnailQuery(), new FakeIReverseGeoCodeService(),
			new FakeMemoryCache());

		var result = await importService.CreateMataThumbnail(null!,
			new ImportSettingsModel());
		Assert.IsFalse(result.FirstOrDefault().Item1);
	}

	[TestMethod]
	public async Task CreateMataThumbnail_SkipWhenIndexIsOff()
	{
		var importService = new Import(new FakeSelectorStorage(_iStorageFake),
			new AppSettings(),
			new FakeIImportQuery(), new FakeExifTool(_iStorageFake, new AppSettings()),
			new FakeIQuery(), _console,
			new FakeIMetaExifThumbnailService(), new FakeIWebLogger(),
			new FakeIThumbnailQuery(), new FakeIReverseGeoCodeService(), new FakeMemoryCache());

		var result = await importService.CreateMataThumbnail(null!,
			new ImportSettingsModel { IndexMode = false });
		Assert.IsFalse(result.FirstOrDefault().Item1);
	}

	[TestMethod]
	public async Task CreateMataThumbnail_SuccessReturnTrue()
	{
		var fakeExifThumbnailService = new FakeIMetaExifThumbnailService();
		var importService = new Import(new FakeSelectorStorage(_iStorageFake),
			new AppSettings(),
			new FakeIImportQuery(), new FakeExifTool(_iStorageFake, new AppSettings()),
			new FakeIQuery(), _console, fakeExifThumbnailService,
			new FakeIWebLogger(), new FakeIThumbnailQuery(), new FakeIReverseGeoCodeService(),
			new FakeMemoryCache());

		var result = await importService.CreateMataThumbnail(
			new List<ImportIndexItem>
			{
				new()
				{
					FileHash = "hash",
					FilePath = "/test.jpg",
					Status = ImportStatus.Ok,
					FileIndexItem = new FileIndexItem { FileHash = "hash" }
				}
			},
			new ImportSettingsModel());

		Assert.IsTrue(result.FirstOrDefault().Item1);
	}

	[TestMethod]
	public async Task CreateMataThumbnail_NotAnyOkResults()
	{
		var fakeExifThumbnailService = new FakeIMetaExifThumbnailService();
		var importService = new Import(new FakeSelectorStorage(_iStorageFake),
			new AppSettings(),
			new FakeIImportQuery(), new FakeExifTool(_iStorageFake, new AppSettings()),
			new FakeIQuery(), _console, fakeExifThumbnailService,
			new FakeIWebLogger(), new FakeIThumbnailQuery(), new FakeIReverseGeoCodeService(),
			new FakeMemoryCache());

		await importService.CreateMataThumbnail(
			new List<ImportIndexItem>
			{
				new()
				{
					FileHash = "hash",
					FilePath = "/test.jpg",
					Status = ImportStatus.FileError
				}
			},
			new ImportSettingsModel());

		Assert.AreEqual(0, fakeExifThumbnailService.Input.Count);
	}

	[TestMethod]
	public async Task CreateMataThumbnail_ShouldGiveBack()
	{
		var fakeExifThumbnailService = new FakeIMetaExifThumbnailService();
		var importService = new Import(new FakeSelectorStorage(_iStorageFake),
			new AppSettings(),
			new FakeIImportQuery(), new FakeExifTool(_iStorageFake, new AppSettings()),
			new FakeIQuery(), _console, fakeExifThumbnailService,
			new FakeIWebLogger(), new FakeIThumbnailQuery(), new FakeIReverseGeoCodeService(),
			new FakeMemoryCache());

		await importService.CreateMataThumbnail(
			new List<ImportIndexItem>
			{
				new()
				{
					FileHash = "hash",
					FilePath = "/test.jpg",
					Status = ImportStatus.Ok,
					FileIndexItem = new FileIndexItem { FileHash = "hash" }
				}
			},
			new ImportSettingsModel());

		Assert.IsTrue(fakeExifThumbnailService.Input.Exists(p => p.Item1 == "/test.jpg"));
	}

	[TestMethod]
	public void ExistXmpSidecarForThisFileType_Nothing_Filled_Ignore()
	{
		var importService = new Import(new FakeSelectorStorage(_iStorageFake),
			new AppSettings(),
			new FakeIImportQuery(), new FakeExifTool(_iStorageFake, new AppSettings()),
			new FakeIQuery(), _console, new FakeIMetaExifThumbnailService(),
			new FakeIWebLogger(), new FakeIThumbnailQuery(), new FakeIReverseGeoCodeService(),
			new FakeMemoryCache());
		var result = importService.ExistXmpSidecarForThisFileType(new ImportIndexItem());
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void ExistXmpSidecarForThisFileType_DngReturn_True()
	{
		var storage = new FakeIStorage(
			new List<string> { "/" },
			new List<string> { "/test.dng", "/test.xmp" },
			new List<byte[]> { CreateAnPng.Bytes.ToArray(), CreateAnXmp.Bytes.ToArray() });
		var appSettings = new AppSettings();
		var importService = new Import(new FakeSelectorStorage(storage),
			appSettings, new FakeIImportQuery(), new FakeExifTool(storage, appSettings),
			new FakeIQuery(),
			_console, new FakeIMetaExifThumbnailService(), new FakeIWebLogger(),
			new FakeIThumbnailQuery(), new FakeIReverseGeoCodeService(), new FakeMemoryCache());

		var result = importService.ExistXmpSidecarForThisFileType(new ImportIndexItem
		{
			SourceFullFilePath = "/test.dng"
		});
		Assert.IsTrue(result);
	}

	[TestMethod]
	public void ExistXmpSidecarForThisFileType_JpegReturn_False()
	{
		var storage = new FakeIStorage(
			new List<string> { "/" },
			new List<string> { "/test.jpg", "/test.xmp" },
			new List<byte[]> { CreateAnPng.Bytes.ToArray(), CreateAnXmp.Bytes.ToArray() });
		var appSettings = new AppSettings();
		var importService = new Import(new FakeSelectorStorage(storage),
			appSettings, new FakeIImportQuery(), new FakeExifTool(storage, appSettings),
			new FakeIQuery(),
			_console, new FakeIMetaExifThumbnailService(), new FakeIWebLogger(),
			new FakeIThumbnailQuery(), new FakeIReverseGeoCodeService(), new FakeMemoryCache());

		var result = importService.ExistXmpSidecarForThisFileType(new ImportIndexItem
		{
			SourceFullFilePath = "/test.jpg"
		});
		Assert.IsFalse(result);
	}

	[TestMethod]
	public async Task AddToQueryAndImportDatabaseAsync_NoConnection_NoVerbose()
	{
		var storage = new FakeIStorage(
			new List<string> { "/" },
			new List<string> { "/test.jpg", "/test.xmp" },
			new List<byte[]> { CreateAnPng.Bytes.ToArray(), CreateAnXmp.Bytes.ToArray() });

		var logger = new FakeIWebLogger();
		var appSettings = new AppSettings();
		var importService = new Import(new FakeSelectorStorage(storage),
			appSettings, new FakeIImportQuery(new List<string>(), false),
			new FakeExifTool(storage, appSettings), new FakeIQuery(),
			_console, new FakeIMetaExifThumbnailService(), logger, new FakeIThumbnailQuery(),
			new FakeIReverseGeoCodeService(),
			new FakeMemoryCache());

		await importService.AddToQueryAndImportDatabaseAsync(
			new ImportIndexItem(), new ImportSettingsModel { IndexMode = false });

		Assert.AreEqual(0,
			logger.TrackedInformation.Count(p =>
				p.Item2?.Contains("AddToQueryAndImportDatabaseAsync") == true));
	}

	[TestMethod]
	public async Task RemoveFromQueryAndImportDatabaseAsync_NoConnection_NoVerbose()
	{
		var storage = new FakeIStorage(
			new List<string> { "/" },
			new List<string> { "/test.jpg", "/test.xmp" },
			new List<byte[]> { CreateAnPng.Bytes.ToArray(), CreateAnXmp.Bytes.ToArray() });

		var logger = new FakeIWebLogger();
		var appSettings = new AppSettings();
		var importService = new Import(new FakeSelectorStorage(storage),
			appSettings, new FakeIImportQuery(new List<string>(), false),
			new FakeExifTool(storage, appSettings), new FakeIQuery(),
			_console, new FakeIMetaExifThumbnailService(), logger, new FakeIThumbnailQuery(),
			new FakeIReverseGeoCodeService(),
			new FakeMemoryCache());

		await importService.RemoveFromQueryAndImportDatabaseAsync(
			new ImportIndexItem(), new ImportSettingsModel { IndexMode = false });

		Assert.AreEqual(0,
			logger.TrackedInformation.Count(p =>
				p.Item2?.Contains("AddToQueryAndImportDatabaseAsync") == true));
	}

	[TestMethod]
	public async Task AddToQueryAndImportDatabaseAsync_NoConnection_YesVerbose()
	{
		var storage = new FakeIStorage(
			new List<string> { "/" },
			new List<string> { "/test.jpg", "/test.xmp" },
			new List<byte[]> { CreateAnPng.Bytes.ToArray(), CreateAnXmp.Bytes.ToArray() });

		var logger = new FakeIWebLogger();
		var appSettings = new AppSettings { Verbose = true };
		var importService = new Import(new FakeSelectorStorage(storage),
			appSettings, new FakeIImportQuery(new List<string>(), false),
			new FakeExifTool(storage, appSettings), new FakeIQuery(),
			_console, new FakeIMetaExifThumbnailService(), logger, new FakeIThumbnailQuery(),
			new FakeIReverseGeoCodeService(),
			new FakeMemoryCache());

		await importService.AddToQueryAndImportDatabaseAsync(
			new ImportIndexItem(), new ImportSettingsModel { IndexMode = false });

		Assert.AreEqual(1, logger.TrackedInformation.Count(p =>
			p.Item2?.Contains("AddToQueryAndImportDatabaseAsync") == true));
	}

	private static string DefaultPath()
	{
		return new AppSettings().IsWindows
			? Directory.GetCurrentDirectory().Split("\\").FirstOrDefault() + "\\"
			: "/";
	}

	[TestMethod]
	public void CheckForReadOnlyFileSystems_1()
	{
		var storage = new FakeIStorage(
			new List<string> { "/" },
			new List<string> { "/test.jpg", "/test.xmp" },
			new List<byte[]> { CreateAnPng.Bytes.ToArray(), CreateAnXmp.Bytes.ToArray() },
			new List<DateTime> { DateTime.Now, DateTime.Now });
		var appSettings = new AppSettings { Verbose = true };
		var logger = new FakeIWebLogger();

		var importService = new Import(new FakeSelectorStorage(storage),
			appSettings, new FakeIImportQuery(new List<string>(), false),
			new FakeExifTool(storage, appSettings), new FakeIQuery(),
			_console, new FakeIMetaExifThumbnailService(), logger, new FakeIThumbnailQuery(),
			new FakeIReverseGeoCodeService(),
			new FakeMemoryCache());

		var readOnlyFileSystems = importService.CheckForReadOnlyFileSystems(
			new List<ImportIndexItem> { new() { SourceFullFilePath = "/test.jpg" } });

		Assert.AreEqual(1, readOnlyFileSystems.Count);
		Assert.AreEqual(DefaultPath(), readOnlyFileSystems[0].Item1);
	}

	[TestMethod]
	public void CheckForReadOnlyFileSystems_1_DirectoryGetParentNull()
	{
		var storage = new FakeIStorage(
			new List<string> { "/" },
			new List<string> { "/test.jpg", "/test.xmp" },
			new List<byte[]> { CreateAnPng.Bytes.ToArray(), CreateAnXmp.Bytes.ToArray() },
			new List<DateTime> { DateTime.Now, DateTime.Now });
		var appSettings = new AppSettings { Verbose = true };
		var logger = new FakeIWebLogger();

		var importService = new Import(new FakeSelectorStorage(storage),
			appSettings, new FakeIImportQuery(new List<string>(), false),
			new FakeExifTool(storage, appSettings), new FakeIQuery(),
			_console, new FakeIMetaExifThumbnailService(), logger, new FakeIThumbnailQuery(),
			new FakeIReverseGeoCodeService(),
			new FakeMemoryCache());

		var readOnlyFileSystems = importService.CheckForReadOnlyFileSystems(
			new List<ImportIndexItem> { new() { SourceFullFilePath = "/" } });

		// Directory.GetParent returns null
		Assert.AreEqual(1, readOnlyFileSystems.Count);
		Assert.IsNull(readOnlyFileSystems[0].Item1);
	}

	[TestMethod]
	public void CheckForReadOnlyFileSystems_2()
	{
		var storage = new FakeReadOnlyStorage();
		var appSettings = new AppSettings { Verbose = true };
		var logger = new FakeIWebLogger();

		var importService = new Import(new FakeSelectorStorage(storage),
			appSettings, new FakeIImportQuery(new List<string>(), false),
			new FakeExifTool(storage, appSettings), new FakeIQuery(),
			_console, new FakeIMetaExifThumbnailService(), logger, new FakeIThumbnailQuery(),
			new FakeIReverseGeoCodeService(),
			new FakeMemoryCache());

		var importIndexItems = new List<ImportIndexItem>
		{
			new() { Status = ImportStatus.Ok, SourceFullFilePath = "/test.jpg" },
			new() { Status = ImportStatus.Ok, SourceFullFilePath = "/test/test/test.jpg" }
		};
		var readOnlyFileSystems = importService.CheckForReadOnlyFileSystems(importIndexItems);

		Assert.AreEqual(2, readOnlyFileSystems.Count);

		Assert.AreEqual(DefaultPath(), readOnlyFileSystems[0].Item1);
		var testItem = importIndexItems.Find(p =>
			p.SourceFullFilePath == "/test.jpg");
		Assert.AreEqual(ImportStatus.ReadOnlyFileSystem, testItem?.Status);
	}

	[TestMethod]
	public void CheckForReadOnlyFileSystems_2a_same_folder()
	{
		var storage = new FakeReadOnlyStorage();
		var appSettings = new AppSettings { Verbose = true };
		var logger = new FakeIWebLogger();

		var importService = new Import(new FakeSelectorStorage(storage),
			appSettings, new FakeIImportQuery(new List<string>(), false),
			new FakeExifTool(storage, appSettings), new FakeIQuery(),
			_console, new FakeIMetaExifThumbnailService(), logger, new FakeIThumbnailQuery(),
			new FakeIReverseGeoCodeService(),
			new FakeMemoryCache());

		var importIndexItems = new List<ImportIndexItem>
		{
			new() { Status = ImportStatus.Ok, SourceFullFilePath = "/test/test/test2.jpg" },
			new() { Status = ImportStatus.Ok, SourceFullFilePath = "/test/test/test.jpg" }
		};
		var readOnlyFileSystems = importService.CheckForReadOnlyFileSystems(importIndexItems);

		Assert.AreEqual(1, readOnlyFileSystems.Count);
		Assert.AreEqual(DefaultPath() + Path.Combine("test", "test"),
			readOnlyFileSystems[0].Item1);
		var testItem = importIndexItems.Find(p =>
			p.SourceFullFilePath == "/test/test/test.jpg");
		Assert.AreEqual(ImportStatus.ReadOnlyFileSystem, testItem?.Status);
	}

	[TestMethod]
	public void CheckForReadOnlyFileSystems_3()
	{
		var storage = new FakeIStorage(
			new List<string> { "/" },
			new List<string> { "/test.jpg", "/test.xmp" },
			new List<byte[]> { CreateAnPng.Bytes.ToArray(), CreateAnXmp.Bytes.ToArray() },
			new List<DateTime> { DateTime.Now, DateTime.Now });
		var appSettings = new AppSettings { Verbose = true };
		var logger = new FakeIWebLogger();

		var importService = new Import(new FakeSelectorStorage(storage),
			appSettings, new FakeIImportQuery(new List<string>(), false),
			new FakeExifTool(storage, appSettings), new FakeIQuery(),
			_console, new FakeIMetaExifThumbnailService(), logger, new FakeIThumbnailQuery(),
			new FakeIReverseGeoCodeService(),
			new FakeMemoryCache());

		var importIndexItems = new List<ImportIndexItem>
		{
			new() { SourceFullFilePath = "/not-found.jpg" }
		};
		var readOnlyFileSystems = importService.CheckForReadOnlyFileSystems(importIndexItems);

		Assert.AreEqual(1, readOnlyFileSystems.Count);

		Assert.AreEqual(DefaultPath(), readOnlyFileSystems[0].Item1);
		var testItem = importIndexItems.Find(p =>
			p.SourceFullFilePath == "/not-found.jpg");
		Assert.AreEqual(ImportStatus.Default, testItem?.Status);
	}

	private sealed class FakeReadOnlyStorage : FakeIStorage
	{
		public override StorageInfo Info(string path)
		{
			return new StorageInfo
			{
				IsFolderOrFile = FolderOrFileModel.FolderOrFileTypeList.Folder,
				IsFileSystemReadOnly = true
			};
		}
	}
}
