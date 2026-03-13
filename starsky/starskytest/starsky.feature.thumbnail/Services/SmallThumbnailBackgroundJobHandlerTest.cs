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
public sealed class SmallThumbnailBackgroundJobHandlerTest
{
	[TestMethod]
	public void JobType_ShouldReturnCorrectType()
	{
		var handler = new SmallThumbnailBackgroundJobHandler(
			new FakeISmallThumbnailBackgroundJobService());
		Assert.AreEqual(SmallThumbnailBackgroundJobService.JobType, handler.JobType);
	}

	[TestMethod]
	public async Task ExecuteAsync_NullPayload_ThrowsArgumentException()
	{
		var handler = new SmallThumbnailBackgroundJobHandler(
			new FakeISmallThumbnailBackgroundJobService());
		await Assert.ThrowsExactlyAsync<ArgumentException>(() =>
			handler.ExecuteAsync(null, CancellationToken.None));
	}

	[TestMethod]
	public async Task ExecuteAsync_EmptyPayload_ThrowsArgumentException()
	{
		var handler = new SmallThumbnailBackgroundJobHandler(
			new FakeISmallThumbnailBackgroundJobService());
		await Assert.ThrowsExactlyAsync<ArgumentException>(() =>
			handler.ExecuteAsync(string.Empty, CancellationToken.None));
	}

	[TestMethod]
	public async Task ExecuteAsync_InvalidJson_ThrowsJsonException()
	{
		var handler = new SmallThumbnailBackgroundJobHandler(
			new FakeISmallThumbnailBackgroundJobService());
		await Assert.ThrowsExactlyAsync<JsonException>(() =>
			handler.ExecuteAsync("invalid-json", CancellationToken.None));
	}

	[TestMethod]
	public async Task ExecuteAsync_NullDeserializedPayload_ThrowsArgumentException()
	{
		var handler = new SmallThumbnailBackgroundJobHandler(
			new FakeISmallThumbnailBackgroundJobService());
		await Assert.ThrowsExactlyAsync<ArgumentException>(() =>
			handler.ExecuteAsync("null", CancellationToken.None));
	}

	[TestMethod]
	public async Task ExecuteAsync_InterfaceMismatch_ThrowsInvalidOperationException()
	{
		var handler = new SmallThumbnailBackgroundJobHandler(
			new FakeISmallThumbnailBackgroundJobService());
		var payload = new SmallThumbnailBackgroundPayload { Path = "test" };
		var json = JsonSerializer.Serialize(payload);
		await Assert.ThrowsExactlyAsync<InvalidOperationException>(() =>
			handler.ExecuteAsync(json, CancellationToken.None));
	}

	private class FakeISmallThumbnailBackgroundJobService : ISmallThumbnailBackgroundJobService
	{
		public Task<bool> CreateJob(bool? isAuthenticated, string? filePath) =>
			Task.FromResult(true);
	}

		[TestMethod]
		public async Task ExecuteAsync_ValidPayload_CallsService()
		{
			var fakeQueue = new FakeThumbnailBackgroundTaskQueue();
			var fakeStorage = new FakeIStorage();
			var fakeSelectorStorage = new FakeSelectorStorage(fakeStorage);
			var fakeThumbnailService = new FakeIThumbnailService(fakeSelectorStorage);
			var fakeSocketService = new FakeIThumbnailSocketService();
			var fakeLogger = new FakeIWebLogger();

			var service = new SmallThumbnailBackgroundJobService(
				fakeQueue,
				fakeThumbnailService,
				fakeSelectorStorage,
				fakeSocketService,
				fakeLogger);

			var handler = new SmallThumbnailBackgroundJobHandler(service);
			var payload = new SmallThumbnailBackgroundPayload { Path = "/test.jpg" };
			var json = JsonSerializer.Serialize(payload);

			await handler.ExecuteAsync(json, CancellationToken.None);

			Assert.AreEqual("/test.jpg", fakeThumbnailService.Inputs[0].Item1);
		}
}
