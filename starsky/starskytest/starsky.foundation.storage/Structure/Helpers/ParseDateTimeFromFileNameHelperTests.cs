using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Structure;
using starsky.foundation.storage.Structure.Helpers;

namespace starskytest.starsky.foundation.storage.Structure.Helpers;

[TestClass]
public class ParseDateTimeFromFileNameHelperTests
{
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
			new ParseDateTimeFromFileNameHelper(new AppSettings()).ParseDateTimeFromFileName(
				new StructureInputModel(
					new DateTime(0, DateTimeKind.Utc), string.Empty,
					string.Empty, ExtensionRolesHelper.ImageFormat.notfound));
		Assert.AreEqual(new DateTime(), dateTime);
	}

	[TestMethod]
	public void ParseDateTimeFromFileName_ReturnsValidDateTime()
	{
		// Arrange
		var sourceFilePath = "path/to/2019-10-01_235959_filename.ext";
		var structure = "/yyyy-MM-dd_HHmmss_{filenamebase}.ext";
		var parser = new ImportIndexItem(_appSettings)
		{
			SourceFullFilePath = sourceFilePath, Structure = structure
		};

		// Act
		var result = parser.ParseDateTimeFromFileName();

		// Assert
		Assert.AreEqual(new DateTime(2019, 10, 1,
			23, 59, 59, DateTimeKind.Local), result);
	}

	[TestMethod]
	public void ParseDateTimeFromFileName_ReturnsNonValidName()
	{
		// Arrange
		var sourceFilePath = "..jpg";
		var structure = "/yyyy-MM-dd_HHmmss_{filenamebase}.ext";
		var parser = new ImportIndexItem(_appSettings)
		{
			SourceFullFilePath = sourceFilePath, Structure = structure
		};

		// Act
		var result = parser.ParseDateTimeFromFileName();

		// Assert
		Assert.AreEqual(new DateTime(), result);
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

		Assert.AreEqual(answerDateTime, input.DateTime);
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

		Assert.AreEqual(answerDateTime, input.DateTime);
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

		Assert.AreEqual(answerDateTime, input.DateTime);
	}
	
	[TestMethod]
	public void ImportIndexItemParse_Structure_Fallback()
	{
		_appSettings.Structure = null!;
		var input = new ImportIndexItem(_appSettings) { SourceFullFilePath = ".jpg" };
		var result = input.ParseDateTimeFromFileName();
		Assert.AreEqual(result, new DateTime());
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

		DateTime.TryParseExact(
			"20180203_184735",
			"yyyyMMdd_HHmmss",
			CultureInfo.InvariantCulture,
			DateTimeStyles.None,
			out var answerDateTime);

		Assert.AreEqual(answerDateTime, input.DateTime);
	}

}
