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
		var importItem = new StructureService(new FakeSelectorStorage(), structureModel,
			new FakeIWebLogger());
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
		var importItem = new StructureService(new FakeSelectorStorage(), structureModel,
			new FakeIWebLogger());
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

		var importItem = new StructureService(new FakeSelectorStorage(), structureModel,
			new FakeIWebLogger());
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

		var importItem = new StructureService(new FakeSelectorStorage(), structureModel,
			new FakeIWebLogger());
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

		var service = new StructureService(new FakeSelectorStorage(), structureModel,
			new FakeIWebLogger());

		// Act & Assert
		var model =
			new StructureInputModel(new DateTime(2020, 01, 01, 01, 01, 01, DateTimeKind.Local),
				string.Empty, string.Empty, ExtensionRolesHelper.ImageFormat.jpg, string.Empty);
		var result = service.ParseSubfolders(model);

		Assert.AreEqual("/2020/01/2020_01_01", result);
		Assert.IsEmpty(structureModel.Errors);
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
		var result = new StructureService(new FakeSelectorStorage(storage), structureModel,
				new FakeIWebLogger())
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

		var result = new StructureService(new FakeSelectorStorage(storage), structureModel,
				new FakeIWebLogger())
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

		var result = new StructureService(new FakeSelectorStorage(storage), structureModel,
				new FakeIWebLogger())
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

		var result = new StructureService(new FakeSelectorStorage(storage), structureModel,
				new FakeIWebLogger())
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

		var result = new StructureService(new FakeSelectorStorage(storage), structureModel,
				new FakeIWebLogger())
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
			new AppSettingsStructureModel(structure), new FakeIWebLogger()).ParseSubfolders(model);

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
			new AppSettingsStructureModel(structure), new FakeIWebLogger()).ParseSubfolders(model);

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
			new AppSettingsStructureModel(structure), new FakeIWebLogger()).ParseSubfolders(model);

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
			new AppSettingsStructureModel(structure), new FakeIWebLogger()).ParseSubfolders(model);

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
			appSettingsStructureModel, new FakeIWebLogger());
		var result = sut.ParseSubfolders(model);

		Assert.IsNotNull(result);
		Assert.AreEqual("/2020/01/2020_01_01", result);
		Assert.IsEmpty(appSettingsStructureModel.Errors);
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
			appSettingsStructureModel, new FakeIWebLogger());
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
		var sut = new StructureService(new FakeSelectorStorage(), appSettings,
			new FakeIWebLogger());
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
			new AppSettingsStructureModel(structure), new FakeIWebLogger());
		Assert.ThrowsExactly<ArgumentNullException>(() => sut.ParseSubfolders(subPathRelative));
	}

	[TestMethod]
	public void ParseSubfolders_Int_RelativeToday()
	{
		const string structure = "/yyyy/MM/yyyy_MM_dd*/yyyyMMdd_HHmmss_{filenamebase}.ext";
		var result =
			new StructureService(new FakeSelectorStorage(),
				new AppSettingsStructureModel(structure), new FakeIWebLogger()).ParseSubfolders(0);
		Assert.Contains(DateTime.Now.ToString("yyyy_MM_dd"), result);
	}

	[TestMethod]
	[DataRow("/{filenamebase}.ext", ExtensionRolesHelper.ImageFormat.png, "source1", "")]
	[DataRow("/{filenamebase}.ext", ExtensionRolesHelper.ImageFormat.jpg, "source2", "")]
	[DataRow("/yyyy/MM/yyyy_MM_dd*/yyyyMMdd_HHmmss_{filenamebase}.ext",
		ExtensionRolesHelper.ImageFormat.gif, "source3", "")]
	[DataRow("/yyyy/MM/yyyy_MM_dd*/yyyyMMdd_HHmmss_{filenamebase}.ext",
		ExtensionRolesHelper.ImageFormat.bmp, "unknownSource",
		"/yyyy/MM/yyyy_MM_dd*/yyyyMMdd_HHmmss_{filenamebase}.ext")]
	public void GetStructureSetting_ShouldReturnExpectedPattern(
		string expectedPattern, ExtensionRolesHelper.ImageFormat imageFormat,
		string origin, string defaultPattern)
	{
		// Arrange
		var fakeConfig = new AppSettingsStructureModel
		{
			Rules =
			[
				new StructureRule
				{
					Conditions = new StructureRuleConditions
					{
						ImageFormats =
							[imageFormat],
						Origin = origin
					},
					Pattern = expectedPattern
				}
			],
			DefaultPattern = defaultPattern
		};

		var fakeInput = new StructureInputModel(DateTime.MinValue, string.Empty,
			string.Empty, imageFormat, origin);

		// Act
		var result = StructureService.GetStructureSetting(fakeConfig, fakeInput);

		// Assert
		Assert.AreEqual(expectedPattern, result);
	}

	[TestMethod]
	[DataRow(ExtensionRolesHelper.ImageFormat.png,
		@"/yyyy/MM/yyyy_MM_dd_\d/_\SCREEN/yyyyMMdd_HHmmss_\d.ext")]
	[DataRow(ExtensionRolesHelper.ImageFormat.mp4,
		@"/yyyy/MM/yyyy_MM_dd_\d/_CLIP/yyyyMMdd_HHmmss_\d.ext")]
	public void GetStructureSetting_ShouldReturnExpectedPattern(
		ExtensionRolesHelper.ImageFormat imageFormat, string expectedPattern)
	{
		const string origin = "test";
		const string mp4Pattern = @"/yyyy/MM/yyyy_MM_dd_\d/_CLIP/yyyyMMdd_HHmmss_\d.ext";
		const string pngPattern = @"/yyyy/MM/yyyy_MM_dd_\d/_\SCREEN/yyyyMMdd_HHmmss_\d.ext";
		const string defaultPattern = "/yyyy/MM/yyyy_MM_dd*/yyyyMMdd_HHmmss_{filenamebase}.ext";

		var fakeConfig = new AppSettingsStructureModel
		{
			Rules =
			[
				new StructureRule
				{
					Conditions = new StructureRuleConditions
					{
						ImageFormats =
							[ExtensionRolesHelper.ImageFormat.mp4],
						Origin = origin
					},
					Pattern = mp4Pattern
				},
				new StructureRule
				{
					Conditions = new StructureRuleConditions
					{
						ImageFormats =
							[ExtensionRolesHelper.ImageFormat.png],
						Origin = origin
					},
					Pattern = pngPattern
				}
			],
			DefaultPattern = defaultPattern
		};

		var fakeInput = new StructureInputModel(DateTime.MinValue, string.Empty,
			string.Empty, imageFormat, origin);

		// Act
		var result = StructureService.GetStructureSetting(fakeConfig, fakeInput);

		// Assert
		Assert.AreEqual(expectedPattern, result);
	}

	[TestMethod]
	[DataRow(ExtensionRolesHelper.ImageFormat.png,
		@"/yyyy/MM/yyyy_MM_dd_\d/_\SCREEN/yyyyMMdd_HHmmss_\d.ext")]
	[DataRow(ExtensionRolesHelper.ImageFormat.mp4,
		@"/yyyy/MM/yyyy_MM_dd_\d/_CLIP/yyyyMMdd_HHmmss_\d.ext")]
	public void GetStructureSetting_ShouldReturnExpectedPattern_OnImageFormat(
		ExtensionRolesHelper.ImageFormat imageFormat, string expectedPattern)
	{
		const string mp4Pattern = @"/yyyy/MM/yyyy_MM_dd_\d/_CLIP/yyyyMMdd_HHmmss_\d.ext";
		const string pngPattern = @"/yyyy/MM/yyyy_MM_dd_\d/_\SCREEN/yyyyMMdd_HHmmss_\d.ext";
		const string defaultPattern = "/yyyy/MM/yyyy_MM_dd*/yyyyMMdd_HHmmss_{filenamebase}.ext";

		var fakeConfig = new AppSettingsStructureModel
		{
			Rules =
			[
				new StructureRule
				{
					Conditions = new StructureRuleConditions
					{
						// no origin here
						ImageFormats =
							[ExtensionRolesHelper.ImageFormat.mp4],
					},
					Pattern = mp4Pattern
				},
				new StructureRule
				{
					Conditions = new StructureRuleConditions
					{
						// no origin here
						ImageFormats =
							[ExtensionRolesHelper.ImageFormat.png],
					},
					Pattern = pngPattern
				}
			],
			DefaultPattern = defaultPattern
		};

		var fakeInput = new StructureInputModel(DateTime.MinValue, string.Empty,
			string.Empty, imageFormat, string.Empty);

		// Act
		var result = StructureService.GetStructureSetting(fakeConfig, fakeInput);

		// Assert
		Assert.AreEqual(expectedPattern, result);
	}
	
	[TestMethod]
	[DataRow("test", @"/yyyy/MM/yyyy_MM_dd_\d/_CLIP/yyyyMMdd_HHmmss_\d.ext")]
	[DataRow("test2", @"/yyyy/MM/yyyy_MM_dd_\d/_\SCREEN/yyyyMMdd_HHmmss_\d.ext")]
	public void GetStructureSetting_ShouldReturnExpectedPattern_OnOrigin(
		string origin, string expectedPattern)
	{
		const string mp4Pattern = @"/yyyy/MM/yyyy_MM_dd_\d/_CLIP/yyyyMMdd_HHmmss_\d.ext";
		const string pngPattern = @"/yyyy/MM/yyyy_MM_dd_\d/_\SCREEN/yyyyMMdd_HHmmss_\d.ext";
		const string defaultPattern = "/yyyy/MM/yyyy_MM_dd*/yyyyMMdd_HHmmss_{filenamebase}.ext";

		var fakeConfig = new AppSettingsStructureModel
		{
			Rules =
			[
				new StructureRule
				{
					Conditions = new StructureRuleConditions
					{
						// no ImageFormats here
						Origin = "test"
					},
					Pattern = mp4Pattern
				},
				new StructureRule
				{
					Conditions = new StructureRuleConditions
					{
						// no ImageFormats here
						Origin = "test2"
					},
					Pattern = pngPattern
				}
			],
			DefaultPattern = defaultPattern
		};

		var fakeInput = new StructureInputModel(DateTime.MinValue, string.Empty,
			string.Empty, ExtensionRolesHelper.ImageFormat.unknown, origin);

		// Act
		var result = StructureService.GetStructureSetting(fakeConfig, fakeInput);

		// Assert
		Assert.AreEqual(expectedPattern, result);
	}

	[TestMethod]
	[DataRow("yyyy", "2023")]
	[DataRow("fffff", "00000")]
	[DataRow("FFFFF", "")]
	[DataRow("gg", "A.D.")]
	[DataRow("HH", "01")]
	[DataRow("mm", "01")]
	[DataRow("M", "January 01")]
	[DataRow("ss", "01")]
	[DataRow("tt", "AM")]
	[DataRow("zz", "+00")]
	public void OutputStructureRangeItemParser_ShouldHandleVariousPatterns(
		string pattern, string expected)
	{
		// Arrange
		var fixedDateTime = new DateTime(2023,
			01, 01, 01, 01,
			01, DateTimeKind.Utc);
		const string fileNameBase = "testFile";
		const string extensionWithoutDot = "jpg";
		var fakeLogger = new FakeIWebLogger();
		var service =
			new StructureService(new FakeSelectorStorage(), new AppSettings(), fakeLogger);

		// Act
		var result = service.OutputStructureRangeItemParser(pattern, fixedDateTime, fileNameBase,
			extensionWithoutDot);

		// Assert
		Assert.AreEqual(expected, result);
	}

	[TestMethod]
	[DataRow("K")]
	public void OutputStructureRangeItemParser_ErrorHandling(
		string pattern)
	{
		var fixedDateTime = new DateTime(2023,
			01, 01, 01, 01,
			01, DateTimeKind.Utc);
		const string fileNameBase = "testFile";
		const string extensionWithoutDot = "jpg";
		var fakeLogger = new FakeIWebLogger();
		var service = new StructureService(new FakeSelectorStorage(),
			new AppSettings(), fakeLogger);

		// Act
		Assert.ThrowsExactly<FormatException>(() =>
			service.OutputStructureRangeItemParser(pattern, fixedDateTime,
				fileNameBase, extensionWithoutDot));
	}
}
