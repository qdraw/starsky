﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;
using ExifToolCmdHelper = starsky.foundation.writemeta.Helpers.ExifToolCmdHelper;

namespace starskytest.starsky.foundation.writemeta.Helpers;

[TestClass]
public sealed class ExifToolCmdHelperTest
{
	private readonly AppSettings _appSettings;

	public ExifToolCmdHelperTest()
	{
		// get the service
		_appSettings = new AppSettings();
	}

	[TestMethod]
	public async Task ExifToolCmdHelper_UpdateTest()
	{
		var updateModel = new FileIndexItem
		{
			Tags = "tags",
			Description = "Description",
			Latitude = 52,
			Longitude = 3,
			LocationAltitude = 41,
			LocationCity = "LocationCity",
			LocationState = "LocationState",
			LocationCountry = "LocationCountry",
			LocationCountryCode = "NLD",
			Title = "Title",
			ColorClass = ColorClassParser.Color.Trash,
			Orientation = ImageRotation.Rotation.Rotate90Cw,
			DateTime = DateTime.Now,
			MakeModel = "Apple|iPhone SE|iPhone SE back camera 4.15mm f/2.2|001"
		};
		var comparedNames = new List<string>
		{
			nameof(FileIndexItem.Tags).ToLowerInvariant(),
			nameof(FileIndexItem.Description).ToLowerInvariant(),
			nameof(FileIndexItem.Latitude).ToLowerInvariant(),
			nameof(FileIndexItem.Longitude).ToLowerInvariant(),
			nameof(FileIndexItem.LocationAltitude).ToLowerInvariant(),
			nameof(FileIndexItem.LocationCity).ToLowerInvariant(),
			nameof(FileIndexItem.LocationState).ToLowerInvariant(),
			nameof(FileIndexItem.LocationCountry).ToLowerInvariant(),
			nameof(FileIndexItem.LocationCountryCode).ToLowerInvariant(),
			nameof(FileIndexItem.Title).ToLowerInvariant(),
			nameof(FileIndexItem.ColorClass).ToLowerInvariant(),
			nameof(FileIndexItem.Orientation).ToLowerInvariant(),
			nameof(FileIndexItem.DateTime).ToLowerInvariant(),
			nameof(FileIndexItem.MakeModel).ToLowerInvariant()
		};

		var storage = new FakeIStorage(new List<string> { "/" },
			new List<string> { "/test.jpg" }, new List<byte[]>());

		var fakeExifTool = new FakeExifTool(storage, _appSettings);
		var sut = new ExifToolCmdHelper(fakeExifTool, storage, storage,
			new FakeReadMeta(), new FakeIThumbnailQuery(), new FakeIWebLogger());
		var helperResult = await sut.UpdateAsync(updateModel, comparedNames);

		Assert.IsTrue(helperResult.Command.Contains(updateModel.Tags));
		Assert.IsTrue(helperResult.Command.Contains(updateModel.Description));
		Assert.IsTrue(
			helperResult.Command.Contains(
				updateModel.Latitude.ToString(CultureInfo.InvariantCulture)));
		Assert.IsTrue(
			helperResult.Command.Contains(
				updateModel.Longitude.ToString(CultureInfo.InvariantCulture)));
		Assert.IsTrue(
			helperResult.Command.Contains(
				updateModel.LocationAltitude.ToString(CultureInfo.InvariantCulture)));
		Assert.IsTrue(helperResult.Command.Contains(updateModel.LocationCity));
		Assert.IsTrue(helperResult.Command.Contains(updateModel.LocationState));
		Assert.IsTrue(helperResult.Command.Contains(updateModel.LocationCountry));
		Assert.IsTrue(helperResult.Command.Contains(updateModel.LocationCountryCode));
		Assert.IsTrue(helperResult.Command.Contains(updateModel.Title));
		Assert.IsTrue(helperResult.Command.Contains(updateModel.MakeCameraSerial));
		Assert.IsTrue(helperResult.Command.Contains(updateModel.Make));
		Assert.IsTrue(helperResult.Command.Contains(updateModel.Model));
		Assert.IsTrue(helperResult.Command.Contains(updateModel.LensModel));
	}

	[TestMethod]
	public async Task ExifToolCmdHelper_UpdateQuoteTest()
	{
		var updateModel = new FileIndexItem
		{
			Tags = "tags,\"test\"",
			Description = "Description \"test\"",
			Title = "Title \"test\""
		};
		var comparedNames = new List<string>
		{
			nameof(FileIndexItem.Tags).ToLowerInvariant(),
			nameof(FileIndexItem.Description).ToLowerInvariant(),
			nameof(FileIndexItem.Title).ToLowerInvariant()
		};

		var storage = new FakeIStorage(new List<string> { "/" },
			new List<string> { "/test.jpg" }, new List<byte[]>());

		var fakeExifTool = new FakeExifTool(storage, _appSettings);
		var helperResult = await new ExifToolCmdHelper(fakeExifTool, storage, storage,
				new FakeReadMeta(), new FakeIThumbnailQuery(), new FakeIWebLogger())
			.UpdateAsync(updateModel, comparedNames);

		const string expectedResult =
			"-json -overwrite_original -sep \", \" \"-xmp:subject\"=\"tags, " +
			"\\\"test\\\" \" -Keywords=\"tags, \\\"test\\\"\" " +
			"-Caption-Abstract=\"Description \\\"test\\\"\" " +
			"-Description=\"Description \\\"test\\\"\" " +
			"\"-xmp-dc:description=Description \\\"test\\\"\" " +
			"-ObjectName=\"Title \\\"test\\\"\" " +
			"\"-title\"=\"Title \\\"test\\\"\" " +
			"\"-xmp-dc:title=Title \\\"test\\\"\"";
		Assert.AreEqual(expectedResult, helperResult.Command);
	}

	[TestMethod]
	public async Task ExifToolCmdHelper_Update_UpdateLocationAltitudeCommandTest()
	{
		var updateModel = new FileIndexItem { LocationAltitude = -41 };
		var comparedNames = new List<string>
		{
			nameof(FileIndexItem.LocationAltitude).ToLowerInvariant()
		};

		var folderPaths = new List<string> { "/" };

		var inputSubPaths = new List<string> { "/test.jpg" };

		var storage =
			new FakeIStorage(folderPaths, inputSubPaths);
		var fakeExifTool = new FakeExifTool(storage, _appSettings);

		var helperResult = await new ExifToolCmdHelper(fakeExifTool,
				storage, storage,
				new FakeReadMeta(), new FakeIThumbnailQuery(), new FakeIWebLogger())
			.UpdateAsync(updateModel, comparedNames);

		Assert.IsTrue(helperResult.Command.Contains("-GPSAltitude=\"-41"));
		Assert.IsTrue(helperResult.Command.Contains("gpsaltituderef#=\"1"));
	}

	[TestMethod]
	public async Task CreateXmpFileIsNotExist_NotCreateFile_jpg()
	{
		var updateModel = new FileIndexItem { LocationAltitude = -41 };
		var folderPaths = new List<string> { "/" };

		var inputSubPaths = new List<string> { "/test.jpg" };

		var storage =
			new FakeIStorage(folderPaths, inputSubPaths);
		var fakeExifTool = new FakeExifTool(storage, _appSettings);
		await new ExifToolCmdHelper(fakeExifTool,
				storage, storage,
				new FakeReadMeta(), new FakeIThumbnailQuery(), new FakeIWebLogger())
			.CreateXmpFileIsNotExist(updateModel, inputSubPaths);

		Assert.IsFalse(storage.ExistFile("/test.xmp"));
	}

	[TestMethod]
	public async Task CreateXmpFileIsNotExist_CreateFile_dng()
	{
		var updateModel = new FileIndexItem { LocationAltitude = -41 };
		var folderPaths = new List<string> { "/" };

		var inputSubPaths = new List<string> { "/test.dng" };

		var storage =
			new FakeIStorage(folderPaths, inputSubPaths);
		var fakeExifTool = new FakeExifTool(storage, _appSettings);
		await new ExifToolCmdHelper(fakeExifTool,
				storage, storage,
				new FakeReadMeta(), new FakeIThumbnailQuery(), new FakeIWebLogger())
			.CreateXmpFileIsNotExist(updateModel, inputSubPaths);

		Assert.IsTrue(storage.ExistFile("/test.xmp"));
	}

	[TestMethod]
	public async Task UpdateAsync_ShouldUpdate_SkipFileHash()
	{
		var updateModel = new FileIndexItem { Tags = "tags", Description = "Description" };
		var comparedNames = new List<string>
		{
			nameof(FileIndexItem.Tags).ToLowerInvariant(),
			nameof(FileIndexItem.Description).ToLowerInvariant()
		};

		var storage = new FakeIStorage(new List<string> { "/" },
			new List<string> { "/test.jpg" }, new List<byte[]>());

		var fakeExifTool = new FakeExifTool(storage, _appSettings);
		var helperResult = await new ExifToolCmdHelper(fakeExifTool, storage, storage,
				new FakeReadMeta(), new FakeIThumbnailQuery(), new FakeIWebLogger())
			.UpdateAsync(updateModel, comparedNames);

		Assert.IsTrue(helperResult.Command.Contains("tags"));
		Assert.IsTrue(helperResult.Command.Contains("Description"));
	}

	[TestMethod]
	public async Task UpdateAsync_ShouldUpdate_IncludeFileHash()
	{
		var updateModel = new FileIndexItem
		{
			Tags = "tags",
			Description = "Description",
			FileHash = "_hash_test" // < - - - - include here
		};
		var comparedNames = new List<string>
		{
			nameof(FileIndexItem.Tags).ToLowerInvariant(),
			nameof(FileIndexItem.Description).ToLowerInvariant()
		};

		var storage = new FakeIStorage(new List<string> { "/" },
			new List<string> { "/test.jpg" }, new List<byte[]>());

		var fakeExifTool = new FakeExifTool(storage, _appSettings);
		var helperResult = await new ExifToolCmdHelper(fakeExifTool, storage, storage,
				new FakeReadMeta(), new FakeIThumbnailQuery(), new FakeIWebLogger())
			.UpdateAsync(updateModel, comparedNames);

		Assert.IsTrue(helperResult.Command.Contains("tags"));
		Assert.IsTrue(helperResult.Command.Contains("Description"));
	}


	[TestMethod]
	public void ExifToolCommandLineArgsImageStabilisation()
	{
		var updateModel = new FileIndexItem
		{
			ImageStabilisation = ImageStabilisationType.On // < - - - - include here
		};
		var comparedNames = new List<string>
		{
			nameof(FileIndexItem.ImageStabilisation).ToLowerInvariant()
		};

		var result = ExifToolCmdHelper.ExifToolCommandLineArgs(updateModel,
			comparedNames, true);

		Assert.AreEqual("-json -overwrite_original -ImageStabilization=\"On\"", result);
	}

	[TestMethod]
	public void ExifToolCommandLineArgsImageStabilisationUnknown()
	{
		var updateModel = new FileIndexItem
		{
			ImageStabilisation = ImageStabilisationType.Unknown // < - - - - include here
		};
		var comparedNames = new List<string>
		{
			nameof(FileIndexItem.ImageStabilisation).ToLowerInvariant()
		};

		var result = ExifToolCmdHelper.ExifToolCommandLineArgs(updateModel,
			comparedNames, true);

		Assert.AreEqual(string.Empty, result);
	}

	[TestMethod]
	public void ExifToolCommandLineArgs_LocationCountryCode()
	{
		var updateModel = new FileIndexItem
		{
			LocationCountryCode = "NLD" // < - - - - include here
		};
		var comparedNames = new List<string>
		{
			nameof(FileIndexItem.LocationCountryCode).ToLowerInvariant()
		};

		var result = ExifToolCmdHelper.ExifToolCommandLineArgs(updateModel,
			comparedNames, true);

		Assert.AreEqual(
			"-json -overwrite_original -Country-PrimaryLocationCode=\"NLD\" -XMP:CountryCode=\"NLD\"",
			result);
	}


	[TestMethod]
	public void UpdateSoftwareCommand_True()
	{
		var updateModel = new FileIndexItem
		{
			Software = "Test" // < - - - - include here
		};
		var comparedNames =
			new List<string> { nameof(FileIndexItem.Software).ToLowerInvariant() };

		var result =
			ExifToolCmdHelper.UpdateSoftwareCommand(string.Empty, comparedNames, updateModel,
				true);

		Assert.AreEqual(" -Software=\"Test\" -CreatorTool=\"Test\" " +
		                "-HistorySoftwareAgent=\"Test\" -HistoryParameters=\"\" -PMVersion=\"\" ",
			result);
	}

	[TestMethod]
	public void UpdateSoftwareCommand_False()
	{
		var updateModel = new FileIndexItem
		{
			Software = "Test" // < - - - - include here
		};
		var comparedNames =
			new List<string> { nameof(FileIndexItem.Software).ToLowerInvariant() };

		var result =
			ExifToolCmdHelper.UpdateSoftwareCommand(string.Empty, comparedNames, updateModel,
				false);

		Assert.AreEqual(" -Software=\"Starsky\" -CreatorTool=\"Starsky\" " +
		                "-HistorySoftwareAgent=\"Starsky\" -HistoryParameters=\"\" -PMVersion=\"\" ",
			result);
	}
}
