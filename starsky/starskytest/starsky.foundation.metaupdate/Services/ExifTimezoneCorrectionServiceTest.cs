using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.metaupdate.Models;
using starsky.foundation.metaupdate.Services;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.writemeta.Interfaces;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.metaupdate.Services;

[TestClass]
public sealed class ExifTimezoneCorrectionServiceTest
{
	private static ExifTimezoneCorrectionService CreateService(
		IExifTool? exifTool = null,
		IStorage? storage = null,
		IQuery? query = null)
	{
		storage ??= new FakeIStorage(["/"],
			["/test.jpg"]);
		exifTool ??= new FakeExifTool(storage, new AppSettings());
		var logger = new FakeIWebLogger();
		var thumbnailStorage = new FakeIStorage();
		var thumbnailQuery = new FakeIThumbnailQuery();
		var appSettings = new AppSettings();
		query ??= new FakeIQuery();

		return new ExifTimezoneCorrectionService(exifTool,
			new FakeSelectorStorageByType(storage, thumbnailStorage,
				null!, null!),
			query, thumbnailQuery, appSettings, logger);
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
			RecordedTimezone = "", CorrectTimezone = "Europe/Amsterdam"
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
			RecordedTimezone = "UTC", CorrectTimezone = ""
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
			RecordedTimezone = "Invalid/Timezone", CorrectTimezone = "Europe/Amsterdam"
		};

		// Act
		var result = service.ValidateCorrection(fileIndexItem, request);

		// Assert
		Assert.IsFalse(result.Success);
		Assert.Contains("Invalid recorded timezone", result.Error);
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
			RecordedTimezone = "UTC", CorrectTimezone = "Invalid/Timezone"
		};

		// Act
		var result = service.ValidateCorrection(fileIndexItem, request);

		// Assert
		Assert.IsFalse(result.Success);
		Assert.Contains("Invalid correct timezone", result.Error);
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
			RecordedTimezone = "UTC", CorrectTimezone = "Europe/Amsterdam"
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
			RecordedTimezone = "Europe/Amsterdam", CorrectTimezone = "Europe/Amsterdam"
		};

		// Act
		var result = service.ValidateCorrection(fileIndexItem, request);

		// Assert
		Assert.AreEqual("Recorded and correct timezones are the same - no correction needed",
			result.Warning);
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
		Assert.Contains("Correction will change the day", result.Warning);
		Assert.Contains("2024-06-15", result.Warning);
		Assert.Contains("2024-06-16", result.Warning);
	}

	[TestMethod]
	public async Task CorrectTimezoneAsync_ValidCorrection_ShouldSucceed()
	{
		// Arrange
		var storage = new FakeIStorage(["/"],
			["/test.jpg"]);
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
		Assert.AreEqual(new DateTime(2024, 6, 15, 14, 30, 0, DateTimeKind.Local),
			result.OriginalDateTime);
		Assert.AreEqual(new DateTime(2024, 6, 15, 16, 30, 0, DateTimeKind.Local),
			result.CorrectedDateTime); // +2 hours
		Assert.AreEqual(2.0, result.DeltaHours); // +2 hours difference
	}

	[TestMethod]
	public async Task CorrectTimezoneAsync_WinterTime_ShouldHandleDST()
	{
		// Arrange
		var storage = new FakeIStorage(["/"],
			["/test.jpg"]);
		var service = CreateService(storage: storage);

		var fileIndexItem = new FileIndexItem
		{
			FilePath = "/test.jpg",
			DateTime = new DateTime(2024, 1, 15,
				14, 30, 0, DateTimeKind.Local) // January (winter)
		};
		var request = new ExifTimezoneCorrectionRequest
		{
			RecordedTimezone = "UTC", CorrectTimezone = "Europe/Amsterdam" // GMT+01 in winter
		};

		// Act
		var result = await service.CorrectTimezoneAsync(fileIndexItem, request);

		// Assert
		Assert.IsTrue(result.Success);
		Assert.AreEqual(new DateTime(2024, 1, 15, 15, 30, 0, DateTimeKind.Local),
			result.CorrectedDateTime); // +1 hour in winter
		Assert.AreEqual(1.0, result.DeltaHours); // +1-hour difference in winter
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
		Assert.AreEqual(new DateTime(2024, 6, 15, 12, 30, 0, DateTimeKind.Local),
			result.CorrectedDateTime); // -2 hours
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
			DateTime = new DateTime(2024, 6, 15,
				23, 30, 0, DateTimeKind.Local) // 23:30
		};
		var request = new ExifTimezoneCorrectionRequest
		{
			RecordedTimezone = "UTC", CorrectTimezone = "Pacific/Auckland" // UTC+12
		};

		// Act
		var result = await service.CorrectTimezoneAsync(fileIndexItem, request);

		// Assert
		Assert.IsTrue(result.Success);
		Assert.AreEqual(16, result.CorrectedDateTime.Day); // Rolled to next day
		Assert.AreEqual(11, result.CorrectedDateTime.Hour); // 23:30 + 12:00 = 11:30 next day
		Assert.AreEqual(30, result.CorrectedDateTime.Minute);
	}

	[TestMethod]
	public async Task CorrectTimezoneAsync_MultipleImages_ShouldCorrectAll()
	{
		// Arrange
		var storage = new FakeIStorage(["/"],
			["/test1.jpg", "/test2.jpg"]);
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
			RecordedTimezone = "UTC", CorrectTimezone = "Europe/Amsterdam"
		};

		// Act
		var results = await service.CorrectTimezoneAsync(fileIndexItems, request);

		// Assert
		Assert.HasCount(2, results);
		Assert.IsTrue(results[0].Success);
		Assert.IsTrue(results[1].Success);
		Assert.AreEqual(new DateTime(2024, 6, 15, 16, 30, 0, DateTimeKind.Local),
			results[0].CorrectedDateTime);
		Assert.AreEqual(new DateTime(2024, 6, 16, 12, 0, 0, DateTimeKind.Local),
			results[1].CorrectedDateTime);
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
			RecordedTimezone = "UTC", CorrectTimezone = "Europe/Amsterdam"
		};

		// Act
		var result = await service.CorrectTimezoneAsync(fileIndexItem, request);

		// Assert
		Assert.IsFalse(result.Success);
		Assert.IsNotNull(result.Error);
	}

	[TestMethod]
	public void ValidateCorrection_FileNotFound_ShouldReturnError()
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
			RecordedTimezone = "UTC", CorrectTimezone = "Europe/Amsterdam"
		};

		// Act
		var result = service.ValidateCorrection(fileIndexItem, request);

		// Assert
		Assert.IsFalse(result.Success);
		Assert.AreEqual("File does not exist", result.Error);
	}

	[TestMethod]
	public void ValidateCorrection_ValidInput_ShouldSucceed()
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
			RecordedTimezone = "UTC", CorrectTimezone = "Europe/Amsterdam"
		};

		// Act
		var result = service.ValidateCorrection(fileIndexItem, request);

		// Assert
		Assert.IsTrue(!result.Success || string.IsNullOrEmpty(result.Error));
	}

	// ==================== DST Transition Tests ====================

	[TestMethod]
	public async Task CorrectTimezoneAsync_DSTTransitionBefore_March30_2024()
	{
		// Arrange - Before DST in Europe (UTC+1)
		var storage = new FakeIStorage(["/"], ["/test.jpg"]);
		var service = CreateService(storage: storage);

		var fileIndexItem = new FileIndexItem
		{
			FilePath = "/test.jpg",
			DateTime = new DateTime(2024, 3, 30, 14, 0, 0, DateTimeKind.Local)
		};
		var request = new ExifTimezoneCorrectionRequest
		{
			RecordedTimezone = "Etc/GMT-1", // Fixed UTC+1
			CorrectTimezone = "Europe/Amsterdam" // UTC+1 before DST
		};

		// Act
		var result = await service.CorrectTimezoneAsync(fileIndexItem, request);

		// Assert
		Assert.IsTrue(result.Success);
		Assert.AreEqual(0.0, result.DeltaHours); // No delta, both UTC+1
		Assert.AreEqual(new DateTime(2024, 3, 30, 14, 0, 0, DateTimeKind.Local),
			result.CorrectedDateTime);
	}

	[TestMethod]
	public async Task CorrectTimezoneAsync_DSTTransitionAfter_March31_2024()
	{
		// Arrange - After DST in Europe (UTC+2)
		var storage = new FakeIStorage(["/"], ["/test.jpg"]);
		var service = CreateService(storage: storage);

		var fileIndexItem = new FileIndexItem
		{
			FilePath = "/test.jpg",
			DateTime = new DateTime(2024, 3, 31, 14, 0, 0, DateTimeKind.Local)
		};
		var request = new ExifTimezoneCorrectionRequest
		{
			RecordedTimezone = "Etc/GMT-1", // Fixed UTC+1 (camera didn't update)
			CorrectTimezone = "Europe/Amsterdam" // UTC+2 after DST
		};

		// Act
		var result = await service.CorrectTimezoneAsync(fileIndexItem, request);

		// Assert
		Assert.IsTrue(result.Success);
		Assert.AreEqual(1.0, result.DeltaHours); // +1 hour delta due to DST
		Assert.AreEqual(new DateTime(2024, 3, 31, 15, 0, 0, DateTimeKind.Local),
			result.CorrectedDateTime);
	}

	[TestMethod]
	public async Task CorrectTimezoneAsync_DSTBeforeFallBack_October26_2024()
	{
		// Arrange - Before fall-back in Europe (still UTC+2)
		// Note: Fall-back happens on October 27 at 3:00 AM
		// On October 26, Europe is still in UTC+2
		var storage = new FakeIStorage(["/"], ["/test.jpg"]);
		var service = CreateService(storage: storage);

		var fileIndexItem = new FileIndexItem
		{
			FilePath = "/test.jpg",
			DateTime = new DateTime(2024, 10, 26, 14, 0, 0, DateTimeKind.Local)
		};
		var request = new ExifTimezoneCorrectionRequest
		{
			RecordedTimezone = "Etc/GMT-2", // Fixed UTC+2
			CorrectTimezone = "Europe/Amsterdam" // UTC+2 on October 26 (before fall-back)
		};

		// Act
		var result = await service.CorrectTimezoneAsync(fileIndexItem, request);

		// Assert
		Assert.IsTrue(result.Success);
		Assert.AreEqual(0.0, result.DeltaHours); // No delta, both UTC+2
		Assert.AreEqual(new DateTime(2024, 10, 26, 14, 0, 0, DateTimeKind.Local),
			result.CorrectedDateTime);
	}

	[TestMethod]
	public async Task CorrectTimezoneAsync_DSTFallBack_October27_2024()
	{
		// Arrange - After fall-back (UTC+1)
		// Note: In Europe, fall-back happens on the last Sunday of October at 3:00 AM
		// 2024: October 27 at 3:00 AM (from UTC+2 to UTC+1)
		var storage = new FakeIStorage(["/"], ["/test.jpg"]);
		var service = CreateService(storage: storage);

		var fileIndexItem = new FileIndexItem
		{
			FilePath = "/test.jpg",
			DateTime = new DateTime(2024, 10, 27, 14, 0, 0, DateTimeKind.Local)
		};
		var request = new ExifTimezoneCorrectionRequest
		{
			RecordedTimezone = "Etc/GMT-2", // Fixed UTC+2 (camera didn't update for fall-back)
			CorrectTimezone = "Europe/Amsterdam" // UTC+1 after fall-back
		};

		// Act
		var result = await service.CorrectTimezoneAsync(fileIndexItem, request);

		// Assert
		Assert.IsTrue(result.Success);
		Assert.AreEqual(-1.0, result.DeltaHours); // -1 hour delta after fall-back
		Assert.AreEqual(new DateTime(2024, 10, 27, 13, 0, 0, DateTimeKind.Local),
			result.CorrectedDateTime);
	}

	// ==================== International Timezone Tests ====================

	[TestMethod]
	public async Task CorrectTimezoneAsync_USEastCoast_ToUSWestCoast()
	{
		// Arrange - Traveled from New York to LA
		var storage = new FakeIStorage(["/"], ["/test.jpg"]);
		var service = CreateService(storage: storage);

		var fileIndexItem = new FileIndexItem
		{
			FilePath = "/test.jpg",
			DateTime = new DateTime(2024, 6, 15, 14, 0, 0, DateTimeKind.Local)
		};
		var request = new ExifTimezoneCorrectionRequest
		{
			RecordedTimezone = "America/New_York", // Camera set to NY (UTC-4 EDT)
			CorrectTimezone = "America/Los_Angeles" // Actually in LA (UTC-7 PDT)
		};

		// Act
		var result = await service.CorrectTimezoneAsync(fileIndexItem, request);

		// Assert
		Assert.IsTrue(result.Success);
		Assert.AreEqual(-3.0, result.DeltaHours); // 3 hours behind
		Assert.AreEqual(new DateTime(2024, 6, 15, 11, 0, 0, DateTimeKind.Local),
			result.CorrectedDateTime);
	}

	[TestMethod]
	public async Task CorrectTimezoneAsync_TokyoWithBigOffset()
	{
		// Arrange - Large offset correction
		var storage = new FakeIStorage(["/"], ["/test.jpg"]);
		var service = CreateService(storage: storage);

		var fileIndexItem = new FileIndexItem
		{
			FilePath = "/test.jpg",
			DateTime = new DateTime(2024, 6, 15, 14, 0, 0, DateTimeKind.Local)
		};
		var request = new ExifTimezoneCorrectionRequest
		{
			RecordedTimezone = "UTC", // Camera set to UTC
			CorrectTimezone = "Asia/Tokyo" // Actually in Tokyo (UTC+9)
		};

		// Act
		var result = await service.CorrectTimezoneAsync(fileIndexItem, request);

		// Assert
		Assert.IsTrue(result.Success);
		Assert.AreEqual(9.0, result.DeltaHours);
		Assert.AreEqual(new DateTime(2024, 6, 15, 23, 0, 0, DateTimeKind.Local),
			result.CorrectedDateTime);
	}

	[TestMethod]
	public async Task CorrectTimezoneAsync_AustraliaTimezone()
	{
		// Arrange - Australia winter time
		var storage = new FakeIStorage(["/"], ["/test.jpg"]);
		var service = CreateService(storage: storage);

		var fileIndexItem = new FileIndexItem
		{
			FilePath = "/test.jpg",
			DateTime = new DateTime(2024, 1, 15, 14, 0, 0, DateTimeKind.Local)
		};
		var request = new ExifTimezoneCorrectionRequest
		{
			RecordedTimezone = "UTC", // Camera set to UTC
			CorrectTimezone = "Australia/Sydney" // UTC+11 in January
		};

		// Act
		var result = await service.CorrectTimezoneAsync(fileIndexItem, request);

		// Assert
		Assert.IsTrue(result.Success);
		Assert.AreEqual(11.0, result.DeltaHours);
		Assert.AreEqual(new DateTime(2024, 1, 16, 1, 0, 0, DateTimeKind.Local),
			result.CorrectedDateTime);
	}

	// ==================== Fixed Offset Tests ====================

	[TestMethod]
	public async Task CorrectTimezoneAsync_FixedUTCPlus1_ToNamedTimezone()
	{
		// Arrange
		var storage = new FakeIStorage(["/"], ["/test.jpg"]);
		var service = CreateService(storage: storage);

		var fileIndexItem = new FileIndexItem
		{
			FilePath = "/test.jpg",
			DateTime = new DateTime(2024, 6, 15, 14, 0, 0, DateTimeKind.Local)
		};
		var request = new ExifTimezoneCorrectionRequest
		{
			RecordedTimezone = "Etc/GMT-1", // Fixed UTC+1
			CorrectTimezone = "Europe/London" // UTC+1 in summer
		};

		// Act
		var result = await service.CorrectTimezoneAsync(fileIndexItem, request);

		// Assert
		Assert.IsTrue(result.Success);
		Assert.AreEqual(0.0, result.DeltaHours);
	}

	[TestMethod]
	public async Task CorrectTimezoneAsync_FixedNegativeOffset()
	{
		// Arrange - Fixed negative offset
		var storage = new FakeIStorage(["/"], ["/test.jpg"]);
		var service = CreateService(storage: storage);

		var fileIndexItem = new FileIndexItem
		{
			FilePath = "/test.jpg",
			DateTime = new DateTime(2024, 6, 15, 14, 0, 0, DateTimeKind.Local)
		};
		var request = new ExifTimezoneCorrectionRequest
		{
			RecordedTimezone = "Etc/GMT+5", // Fixed UTC-5
			CorrectTimezone = "UTC" // UTC+0
		};

		// Act
		var result = await service.CorrectTimezoneAsync(fileIndexItem, request);

		// Assert
		Assert.IsTrue(result.Success);
		Assert.AreEqual(5.0, result.DeltaHours);
		Assert.AreEqual(new DateTime(2024, 6, 15, 19, 0, 0, DateTimeKind.Local),
			result.CorrectedDateTime);
	}

	// ==================== Edge Case Tests ====================

	[TestMethod]
	public async Task CorrectTimezoneAsync_MidnightRollover_BeforeMidnight()
	{
		// Arrange - Photo at 22:00, correction adds 4 hours
		var storage = new FakeIStorage(["/"], ["/test.jpg"]);
		var service = CreateService(storage: storage);

		var fileIndexItem = new FileIndexItem
		{
			FilePath = "/test.jpg",
			DateTime = new DateTime(2024, 6, 15, 22, 0, 0, DateTimeKind.Local)
		};
		var request = new ExifTimezoneCorrectionRequest
		{
			RecordedTimezone = "UTC", CorrectTimezone = "Pacific/Auckland" // UTC+12
		};

		// Act
		var result = await service.CorrectTimezoneAsync(fileIndexItem, request);

		// Assert
		Assert.IsTrue(result.Success);
		Assert.AreEqual(16, result.CorrectedDateTime.Day); // Rolled to next day
		Assert.AreEqual(10, result.CorrectedDateTime.Hour); // 22:00 + 12:00 = 10:00 next day
	}

	[TestMethod]
	public async Task CorrectTimezoneAsync_MonthRollover_EndOfMonth()
	{
		// Arrange - Photo on last day of month (June 30)
		// Adding 12 hours crosses into next month (July 1)
		var storage = new FakeIStorage(["/"], ["/test.jpg"]);
		var service = CreateService(storage: storage);

		var fileIndexItem = new FileIndexItem
		{
			FilePath = "/test.jpg",
			DateTime = new DateTime(2024, 6, 30, 23, 0, 0, DateTimeKind.Local)
		};
		var request = new ExifTimezoneCorrectionRequest
		{
			RecordedTimezone = "UTC", CorrectTimezone = "Pacific/Auckland" // UTC+12
		};

		// Act
		var result = await service.CorrectTimezoneAsync(fileIndexItem, request);

		// Assert
		Assert.IsTrue(result.Success);
		Assert.AreEqual(7, result.CorrectedDateTime.Month); // Rolled to July (month 7)
		Assert.AreEqual(1, result.CorrectedDateTime.Day); // July 1st
		Assert.AreEqual(11, result.CorrectedDateTime.Hour); // 23:00 + 12:00 = 11:00 next day
	}

	[TestMethod]
	public async Task CorrectTimezoneAsync_YearRollover_EndOfYear()
	{
		// Arrange - Photo on Dec 31 near midnight
		var storage = new FakeIStorage(["/"], ["/test.jpg"]);
		var service = CreateService(storage: storage);

		var fileIndexItem = new FileIndexItem
		{
			FilePath = "/test.jpg",
			DateTime = new DateTime(2024, 12, 31, 22, 0, 0, DateTimeKind.Local)
		};
		var request = new ExifTimezoneCorrectionRequest
		{
			RecordedTimezone = "UTC", CorrectTimezone = "Pacific/Auckland" // UTC+12
		};

		// Act
		var result = await service.CorrectTimezoneAsync(fileIndexItem, request);

		// Assert
		Assert.IsTrue(result.Success);
		Assert.AreEqual(2025, result.CorrectedDateTime.Year); // Rolled to next year
		Assert.AreEqual(1, result.CorrectedDateTime.Month); // January
		Assert.AreEqual(1, result.CorrectedDateTime.Day); // 1st
	}

	[TestMethod]
	public async Task CorrectTimezoneAsync_VerySmallOffset_HalfHour()
	{
		// Arrange - Timezone with half-hour offset (India Standard Time)
		var storage = new FakeIStorage(["/"], ["/test.jpg"]);
		var service = CreateService(storage: storage);

		var fileIndexItem = new FileIndexItem
		{
			FilePath = "/test.jpg",
			DateTime = new DateTime(2024, 6, 15, 14, 0, 0, DateTimeKind.Local)
		};
		var request = new ExifTimezoneCorrectionRequest
		{
			RecordedTimezone = "UTC", CorrectTimezone = "Asia/Kolkata" // UTC+5:30
		};

		// Act
		var result = await service.CorrectTimezoneAsync(fileIndexItem, request);

		// Assert
		Assert.IsTrue(result.Success);
		Assert.AreEqual(5.5, result.DeltaHours); // 5.5 hours
		Assert.AreEqual(new DateTime(2024, 6, 15, 19, 30, 0, DateTimeKind.Local),
			result.CorrectedDateTime);
	}

	// ==================== Batch Operation Tests ====================

	[TestMethod]
	public async Task CorrectTimezoneAsync_BatchWithDifferentDates_DSTAware()
	{
		// Arrange - Batch with photos before and after DST transition
		var storage = new FakeIStorage(["/"], ["/test1.jpg", "/test2.jpg", "/test3.jpg"]);
		var service = CreateService(storage: storage);

		var fileIndexItems = new List<FileIndexItem>
		{
			// Before DST
			new()
			{
				FilePath = "/test1.jpg",
				DateTime = new DateTime(2024, 3, 30, 14, 0, 0, DateTimeKind.Local)
			},
			// On DST transition
			new()
			{
				FilePath = "/test2.jpg",
				DateTime = new DateTime(2024, 3, 31, 14, 0, 0, DateTimeKind.Local)
			},
			// After DST
			new()
			{
				FilePath = "/test3.jpg",
				DateTime = new DateTime(2024, 4, 15, 14, 0, 0, DateTimeKind.Local)
			}
		};

		var request = new ExifTimezoneCorrectionRequest
		{
			RecordedTimezone = "Etc/GMT-1", 
			CorrectTimezone = "Europe/Amsterdam"
		};

		// Act
		var results = await service.CorrectTimezoneAsync(fileIndexItems, request);

		// Assert
		Assert.HasCount(3, results);
		Assert.IsTrue(results[0].Success);
		Assert.IsTrue(results[1].Success);
		Assert.IsTrue(results[2].Success);

		// Before DST: +0
		Assert.AreEqual(0.0, results[0].DeltaHours);

		// On DST: +1
		Assert.AreEqual(1.0, results[1].DeltaHours);

		// After DST: +1
		Assert.AreEqual(1.0, results[2].DeltaHours);
	}

	[TestMethod]
	public async Task CorrectTimezoneAsync_BatchWithMixedSuccess_QueryUpdate()
	{
		// Arrange - Batch where some files exist, some don't
		var storage = new FakeIStorage(["/"], ["/test1.jpg"]);
		var fileIndexItems = new List<FileIndexItem>
		{
			new()
			{
				FilePath = "/test1.jpg",
				DateTime = new DateTime(2024, 6, 15, 14, 0, 0, DateTimeKind.Local)
			},
			new()
			{
				FilePath = "/nonexistent.jpg",
				DateTime = new DateTime(2024, 6, 15, 14, 0, 0, DateTimeKind.Local)
			}
		};
		var fakeQuery = new FakeIQuery(fileIndexItems);
		var service = CreateService(storage: storage, query: fakeQuery);

		var request = new ExifTimezoneCorrectionRequest
		{
			RecordedTimezone = "UTC", CorrectTimezone = "Europe/Amsterdam"
		};

		// Act
		var results = await service.CorrectTimezoneAsync(fileIndexItems, request);

		// Assert
		Assert.HasCount(2, results);
		Assert.IsTrue(results[0].Success);
		Assert.IsFalse(results[1].Success);
		
		// Query
		var queryResult = await fakeQuery.GetObjectByFilePathAsync("/test1.jpg");
		Assert.IsNotNull(queryResult);
		Assert.AreEqual("/test1.jpg", queryResult.FilePath);
		Assert.AreEqual("2024-06-15 16:00:00", queryResult.DateTime.ToString("yyyy-MM-dd HH:mm:ss"));
		var queryResultMissing = await fakeQuery.GetObjectByFilePathAsync("/nonexistent.jpg");
		Assert.IsNotNull(queryResultMissing);
		Assert.AreEqual("2024-06-15 14:00:00", queryResultMissing.DateTime.ToString("yyyy-MM-dd HH:mm:ss"));
	}

	[TestMethod]
	public async Task CorrectTimezoneAsync_BatchEmpty_ShouldReturnEmpty()
	{
		// Arrange
		var service = CreateService();
		var fileIndexItems = new List<FileIndexItem>();

		var request = new ExifTimezoneCorrectionRequest
		{
			RecordedTimezone = "UTC", CorrectTimezone = "Europe/Amsterdam"
		};

		// Act
		var results = await service.CorrectTimezoneAsync(fileIndexItems, request);

		// Assert
		Assert.HasCount(0, results);
	}

	// ==================== Fractional Hour Offset Tests ====================

	[TestMethod]
	public async Task CorrectTimezoneAsync_FractionalHourOffset_Nepal()
	{
		// Arrange - Nepal has UTC+5:45
		var storage = new FakeIStorage(["/"], ["/test.jpg"]);
		var service = CreateService(storage: storage);

		var fileIndexItem = new FileIndexItem
		{
			FilePath = "/test.jpg",
			DateTime = new DateTime(2024, 6, 15, 14, 0, 0, DateTimeKind.Local)
		};
		var request = new ExifTimezoneCorrectionRequest
		{
			RecordedTimezone = "UTC", CorrectTimezone = "Asia/Kathmandu" // UTC+5:45
		};

		// Act
		var result = await service.CorrectTimezoneAsync(fileIndexItem, request);

		// Assert
		Assert.IsTrue(result.Success);
		Assert.AreEqual(5.75, result.DeltaHours); // 5 hours 45 minutes
		Assert.AreEqual(new DateTime(2024, 6, 15, 19, 45, 0, DateTimeKind.Local),
			result.CorrectedDateTime);
	}

	// ==================== Validation Tests ====================

	[TestMethod]
	public async Task Validate_WithFakeIQueryIStorage_ValidFile_ReturnsSuccess()
	{
		var fileIndexItem = new FileIndexItem
		{
			FilePath = "/test.jpg",
			DateTime = new DateTime(2024, 6, 30, 23, 0, 0, DateTimeKind.Local)
		};
		var fakeQuery = new FakeIQuery([fileIndexItem]);
		var fakeStorage = new FakeIStorage([], 
			[fileIndexItem.FilePath]);
		var service = CreateService(storage: fakeStorage, query: fakeQuery);
		var request = new ExifTimezoneCorrectionRequest
		{
			CorrectTimezone = "Europe/Amsterdam",
			RecordedTimezone = "UTC",
		};
		var results = await service.
			Validate([fileIndexItem.FilePath], false, request);
		Assert.HasCount(1, results);
		Assert.HasCount(0, results[0].Error);
		Assert.AreEqual(fileIndexItem.DateTime, results[0].OriginalDateTime);
	}

	[TestMethod]
	public async Task Validate_WithFakeIQueryIStorage_FileDoesNotExist_ReturnsError()
	{
		var fileIndexItem = new FileIndexItem
		{
			FilePath = "/notfound.jpg",
			DateTime = new DateTime(2024, 6, 30, 23, 0, 0, DateTimeKind.Local)
		};
		var fakeQuery = new FakeIQuery(new List<FileIndexItem> { fileIndexItem });
		var fakeStorage = new FakeIStorage(new List<string>()); // No files exist
		var service = CreateService(storage: fakeStorage, query: fakeQuery);
		var request = new ExifTimezoneCorrectionRequest
		{
			RecordedTimezone = "Central European Standard Time",
			CorrectTimezone = "Central European Summer Time"
		};
		var results = await service.Validate(new[] { fileIndexItem.FilePath }, false, request);
		Assert.HasCount(1, results);
		Assert.AreEqual("File does not exist", results[0].Error);
	}

	[TestMethod]
	public async Task CorrectTimezoneAsync_WithFakeIQueryException_ReturnsError()
	{
		// Arrange
		var fileIndexItem = new FileIndexItem
		{
			FilePath = "/test.jpg",
			DateTime = new DateTime(2024, 6, 30, 23, 0, 0, DateTimeKind.Local)
		};
		var fakeQuery = new FakeIQueryException(new Exception()); // Simulates exception on query
		var fakeStorage = new FakeIStorage([],[fileIndexItem.FilePath]);
		var service = CreateService(storage: fakeStorage, query: fakeQuery);
		var request = new ExifTimezoneCorrectionRequest
		{
			RecordedTimezone = "UTC", // Camera thought it was UTC (GMT+00)
			CorrectTimezone = "Europe/Amsterdam" // Actually in Amsterdam (GMT+02 in summer)
		};

		// Act
		var result = await service.CorrectTimezoneAsync(fileIndexItem, request); 
		
		// Assert
		Assert.IsNotNull(result.Error);
		Assert.Contains("[ExifTimezoneCorrection] Exception", result.Error.ToLowerInvariant());
	}

	[TestMethod]
	public void ValidateCorrection_MultipleWarnings_DayRolloverAndSameTimezone()
	{
		// Arrange
		var service = CreateService();
		var fileIndexItem = new FileIndexItem
		{
			FilePath = "/test.jpg",
			DateTime = new DateTime(2024, 6, 15, 23, 30, 0, DateTimeKind.Local)
		};
		var request = new ExifTimezoneCorrectionRequest
		{
			RecordedTimezone = "Europe/Amsterdam", CorrectTimezone = "Europe/Amsterdam"
		};

		// Act
		var result = service.ValidateCorrection(fileIndexItem, request);

		// Assert
		// Should have warning about same timezone
		Assert.IsNotNull(result.Warning);
		Assert.Contains("same", result.Warning.ToLower());
	}

	[TestMethod]
	public void ValidateCorrection_NullWhitespaceTimezone_ShouldReturnError()
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
			RecordedTimezone = "   ", // Whitespace only
			CorrectTimezone = "Europe/Amsterdam"
		};

		// Act
		var result = service.ValidateCorrection(fileIndexItem, request);

		// Assert
		Assert.IsFalse(result.Success);
		Assert.AreEqual("Recorded timezone is required", result.Error);
	}
}
