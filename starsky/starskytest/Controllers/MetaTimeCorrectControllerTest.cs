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
public sealed class MetaTimeCorrectControllerTest
{
	private static MetaTimeCorrectController CreateController(
		IExifTimezoneCorrectionService? timezoneService = null,
		IUpdateBackgroundTaskQueue? queue = null,
		IWebLogger? logger = null,
		IServiceScopeFactory? scopeFactory = null)
	{
		timezoneService ??= new FakeIExifTimezoneCorrectionService();
		queue ??= new FakeIUpdateBackgroundTaskQueue();
		logger ??= new FakeIWebLogger();

		// Configure scope factory with the timezone service
		scopeFactory ??= new FakeIServiceScopeFactory(
			nameof(MetaTimeCorrectControllerTest),
			services =>
			{
				services.AddSingleton(timezoneService);
				services.AddSingleton<IRealtimeConnectionsService,
					FakeIRealtimeConnectionsService>();
				services.AddSingleton(logger);
			});

		var controller = new MetaTimeCorrectController(
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
				OriginalDateTime = new DateTime(2024, 6, 15,
					14, 30, 0, DateTimeKind.Local),
				CorrectedDateTime =
					new DateTime(2024, 6, 15,
						16, 30, 0, DateTimeKind.Local),
				Delta = TimeSpan.FromHours(2),
				FileIndexItem = new FileIndexItem { FilePath = "/test.jpg" }
			}
		};

		var timezoneService = new FakeIExifTimezoneCorrectionService(mockResults);
		var controller = CreateController(timezoneService);

		var request = new ExifTimezoneBasedCorrectionRequest
		{
			RecordedTimezoneId = "UTC", CorrectTimezoneId = "Europe/Amsterdam"
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
		Assert.AreEqual(TimeSpan.FromHours(2), returnedResults[0].Delta);
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
				OriginalDateTime = new DateTime(2024, 6, 15,
					14, 30, 0, DateTimeKind.Local),
				CorrectedDateTime =
					new DateTime(2024, 6, 15,
						16, 30, 0, DateTimeKind.Local),
				Delta = TimeSpan.FromHours(2.0),
				FileIndexItem = new FileIndexItem { FilePath = "/test1.jpg" }
			},
			new()
			{
				Success = true,
				OriginalDateTime = new DateTime(2024, 6, 16,
					10, 0, 0, DateTimeKind.Local),
				CorrectedDateTime = new DateTime(2024, 6, 16,
					12, 0, 0, DateTimeKind.Local),
				Delta = TimeSpan.FromHours(2.0),
				FileIndexItem = new FileIndexItem { FilePath = "/test2.jpg" }
			}
		};

		var timezoneService = new FakeIExifTimezoneCorrectionService(mockResults);
		var controller = CreateController(timezoneService);

		var request = new ExifTimezoneBasedCorrectionRequest
		{
			RecordedTimezoneId = "UTC", CorrectTimezoneId = "Europe/Amsterdam"
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
		var request = new ExifTimezoneBasedCorrectionRequest
		{
			RecordedTimezoneId = "UTC", CorrectTimezoneId = "Europe/Amsterdam"
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
		var request = new ExifTimezoneBasedCorrectionRequest
		{
			RecordedTimezoneId = "UTC", CorrectTimezoneId = "Europe/Amsterdam"
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

		var request = new ExifTimezoneBasedCorrectionRequest
		{
			RecordedTimezoneId = "UTC", CorrectTimezoneId = "Europe/Amsterdam"
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
				OriginalDateTime = new DateTime(2024, 6, 30,
					23, 0, 0, DateTimeKind.Local),
				CorrectedDateTime = new DateTime(2024, 7, 1,
					11, 0, 0, DateTimeKind.Local),
				Delta = TimeSpan.FromHours(12.0),
				Warning = "Day rollover: correction will change the date",
				FileIndexItem = new FileIndexItem { FilePath = "/test.jpg" }
			}
		};

		var timezoneService = new FakeIExifTimezoneCorrectionService(mockResults);
		var controller = CreateController(timezoneService);

		var request = new ExifTimezoneBasedCorrectionRequest
		{
			RecordedTimezoneId = "UTC", CorrectTimezoneId = "Europe/Amsterdam"
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
				OriginalDateTime = new DateTime(2024, 10, 26,
					14, 0, 0, DateTimeKind.Local),
				CorrectedDateTime =
					new DateTime(2024, 10, 26,
						15, 0, 0, DateTimeKind.Local),
				Delta = TimeSpan.FromHours(1.0),
				FileIndexItem = new FileIndexItem { FilePath = "/test.jpg" }
			}
		};

		var timezoneService = new FakeIExifTimezoneCorrectionService(mockResults);
		var controller = CreateController(timezoneService);

		var request = new ExifTimezoneBasedCorrectionRequest
		{
			RecordedTimezoneId = "Europe/Amsterdam", // UTC+1 (after DST)
			CorrectTimezoneId = "Europe/Amsterdam" // UTC+2 (during DST)
		};

		// Act
		var result = await controller.PreviewTimezoneCorrectionAsync(
			"/test.jpg",
			true,
			request);

		// Assert
		var jsonResult = result as OkObjectResult;
		Assert.IsNotNull(jsonResult);
		var returnedResults = jsonResult.Value as
			List<ExifTimezoneCorrectionResult>;
		Assert.IsNotNull(returnedResults);
		Assert.AreEqual(TimeSpan.FromHours(1.0), returnedResults[0].Delta);
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
				OriginalDateTime = new DateTime(2024, 6, 15,
					14, 30, 0, DateTimeKind.Local),
				CorrectedDateTime =
					new DateTime(2024, 6, 15,
						14, 30, 0, DateTimeKind.Local),
				Delta = TimeSpan.FromHours(0.0),
				Warning =
					"No correction needed: recorded timezone matches correct timezone",
				FileIndexItem = new FileIndexItem { FilePath = "/test.jpg" }
			}
		};

		var timezoneService = new FakeIExifTimezoneCorrectionService(mockResults);
		var controller = CreateController(timezoneService);

		var request = new ExifTimezoneBasedCorrectionRequest
		{
			RecordedTimezoneId = "Europe/Amsterdam", CorrectTimezoneId = "Europe/Amsterdam"
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
		Assert.AreEqual(TimeSpan.FromHours(0.0), returnedResults[0].Delta);
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
				OriginalDateTime = new DateTime(2024, 6, 15, 14,
					30, 0, DateTimeKind.Local),
				CorrectedDateTime =
					new DateTime(2024, 6, 15,
						16, 30, 0, DateTimeKind.Local),
				Delta = TimeSpan.FromHours(2.0),
				FileIndexItem = new FileIndexItem { FilePath = "/test.jpg" }
			}
		};

		var timezoneService = new FakeIExifTimezoneCorrectionService(mockResults);
		var controller = CreateController(timezoneService);

		var request = new ExifTimezoneBasedCorrectionRequest
		{
			RecordedTimezoneId = "UTC", CorrectTimezoneId = "Europe/Amsterdam"
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

		var request = new ExifTimezoneBasedCorrectionRequest
		{
			RecordedTimezoneId = "InvalidTZ", CorrectTimezoneId = "Europe/Amsterdam"
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
				OriginalDateTime = new DateTime(2024, 6, 15,
					14, 30, 0, DateTimeKind.Local),
				CorrectedDateTime =
					new DateTime(2024, 6, 15,
						12, 30, 0, DateTimeKind.Local),
				Delta = TimeSpan.FromHours(-2.0),
				FileIndexItem = new FileIndexItem { FilePath = "/test.jpg" }
			}
		};

		var timezoneService = new FakeIExifTimezoneCorrectionService(mockResults);
		var controller = CreateController(timezoneService);

		var request = new ExifTimezoneBasedCorrectionRequest
		{
			RecordedTimezoneId = "Europe/Amsterdam", CorrectTimezoneId = "UTC"
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
		Assert.AreEqual(TimeSpan.FromHours(-2.0), returnedResults[0].Delta);
	}

	[TestMethod]
	public async Task PreviewTimezoneCorrectionAsync_NoFilesProvided_ReturnsBadRequest()
	{
		// Arrange
		var controller = CreateController();
		var request = new ExifTimezoneBasedCorrectionRequest
		{
			RecordedTimezoneId = "UTC", CorrectTimezoneId = "Europe/Amsterdam"
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

	[TestMethod]
	public async Task ExecuteTimezoneCorrectionAsync_ValidInput_ReturnsOkAndQueuesTask()
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
				Delta = TimeSpan.FromHours(2.0),
				FileIndexItem = new FileIndexItem { FilePath = "/test.jpg" }
			}
		};

		var timezoneService = new FakeIExifTimezoneCorrectionService(mockResults);
		var queue = new FakeIUpdateBackgroundTaskQueue();
		var controller = CreateController(timezoneService, queue);

		var request = new ExifTimezoneBasedCorrectionRequest
		{
			RecordedTimezoneId = "UTC", CorrectTimezoneId = "Europe/Amsterdam"
		};

		// Act
		var result = await controller.ExecuteTimezoneCorrectionAsync(
			"/test.jpg",
			true,
			request);

		// Assert
		Assert.IsNotNull(result);
		var jsonResult = result as JsonResult;
		Assert.IsNotNull(jsonResult);
		var returnedResults = jsonResult.Value as List<ExifTimezoneCorrectionResult>;
		Assert.IsNotNull(returnedResults);
		Assert.HasCount(1, returnedResults);
		Assert.IsTrue(queue.QueueBackgroundWorkItemCalled);
	}

	[TestMethod]
	public async Task ExecuteTimezoneCorrectionAsync_MultipleFiles_QueuesAndReturnsAllResults()
	{
		// Arrange
		var mockResults = new List<ExifTimezoneCorrectionResult>
		{
			new()
			{
				Success = true,
				OriginalDateTime = new DateTime(2024, 6, 15,
					14, 30, 0, DateTimeKind.Local),
				CorrectedDateTime = new DateTime(2024, 6, 15,
					16, 30, 0, DateTimeKind.Local),
				Delta = TimeSpan.FromHours(2.0),
				FileIndexItem = new FileIndexItem { FilePath = "/test1.jpg" }
			},
			new()
			{
				Success = true,
				OriginalDateTime = new DateTime(2024, 6, 16,
					10, 0, 0, DateTimeKind.Local),
				CorrectedDateTime = new DateTime(2024, 6, 16,
					12, 0, 0, DateTimeKind.Local),
				Delta = TimeSpan.FromHours(2.0),
				FileIndexItem = new FileIndexItem { FilePath = "/test2.jpg" }
			}
		};

		var timezoneService = new FakeIExifTimezoneCorrectionService(mockResults);
		var queue = new FakeIUpdateBackgroundTaskQueue();
		var controller = CreateController(timezoneService, queue);

		var request = new ExifTimezoneBasedCorrectionRequest
		{
			RecordedTimezoneId = "UTC", CorrectTimezoneId = "Europe/Amsterdam"
		};

		// Act
		var result = await controller.ExecuteTimezoneCorrectionAsync(
			"/test1.jpg;/test2.jpg",
			true,
			request);

		// Assert
		var jsonResult = result as JsonResult;
		Assert.IsNotNull(jsonResult);
		var returnedResults = jsonResult.Value as List<ExifTimezoneCorrectionResult>;
		Assert.IsNotNull(returnedResults);
		Assert.HasCount(2, returnedResults);
		Assert.IsTrue(queue.QueueBackgroundWorkItemCalled);
	}

	[TestMethod]
	public async Task ExecuteTimezoneCorrectionAsync_EmptyFilePath_ReturnsBadRequest()
	{
		// Arrange
		var controller = CreateController();
		var request = new ExifTimezoneBasedCorrectionRequest
		{
			RecordedTimezoneId = "UTC", CorrectTimezoneId = "Europe/Amsterdam"
		};

		// Act
		var result = await controller.ExecuteTimezoneCorrectionAsync(
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
	public async Task ExecuteTimezoneCorrectionAsync_NullCollections_ReturnsBadRequest()
	{
		// Arrange
		var controller = CreateController();
		var request = new ExifTimezoneBasedCorrectionRequest
		{
			RecordedTimezoneId = "UTC", CorrectTimezoneId = "Europe/Amsterdam"
		};

		// Act
		var result = await controller.ExecuteTimezoneCorrectionAsync(
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
	public async Task ExecuteTimezoneCorrectionAsync_NoFilesProvided_ReturnsBadRequest()
	{
		// Arrange
		var controller = CreateController();
		var request = new ExifTimezoneBasedCorrectionRequest
		{
			RecordedTimezoneId = "UTC", CorrectTimezoneId = "Europe/Amsterdam"
		};

		// Act
		var result = await controller.ExecuteTimezoneCorrectionAsync(
			";;;",
			true,
			request);

		// Assert
		Assert.IsNotNull(result);
		var badRequestResult = result as BadRequestObjectResult;
		Assert.IsNotNull(badRequestResult);
		Assert.AreEqual(400, badRequestResult.StatusCode);
	}

	[TestMethod]
	public async Task ExecuteTimezoneCorrectionAsync_InvalidModelState_ReturnsBadRequest()
	{
		// Arrange
		var controller = CreateController();
		controller.ModelState.AddModelError("Key", "ErrorMessage");

		var request = new ExifTimezoneBasedCorrectionRequest
		{
			RecordedTimezoneId = "UTC", CorrectTimezoneId = "Europe/Amsterdam"
		};

		// Act
		var result = await controller.ExecuteTimezoneCorrectionAsync(
			"/test.jpg",
			true,
			request);

		// Assert
		Assert.IsNotNull(result);
		var badRequestResult = result as BadRequestObjectResult;
		Assert.IsNotNull(badRequestResult);
		Assert.AreEqual(400, badRequestResult.StatusCode);
	}

	[TestMethod]
	public async Task ExecuteTimezoneCorrectionAsync_ValidationErrors_ReturnsErrorsAndQueuesTask()
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
		var queue = new FakeIUpdateBackgroundTaskQueue();
		var controller = CreateController(timezoneService, queue);

		var request = new ExifTimezoneBasedCorrectionRequest
		{
			RecordedTimezoneId = "UTC", CorrectTimezoneId = "Europe/Amsterdam"
		};

		// Act
		var result = await controller.ExecuteTimezoneCorrectionAsync(
			"/nonexistent.jpg",
			true,
			request);

		// Assert
		var jsonResult = result as JsonResult;
		Assert.IsNotNull(jsonResult);
		var returnedResults = jsonResult.Value as List<ExifTimezoneCorrectionResult>;
		Assert.IsNotNull(returnedResults);
		Assert.IsFalse(returnedResults[0].Success);
		Assert.AreEqual("File does not exist", returnedResults[0].Error);
		// Background task should still be queued
		Assert.IsTrue(queue.QueueBackgroundWorkItemCalled);
	}

	[TestMethod]
	public async Task ExecuteTimezoneCorrectionAsync_WithWarnings_QueuesTaskAndReturnsWarnings()
	{
		// Arrange
		var mockResults = new List<ExifTimezoneCorrectionResult>
		{
			new()
			{
				Success = true,
				OriginalDateTime = new DateTime(2024, 6, 30,
					23, 0, 0, DateTimeKind.Local),
				CorrectedDateTime = new DateTime(2024, 7,
					1, 11, 0, 0, DateTimeKind.Local),
				Delta = TimeSpan.FromHours(12.0),
				Warning = "Day rollover: correction will change the date",
				FileIndexItem = new FileIndexItem { FilePath = "/test.jpg" }
			}
		};

		var timezoneService = new FakeIExifTimezoneCorrectionService(mockResults);
		var queue = new FakeIUpdateBackgroundTaskQueue();
		var controller = CreateController(timezoneService, queue);

		var request = new ExifTimezoneBasedCorrectionRequest
		{
			RecordedTimezoneId = "UTC", CorrectTimezoneId = "Europe/Amsterdam"
		};

		// Act
		var result = await controller.ExecuteTimezoneCorrectionAsync(
			"/test.jpg",
			true,
			request);

		// Assert
		var jsonResult = result as JsonResult;
		Assert.IsNotNull(jsonResult);
		var returnedResults = jsonResult.Value as List<ExifTimezoneCorrectionResult>;
		Assert.IsNotNull(returnedResults);
		Assert.IsTrue(returnedResults[0].Success);
		Assert.AreEqual("Day rollover: correction will change the date",
			returnedResults[0].Warning);
		Assert.IsTrue(queue.QueueBackgroundWorkItemCalled);
	}

	[TestMethod]
	public async Task ExecuteTimezoneCorrectionAsync_DSTScenario_QueuesAndReturnsCorrectDelta()
	{
		// Arrange - DST fallback scenario
		var mockResults = new List<ExifTimezoneCorrectionResult>
		{
			new()
			{
				Success = true,
				OriginalDateTime = new DateTime(2024, 10, 26,
					14, 0, 0, DateTimeKind.Local),
				CorrectedDateTime = new DateTime(2024, 10, 26,
					15, 0, 0, DateTimeKind.Local),
				Delta = TimeSpan.FromHours(1.0),
				FileIndexItem = new FileIndexItem { FilePath = "/test.jpg" }
			}
		};

		var timezoneService = new FakeIExifTimezoneCorrectionService(mockResults);
		var queue = new FakeIUpdateBackgroundTaskQueue();
		var controller = CreateController(timezoneService, queue);

		var request = new ExifTimezoneBasedCorrectionRequest
		{
			RecordedTimezoneId = "Europe/Amsterdam", CorrectTimezoneId = "Europe/Amsterdam"
		};

		// Act
		var result = await controller.ExecuteTimezoneCorrectionAsync(
			"/test.jpg",
			true,
			request);

		// Assert
		var jsonResult = result as JsonResult;
		Assert.IsNotNull(jsonResult);
		var returnedResults = jsonResult.Value as List<ExifTimezoneCorrectionResult>;
		Assert.IsNotNull(returnedResults);
		Assert.AreEqual(TimeSpan.FromHours(1.0), returnedResults[0].Delta);
		Assert.IsTrue(queue.QueueBackgroundWorkItemCalled);
	}

	[TestMethod]
	public async Task ExecuteTimezoneCorrectionAsync_SameTimezone_QueuesTaskWithZeroDelta()
	{
		// Arrange
		var mockResults = new List<ExifTimezoneCorrectionResult>
		{
			new()
			{
				Success = true,
				OriginalDateTime = new DateTime(2024, 6, 15,
					14, 30, 0, DateTimeKind.Local),
				CorrectedDateTime = new DateTime(2024, 6, 15,
					14, 30, 0, DateTimeKind.Local),
				Delta = TimeSpan.FromHours(0.0),
				Warning = "No correction needed: recorded timezone matches correct timezone",
				FileIndexItem = new FileIndexItem { FilePath = "/test.jpg" }
			}
		};

		var timezoneService = new FakeIExifTimezoneCorrectionService(mockResults);
		var queue = new FakeIUpdateBackgroundTaskQueue();
		var controller = CreateController(timezoneService, queue);

		var request = new ExifTimezoneBasedCorrectionRequest
		{
			RecordedTimezoneId = "Europe/Amsterdam", CorrectTimezoneId = "Europe/Amsterdam"
		};

		// Act
		var result = await controller.ExecuteTimezoneCorrectionAsync(
			"/test.jpg",
			true,
			request);

		// Assert
		var jsonResult = result as JsonResult;
		Assert.IsNotNull(jsonResult);
		var returnedResults = jsonResult.Value as List<ExifTimezoneCorrectionResult>;
		Assert.IsNotNull(returnedResults);
		Assert.AreEqual(TimeSpan.FromHours(0.0), returnedResults[0].Delta);
		Assert.IsTrue(queue.QueueBackgroundWorkItemCalled);
	}

	[TestMethod]
	public async Task ExecuteTimezoneCorrectionAsync_WithCollectionsFalse_QueuesTaskCorrectly()
	{
		// Arrange
		var mockResults = new List<ExifTimezoneCorrectionResult>
		{
			new()
			{
				Success = true,
				OriginalDateTime = new DateTime(2024, 6, 15,
					14, 30, 0, DateTimeKind.Local),
				CorrectedDateTime = new DateTime(2024, 6, 15,
					16, 30, 0, DateTimeKind.Local),
				Delta = TimeSpan.FromHours(2.0),
				FileIndexItem = new FileIndexItem { FilePath = "/test.jpg" }
			}
		};

		var timezoneService = new FakeIExifTimezoneCorrectionService(mockResults);
		var queue = new FakeIUpdateBackgroundTaskQueue();
		var controller = CreateController(timezoneService, queue);

		var request = new ExifTimezoneBasedCorrectionRequest
		{
			RecordedTimezoneId = "UTC", CorrectTimezoneId = "Europe/Amsterdam"
		};

		// Act
		var result = await controller.ExecuteTimezoneCorrectionAsync(
			"/test.jpg",
			false,
			request);

		// Assert
		Assert.IsNotNull(result);
		var jsonResult = result as JsonResult;
		Assert.IsNotNull(jsonResult);
		Assert.IsTrue(queue.QueueBackgroundWorkItemCalled);
	}

	[TestMethod]
	public async Task ExecuteTimezoneCorrectionAsync_InvalidTimezone_QueuesButReturnsError()
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
		var queue = new FakeIUpdateBackgroundTaskQueue();
		var controller = CreateController(timezoneService, queue);

		var request = new ExifTimezoneBasedCorrectionRequest
		{
			RecordedTimezoneId = "InvalidTZ", CorrectTimezoneId = "Europe/Amsterdam"
		};

		// Act
		var result = await controller.ExecuteTimezoneCorrectionAsync(
			"/test.jpg",
			true,
			request);

		// Assert
		var jsonResult = result as JsonResult;
		Assert.IsNotNull(jsonResult);
		var returnedResults = jsonResult.Value as List<ExifTimezoneCorrectionResult>;
		Assert.IsNotNull(returnedResults);
		Assert.IsFalse(returnedResults[0].Success);
		Assert.IsTrue(queue.QueueBackgroundWorkItemCalled);
	}

	[TestMethod]
	public async Task ExecuteTimezoneCorrectionAsync_NegativeDelta_CorrectlyCalculatedAndQueued()
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
				Delta = TimeSpan.FromHours(-2.0),
				FileIndexItem = new FileIndexItem { FilePath = "/test.jpg" }
			}
		};

		var timezoneService = new FakeIExifTimezoneCorrectionService(mockResults);
		var queue = new FakeIUpdateBackgroundTaskQueue();
		var controller = CreateController(timezoneService, queue);

		var request = new ExifTimezoneBasedCorrectionRequest
		{
			RecordedTimezoneId = "Europe/Amsterdam", CorrectTimezoneId = "UTC"
		};

		// Act
		var result = await controller.ExecuteTimezoneCorrectionAsync(
			"/test.jpg",
			true,
			request);

		// Assert
		var jsonResult = result as JsonResult;
		Assert.IsNotNull(jsonResult);
		var returnedResults = jsonResult.Value as List<ExifTimezoneCorrectionResult>;
		Assert.IsNotNull(returnedResults);
		Assert.AreEqual(TimeSpan.FromHours(-2.0), returnedResults[0].Delta);
		Assert.IsTrue(queue.QueueBackgroundWorkItemCalled);
	}

	[TestMethod]
	public async Task ExecuteTimezoneCorrectionAsync_BackgroundTaskQueuedWithCorrectMetadata()
	{
		// Arrange
		var mockResults = new List<ExifTimezoneCorrectionResult>
		{
			new()
			{
				Success = true,
				OriginalDateTime = new DateTime(2024, 6, 15,
					14, 30, 0, DateTimeKind.Local),
				CorrectedDateTime = new DateTime(2024, 6, 15,
					16, 30, 0, DateTimeKind.Local),
				Delta = TimeSpan.FromHours(2.0),
				FileIndexItem = new FileIndexItem { FilePath = "/test.jpg" }
			}
		};

		var timezoneService = new FakeIExifTimezoneCorrectionService(mockResults);
		var queue = new FakeIUpdateBackgroundTaskQueue();
		var controller = CreateController(timezoneService, queue);

		var request = new ExifTimezoneBasedCorrectionRequest
		{
			RecordedTimezoneId = "UTC", CorrectTimezoneId = "Europe/Amsterdam"
		};

		// Act
		await controller.ExecuteTimezoneCorrectionAsync("/test.jpg", true, request);

		// Assert
		Assert.IsTrue(queue.QueueBackgroundWorkItemCalled);
		Assert.AreEqual(1, queue.QueueBackgroundWorkItemCalledCounter);
	}

	[TestMethod]
	public async Task
		ExecuteTimezoneCorrectionAsync_MixedResultsWithSuccessAndErrors_QueuesAndReturnsAll()
	{
		// Arrange
		var mockResults = new List<ExifTimezoneCorrectionResult>
		{
			new()
			{
				Success = true,
				OriginalDateTime = new DateTime(2024, 6, 15,
					14, 30, 0, DateTimeKind.Local),
				CorrectedDateTime = new DateTime(2024, 6,
					15, 16, 30, 0, DateTimeKind.Local),
				Delta = TimeSpan.FromHours(2.0),
				FileIndexItem = new FileIndexItem { FilePath = "/test1.jpg" }
			},
			new()
			{
				Success = false,
				Error = "File does not exist",
				FileIndexItem = new FileIndexItem { FilePath = "/test2.jpg" }
			},
			new()
			{
				Success = true,
				OriginalDateTime = new DateTime(2024, 6, 16,
					10, 0, 0, DateTimeKind.Local),
				CorrectedDateTime = new DateTime(2024, 6, 16,
					12, 0, 0, DateTimeKind.Local),
				Delta = TimeSpan.FromHours(2.0),
				FileIndexItem = new FileIndexItem { FilePath = "/test3.jpg" }
			}
		};

		var timezoneService = new FakeIExifTimezoneCorrectionService(mockResults);
		var queue = new FakeIUpdateBackgroundTaskQueue();
		var controller = CreateController(timezoneService, queue);

		var request = new ExifTimezoneBasedCorrectionRequest
		{
			RecordedTimezoneId = "UTC", CorrectTimezoneId = "Europe/Amsterdam"
		};

		// Act
		var result = await controller.ExecuteTimezoneCorrectionAsync(
			"/test1.jpg;/test2.jpg;/test3.jpg",
			true,
			request);

		// Assert
		var jsonResult = result as JsonResult;
		Assert.IsNotNull(jsonResult);
		var returnedResults = jsonResult.Value as List<ExifTimezoneCorrectionResult>;
		Assert.IsNotNull(returnedResults);
		Assert.HasCount(3, returnedResults);
		Assert.IsTrue(returnedResults[0].Success);
		Assert.IsFalse(returnedResults[1].Success);
		Assert.IsTrue(returnedResults[2].Success);
		Assert.IsTrue(queue.QueueBackgroundWorkItemCalled);
	}

	[TestMethod]
	public async Task PreviewCustomOffsetCorrectionAsync_ValidInput_ReturnsOkResult()
	{
		// Arrange
		var mockResults = new List<ExifTimezoneCorrectionResult>
		{
			new()
			{
				Success = true,
				OriginalDateTime = new DateTime(2024, 6, 15, 14, 30, 0, DateTimeKind.Local),
				CorrectedDateTime =
					new DateTime(2024, 6, 15, 15, 30, 0, DateTimeKind.Local),
				Delta = TimeSpan.FromHours(1),
				FileIndexItem = new FileIndexItem { FilePath = "/test.jpg" }
			}
		};

		var timezoneService = new FakeIExifTimezoneCorrectionService(mockResults);
		var controller = CreateController(timezoneService);

		var request = new ExifCustomOffsetCorrectionRequest { Hour = 1 };

		// Act
		var result = await controller.PreviewCustomOffsetCorrectionAsync(
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
		Assert.AreEqual(TimeSpan.FromHours(1), returnedResults[0].Delta);
	}

	[TestMethod]
	public async Task ExecuteCustomOffsetCorrectionAsync_ValidInput_ReturnsOkAndQueuesTask()
	{
		// Arrange
		var mockResults = new List<ExifTimezoneCorrectionResult>
		{
			new()
			{
				Success = true,
				OriginalDateTime = new DateTime(2024, 6, 15, 14, 30, 0, DateTimeKind.Local),
				CorrectedDateTime =
					new DateTime(2024, 6, 15, 15, 30, 0, DateTimeKind.Local),
				Delta = TimeSpan.FromHours(1),
				FileIndexItem = new FileIndexItem { FilePath = "/test.jpg" }
			}
		};

		var timezoneService = new FakeIExifTimezoneCorrectionService(mockResults);
		var queue = new FakeIUpdateBackgroundTaskQueue();
		var controller = CreateController(timezoneService, queue);

		var request = new ExifCustomOffsetCorrectionRequest { Hour = 1 };

		// Act
		var result = await controller.ExecuteCustomOffsetCorrectionAsync(
			"/test.jpg",
			true,
			request);

		// Assert
		Assert.IsNotNull(result);
		var jsonResult = result as JsonResult;
		Assert.IsNotNull(jsonResult);
		var returnedResults = jsonResult.Value as List<ExifTimezoneCorrectionResult>;
		Assert.IsNotNull(returnedResults);
		Assert.HasCount(1, returnedResults);
		Assert.IsTrue(returnedResults[0].Success);
		Assert.AreEqual(TimeSpan.FromHours(1), returnedResults[0].Delta);
		Assert.IsTrue(queue.QueueBackgroundWorkItemCalled);
	}

	[TestMethod]
	public async Task PreviewCustomOffsetCorrectionAsync_InvalidModelState_ReturnsBadRequest()
	{
		// Arrange
		var controller = CreateController();
		controller.ModelState.AddModelError("Key", "ErrorMessage");
		var request = new ExifCustomOffsetCorrectionRequest { Hour = 1 };

		// Act
		var result = await controller.PreviewCustomOffsetCorrectionAsync(
			"/test.jpg",
			true,
			request);

		// Assert
		Assert.IsNotNull(result);
		var badRequestResult = result as BadRequestObjectResult;
		Assert.IsNotNull(badRequestResult);
		Assert.AreEqual(400, badRequestResult.StatusCode);
	}

	[TestMethod]
	public async Task ExecuteCustomOffsetCorrectionAsync_InvalidModelState_ReturnsBadRequest()
	{
		// Arrange
		var controller = CreateController();
		controller.ModelState.AddModelError("Key", "ErrorMessage");
		var request = new ExifCustomOffsetCorrectionRequest { Hour = 1 };

		// Act
		var result = await controller.ExecuteCustomOffsetCorrectionAsync(
			"/test.jpg",
			true,
			request);

		// Assert
		Assert.IsNotNull(result);
		var badRequestResult = result as BadRequestObjectResult;
		Assert.IsNotNull(badRequestResult);
		Assert.AreEqual(400, badRequestResult.StatusCode);
	}
}
