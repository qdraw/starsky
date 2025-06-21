using System;
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Storage;
using starsky.foundation.storage.Structure;
using starsky.foundation.storage.Structure.Helpers;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.storage.Structure.Helpers;

[TestClass]
public class ParseDateTimeFromFileNameHelperTests
{
	private readonly AppSettings _appSettings = new()
	{
		StorageFolder = new CreateAnImage().BasePath
	};

	[TestMethod]
	public void ParseDateTimeFromFileNameHelper_RemoveEscapedCharactersTest()
	{
		const string structuredFileName = "yyyyMMdd_HHmmss_\\d.ext";
		var result = ParseDateTimeFromFileNameHelper.RemoveEscapedCharacters(structuredFileName);
		Assert.AreEqual("yyyyMMdd_HHmmss_.ext", result);
	}

	[TestMethod]
	public void ParseDateTimeFromFileName_Null()
	{
		var dateTime =
			new ParseDateTimeFromFileNameHelper(new AppSettings().Structure)
				.ParseDateTimeFromFileName(
					new StructureInputModel(
						new DateTime(0, DateTimeKind.Utc), string.Empty,
						string.Empty, ExtensionRolesHelper.ImageFormat.notfound, string.Empty));
		Assert.AreEqual(DateTime.MinValue, dateTime);
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
			Structure = new AppSettingsStructureModel("/HHmmss_yyyyMMdd.ext")
		};

		var model = new StructureInputModel(input.DateTime,
			createAnImageNoExif.FileName, "jpg", ExtensionRolesHelper.ImageFormat.notfound,
			string.Empty);
		var result =
			new ParseDateTimeFromFileNameHelper(input.Structure).ParseDateTimeFromFileName(model);

		DateTime.TryParseExact(
			"20120101_123300",
			"yyyyMMdd_HHmmss",
			CultureInfo.InvariantCulture,
			DateTimeStyles.None,
			out var answerDateTime);

		// Check if those overwrite is accepted
		Assert.AreEqual(answerDateTime, result);

		new StorageHostFullPathFilesystem(new FakeIWebLogger()).FileDelete(createAnImageNoExif
			.FullFilePathWithDate);
	}

	[TestMethod]
	public void ParseDateTimeFromFileNameWithSpaces_Test()
	{
		var model = new StructureInputModel(DateTime.Now,
			"2018 08 20 19 03 00.jpg", "jpg",
			ExtensionRolesHelper.ImageFormat.notfound, string.Empty);
		var result =
			new ParseDateTimeFromFileNameHelper(_appSettings.Structure)
				.ParseDateTimeFromFileName(model);

		DateTime.TryParseExact(
			"20180820_190300",
			"yyyyMMdd_HHmmss",
			CultureInfo.InvariantCulture,
			DateTimeStyles.None,
			out var answerDateTime);

		Assert.AreEqual(answerDateTime, result);
	}

	[TestMethod]
	public void ParseDateTimeFromFileName_ReturnsValidDateTime()
	{
		// Arrange
		const string fileNameBase = "2019-10-01_235959_filename.ext";
		const string structure = "/yyyy-MM-dd_HHmmss_{filenamebase}.ext";
		_appSettings.Structure = new AppSettingsStructureModel(structure);

		var model = new StructureInputModel(new DateTime(2019, 10, 1,
				23, 59, 59, DateTimeKind.Local),
			fileNameBase, "jpg",
			ExtensionRolesHelper.ImageFormat.notfound, string.Empty);

		// Act
		var result =
			new ParseDateTimeFromFileNameHelper(_appSettings.Structure)
				.ParseDateTimeFromFileName(model);

		// Assert
		Assert.AreEqual(new DateTime(2019, 10, 1,
			23, 59, 59, DateTimeKind.Local), result);
	}

	[TestMethod]
	public void ParseDateTimeFromFileName_ReturnsNonValidName()
	{
		// Arrange
		const string sourceFilePath = "..jpg";
		const string structure = "/yyyy-MM-dd_HHmmss_{filenamebase}.ext";
		_appSettings.Structure = new AppSettingsStructureModel(structure);

		// Act
		var result = new ParseDateTimeFromFileNameHelper(_appSettings.Structure)
			.ParseDateTimeFromFileName(
				new StructureInputModel(DateTime.UtcNow, sourceFilePath,
					"jpg", ExtensionRolesHelper.ImageFormat.notfound, string.Empty));

		// Assert
		Assert.AreEqual(DateTime.MinValue, result);
	}

	[TestMethod]
	public void ParseDateTimeFromFileName_Test()
	{
		const string structure = "/yyyyMMdd_HHmmss.ext";
		_appSettings.Structure = new AppSettingsStructureModel(structure);

		var result = new ParseDateTimeFromFileNameHelper(_appSettings.Structure)
			.ParseDateTimeFromFileName(
				new StructureInputModel(DateTime.UtcNow,
					"20180101_011223.jpg",
					"jpg", ExtensionRolesHelper.ImageFormat.notfound, string.Empty));

		DateTime.TryParseExact(
			"20180101_011223",
			"yyyyMMdd_HHmmss",
			CultureInfo.InvariantCulture,
			DateTimeStyles.None,
			out var answerDateTime);

		Assert.AreEqual(answerDateTime, result);
	}

	[TestMethod]
	public void ImportIndexItemParse_ParseDateTimeFromFileNameWithExtraFileNameBase_Test()
	{
		const string structure = "/yyyyMMdd_HHmmss_{filenamebase}.ext";
		_appSettings.Structure = new AppSettingsStructureModel(structure);

		var result = new ParseDateTimeFromFileNameHelper(_appSettings.Structure)
			.ParseDateTimeFromFileName(
				new StructureInputModel(DateTime.UtcNow,
					"2018-07-26 19.45.23.jpg",
					"jpg", ExtensionRolesHelper.ImageFormat.notfound, string.Empty));

		DateTime.TryParseExact(
			"20180726_194523",
			"yyyyMMdd_HHmmss",
			CultureInfo.InvariantCulture,
			DateTimeStyles.None,
			out var answerDateTime);

		Assert.AreEqual(answerDateTime, result);
	}

	[TestMethod]
	public void ImportIndexItemParse_ParseDateTimeFromFileName_AppendixUsedInConfig()
	{
		const string structure = "/yyyyMMdd_HHmmss_\\d\\e\\f\\g.ext";
		_appSettings.Structure = new AppSettingsStructureModel(structure);

		var result = new ParseDateTimeFromFileNameHelper(_appSettings.Structure)
			.ParseDateTimeFromFileName(
				new StructureInputModel(DateTime.UtcNow,
					"20180726_194523.jpg",
					"jpg", ExtensionRolesHelper.ImageFormat.notfound, string.Empty));

		DateTime.TryParseExact(
			"20180726_194523",
			"yyyyMMdd_HHmmss",
			CultureInfo.InvariantCulture,
			DateTimeStyles.None,
			out var answerDateTime);

		Assert.AreEqual(answerDateTime, result);
	}

	[TestMethod]
	public void ImportIndexItemParse_Structure_Fallback()
	{
		_appSettings.Structure = new AppSettingsStructureModel();

		var result = new ParseDateTimeFromFileNameHelper(_appSettings.Structure)
			.ParseDateTimeFromFileName(
				new StructureInputModel(DateTime.UtcNow,
					".jpg",
					"jpg", ExtensionRolesHelper.ImageFormat.notfound, string.Empty));

		Assert.AreEqual(result, result);
	}

	[TestMethod]
	public void ImportIndexItemParse_ParseDateTimeFromFileName_WithExtraDotsInName_Test()
	{
		const string structure = "/yyyyMMdd_HHmmss.ext";
		_appSettings.Structure = new AppSettingsStructureModel(structure);

		var result = new ParseDateTimeFromFileNameHelper(_appSettings.Structure)
			.ParseDateTimeFromFileName(
				new StructureInputModel(DateTime.UtcNow,
					"2018-02-03 18.47.35.jpg",
					"jpg", ExtensionRolesHelper.ImageFormat.notfound, string.Empty));

		DateTime.TryParseExact(
			"20180203_184735",
			"yyyyMMdd_HHmmss",
			CultureInfo.InvariantCulture,
			DateTimeStyles.None,
			out var answerDateTime);

		Assert.AreEqual(answerDateTime, result);
	}

	[TestMethod]
	[DataRow("2023-10-05 12:00:00", "2023-10-05 12:00:00")]
	[DataRow("Schermafbeelding 2025-06-12 om 15.16.00.png", "2025-06-12 15:16:00")]
	[DataRow("20231005", "2023-10-05")]
	[DataRow("invalid-date", "0001-01-01")]
	[DataRow("2023-10-05@12:00:00.jpg", "2023-10-05 12:00:00")]
	[DataRow("filename_without_date.jpg", "0001-01-01")]
	[DataRow("2023-10-05_2023-10-06.jpg", "2023-10-05 20:23:10")]
	[DataRow("  2023-10-05  12.00.00.jpg", "2023-10-05 12:00:00")]
	[DataRow("123456.jpg", "0001-01-01")]
	[DataRow("invalid_2023-10-05_valid.jpg", "0001-01-01")]
	[DataRow("2023-10-05_invalidtime.jpg", "0001-01-01")]
	[DataRow("2023-10-05T12:00:00Z.jpg", "2023-10-05 12:00:00")]
	public void ParseDateTimeFromFileName_ValidatesVariousFormats(
		string fileNameBase, string expectedDate)
	{
		// Arrange
		var settingsStructure = new AppSettingsStructureModel();
		var inputModel = new StructureInputModel(DateTime.MinValue, fileNameBase, string.Empty,
			ExtensionRolesHelper.ImageFormat.notfound, string.Empty);
		var helper = new ParseDateTimeFromFileNameHelper(settingsStructure);

		// Act
		var result = helper.ParseDateTimeFromFileName(inputModel);

		// Assert
		Assert.AreEqual(DateTime.Parse(expectedDate, CultureInfo.InvariantCulture), result);
	}
}
