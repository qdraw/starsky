using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Services;
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

		var storage = new FakeIStorage(["/"],
			["/test.jpg"], new List<byte[]>());

		var fakeExifTool = new FakeExifTool(storage, _appSettings);
		var sut = new ExifToolCmdHelper(fakeExifTool, storage, storage,
			new FakeReadMeta(), new FakeIThumbnailQuery(), new FakeIWebLogger());
		var helperResult = await sut.UpdateAsync(updateModel, comparedNames);

		Assert.Contains(updateModel.Tags, helperResult.Command);
		Assert.Contains(updateModel.Description, helperResult.Command);
		Assert.Contains(
			updateModel.Latitude.ToString(CultureInfo.InvariantCulture), helperResult.Command);
		Assert.Contains(
			updateModel.Longitude.ToString(CultureInfo.InvariantCulture), helperResult.Command);
		Assert.Contains(
			updateModel.LocationAltitude.ToString(CultureInfo.InvariantCulture),
			helperResult.Command);
		Assert.Contains(updateModel.LocationCity, helperResult.Command);
		Assert.Contains(updateModel.LocationState, helperResult.Command);
		Assert.Contains(updateModel.LocationCountry, helperResult.Command);
		Assert.Contains(updateModel.LocationCountryCode, helperResult.Command);
		Assert.Contains(updateModel.Title, helperResult.Command);
		Assert.Contains(updateModel.MakeCameraSerial, helperResult.Command);
		Assert.Contains(updateModel.Make, helperResult.Command);
		Assert.Contains(updateModel.Model, helperResult.Command);
		Assert.Contains(updateModel.LensModel, helperResult.Command);
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

		var storage = new FakeIStorage(["/"],
			["/test.jpg"], new List<byte[]>());

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

		Assert.Contains("-GPSAltitude=\"-41", helperResult.Command);
		Assert.Contains("gpsaltituderef#=\"1", helperResult.Command);
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

		var storage = new FakeIStorage(["/"],
			["/test.jpg"], new List<byte[]>());

		var fakeExifTool = new FakeExifTool(storage, _appSettings);
		var helperResult = await new ExifToolCmdHelper(fakeExifTool, storage, storage,
				new FakeReadMeta(), new FakeIThumbnailQuery(), new FakeIWebLogger())
			.UpdateAsync(updateModel, comparedNames);

		Assert.Contains("tags", helperResult.Command);
		Assert.Contains("Description", helperResult.Command);
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

		var storage = new FakeIStorage(["/"],
			["/test.jpg"], new List<byte[]>());

		var fakeExifTool = new FakeExifTool(storage, _appSettings);
		var helperResult = await new ExifToolCmdHelper(fakeExifTool, storage, storage,
				new FakeReadMeta(), new FakeIThumbnailQuery(), new FakeIWebLogger())
			.UpdateAsync(updateModel, comparedNames);

		Assert.Contains("tags", helperResult.Command);
		Assert.Contains("Description", helperResult.Command);
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

	/// <summary>
	///     Tests the BeforeFileHash method via UpdateAsync
	///     When FilePath doesn't match the path parameter, it should call FileHash.GetHashCodeAsync
	/// </summary>
	[TestMethod]
	public async Task ExifToolCmdHelper_BeforeFileHash_WithDifferentPath_CallsFileHashService()
	{
		// Arrange
		const string testPath = "/different.jpg";
		var updateModel = new FileIndexItem
		{
			FilePath = "/original.jpg",
			FileHash = "ORIGINALHASH123",
			ImageFormat = ExtensionRolesHelper.ImageFormat.jpg,
			Tags = "test"  // Add a field to compare so command is not empty
		};
		var comparedNames = new List<string>
		{
			nameof(FileIndexItem.Tags).ToLowerInvariant()
		};

		// Create storage with a test file
		var storage = new FakeIStorage(["/"],
			[testPath], 
			new List<byte[]> { new byte[] { 1, 2, 3, 4, 5 } });
		
		var fileHashService = new FileHash(storage, new FakeIWebLogger());
		var inputSubPaths = new List<string> { testPath };
		var thumbnailQuery = new FakeIThumbnailQuery([
			new ThumbnailItem { 
				FileHash = (await fileHashService.GetHashCodeAsync(testPath, 
					ExtensionRolesHelper.ImageFormat.jpg)).Key,
				ExtraLarge = false, 
				Reasons = "test" }
		]);
		var fakeExifTool = new FakeExifTool(storage, _appSettings);
		var sut = new ExifToolCmdHelper(fakeExifTool, storage, storage,
			new FakeReadMeta(), thumbnailQuery, new FakeIWebLogger());

		// Act - call UpdateAsync which internally calls BeforeFileHash
		// Since FilePath ("/original.jpg") != path ("/different.jpg"), it should calculate the hash
		var result = await sut.UpdateAsync(updateModel, inputSubPaths, comparedNames, 
			includeSoftware: true, renameThumbnail: true, TestContext.CancellationToken);

		// Assert - verify that the command was created successfully
		// If FileHash.GetHashCodeAsync was not called, the test would have failed
		Assert.IsNotNull(result);
		Assert.IsNotNull(result.Command);
		Assert.HasCount(1,result.Rename);
		Assert.AreEqual(26, result.Rename[0].NewFileHash.Length);
		
		// check if it does rename it correctly in the thumbnail
		var afterQueryResult = (await thumbnailQuery.Get(result.Rename[0].NewFileHash))[0];
		Assert.IsNotNull(afterQueryResult);
		Assert.AreEqual(result.Rename[0].NewFileHash, afterQueryResult.FileHash);
		Assert.AreEqual("test", afterQueryResult.Reasons);
	}

	/// <summary>
	///     Tests the BeforeFileHash method when FilePath matches the path parameter
	///     It should return the cached FileHash without calling FileHash service
	/// </summary>
	[TestMethod]
	public async Task ExifToolCmdHelper_BeforeFileHash_WithSameFilePath_RenameQuery()
	{
		// Arrange
		const string testPath = "/test.jpg";
		const string cachedHash = "CACHEDFILEHASH123";
		var updateModel = new FileIndexItem
		{
			FilePath = testPath,
			FileHash = cachedHash,
			ImageFormat = ExtensionRolesHelper.ImageFormat.jpg,
			Tags = "test"  // Add a field to compare so command is not empty
		};
		var comparedNames = new List<string>
		{
			nameof(FileIndexItem.Tags).ToLowerInvariant()
		};
		var inputSubPaths = new List<string> { testPath };

		var thumbnailQuery = new FakeIThumbnailQuery([
			new ThumbnailItem { FileHash = cachedHash, ExtraLarge = false, 
				Reasons = "test" }
		]);

		var storage = new FakeIStorage(["/"],
			[testPath], 
			new List<byte[]> { new byte[] { 1, 2, 3, 4, 5 } });

		var fakeExifTool = new FakeExifTool(storage, _appSettings);
		var sut = new ExifToolCmdHelper(fakeExifTool, storage, storage,
			new FakeReadMeta(), thumbnailQuery, new FakeIWebLogger());

		// Act - call UpdateAsync with matching FilePath and path
		// Since they match, it should use the cached FileHash
		var result = await sut.UpdateAsync(updateModel, inputSubPaths, comparedNames,
			includeSoftware: true, renameThumbnail: true, TestContext.CancellationToken);

		// Assert - verify the command was created with the cached hash
		Assert.IsNotNull(result);
		Assert.IsNotNull(result.Command);
		
		Assert.HasCount(1,result.Rename);
		Assert.AreEqual(26, result.Rename[0].NewFileHash.Length);

		// check if it does rename it correctly in the thumbnail
		var afterQueryResult = (await thumbnailQuery.Get(result.Rename[0].NewFileHash))[0];
		Assert.IsNotNull(afterQueryResult);
		Assert.AreEqual(result.Rename[0].NewFileHash, afterQueryResult.FileHash);
		Assert.AreEqual("test", afterQueryResult.Reasons);
	}

	public TestContext TestContext { get; set; }
}
