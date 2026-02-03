using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.rename.DateTimeRepair.Models;
using starsky.feature.rename.DateTimeRepair.Services;
using starsky.feature.rename.Models;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.metaupdate.Models;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.rename.DateTimeRepair.Services;

[TestClass]
public class FilenameDatetimeRepairServiceTest
{
	private static FilenameDatetimeRepairService CreateSut(FakeIStorage? storage = null,
		IQuery? query = null, FakeIWebLogger? logger = null)
	{
		query ??= new FakeIQuery();
		storage ??= new FakeIStorage();
		logger ??= new FakeIWebLogger();
		return new FilenameDatetimeRepairService(query, storage, logger,
			new AppSettings { ReadOnlyFolders = ["/readonly"] });
	}

	[TestMethod]
	public void PreviewRepair_NoDatetimePattern_ReturnsError()
	{
		// Arrange
		var sut = CreateSut(new FakeIStorage(
				["/test"],
				["/test/image.jpg"]),
			new FakeIQuery([new FileIndexItem("/test/image.jpg")]));
		var filePaths = new List<string> { "/test/image.jpg" };
		var request = new ExifTimezoneBasedCorrectionRequest
		{
			RecordedTimezoneId = "UTC", CorrectTimezoneId = "Europe/Amsterdam"
		};

		// Act
		var result = sut.PreviewRepair(filePaths, request);

		// Assert
		Assert.HasCount(1, result);
		Assert.IsTrue(result[0].HasError);
		Assert.IsTrue(result[0].ErrorMessage?.Contains("No datetime pattern detected"));
	}

	[TestMethod]
	public void PreviewRepair_WithYYYYMMDD_HHMMSS_Pattern_DetectsCorrectly()
	{
		// Arrange
		var query = new FakeIQuery([
			new FileIndexItem("/test/20240313_011530_IMG_001.jpg")
			{
				FileName = "20240313_011530_IMG_001.jpg",
				ParentDirectory = "/test",
				Status = FileIndexItem.ExifStatus.Ok
			}
		]);
		var storage = new FakeIStorage();
		var logger = new FakeIWebLogger();
		var sut = new FilenameDatetimeRepairService(query, storage, logger, new AppSettings());

		var filePaths = new List<string> { "/test/20240313_011530_IMG_001.jpg" };
		var request = new ExifTimezoneBasedCorrectionRequest
		{
			RecordedTimezoneId = "UTC", CorrectTimezoneId = "Europe/Amsterdam"
		};

		// Act
		var result = sut.PreviewRepair(filePaths, request);

		// Assert
		Assert.HasCount(1, result);
		Assert.IsFalse(result[0].HasError);
		Assert.AreEqual("YYYYMMDD_HHMMSS", result[0].DetectedPatternDescription);
		Assert.AreEqual(new DateTime(2024, 3, 13,
			1, 15, 30, DateTimeKind.Local), result[0].OriginalDateTime);
		Assert.IsNotNull(result[0].CorrectedDateTime);
		Assert.Contains("20240313_", result[0].TargetFilePath);
	}

	[TestMethod]
	public void PreviewRepair_WithYYYYMMDD_HHMM_Pattern_DetectsCorrectly()
	{
		// Arrange
		var query = new FakeIQuery([
			new FileIndexItem("/test/20240313_0115_photo.jpg")
			{
				FileName = "20240313_0115_photo.jpg",
				ParentDirectory = "/test",
				Status = FileIndexItem.ExifStatus.Ok
			}
		]);
		var storage = new FakeIStorage();
		var logger = new FakeIWebLogger();
		var sut = new FilenameDatetimeRepairService(query, storage, logger, new AppSettings());

		var filePaths = new List<string> { "/test/20240313_0115_photo.jpg" };
		var request = new ExifTimezoneBasedCorrectionRequest
		{
			RecordedTimezoneId = "UTC", CorrectTimezoneId = "UTC"
		};

		// Act
		var result = sut.PreviewRepair(filePaths, request);

		// Assert
		Assert.HasCount(1, result);
		Assert.IsFalse(result[0].HasError);
		Assert.AreEqual("YYYYMMDD_HHMM", result[0].DetectedPatternDescription);
		Assert.AreEqual(new DateTime(2024, 3, 13,
			1, 15, 0, DateTimeKind.Local), result[0].OriginalDateTime);
	}

	[TestMethod]
	public void PreviewRepair_WithYYYYMMDD_Pattern_DetectsCorrectly()
	{
		// Arrange
		var query = new FakeIQuery([
			new FileIndexItem("/test/20240313_vacation.jpg")
			{
				FileName = "20240313_vacation.jpg",
				ParentDirectory = "/test",
				Status = FileIndexItem.ExifStatus.Ok
			}
		]);
		var storage = new FakeIStorage();
		var logger = new FakeIWebLogger();
		var sut = new FilenameDatetimeRepairService(query, storage, logger, new AppSettings());

		var filePaths = new List<string> { "/test/20240313_vacation.jpg" };
		var request = new ExifTimezoneBasedCorrectionRequest
		{
			RecordedTimezoneId = "UTC", CorrectTimezoneId = "UTC"
		};

		// Act
		var result = sut.PreviewRepair(filePaths, request);

		// Assert
		Assert.HasCount(1, result);
		Assert.IsFalse(result[0].HasError);
		Assert.AreEqual("YYYYMMDD", result[0].DetectedPatternDescription);
		Assert.AreEqual(new DateTime(2024, 3, 13,
			0, 0, 0, DateTimeKind.Local), result[0].OriginalDateTime);
	}

	[TestMethod]
	public void PreviewRepair_WithTimezoneOffset_CalculatesCorrectly()
	{
		// Arrange
		var query = new FakeIQuery([
			new FileIndexItem("/test/20240313_011530_IMG_001.jpg")
			{
				FileName = "20240313_011530_IMG_001.jpg",
				ParentDirectory = "/test",
				Status = FileIndexItem.ExifStatus.Ok
			}
		]);
		var storage = new FakeIStorage();
		var logger = new FakeIWebLogger();
		var sut = new FilenameDatetimeRepairService(query, storage, logger, new AppSettings());

		var filePaths = new List<string> { "/test/20240313_011530_IMG_001.jpg" };
		var request = new ExifTimezoneBasedCorrectionRequest
		{
			RecordedTimezoneId = "UTC",
			CorrectTimezoneId = "Europe/Amsterdam" // UTC+1 in winter, UTC+2 in summer
		};

		// Act
		var result = sut.PreviewRepair(filePaths, request);

		// Assert
		Assert.HasCount(1, result);
		Assert.IsFalse(result[0].HasError);
		Assert.AreEqual(new DateTime(2024, 3, 13,
			1, 15, 30, DateTimeKind.Local), result[0].OriginalDateTime);
		// 2024-03-13 is after DST change (March 31 in Europe), so UTC+1
		Assert.AreEqual(new DateTime(2024, 3, 13,
			2, 15, 30, DateTimeKind.Local), result[0].CorrectedDateTime);
		Assert.AreEqual("/test/20240313_021530_IMG_001.jpg", result[0].TargetFilePath);
		Assert.AreEqual(1.0, result[0].OffsetHours);
	}

	[TestMethod]
	public void PreviewRepair_WithCustomOffset_OneHour_CalculatesCorrectly()
	{
		// Arrange
		var query = new FakeIQuery([
			new FileIndexItem("/test/20240313_011530_IMG_001.jpg")
			{
				FileName = "20240313_011530_IMG_001.jpg",
				ParentDirectory = "/test",
				Status = FileIndexItem.ExifStatus.Ok
			}
		]);
		var storage = new FakeIStorage();
		var logger = new FakeIWebLogger();
		var sut = new FilenameDatetimeRepairService(query, storage, logger, new AppSettings());

		var filePaths = new List<string> { "/test/20240313_011530_IMG_001.jpg" };
		var request = new ExifCustomOffsetCorrectionRequest
		{
			Year = 0,
			Month = 0,
			Day = 0,
			Hour = 1,
			Minute = 0,
			Second = 0
		};

		// Act
		var result = sut.PreviewRepair(filePaths, request);

		// Assert
		Assert.HasCount(1, result);
		Assert.IsFalse(result[0].HasError);
		Assert.AreEqual(new DateTime(2024, 3, 13,
			2, 15, 30, DateTimeKind.Local), result[0].CorrectedDateTime);
		Assert.AreEqual("/test/20240313_021530_IMG_001.jpg", result[0].TargetFilePath);
		Assert.AreEqual(1.0, result[0].OffsetHours);
	}

	[TestMethod]
	public void PreviewRepair_WithCustomOffset_MultipleComponents_CalculatesCorrectly()
	{
		// Arrange
		var query = new FakeIQuery([
			new FileIndexItem("/test/20240313_011530_IMG_001.jpg")
			{
				FileName = "20240313_011530_IMG_001.jpg",
				ParentDirectory = "/test",
				Status = FileIndexItem.ExifStatus.Ok
			}
		]);
		var storage = new FakeIStorage();
		var logger = new FakeIWebLogger();
		var sut = new FilenameDatetimeRepairService(query, storage, logger, new AppSettings());

		var filePaths = new List<string> { "/test/20240313_011530_IMG_001.jpg" };
		var request = new ExifCustomOffsetCorrectionRequest
		{
			Year = 1,
			Month = 1,
			Day = 1,
			Hour = 2,
			Minute = 30,
			Second = 45
		};

		// Act
		var result = sut.PreviewRepair(filePaths, request);

		// Assert
		Assert.HasCount(1, result);
		Assert.IsFalse(result[0].HasError);
		// Original: 2024-03-13 01:15:30
		// +1 year, +1 month, +1 day, +2h 30m 45s = 2025-04-14 03:46:15
		Assert.AreEqual(new DateTime(2025, 4, 14,
			3, 46, 15, DateTimeKind.Local), result[0].CorrectedDateTime);
		Assert.AreEqual("/test/20250414_034615_IMG_001.jpg", result[0].TargetFilePath);
	}

	[TestMethod]
	public void PreviewRepair_DayRollover_ShowsWarning()
	{
		// Arrange
		var query = new FakeIQuery([
			new FileIndexItem("/test/20240313_233000_IMG_001.jpg")
			{
				FileName = "20240313_233000_IMG_001.jpg",
				ParentDirectory = "/test",
				Status = FileIndexItem.ExifStatus.Ok
			}
		]);
		var storage = new FakeIStorage();
		var logger = new FakeIWebLogger();
		var sut = new FilenameDatetimeRepairService(query, storage, logger, new AppSettings());

		var filePaths = new List<string> { "/test/20240313_233000_IMG_001.jpg" };
		var request = new ExifCustomOffsetCorrectionRequest
		{
			Hour = 2 // Will roll to next day
		};

		// Act
		var result = sut.PreviewRepair(filePaths, request);

		// Assert
		Assert.HasCount(1, result);
		Assert.IsFalse(result[0].HasError);
		Assert.IsNotNull(result[0].Warning);
		Assert.Contains("change the day", result[0].Warning!);
		Assert.AreEqual(new DateTime(2024, 3, 14,
			1, 30, 0, DateTimeKind.Local), result[0].CorrectedDateTime);
	}

	[TestMethod]
	public void PreviewRepair_WithCollections_IncludesRelatedFiles()
	{
		// Arrange
		var query = new FakeIQuery([
			new FileIndexItem("/test/20240313_011530_IMG_001.jpg")
			{
				FileName = "20240313_011530_IMG_001.jpg",
				ParentDirectory = "/test",
				Status = FileIndexItem.ExifStatus.Ok,
				CollectionPaths =
					["/test/20240313_011530_IMG_001.jpg", "/test/20240313_011530_IMG_001.xmp"]
			},
			new FileIndexItem("/test/20240313_011530_IMG_001.xmp")
			{
				FileName = "20240313_011530_IMG_001.xmp",
				ParentDirectory = "/test",
				Status = FileIndexItem.ExifStatus.Ok,
				CollectionPaths =
					["/test/20240313_011530_IMG_001.jpg", "/test/20240313_011530_IMG_001.xmp"]
			}
		]);
		var storage = new FakeIStorage(["/test"],
		[
			"/test/20240313_011530_IMG_001.jpg",
			"/test/20240313_011530_IMG_001.xmp"
		]);
		var logger = new FakeIWebLogger();
		var sut = new FilenameDatetimeRepairService(query, storage, logger, new AppSettings());

		var filePaths = new List<string> { "/test/20240313_011530_IMG_001.jpg" };
		var request = new ExifCustomOffsetCorrectionRequest { Hour = 1 };

		// Act
		var result = sut.PreviewRepair(filePaths, request);

		// Assert
		Assert.HasCount(2, result);
		Assert.IsFalse(result[0].HasError);
		Assert.IsFalse(result[1].HasError);
	}

	[TestMethod]
	public void PreviewRepair_FileNotFound_ReturnsError()
	{
		// Arrange
		var query = new FakeIQuery(); // Empty query
		var storage = new FakeIStorage();
		var logger = new FakeIWebLogger();
		var sut = new FilenameDatetimeRepairService(query, storage, logger, new AppSettings());

		var filePaths = new List<string> { "/test/nonexistent.jpg" };
		var request = new ExifCustomOffsetCorrectionRequest { Hour = 1 };

		// Act
		var result = sut.PreviewRepair(filePaths, request);

		// Assert
		Assert.HasCount(1, result);
		Assert.IsTrue(result[0].HasError);
		Assert.IsTrue(result[0].ErrorMessage?.Contains("File not found"));
	}


	[TestMethod]
	public async Task ExecuteRepairAsync_Mapping_FileNotFound()
	{
		// Arrange
		var query = new FakeIQuery();
		var storage = new FakeIStorage();
		var logger = new FakeIWebLogger();
		var sut = new FilenameDatetimeRepairService(query, storage, logger, new AppSettings());

		var mappings = new List<FilenameDatetimeRepairMapping>
		{
			new()
			{
				SourceFilePath = "/test/20240313_011530_IMG_001.jpg",
				TargetFilePath = "/test/20240313_021530_IMG_001.jpg",
				HasError = false,
				FileIndexItem = null
			}
		};

		// Act
		var result = await sut.ExecuteRepairAsync(mappings);

		// Assert
		Assert.HasCount(0, result, "No items should be returned");
		Assert.Contains("File not found", logger.TrackedExceptions.LastOrDefault().Item2!);
	}
	
	[TestMethod]
	public async Task ExecuteRepairAsync_Exception_Catched()
	{
		// Arrange
		var query = new FakeIQueryException(new ApplicationException());
		var storage = new FakeIStorage();
		var logger = new FakeIWebLogger();
		var sut = new FilenameDatetimeRepairService(query, storage, logger, new AppSettings());

		var mappings = new List<FilenameDatetimeRepairMapping>
		{
			new()
			{
				SourceFilePath = "/test/20240313_011530_IMG_001.jpg",
				TargetFilePath = "/test/20240313_021530_IMG_001.jpg",
				HasError = false,
				FileIndexItem = null
			}
		};

		// Act
		var result = await sut.ExecuteRepairAsync(mappings);

		// Assert
		Assert.HasCount(0, result, "No items should be returned");
		Assert.Contains("Failed to rename", logger.TrackedExceptions.LastOrDefault().Item2!);
	}

	[TestMethod]
	public async Task ExecuteRepairAsync_ValidMapping_RenamesFile()
	{
		// Arrange
		var query = new FakeIQuery([
			new FileIndexItem("/test/20240313_011530_IMG_001.jpg")
			{
				FileName = "20240313_011530_IMG_001.jpg",
				ParentDirectory = "/test",
				Status = FileIndexItem.ExifStatus.Ok
			}
		]);
		var storage = new FakeIStorage(["/test/20240313_011530_IMG_001.jpg"]);
		var logger = new FakeIWebLogger();
		var sut = new FilenameDatetimeRepairService(query, storage, logger, new AppSettings());

		var mappings = new List<FilenameDatetimeRepairMapping>
		{
			new()
			{
				SourceFilePath = "/test/20240313_011530_IMG_001.jpg",
				TargetFilePath = "/test/20240313_021530_IMG_001.jpg",
				HasError = false
			}
		};

		// Act
		var result = await sut.ExecuteRepairAsync(mappings);

		// Assert
		Assert.HasCount(2, result, "Should return both deleted and new item");

		var deleted = result.FirstOrDefault(x => x.Status == FileIndexItem.ExifStatus.Deleted);
		var ok = result.FirstOrDefault(x => x.Status == FileIndexItem.ExifStatus.Ok);

		Assert.IsNotNull(deleted, "Deleted item should be present");
		Assert.IsNotNull(ok, "Ok item should be present");

		Assert.AreEqual("20240313_011530_IMG_001.jpg", deleted.FileName);
		Assert.AreEqual("/test/20240313_011530_IMG_001.jpg", deleted.FilePath);

		Assert.AreEqual("20240313_021530_IMG_001.jpg", ok.FileName);
		Assert.AreEqual("/test/20240313_021530_IMG_001.jpg", ok.FilePath);
	}

	[TestMethod]
	public async Task ExecuteRepairAsync_WithError_SkipsFile()
	{
		// Arrange
		var sut = CreateSut();
		var mappings = new List<FilenameDatetimeRepairMapping>
		{
			new()
			{
				SourceFilePath = "/test/image.jpg",
				TargetFilePath = "/test/image_new.jpg",
				HasError = true,
				ErrorMessage = "Test error"
			}
		};

		// Act
		var result = await sut.ExecuteRepairAsync(mappings);

		// Assert
		Assert.IsEmpty(result);
	}

	[TestMethod]
	public async Task ExecuteRepairAsync_SameSourceAndTarget_SkipsFile()
	{
		// Arrange
		var query = new FakeIQuery([
			new FileIndexItem("/test/image.jpg")
			{
				FileName = "image.jpg",
				ParentDirectory = "/test",
				Status = FileIndexItem.ExifStatus.Ok
			}
		]);
		var storage = new FakeIStorage();
		var logger = new FakeIWebLogger();
		var sut = new FilenameDatetimeRepairService(query, storage, logger, new AppSettings());

		var mappings = new List<FilenameDatetimeRepairMapping>
		{
			new()
			{
				SourceFilePath = "/test/image.jpg",
				TargetFilePath = "/test/image.jpg", // Same!
				HasError = false
			}
		};

		// Act
		var result = await sut.ExecuteRepairAsync(mappings);

		// Assert
		Assert.HasCount(1, result);
		Assert.IsTrue(logger.TrackedInformation.Any(x => x.Item2?.Contains("same") == true));
	}

	[TestMethod]
	public async Task ExecuteRepairAsync_WithRelatedFiles_RenamesSidecars()
	{
		// Arrange
		var query = new FakeIQuery([
			new FileIndexItem("/test/20240313_011530_IMG_001.jpg")
			{
				FileName = "20240313_011530_IMG_001.jpg",
				ParentDirectory = "/test",
				Status = FileIndexItem.ExifStatus.Ok
			}
		]);
		var storage = new FakeIStorage(["/", "/test"], [
				"/test/20240313_011530_IMG_001.jpg", "/test/20240313_011530_IMG_001.xmp"
			]
		);
		var logger = new FakeIWebLogger();
		var sut = new FilenameDatetimeRepairService(query, storage, logger, new AppSettings());

		var mappings = new List<FilenameDatetimeRepairMapping>
		{
			new()
			{
				SourceFilePath = "/test/20240313_011530_IMG_001.jpg",
				TargetFilePath = "/test/20240313_021530_IMG_001.jpg",
				RelatedFilePaths =
				[
					new ValueTuple<string, string>("/test/20240313_011530_IMG_001.jpg",
						"/test/20240313_021530_IMG_001.jpg"),
					new ValueTuple<string, string>("/test/20240313_011530_IMG_001.xmp",
						"/test/20240313_021530_IMG_001.xmp")
				],
				HasError = false
			}
		};

		// Act
		var fileItems = await sut.ExecuteRepairAsync(mappings);

		// Assert
		Assert.HasCount(1,
			fileItems.Where(p
				=> p.Status == FileIndexItem.ExifStatus.Ok).ToList());
		Assert.HasCount(1,
			fileItems.Where(p
				=> p.Status == FileIndexItem.ExifStatus.Deleted).ToList());
		// Verify sidecar was renamed
		Assert.IsTrue(storage.ExistFile("/test/20240313_021530_IMG_001.xmp"));
	}

	[TestMethod]
	public void PreviewRepair_MultipleFFiles_ProcessesAll()
	{
		// Arrange
		var query = new FakeIQuery([
			new FileIndexItem("/test/20240313_011530_IMG_001.jpg")
			{
				FileName = "20240313_011530_IMG_001.jpg",
				ParentDirectory = "/test",
				Status = FileIndexItem.ExifStatus.Ok
			},

			new FileIndexItem("/test/20240313_021530_IMG_002.jpg")
			{
				FileName = "20240313_021530_IMG_002.jpg",
				ParentDirectory = "/test",
				Status = FileIndexItem.ExifStatus.Ok
			},

			new FileIndexItem("/test/image_no_pattern.jpg")
			{
				FileName = "image_no_pattern.jpg",
				ParentDirectory = "/test",
				Status = FileIndexItem.ExifStatus.Ok
			}
		]);
		var filePaths = new List<string>
		{
			"/test/20240313_011530_IMG_001.jpg",
			"/test/20240313_021530_IMG_002.jpg",
			"/test/image_no_pattern.jpg"
		};

		var storage = new FakeIStorage(["/test"], filePaths);
		var logger = new FakeIWebLogger();
		var sut = new FilenameDatetimeRepairService(query, storage, logger, new AppSettings());

		var request = new ExifCustomOffsetCorrectionRequest { Hour = 1 };

		// Act
		var result = sut.PreviewRepair(filePaths, request)
			.OrderBy(p => p.SourceFilePath).ToList();

		// Assert
		Assert.HasCount(3, result);
		Assert.IsFalse(result[0].HasError);
		Assert.IsFalse(result[1].HasError);
		Assert.IsTrue(result[2].HasError); // No pattern in filename
	}

	[TestMethod]
	public void PreviewRepair_NegativeOffset_MovesBackward()
	{
		// Arrange
		var query = new FakeIQuery([
			new FileIndexItem("/test/20240313_051530_IMG_001.jpg")
			{
				FileName = "20240313_051530_IMG_001.jpg",
				ParentDirectory = "/test",
				Status = FileIndexItem.ExifStatus.Ok
			}
		]);
		var storage = new FakeIStorage();
		var logger = new FakeIWebLogger();
		var sut = new FilenameDatetimeRepairService(query, storage, logger, new AppSettings());

		var filePaths = new List<string> { "/test/20240313_051530_IMG_001.jpg" };
		var request = new ExifCustomOffsetCorrectionRequest
		{
			Hour = -2 // Negative offset
		};

		// Act
		var result = sut.PreviewRepair(filePaths, request);

		// Assert
		Assert.HasCount(1, result);
		Assert.IsFalse(result[0].HasError);
		Assert.AreEqual(new DateTime(2024, 3, 13,
			3, 15, 30, DateTimeKind.Local), result[0].CorrectedDateTime);
		Assert.AreEqual("/test/20240313_031530_IMG_001.jpg", result[0].TargetFilePath);
		Assert.AreEqual(-2.0, result[0].OffsetHours);
	}

	[TestMethod]
	[DataRow("/readonly/20230101_120000.jpg", "Read-only location")]
	[DataRow("/test", "Is a directory")]
	[DataRow("/test/file_without_pattern.txt", "No datetime pattern detected in filename")]
	public void PreviewRepair_Errors(string filePath, string expectedErrorMessage)
	{
		// Arrange
		var sut = CreateSut(null, new FakeIQuery([
			new FileIndexItem("/readonly/20230101_120000.jpg"),
			new FileIndexItem("/test/file_without_pattern.txt"),
			new FileIndexItem("/test") { IsDirectory = true }
		]));

		var filePaths = new List<string> { filePath };
		var correctionRequest = new ExifTimezoneBasedCorrectionRequest
		{
			RecordedTimezoneId = "UTC", CorrectTimezoneId = "UTC"
		};

		// Act
		var result = sut.PreviewRepair(filePaths, correctionRequest);

		// Assert
		Assert.HasCount(1, result);
		Assert.IsTrue(result[0].HasError);
		Assert.AreEqual(expectedErrorMessage, result[0].ErrorMessage);
		Assert.AreEqual(filePath, result[0].SourceFilePath);
	}

	[TestMethod]
	public void PreviewRepair_TestIfExceptionIsCatched()
	{
		// Arrange
		var logger = new FakeIWebLogger();
		var sut = CreateSut(null,
			new FakeIQueryException(new ApplicationException()), logger);

		var filePaths = new List<string> { "/test/20230101_120000.jpg" };
		var correctionRequest = new ExifTimezoneBasedCorrectionRequest
		{
			RecordedTimezoneId = "UTC", CorrectTimezoneId = "UTC"
		};

		// Act
		sut.PreviewRepair(filePaths, correctionRequest);

		// Assert
		Assert.Contains("[FileItemsQueryHelpers]: Failed",
			logger.TrackedExceptions.LastOrDefault().Item2!);
	}

	private static DateTimePattern CreatePattern(string regex, string format,
		string description = "")
	{
		return new DateTimePattern
		{
			Regex = new Regex(regex, RegexOptions.None, TimeSpan.FromSeconds(1)),
			Format = format,
			Description = description
		};
	}

	[TestMethod]
	public void ExtractDateTime_ValidPattern_ValidDateTime()
	{
		var pattern = CreatePattern(@"\d{8}_\d{6}", "yyyyMMdd_HHmmss");
		const string fileName = "20240313_011530_IMG_001.jpg";
		var result = FilenameDatetimeRepairService.ExtractDateTime(fileName, pattern);
		Assert.IsNotNull(result);
		Assert.AreEqual(new DateTime(2024, 3, 13,
			1, 15, 30, DateTimeKind.Local), result.Value);
	}

	[TestMethod]
	public void ExtractDateTime_ValidPattern_ValidDateTime_OnlyDate()
	{
		var pattern = CreatePattern(@"\d{8}", "yyyyMMdd");
		const string fileName = "20240313_vacation.jpg";
		var result = FilenameDatetimeRepairService.ExtractDateTime(fileName, pattern);
		Assert.IsNotNull(result);
		Assert.AreEqual(new DateTime(2024, 3, 13,
			0, 0, 0, DateTimeKind.Local), result.Value);
	}

	[TestMethod]
	public void ExtractDateTime_InvalidPattern_NoMatch_ReturnsNull()
	{
		var pattern = CreatePattern(@"\\d{8}_\\d{6}", "yyyyMMdd_HHmmss");
		const string fileName = "IMG_001.jpg";
		var result = FilenameDatetimeRepairService.ExtractDateTime(fileName, pattern);
		Assert.IsNull(result);
	}
	
	[TestMethod]
	public void ExtractDateTime_InvalidPattern_NoDateTimeMatch_ReturnsNull()
	{
		var pattern = CreatePattern(@"\w", "word");
		const string fileName = "IMG_001.jpg";
		var result = FilenameDatetimeRepairService.ExtractDateTime(fileName, pattern);
		Assert.IsNull(result);
	}

	[TestMethod]
	public void ExtractDateTime_ValidPattern_InvalidDateTime_ReturnsNull()
	{
		var pattern = CreatePattern(@"\\d{8}_\\d{6}", "yyyyMMdd_HHmmss");
		const string fileName = "20241313_011530_IMG_001.jpg"; // Invalid month 13
		var result = FilenameDatetimeRepairService.ExtractDateTime(fileName, pattern);
		Assert.IsNull(result);
	}

	[TestMethod]
	public void ExtractDateTime_ValidPattern_ValidDateTime_WithDifferentFormat()
	{
		var pattern = CreatePattern(@"\d{4}-\d{2}-\d{2}_\d{2}-\d{2}-\d{2}",
			"yyyy-MM-dd_HH-mm-ss");

		const string fileName = "2024-03-13_01-15-30_IMG_001.jpg";
		var result = FilenameDatetimeRepairService.ExtractDateTime(fileName, pattern);
		Assert.IsNotNull(result);
		Assert.AreEqual(new DateTime(2024, 3, 13,
			1, 15, 30, DateTimeKind.Local), result.Value);
	}

	[TestMethod]
	[DataRow("20240313_011530_IMG_001.jpg", "20240313_021530_IMG_001.jpg", @"\d{8}_\d{6}",
		"yyyyMMdd_HHmmss", 2024, 3, 13, 2, 15, 30)]
	[DataRow("20240313_vacation.jpg", "20240414_vacation.jpg", "\\d{8}", "yyyyMMdd", 2024, 4, 14, 0,
		0, 0)]
	[DataRow("IMG_001.jpg", "IMG_001.jpg", "\\d{8}_\\d{6}", "yyyyMMdd_HHmmss", 2024, 3, 13, 2, 15,
		30)] // No match, should return original
	[DataRow("2024-03-13_01-15-30_IMG_001.jpg", "2025-04-14_03-46-15_IMG_001.jpg",
		@"\d{4}-\d{2}-\d{2}_\d{2}-\d{2}-\d{2}", "yyyy-MM-dd_HH-mm-ss", 2025, 4, 14, 3, 46, 15)]
	[DataRow("011530_IMG_001.jpg", "121530_IMG_001.jpg", "\\d{6}", "HHmmss", 12, 1, 1, 12, 15,
		30)] // Time only, date part is default
	[SuppressMessage("Usage",
		"S107: Constructor has 10 parameters, which is greater than the 7 authorized")]
	public void ReplaceDateTime_VariousCases_ReturnsExpected(
		string fileName, string expected, string regex, string format,
		int year, int month, int day, int hour, int minute, int second)
	{
		var pattern = CreatePattern(regex, format);
		var correctedDateTime =
			new DateTime(year, month, day, hour, minute, second, DateTimeKind.Local);
		var result =
			FilenameDatetimeRepairService.ReplaceDateTime(fileName, pattern, correctedDateTime);
		Assert.AreEqual(expected, result);
	}
}
