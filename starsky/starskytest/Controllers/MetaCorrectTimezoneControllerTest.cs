using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Controllers;
using starsky.feature.realtime.Interface;
using starsky.foundation.database.Models;
using starsky.foundation.metaupdate.Interfaces;
using starsky.foundation.metaupdate.Models;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.worker.Interfaces;
using starskytest.FakeMocks;

namespace starskytest.Controllers;

[TestClass]
public sealed class MetaCorrectTimezoneControllerTest
{
	private static MetaCorrectTimezoneController CreateController(
		IExifTimezoneCorrectionService? timezoneService = null,
		IUpdateBackgroundTaskQueue? queue = null,
		IWebLogger? logger = null,
		IServiceScopeFactory? scopeFactory = null)
	{
		timezoneService ??= new FakeIExifTimezoneCorrectionService();
		queue ??= new FakeIUpdateBackgroundTaskQueue();
		logger ??= new FakeIWebLogger();
		scopeFactory ??= new FakeServiceScopeFactory();

		var controller = new MetaCorrectTimezoneController(
			timezoneService,
			queue,
			logger,
			scopeFactory) { ControllerContext = { HttpContext = new DefaultHttpContext() } };

		return controller;
	}

	[TestMethod]
	public async Task PreviewTimezoneCorrectionAsync_ValidInput_ReturnsOkResult()
	{
		// Arrange
		var mockResults = new List<ExifTimezoneCorrectionResult>
		{
			new()
			{
				Success = true,
				OriginalDateTime = new DateTime(2024, 6, 15, 14, 30, 0, DateTimeKind.Local),
				CorrectedDateTime =
					new DateTime(2024, 6, 15, 16, 30, 0, DateTimeKind.Local),
				DeltaHours = 2.0,
				FileIndexItem = new FileIndexItem { FilePath = "/test.jpg" }
			}
		};

		var timezoneService = new FakeIExifTimezoneCorrectionService(mockResults);
		var controller = CreateController(timezoneService);

		var request = new ExifTimezoneCorrectionRequest
		{
			RecordedTimezone = "UTC", CorrectTimezone = "Europe/Amsterdam"
		};

		// Act
		var result = await controller.PreviewTimezoneCorrectionAsync(
			"/test.jpg",
			true,
			request);

		// Assert
		Assert.IsNotNull(result);
		var jsonResult = result as OkObjectResult;
		Assert.IsNotNull(jsonResult);
		Assert.AreEqual(200, jsonResult.StatusCode);

		var returnedResults = jsonResult.Value as List<ExifTimezoneCorrectionResult>;
		Assert.IsNotNull(returnedResults);
		Assert.HasCount(1, returnedResults);
		Assert.IsTrue(returnedResults[0].Success);
		Assert.AreEqual(2.0, returnedResults[0].DeltaHours);
	}

	[TestMethod]
	public async Task PreviewTimezoneCorrectionAsync_MultipleFiles_ReturnsAllResults()
	{
		// Arrange
		var mockResults = new List<ExifTimezoneCorrectionResult>
		{
			new()
			{
				Success = true,
				OriginalDateTime = new DateTime(2024, 6, 15, 14, 30, 0, DateTimeKind.Local),
				CorrectedDateTime =
					new DateTime(2024, 6, 15, 16, 30, 0, DateTimeKind.Local),
				DeltaHours = 2.0,
				FileIndexItem = new FileIndexItem { FilePath = "/test1.jpg" }
			},
			new()
			{
				Success = true,
				OriginalDateTime = new DateTime(2024, 6, 16, 10, 0, 0, DateTimeKind.Local),
				CorrectedDateTime = new DateTime(2024, 6, 16, 12, 0, 0, DateTimeKind.Local),
				DeltaHours = 2.0,
				FileIndexItem = new FileIndexItem { FilePath = "/test2.jpg" }
			}
		};

		var timezoneService = new FakeIExifTimezoneCorrectionService(mockResults);
		var controller = CreateController(timezoneService);

		var request = new ExifTimezoneCorrectionRequest
		{
			RecordedTimezone = "UTC", CorrectTimezone = "Europe/Amsterdam"
		};

		// Act
		var result = await controller.PreviewTimezoneCorrectionAsync(
			"/test1.jpg;/test2.jpg",
			true,
			request);

		// Assert
		var jsonResult = result as OkObjectResult;
		Assert.IsNotNull(jsonResult);
		var returnedResults = jsonResult.Value as List<ExifTimezoneCorrectionResult>;
		Assert.IsNotNull(returnedResults);
		Assert.HasCount(2, returnedResults);
	}

	[TestMethod]
	public async Task PreviewTimezoneCorrectionAsync_EmptyFilePath_ReturnsBadRequest()
	{
		// Arrange
		var controller = CreateController();
		var request = new ExifTimezoneCorrectionRequest
		{
			RecordedTimezone = "UTC", CorrectTimezone = "Europe/Amsterdam"
		};

		// Act
		var result = await controller.PreviewTimezoneCorrectionAsync(
			string.Empty,
			true,
			request);

		// Assert
		Assert.IsNotNull(result);
		var badRequestResult = result as BadRequestObjectResult;
		Assert.IsNotNull(badRequestResult);
		Assert.AreEqual(400, badRequestResult.StatusCode);
	}

	[TestMethod]
	public async Task PreviewTimezoneCorrectionAsync_NullCollections_ReturnsBadRequest()
	{
		// Arrange
		var controller = CreateController();
		var request = new ExifTimezoneCorrectionRequest
		{
			RecordedTimezone = "UTC", CorrectTimezone = "Europe/Amsterdam"
		};

		// Act
		var result = await controller.PreviewTimezoneCorrectionAsync(
			"/test.jpg",
			null,
			request);

		// Assert
		Assert.IsNotNull(result);
		var badRequestResult = result as BadRequestObjectResult;
		Assert.IsNotNull(badRequestResult);
		Assert.AreEqual(400, badRequestResult.StatusCode);
	}

	[TestMethod]
	public async Task PreviewTimezoneCorrectionAsync_ValidationErrors_ReturnsErrorResults()
	{
		// Arrange
		var mockResults = new List<ExifTimezoneCorrectionResult>
		{
			new()
			{
				Success = false,
				Error = "File does not exist",
				FileIndexItem = new FileIndexItem { FilePath = "/nonexistent.jpg" }
			}
		};

		var timezoneService = new FakeIExifTimezoneCorrectionService(mockResults);
		var controller = CreateController(timezoneService);

		var request = new ExifTimezoneCorrectionRequest
		{
			RecordedTimezone = "UTC", CorrectTimezone = "Europe/Amsterdam"
		};

		// Act
		var result = await controller.PreviewTimezoneCorrectionAsync(
			"/nonexistent.jpg",
			true,
			request);

		// Assert
		var jsonResult = result as OkObjectResult;
		Assert.IsNotNull(jsonResult);
		var returnedResults = jsonResult.Value as List<ExifTimezoneCorrectionResult>;
		Assert.IsNotNull(returnedResults);
		Assert.IsFalse(returnedResults[0].Success);
		Assert.AreEqual("File does not exist", returnedResults[0].Error);
	}

	[TestMethod]
	public async Task PreviewTimezoneCorrectionAsync_WithWarnings_ReturnsWarnings()
	{
		// Arrange
		var mockResults = new List<ExifTimezoneCorrectionResult>
		{
			new()
			{
				Success = true,
				OriginalDateTime = new DateTime(2024, 6, 30, 23, 0, 0, DateTimeKind.Local),
				CorrectedDateTime = new DateTime(2024, 7, 1, 11, 0, 0, DateTimeKind.Local),
				DeltaHours = 12.0,
				Warning = "Day rollover: correction will change the date",
				FileIndexItem = new FileIndexItem { FilePath = "/test.jpg" }
			}
		};

		var timezoneService = new FakeIExifTimezoneCorrectionService(mockResults);
		var controller = CreateController(timezoneService);

		var request = new ExifTimezoneCorrectionRequest
		{
			RecordedTimezone = "UTC", CorrectTimezone = "Europe/Amsterdam"
		};

		// Act
		var result = await controller.PreviewTimezoneCorrectionAsync(
			"/test.jpg",
			true,
			request);

		// Assert
		var jsonResult = result as OkObjectResult;
		Assert.IsNotNull(jsonResult);
		var returnedResults = jsonResult.Value as List<ExifTimezoneCorrectionResult>;
		Assert.IsNotNull(returnedResults);
		Assert.IsTrue(returnedResults[0].Success);
		Assert.AreEqual("Day rollover: correction will change the date",
			returnedResults[0].Warning);
	}

	[TestMethod]
	public async Task PreviewTimezoneCorrectionAsync_DSTScenario_ReturnsCorrectDelta()
	{
		// Arrange - DST fallback scenario
		var mockResults = new List<ExifTimezoneCorrectionResult>
		{
			new()
			{
				Success = true,
				OriginalDateTime = new DateTime(2024, 10, 26, 14, 0, 0, DateTimeKind.Local),
				CorrectedDateTime =
					new DateTime(2024, 10, 26, 15, 0, 0, DateTimeKind.Local),
				DeltaHours = 1.0,
				FileIndexItem = new FileIndexItem { FilePath = "/test.jpg" }
			}
		};

		var timezoneService = new FakeIExifTimezoneCorrectionService(mockResults);
		var controller = CreateController(timezoneService);

		var request = new ExifTimezoneCorrectionRequest
		{
			RecordedTimezone = "Europe/Amsterdam", // UTC+1 (after DST)
			CorrectTimezone = "Europe/Amsterdam" // UTC+2 (during DST)
		};

		// Act
		var result = await controller.PreviewTimezoneCorrectionAsync(
			"/test.jpg",
			true,
			request);

		// Assert
		var jsonResult = result as OkObjectResult;
		Assert.IsNotNull(jsonResult);
		var returnedResults = jsonResult.Value as List<ExifTimezoneCorrectionResult>;
		Assert.IsNotNull(returnedResults);
		Assert.AreEqual(1.0, returnedResults[0].DeltaHours);
	}

	[TestMethod]
	public async Task PreviewTimezoneCorrectionAsync_SameTimezone_ReturnsZeroDelta()
	{
		// Arrange
		var mockResults = new List<ExifTimezoneCorrectionResult>
		{
			new()
			{
				Success = true,
				OriginalDateTime = new DateTime(2024, 6, 15, 14, 30, 0, DateTimeKind.Local),
				CorrectedDateTime =
					new DateTime(2024, 6, 15, 14, 30, 0, DateTimeKind.Local),
				DeltaHours = 0.0,
				Warning =
					"No correction needed: recorded timezone matches correct timezone",
				FileIndexItem = new FileIndexItem { FilePath = "/test.jpg" }
			}
		};

		var timezoneService = new FakeIExifTimezoneCorrectionService(mockResults);
		var controller = CreateController(timezoneService);

		var request = new ExifTimezoneCorrectionRequest
		{
			RecordedTimezone = "Europe/Amsterdam", CorrectTimezone = "Europe/Amsterdam"
		};

		// Act
		var result = await controller.PreviewTimezoneCorrectionAsync(
			"/test.jpg",
			true,
			request);

		// Assert
		var jsonResult = result as OkObjectResult;
		Assert.IsNotNull(jsonResult);
		var returnedResults = jsonResult.Value as List<ExifTimezoneCorrectionResult>;
		Assert.IsNotNull(returnedResults);
		Assert.AreEqual(0.0, returnedResults[0].DeltaHours);
	}

	[TestMethod]
	public async Task PreviewTimezoneCorrectionAsync_WithCollectionsFalse_PassesCorrectly()
	{
		// Arrange
		var mockResults = new List<ExifTimezoneCorrectionResult>
		{
			new()
			{
				Success = true,
				OriginalDateTime = new DateTime(2024, 6, 15, 14, 30, 0, DateTimeKind.Local),
				CorrectedDateTime =
					new DateTime(2024, 6, 15, 16, 30, 0, DateTimeKind.Local),
				DeltaHours = 2.0,
				FileIndexItem = new FileIndexItem { FilePath = "/test.jpg" }
			}
		};

		var timezoneService = new FakeIExifTimezoneCorrectionService(mockResults);
		var controller = CreateController(timezoneService);

		var request = new ExifTimezoneCorrectionRequest
		{
			RecordedTimezone = "UTC", CorrectTimezone = "Europe/Amsterdam"
		};

		// Act
		var result = await controller.PreviewTimezoneCorrectionAsync(
			"/test.jpg",
			false,
			request);

		// Assert
		Assert.IsNotNull(result);
		var jsonResult = result as OkObjectResult;
		Assert.IsNotNull(jsonResult);
		Assert.AreEqual(200, jsonResult.StatusCode);
	}

	[TestMethod]
	public async Task PreviewTimezoneCorrectionAsync_InvalidTimezone_ReturnsError()
	{
		// Arrange
		var mockResults = new List<ExifTimezoneCorrectionResult>
		{
			new()
			{
				Success = false,
				Error = "Invalid recorded timezone: InvalidTZ",
				FileIndexItem = new FileIndexItem { FilePath = "/test.jpg" }
			}
		};

		var timezoneService = new FakeIExifTimezoneCorrectionService(mockResults);
		var controller = CreateController(timezoneService);

		var request = new ExifTimezoneCorrectionRequest
		{
			RecordedTimezone = "InvalidTZ", CorrectTimezone = "Europe/Amsterdam"
		};

		// Act
		var result = await controller.PreviewTimezoneCorrectionAsync(
			"/test.jpg",
			true,
			request);

		// Assert
		var jsonResult = result as OkObjectResult;
		Assert.IsNotNull(jsonResult);
		var returnedResults = jsonResult.Value as List<ExifTimezoneCorrectionResult>;
		Assert.IsNotNull(returnedResults);
		Assert.IsFalse(returnedResults[0].Success);
	}

	[TestMethod]
	public async Task PreviewTimezoneCorrectionAsync_NegativeDelta_CorrectlyCalculated()
	{
		// Arrange - Photo taken in a timezone earlier than recorded
		var mockResults = new List<ExifTimezoneCorrectionResult>
		{
			new()
			{
				Success = true,
				OriginalDateTime = new DateTime(2024, 6, 15, 14, 30, 0, DateTimeKind.Local),
				CorrectedDateTime =
					new DateTime(2024, 6, 15, 12, 30, 0, DateTimeKind.Local),
				DeltaHours = -2.0,
				FileIndexItem = new FileIndexItem { FilePath = "/test.jpg" }
			}
		};

		var timezoneService = new FakeIExifTimezoneCorrectionService(mockResults);
		var controller = CreateController(timezoneService);

		var request = new ExifTimezoneCorrectionRequest
		{
			RecordedTimezone = "Europe/Amsterdam", CorrectTimezone = "UTC"
		};

		// Act
		var result = await controller.PreviewTimezoneCorrectionAsync(
			"/test.jpg",
			true,
			request);

		// Assert
		var jsonResult = result as OkObjectResult;
		Assert.IsNotNull(jsonResult);
		var returnedResults = jsonResult.Value as List<ExifTimezoneCorrectionResult>;
		Assert.IsNotNull(returnedResults);
		Assert.AreEqual(-2.0, returnedResults[0].DeltaHours);
	}

	[TestMethod]
	public async Task PreviewTimezoneCorrectionAsync_NoFilesProvided_ReturnsBadRequest()
	{
		// Arrange
		var controller = CreateController();
		var request = new ExifTimezoneCorrectionRequest
		{
			RecordedTimezone = "UTC", CorrectTimezone = "Europe/Amsterdam"
		};

		// Act
		var result = await controller.PreviewTimezoneCorrectionAsync(
			";;;",
			true,
			request);

		// Assert
		Assert.IsNotNull(result);
		var badRequestResult = result as BadRequestObjectResult;
		Assert.IsNotNull(badRequestResult);
		Assert.AreEqual(400, badRequestResult.StatusCode);
	}
}

/// <summary>
///     Fake implementation of IExifTimezoneCorrectionService for testing
/// </summary>
internal class FakeIExifTimezoneCorrectionService : IExifTimezoneCorrectionService
{
	private readonly List<ExifTimezoneCorrectionResult> _validationResults;

	public FakeIExifTimezoneCorrectionService(
		List<ExifTimezoneCorrectionResult>? validationResults = null)
	{
		_validationResults = validationResults ?? new List<ExifTimezoneCorrectionResult>();
	}

	public Task<List<ExifTimezoneCorrectionResult>> Validate(
		string[] subPaths,
		bool collections,
		ExifTimezoneCorrectionRequest request)
	{
		return Task.FromResult(_validationResults);
	}

	public Task<List<ExifTimezoneCorrectionResult>> CorrectTimezoneAsync(
		List<FileIndexItem> fileIndexItems,
		ExifTimezoneCorrectionRequest request)
	{
		return Task.FromResult(_validationResults);
	}
}

/// <summary>
///     Fake implementation of IServiceScopeFactory for testing
/// </summary>
internal class FakeServiceScopeFactory : IServiceScopeFactory
{
	private readonly ServiceCollection _services = new();

	public IServiceScope CreateScope()
	{
		var services = _services.AddSingleton<IRealtimeConnectionsService>(
			new FakeIRealtimeConnectionsService());
		return services.BuildServiceProvider().CreateScope();
	}
}
