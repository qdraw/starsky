using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.metaupdate.Models;
using starsky.feature.metaupdate.Services;
using starsky.foundation.database.Models;
using starsky.foundation.metaupdate.Interfaces;
using starsky.foundation.metaupdate.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.metaupdate.Services;

[TestClass]
public sealed class MetaTimeCorrectBackgroundJobHandlerTest
{
	[TestMethod]
	public void JobType_ShouldMatchExpected()
	{
		var handler = new MetaTimeCorrectBackgroundJobHandler(
			new FakeIServiceScopeFactory(),
			new FakeIWebLogger(),
			new FakeIWebSocketConnectionsService(),
			new FakeINotificationQuery());

		Assert.AreEqual(MetaTimeCorrectBackgroundJobHandler.MetaTimeCorrect, handler.JobType);
	}

	[TestMethod]
	public async Task ExecuteAsync_NullPayload_ShouldThrowArgumentException()
	{
		var handler = new MetaTimeCorrectBackgroundJobHandler(
			new FakeIServiceScopeFactory(),
			new FakeIWebLogger(),
			new FakeIWebSocketConnectionsService(),
			new FakeINotificationQuery());

		await Assert.ThrowsExactlyAsync<ArgumentException>(async () =>
			await handler.ExecuteAsync(null, CancellationToken.None));
	}

	[TestMethod]
	public async Task ExecuteAsync_InvalidJson_ShouldThrowException()
	{
		var handler = new MetaTimeCorrectBackgroundJobHandler(
			new FakeIServiceScopeFactory(),
			new FakeIWebLogger(),
			new FakeIWebSocketConnectionsService(),
			new FakeINotificationQuery());

		await Assert.ThrowsExactlyAsync<JsonException>(async () =>
			await handler.ExecuteAsync("{ invalid }", CancellationToken.None));
	}

	[TestMethod]
	public async Task ExecuteAsync_UnknownRequestType_ShouldThrowArgumentException()
	{
		var timezoneService = new FakeIExifTimezoneCorrectionService();

		var scopeFactory = new FakeIServiceScopeFactory(null, (services) =>
		{
			services.AddSingleton<IExifTimezoneCorrectionService>(timezoneService);
		});

		var handler = new MetaTimeCorrectBackgroundJobHandler(
			scopeFactory,
			new FakeIWebLogger(),
			new FakeIWebSocketConnectionsService(),
			new FakeINotificationQuery());

		var payload = new MetaTimeCorrectBackgroundPayload
		{
			RequestType = "unknown",
			RequestJson = "{}"
		};
		var payloadJson = JsonSerializer.Serialize(payload);

		await Assert.ThrowsExactlyAsync<ArgumentException>(async () =>
			await handler.ExecuteAsync(payloadJson, CancellationToken.None));
	}

	[TestMethod]
	public async Task ExecuteAsync_TimezoneRequest_ShouldCallServiceAndNotify()
	{
		var timezoneService = new FakeIExifTimezoneCorrectionService([
			new ExifTimezoneCorrectionResult { Success = true }
		]);

		var scopeFactory = new FakeIServiceScopeFactory(null, (services) =>
		{
			services.AddSingleton<IExifTimezoneCorrectionService>(timezoneService);
		});
		var webSocketService = new FakeIWebSocketConnectionsService();
		var notificationQuery = new FakeINotificationQuery();

		var handler = new MetaTimeCorrectBackgroundJobHandler(
			scopeFactory,
			new FakeIWebLogger(),
			webSocketService,
			notificationQuery);

		var request = new ExifTimezoneBasedCorrectionRequest
		{
			RecordedTimezoneId = "UTC",
			CorrectTimezoneId = "Europe/Amsterdam"
		};

		var payload = new MetaTimeCorrectBackgroundPayload
		{
			RequestType = "timezone",
			RequestJson = JsonSerializer.Serialize(request),
			CorrectionType = "timezone",
			ValidateResults =
			[
				new ExifTimezoneCorrectionResult
				{
					FileIndexItem = new FileIndexItem("/test.jpg")
				}
			]
		};
		var payloadJson = JsonSerializer.Serialize(payload);

		await handler.ExecuteAsync(payloadJson, CancellationToken.None);

		Assert.HasCount(1, webSocketService.FakeSendToAllAsync);
		Assert.Contains("/test.jpg", webSocketService.FakeSendToAllAsync[0]);

		Assert.HasCount(1, notificationQuery.FakeContent);
	}

	[TestMethod]
	public async Task ExecuteAsync_OffsetRequest_ShouldCallServiceAndNotify()
	{
		var timezoneService = new FakeIExifTimezoneCorrectionService([
			new ExifTimezoneCorrectionResult { Success = true }
		]);

		var scopeFactory = new FakeIServiceScopeFactory(null, (services) =>
		{
			services.AddSingleton<IExifTimezoneCorrectionService>(timezoneService);
		});
		var webSocketService = new FakeIWebSocketConnectionsService();
		var notificationQuery = new FakeINotificationQuery();

		var handler = new MetaTimeCorrectBackgroundJobHandler(
			scopeFactory,
			new FakeIWebLogger(),
			webSocketService,
			notificationQuery);

		var request = new ExifCustomOffsetCorrectionRequest
		{
			Hour = 1
		};

		var payload = new MetaTimeCorrectBackgroundPayload
		{
			RequestType = "offset",
			RequestJson = JsonSerializer.Serialize(request),
			CorrectionType = "offset",
			ValidateResults =
			[
				new ExifTimezoneCorrectionResult
				{
					FileIndexItem = new FileIndexItem("/test.jpg")
				}
			]
		};
		var payloadJson = JsonSerializer.Serialize(payload);

		await handler.ExecuteAsync(payloadJson, CancellationToken.None);

		Assert.HasCount(1, webSocketService.FakeSendToAllAsync);
		Assert.HasCount(1, notificationQuery.FakeContent);
	}
}
