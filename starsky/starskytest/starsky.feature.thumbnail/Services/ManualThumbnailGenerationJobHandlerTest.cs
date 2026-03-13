using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.thumbnail.Interfaces;
using starsky.feature.thumbnail.Services;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.thumbnail.Services;

[TestClass]
public sealed class ManualThumbnailGenerationJobHandlerTest
{
	[TestMethod]
	public void JobType_ShouldMatchExpected()
	{
		var service = new FakeIManualThumbnailGenerationService();
		var handler = new ManualThumbnailGenerationJobHandler(service);
		Assert.AreEqual(ManualThumbnailGenerationService.JobType, handler.JobType);
	}

	[TestMethod]
	public async Task ExecuteAsync_MissingPayload_ShouldThrowArgumentException()
	{
		var service = new FakeIManualThumbnailGenerationService();
		var handler = new ManualThumbnailGenerationJobHandler(service);

		await Assert.ThrowsExactlyAsync<ArgumentException>(async () =>
			await handler.ExecuteAsync(null, CancellationToken.None));

		await Assert.ThrowsExactlyAsync<ArgumentException>(async () =>
			await handler.ExecuteAsync("  ", CancellationToken.None));
	}

	[TestMethod]
	public async Task ExecuteAsync_InvalidPayload_ShouldThrowException()
	{
		var service = new FakeIManualThumbnailGenerationService();
		var handler = new ManualThumbnailGenerationJobHandler(service);

		// Throws JsonException during deserialization
		await Assert.ThrowsExactlyAsync<JsonException>(async () =>
			await handler.ExecuteAsync("{ invalid }", CancellationToken.None));
	}

	[TestMethod]
	public async Task ExecuteAsync_ImplementationMismatch_ShouldThrowInvalidOperationException()
	{
		var service = new FakeIManualThumbnailGenerationService();
		var handler = new ManualThumbnailGenerationJobHandler(service);
		var payload =
			JsonSerializer.Serialize(new ManualThumbnailGenerationPayload { SubPath = "/" });

		await Assert.ThrowsExactlyAsync<InvalidOperationException>(async () =>
			await handler.ExecuteAsync(payload, CancellationToken.None));
	}

	[TestMethod]
	public async Task ExecuteAsync_CorrectImplementation_ShouldCallService()
	{
		var logger = new FakeIWebLogger();
		var thumbnailService = new FakeIThumbnailService(new FakeSelectorStorage());
		var bgTaskQueue = new FakeThumbnailBackgroundTaskQueue();
		var socketService = new FakeIThumbnailSocketService();

		var concreteService = new ManualThumbnailGenerationService(
			logger, socketService, thumbnailService, bgTaskQueue);

		var handler = new ManualThumbnailGenerationJobHandler(concreteService);
		var payload =
			JsonSerializer.Serialize(new ManualThumbnailGenerationPayload { SubPath = "/test" });

		await handler.ExecuteAsync(payload, CancellationToken.None);

		Assert.AreEqual("/test", thumbnailService.Inputs[0].Item1);
	}

	private sealed class FakeIManualThumbnailGenerationService : IManualThumbnailGenerationService
	{
		public Task ManualBackgroundQueue(string subPath)
		{
			return Task.CompletedTask;
		}
	}
}
