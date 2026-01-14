using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models;
using starsky.foundation.metaupdate.Models;
using starsky.foundation.metaupdate.Services;
using starsky.foundation.platform.Models;
using starsky.foundation.readmeta.Interfaces;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.writemeta.Helpers;
using starsky.foundation.writemeta.Interfaces;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.metaupdate.Services;

[TestClass]
public sealed class ExifTimezoneCorrectionServiceTest
{
	private static ExifTimezoneCorrectionService CreateService(
		IReadMeta? readMeta = null,
		IExifTool? exifTool = null,
		IStorage? storage = null)
	{
		readMeta ??= new FakeReadMeta();
		storage ??= new FakeIStorage();
		exifTool ??= new FakeExifTool(storage, new AppSettings());
		var logger = new FakeIWebLogger();
		var thumbnailStorage = new FakeIStorage();

		var exifToolCmdHelper = new ExifToolCmdHelper(
			exifTool,
			storage,
			thumbnailStorage,
			readMeta,
			new FakeIThumbnailQuery(),
			logger);

		return new ExifTimezoneCorrectionService(readMeta, exifToolCmdHelper, logger);
	}

	[TestMethod]
	public void ValidateCorrection_MissingRecordedTimezone_ShouldReturnError()
	{
		// Arrange
		var service = CreateService();
		var fileIndexItem = new FileIndexItem
		{
			FilePath = "/test.jpg",
			DateTime = new DateTime(2024, 6, 15, 14, 30, 0, DateTimeKind.Local)
		};
		var request = new ExifTimezoneCorrectionRequest
		{
			RecordedTimezone = "",
			CorrectTimezone = "Europe/Amsterdam"
		};

		// Act
		var result = service.ValidateCorrection(fileIndexItem, request);

		// Assert
		Assert.IsFalse(result.Success);
		Assert.AreEqual("Recorded timezone is required", result.Error);
	}

	[TestMethod]
	public void ValidateCorrection_MissingCorrectTimezone_ShouldReturnError()
	{
		// Arrange
		var service = CreateService();
		var fileIndexItem = new FileIndexItem
		{
			FilePath = "/test.jpg",
			DateTime = new DateTime(2024, 6, 15, 14, 30, 0, DateTimeKind.Local)
		};
		var request = new ExifTimezoneCorrectionRequest
		{
			RecordedTimezone = "UTC",
			CorrectTimezone = ""
		};

		// Act
		var result = service.ValidateCorrection(fileIndexItem, request);

		// Assert
		Assert.IsFalse(result.Success);
		Assert.AreEqual("Correct timezone is required", result.Error);
	}

	[TestMethod]
	public void ValidateCorrection_InvalidRecordedTimezone_ShouldReturnError()
	{
		// Arrange
		var service = CreateService();
		var fileIndexItem = new FileIndexItem
		{
			FilePath = "/test.jpg",
			DateTime = new DateTime(2024, 6, 15, 14, 30, 0, DateTimeKind.Local)
		};
		var request = new ExifTimezoneCorrectionRequest
		{
			RecordedTimezone = "Invalid/Timezone",
			CorrectTimezone = "Europe/Amsterdam"
		};

		// Act
		var result = service.ValidateCorrection(fileIndexItem, request);

		// Assert
		Assert.IsFalse(result.Success);
		Assert.IsTrue(result.Error!.Contains("Invalid recorded timezone"));
	}

	[TestMethod]
	public void ValidateCorrection_InvalidCorrectTimezone_ShouldReturnError()
	{
		// Arrange
		var service = CreateService();
		var fileIndexItem = new FileIndexItem
		{
			FilePath = "/test.jpg",
			DateTime = new DateTime(2024, 6, 15, 14, 30, 0, DateTimeKind.Local)
		};
		var request = new ExifTimezoneCorrectionRequest
		{
			RecordedTimezone = "UTC",
			CorrectTimezone = "Invalid/Timezone"
		};

		// Act
		var result = service.ValidateCorrection(fileIndexItem, request);

		// Assert
		Assert.IsFalse(result.Success);
		Assert.IsTrue(result.Error!.Contains("Invalid correct timezone"));
	}

	[TestMethod]
	public void ValidateCorrection_InvalidDateTime_ShouldReturnError()
	{
		// Arrange
		var service = CreateService();
		var fileIndexItem = new FileIndexItem
		{
			FilePath = "/test.jpg",
			DateTime = new DateTime(1, 1, 1, 0, 0, 0, DateTimeKind.Local) // Invalid year
		};
		var request = new ExifTimezoneCorrectionRequest
		{
			RecordedTimezone = "UTC",
			CorrectTimezone = "Europe/Amsterdam"
		};

		// Act
		var result = service.ValidateCorrection(fileIndexItem, request);

		// Assert
		Assert.IsFalse(result.Success);
		Assert.AreEqual("Image does not have a valid DateTime in EXIF", result.Error);
	}

	[TestMethod]
	public void ValidateCorrection_SameTimezones_ShouldReturnWarning()
	{
		// Arrange
		var service = CreateService();
		var fileIndexItem = new FileIndexItem
		{
			FilePath = "/test.jpg",
			DateTime = new DateTime(2024, 6, 15, 14, 30, 0, DateTimeKind.Local)
		};
		var request = new ExifTimezoneCorrectionRequest
		{
			RecordedTimezone = "Europe/Amsterdam",
			CorrectTimezone = "Europe/Amsterdam"
		};

		// Act
		var result = service.ValidateCorrection(fileIndexItem, request);

		// Assert
		Assert.AreEqual("Recorded and correct timezones are the same - no correction needed", result.Warning);
	}

	[TestMethod]
	public void ValidateCorrection_DayRollover_ShouldReturnWarning()
	{
		// Arrange
		var service = CreateService();
		var fileIndexItem = new FileIndexItem
		{
			FilePath = "/test.jpg",
			DateTime = new DateTime(2024, 6, 15, 23, 30, 0, DateTimeKind.Local) // Late evening
		};
		var request = new ExifTimezoneCorrectionRequest
		{
			RecordedTimezone = "UTC",
			CorrectTimezone = "Pacific/Auckland" // UTC+12, will roll to next day
		};

		// Act
		var result = service.ValidateCorrection(fileIndexItem, request);

		// Assert
		Assert.IsTrue(result.Warning!.Contains("Correction will change the day"));
		Assert.IsTrue(result.Warning.Contains("2024-06-15"));
		Assert.IsTrue(result.Warning.Contains("2024-06-16"));
	}

	[TestMethod]
	public async Task CorrectTimezoneAsync_ValidCorrection_ShouldSucceed()
	{
		// Arrange
		var storage = new FakeIStorage(new List<string> { "/" },
			new List<string> { "/test.jpg" });
		var service = CreateService(storage: storage);

		var fileIndexItem = new FileIndexItem
		{
			FilePath = "/test.jpg",
			DateTime = new DateTime(2024, 6, 15, 14, 30, 0, DateTimeKind.Local) // 14:30 in UTC
		};
		var request = new ExifTimezoneCorrectionRequest
		{
			RecordedTimezone = "UTC", // Camera thought it was UTC (GMT+00)
			CorrectTimezone = "Europe/Amsterdam" // Actually in Amsterdam (GMT+02 in summer)
		};

		// Act
		var result = await service.CorrectTimezoneAsync(fileIndexItem, request);

		// Assert
		Assert.IsTrue(result.Success, $"Failed: {result.Error}");
		Assert.AreEqual(new DateTime(2024, 6, 15, 14, 30, 0, DateTimeKind.Local), result.OriginalDateTime);
		Assert.AreEqual(new DateTime(2024, 6, 15, 16, 30, 0, DateTimeKind.Local), result.CorrectedDateTime); // +2 hours
		Assert.AreEqual(2.0, result.DeltaHours); // +2 hours difference
	}

	[TestMethod]
	public async Task CorrectTimezoneAsync_WinterTime_ShouldHandleDST()
	{
		// Arrange
		var storage = new FakeIStorage(new List<string> { "/" },
			new List<string> { "/test.jpg" });
		var service = CreateService(storage: storage);

		var fileIndexItem = new FileIndexItem
		{
			FilePath = "/test.jpg",
			DateTime = new DateTime(2024, 1, 15, 14, 30, 0, DateTimeKind.Local) // January (winter)
		};
		var request = new ExifTimezoneCorrectionRequest
		{
			RecordedTimezone = "UTC",
			CorrectTimezone = "Europe/Amsterdam" // GMT+01 in winter
		};

		// Act
		var result = await service.CorrectTimezoneAsync(fileIndexItem, request);

		// Assert
		Assert.IsTrue(result.Success);
		Assert.AreEqual(new DateTime(2024, 1, 15, 15, 30, 0, DateTimeKind.Local), result.CorrectedDateTime); // +1 hour in winter
		Assert.AreEqual(1.0, result.DeltaHours); // +1 hour difference in winter
	}

	[TestMethod]
	public async Task CorrectTimezoneAsync_NegativeOffset_ShouldSubtractTime()
	{
		// Arrange
		var storage = new FakeIStorage(new List<string> { "/" },
			new List<string> { "/test.jpg" });
		var service = CreateService(storage: storage);

		var fileIndexItem = new FileIndexItem
		{
			FilePath = "/test.jpg",
			DateTime = new DateTime(2024, 6, 15, 14, 30, 0, DateTimeKind.Local)
		};
		var request = new ExifTimezoneCorrectionRequest
		{
			RecordedTimezone = "Europe/Amsterdam", // Camera thought GMT+02
			CorrectTimezone = "UTC" // Actually in UTC (GMT+00)
		};

		// Act
		var result = await service.CorrectTimezoneAsync(fileIndexItem, request);

		// Assert
		Assert.IsTrue(result.Success);
		Assert.AreEqual(new DateTime(2024, 6, 15, 12, 30, 0, DateTimeKind.Local), result.CorrectedDateTime); // -2 hours
		Assert.AreEqual(-2.0, result.DeltaHours); // -2 hours difference
	}

	[TestMethod]
	public async Task CorrectTimezoneAsync_CrossDayBoundary_ShouldRollDate()
	{
		// Arrange
		var storage = new FakeIStorage(new List<string> { "/" },
			new List<string> { "/test.jpg" });
		var service = CreateService(storage: storage);

		var fileIndexItem = new FileIndexItem
		{
			FilePath = "/test.jpg",
			DateTime = new DateTime(2024, 6, 15, 23, 30, 0, DateTimeKind.Local) // 23:30
		};
		var request = new ExifTimezoneCorrectionRequest
		{
			RecordedTimezone = "UTC",
			CorrectTimezone = "Pacific/Auckland" // UTC+12
		};

		// Act
		var result = await service.CorrectTimezoneAsync(fileIndexItem, request);

		// Assert
		Assert.IsTrue(result.Success);
		Assert.AreEqual(16, result.CorrectedDateTime?.Day); // Rolled to next day
		Assert.AreEqual(11, result.CorrectedDateTime?.Hour); // 23:30 + 12:00 = 11:30 next day
		Assert.AreEqual(30, result.CorrectedDateTime?.Minute);
	}

	[TestMethod]
	public async Task CorrectTimezoneAsync_MultipleImages_ShouldCorrectAll()
	{
		// Arrange
		var storage = new FakeIStorage(new List<string> { "/" },
			new List<string> { "/test1.jpg", "/test2.jpg" });
		var service = CreateService(storage: storage);

		var fileIndexItems = new List<FileIndexItem>
		{
			new()
			{
				FilePath = "/test1.jpg",
				DateTime = new DateTime(2024, 6, 15, 14, 30, 0, DateTimeKind.Local)
			},
			new()
			{
				FilePath = "/test2.jpg",
				DateTime = new DateTime(2024, 6, 16, 10, 0, 0, DateTimeKind.Local)
			}
		};

		var request = new ExifTimezoneCorrectionRequest
		{
			RecordedTimezone = "UTC",
			CorrectTimezone = "Europe/Amsterdam"
		};

		// Act
		var results = await service.CorrectTimezoneAsync(fileIndexItems, request);

		// Assert
		Assert.AreEqual(2, results.Count);
		Assert.IsTrue(results[0].Success);
		Assert.IsTrue(results[1].Success);
		Assert.AreEqual(new DateTime(2024, 6, 15, 16, 30, 0, DateTimeKind.Local), results[0].CorrectedDateTime);
		Assert.AreEqual(new DateTime(2024, 6, 16, 12, 0, 0, DateTimeKind.Local), results[1].CorrectedDateTime);
	}

	[TestMethod]
	public async Task CorrectTimezoneAsync_InvalidRequest_ShouldReturnError()
	{
		// Arrange
		var service = CreateService();
		var fileIndexItem = new FileIndexItem
		{
			FilePath = "/test.jpg",
			DateTime = new DateTime(2024, 6, 15, 14, 30, 0, DateTimeKind.Local)
		};
		var request = new ExifTimezoneCorrectionRequest
		{
			RecordedTimezone = "", // Invalid
			CorrectTimezone = "Europe/Amsterdam"
		};

		// Act
		var result = await service.CorrectTimezoneAsync(fileIndexItem, request);

		// Assert
		Assert.IsFalse(result.Success);
		Assert.IsNotNull(result.Error);
	}

	[TestMethod]
	public async Task CorrectTimezoneAsync_FileNotFound_ShouldReturnError()
	{
		// Arrange
		var storage = new FakeIStorage(); // Empty storage
		var service = CreateService(storage: storage);

		var fileIndexItem = new FileIndexItem
		{
			FilePath = "/nonexistent.jpg",
			DateTime = new DateTime(2024, 6, 15, 14, 30, 0, DateTimeKind.Local)
		};
		var request = new ExifTimezoneCorrectionRequest
		{
			RecordedTimezone = "UTC",
			CorrectTimezone = "Europe/Amsterdam"
		};

		// Act
		var result = await service.CorrectTimezoneAsync(fileIndexItem, request);

		// Assert
		Assert.IsFalse(result.Success);
		Assert.IsNotNull(result.Error);
	}
}

