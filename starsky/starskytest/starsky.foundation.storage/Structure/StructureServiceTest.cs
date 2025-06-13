using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Storage;
using starsky.foundation.storage.Structure;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.storage.Structure;

[TestClass]
public sealed class StructureServiceTest
{
	[TestMethod]
	public void ParseFileName_DefaultDate()
	{
		const string structure = "/yyyy/MM/yyyy_MM_dd/yyyyMMdd_HHmmss.ext";
		var structureModel = new AppSettingsStructureModel(structure);
		var importItem = new StructureService(new FakeSelectorStorage(), structureModel);
		var dateTime = new DateTime(1, 1, 1, 0, 0, 0, DateTimeKind.Local);

		var fileName = importItem.ParseFileName(new StructureInputModel(dateTime,
			string.Empty, "jpg", ExtensionRolesHelper.ImageFormat.jpg,
			string.Empty));
		Assert.AreEqual("00010101_000000.jpg", fileName);
	}

	[TestMethod]
	public void ParseFileName_LotsOfEscapeChars()
	{
		const string structure = "/yyyyMMdd_HHmmss_\\\\\\h\\\\\\m.ext";
		var structureModel = new AppSettingsStructureModel(structure);
		var importItem = new StructureService(new FakeSelectorStorage(), structureModel);
		var dateTime = new DateTime(1, 1, 1, 0, 0, 0, DateTimeKind.Local);
		var fileName = importItem.ParseFileName(new StructureInputModel(dateTime,
			string.Empty, "jpg", ExtensionRolesHelper.ImageFormat.jpg, string.Empty));
		Assert.AreEqual("00010101_000000_hm.jpg", fileName);
	}

	[TestMethod]
	public void ParseFileName_FileNameWithAppendix()
	{
		const string structure = "/yyyy/MM/yyyy_MM_dd/yyyyMMdd_HHmmss_\\d.ext";
		var structureModel = new AppSettingsStructureModel(structure);

		var importItem = new StructureService(new FakeSelectorStorage(), structureModel);
		var dateTime = new DateTime(1, 1, 1, 0, 0, 0, DateTimeKind.Local);
		var fileName = importItem.ParseFileName(new StructureInputModel(dateTime,
			string.Empty, "jpg",
			ExtensionRolesHelper.ImageFormat.jpg, string.Empty));
		Assert.AreEqual("00010101_000000_d.jpg", fileName);
	}

	[TestMethod]
	public void ParseFileName_FileNameBase()
	{
		const string structure = "/yyyy/MM/yyyy_MM_dd/yyyyMMdd_HHmmss_{filenamebase}.ext";
		var structureModel = new AppSettingsStructureModel(structure);

		var importItem = new StructureService(new FakeSelectorStorage(), structureModel);
		var fileName = importItem.ParseFileName(new StructureInputModel(
			new DateTime(2020, 01, 01, 01, 01, 01, DateTimeKind.Local),
			"test", "jpg", ExtensionRolesHelper.ImageFormat.jpg, string.Empty));

		Assert.AreEqual("20200101_010101_test.jpg", fileName);
	}

	[TestMethod]
	public void ParseSubfolders_DefaultFallback()
	{
		// Arrange
		var structureModel = new AppSettingsStructureModel { DefaultPattern = null! };

		var service = new StructureService(new FakeSelectorStorage(), structureModel);

		// Act & Assert
		var model =
			new StructureInputModel(new DateTime(2020, 01, 01, 01, 01, 01, DateTimeKind.Local),
				string.Empty, string.Empty, ExtensionRolesHelper.ImageFormat.jpg, string.Empty);
		var result = service.ParseSubfolders(model);

		Assert.AreEqual("/2020/01/2020_01_01", result);
		Assert.AreEqual(0, structureModel.Errors.Count);
	}

	[TestMethod]
	public void ParseSubfolders_TestFolder_RealFs()
	{
		const string structure = "/\\te\\s*/yyyyMMdd_HHmmss_{filenamebase}.ext";
		var structureModel = new AppSettingsStructureModel(structure);

		var storage = new StorageSubPathFilesystem(
			new AppSettings { StorageFolder = new CreateAnImage().BasePath }, new FakeIWebLogger());
		storage.CreateDirectory("test");

		var model =
			new StructureInputModel(new DateTime(2020, 01, 01, 01, 01, 01, DateTimeKind.Local),
				string.Empty, string.Empty, ExtensionRolesHelper.ImageFormat.jpg, string.Empty);
		var result = new StructureService(new FakeSelectorStorage(storage), structureModel)
			.ParseSubfolders(model);

		Assert.AreEqual("/test", result);
	}

	[TestMethod]
	public void ParseSubfolders_Asterisk_Test_Folder()
	{
		const string structure = "/\\t*/yyyyMMdd_HHmmss_{filenamebase}.ext";
		var structureModel = new AppSettingsStructureModel(structure);

		var storage = new FakeIStorage(
			["/", "/test", "/something"],
			[]);
		var model =
			new StructureInputModel(new DateTime(2020, 01, 01, 01, 01, 01, DateTimeKind.Local),
				string.Empty, string.Empty, ExtensionRolesHelper.ImageFormat.jpg, string.Empty);

		var result = new StructureService(new FakeSelectorStorage(storage), structureModel)
			.ParseSubfolders(model);

		Assert.AreEqual("/test", result);
	}

	[TestMethod]
	public void ParseSubfolders_ReturnNewChildFolder()
	{
		var storage = new FakeIStorage(
			["/2020", "/2020/01"],
			[]);

		const string structure = "/yyyy/MM/yyyy_MM_dd*/yyyyMMdd_HHmmss_{filenamebase}.ext";
		var structureModel = new AppSettingsStructureModel(structure);
		var model =
			new StructureInputModel(new DateTime(2020, 01, 01, 01, 01, 01, DateTimeKind.Local),
				string.Empty, string.Empty, ExtensionRolesHelper.ImageFormat.jpg, string.Empty);

		var result = new StructureService(new FakeSelectorStorage(storage), structureModel)
			.ParseSubfolders(model);

		Assert.AreEqual("/2020/01/2020_01_01", result);
	}

	[TestMethod]
	public void ParseSubfolders_DefaultAsterisk()
	{
		var storage = new FakeIStorage(
			new List<string> { "/" },
			new List<string>());

		const string structure = "/*/yyyyMMdd_HHmmss_{filenamebase}.ext";
		var structureModel = new AppSettingsStructureModel(structure);
		var model =
			new StructureInputModel(new DateTime(2020, 01, 01, 01, 01, 01, DateTimeKind.Local),
				string.Empty, string.Empty, ExtensionRolesHelper.ImageFormat.jpg, string.Empty);

		var result = new StructureService(new FakeSelectorStorage(storage), structureModel)
			.ParseSubfolders(model);

		Assert.AreEqual("/default", result);
	}

	[TestMethod]
	public void ParseSubfolders_ExistingAsterisk()
	{
		var storage = new FakeIStorage(
			new List<string> { "/", "/any" },
			new List<string>());

		const string structure = "/*/yyyyMMdd_HHmmss_{filenamebase}.ext";
		var structureModel = new AppSettingsStructureModel(structure);
		var model =
			new StructureInputModel(new DateTime(2020, 01, 01, 01, 01, 01, DateTimeKind.Local),
				string.Empty, string.Empty, ExtensionRolesHelper.ImageFormat.jpg, string.Empty);

		var result = new StructureService(new FakeSelectorStorage(storage), structureModel)
			.ParseSubfolders(model);

		Assert.AreEqual("/any", result);
	}

	[TestMethod]
	public void ParseSubfolders_GetExistingFolder()
	{
		var storage = new FakeIStorage(
			new List<string>
			{
				"/",
				"/2020",
				"/2020/01",
				"/2020/01/2020_01_01 test",
				"/2020/01/2020_01_01 test/ignore"
			},
			new List<string>());

		const string structure = "/yyyy/MM/yyyy_MM_dd*/yyyyMMdd_HHmmss_{filenamebase}.ext";
		var model =
			new StructureInputModel(new DateTime(2020, 01, 01, 01, 01, 01, DateTimeKind.Local),
				string.Empty, string.Empty, ExtensionRolesHelper.ImageFormat.jpg, string.Empty);

		var result = new StructureService(new FakeSelectorStorage(storage),
			new AppSettingsStructureModel(structure)).ParseSubfolders(model);

		Assert.AreEqual("/2020/01/2020_01_01 test", result);
	}

	[TestMethod]
	public void ParseSubfolders_GetExistingPreferSimpleName()
	{
		var storage = new FakeIStorage(
			new List<string>
			{
				"/",
				"/2020",
				"/2020/01",
				"/2020/01/2020_01_01",
				"/2020/01/2020_01_01 test"
			},
			new List<string>());

		const string structure = "/yyyy/MM/yyyy_MM_dd*/yyyyMMdd_HHmmss_{filenamebase}.ext";
		var model =
			new StructureInputModel(new DateTime(2020, 01, 01, 01, 01, 01, DateTimeKind.Local),
				string.Empty, string.Empty, ExtensionRolesHelper.ImageFormat.jpg, string.Empty);

		var result = new StructureService(new FakeSelectorStorage(storage),
			new AppSettingsStructureModel(structure)).ParseSubfolders(model);

		Assert.AreEqual("/2020/01/2020_01_01", result);
	}

	[TestMethod]
	public void ParseSubfolders_FileNameBaseOnFolder()
	{
		var storage = new FakeIStorage(new List<string> { "/" },
			new List<string>());

		const string structure = "/{filenamebase}/file.ext";

		var model =
			new StructureInputModel(new DateTime(2020, 01, 01, 01, 01, 01, DateTimeKind.Local),
				"test", string.Empty, ExtensionRolesHelper.ImageFormat.jpg, string.Empty);
		var result = new StructureService(new FakeSelectorStorage(storage),
			new AppSettingsStructureModel(structure)).ParseSubfolders(model);

		Assert.AreEqual("/test", result);
	}

	[TestMethod]
	public void ParseSubfolders_ExtensionWantedInFolderName()
	{
		var storage = new FakeIStorage(new List<string> { "/" },
			[]);

		const string structure = @"/con\ten\t.ext/file.ext";
		var model =
			new StructureInputModel(new DateTime(2020, 01, 01, 01, 01, 01, DateTimeKind.Local),
				"test", string.Empty, ExtensionRolesHelper.ImageFormat.jpg, string.Empty);

		var result = new StructureService(new FakeSelectorStorage(storage),
			new AppSettingsStructureModel(structure)).ParseSubfolders(model);

		Assert.AreEqual("/content.unknown", result);
	}

	[TestMethod]
	public void ParseSubfolders_DefaultValue_String()
	{
		const string structure = "";
		var model =
			new StructureInputModel(new DateTime(2020, 01, 01, 01, 01, 01, DateTimeKind.Local),
				"test", string.Empty, ExtensionRolesHelper.ImageFormat.jpg, string.Empty);

		var appSettingsStructureModel = new AppSettingsStructureModel(structure);
		var sut = new StructureService(new FakeSelectorStorage(),
			appSettingsStructureModel);
		var result = sut.ParseSubfolders(model);

		Assert.IsNotNull(result);
		Assert.AreEqual("/2020/01/2020_01_01", result);
		Assert.AreEqual(0, appSettingsStructureModel.Errors.Count);
	}

	[TestMethod]
	public void ParseSubfolders_Error_DotExt()
	{
		const string structure = "/.ext";
		var model =
			new StructureInputModel(new DateTime(2020, 01, 01, 01, 01, 01, DateTimeKind.Local),
				"test", string.Empty, ExtensionRolesHelper.ImageFormat.jpg, string.Empty);

		var appSettingsStructureModel = new AppSettingsStructureModel(structure);
		var sut = new StructureService(new FakeSelectorStorage(),
			appSettingsStructureModel);
		var result = sut.ParseSubfolders(model);

		Assert.IsNotNull(result);
		Assert.AreEqual("/2020/01/2020_01_01", result);
		Assert.AreEqual("Structure '/.ext' is not valid",
			appSettingsStructureModel.Errors.ToList()[0]);
	}

	[TestMethod]
	public void ParseSubfolders_Error_DoesNotStartWithSlash()
	{
		const string structure = "test/on";
		var model =
			new StructureInputModel(new DateTime(2020, 01, 01, 01, 01, 01, DateTimeKind.Local),
				"test", string.Empty, ExtensionRolesHelper.ImageFormat.jpg, string.Empty);

		var appSettings = new AppSettingsStructureModel(structure);
		var sut = new StructureService(new FakeSelectorStorage(), appSettings);
		var result = sut.ParseSubfolders(model);
		Assert.IsNotNull(result);
		Assert.AreEqual("/2020/01/2020_01_01", result);
		Assert.AreEqual("Structure '/test/on' is not valid", appSettings.Errors.ToList()[0]);
	}

	[TestMethod]
	public void ParseSubfolders_Int_Null()
	{
		const string structure = "/yyyy/MM/yyyy_MM_dd*/yyyyMMdd_HHmmss_{filenamebase}.ext";
		int? subPathRelative = null;
		var sut = new StructureService(new FakeSelectorStorage(),
			new AppSettingsStructureModel(structure));
		Assert.ThrowsExactly<ArgumentNullException>(() => sut.ParseSubfolders(subPathRelative));
	}

	[TestMethod]
	public void ParseSubfolders_Int_RelativeToday()
	{
		const string structure = "/yyyy/MM/yyyy_MM_dd*/yyyyMMdd_HHmmss_{filenamebase}.ext";
		var result =
			new StructureService(new FakeSelectorStorage(),
				new AppSettingsStructureModel(structure)).ParseSubfolders(0);
		Assert.IsTrue(result?.Contains(DateTime.Now.ToString("yyyy_MM_dd")));
	}
}
