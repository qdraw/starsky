using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.realtime.Interfaces;
using starsky.foundation.sync.SyncInterfaces;
using starsky.foundation.database.Interfaces;
using starsky.foundation.sync.WatcherHelpers;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.sync.WatcherHelpers;

[TestClass]
public sealed class QueueProcessorJobHandlerTest
{
	[TestMethod]
	public void JobType_ShouldMatchQueueProcessor()
	{
		var handler = new QueueProcessorJobHandler(new FakeIServiceScopeFactory());
		Assert.AreEqual(QueueProcessor.JobType, handler.JobType);
	}

	[TestMethod]
	public async Task ExecuteAsync_NullPayload_ShouldThrowArgumentException()
	{
		var handler = new QueueProcessorJobHandler(new FakeIServiceScopeFactory());
		await Assert.ThrowsExactlyAsync<ArgumentException>(() =>
			handler.ExecuteAsync(null, CancellationToken.None));
	}

	[TestMethod]
	public async Task ExecuteAsync_EmptyPayload_ShouldThrowArgumentException()
	{
		var handler = new QueueProcessorJobHandler(new FakeIServiceScopeFactory());
		await Assert.ThrowsExactlyAsync<ArgumentException>(() =>
			handler.ExecuteAsync(string.Empty, CancellationToken.None));
	}

	[TestMethod]
	public async Task ExecuteAsync_InvalidJson_ShouldThrowJsonException()
	{
		var handler = new QueueProcessorJobHandler(new FakeIServiceScopeFactory());
		await Assert.ThrowsExactlyAsync<JsonException>(() =>
			handler.ExecuteAsync("invalid-json", CancellationToken.None));
	}

	[TestMethod]
	public async Task ExecuteAsync_ValidPayload_ShouldCallSync()
	{
		var services = new ServiceCollection();
		var synchronize = new FakeISynchronize();
		var appSettings = new AppSettings { StorageFolder = "C:\\" };
		var connectionsService = new FakeIWebSocketConnectionsService();
		var query = new FakeIQuery();
		var logger = new FakeIWebLogger();
		var notificationQuery = new FakeINotificationQuery();

		services.AddSingleton<ISynchronize>(synchronize);
		services.AddSingleton(appSettings);
		services.AddSingleton<IWebSocketConnectionsService>(connectionsService);
		services.AddSingleton<IQuery>(query);
		services.AddSingleton<IWebLogger>(logger);
		services.AddSingleton<INotificationQuery>(notificationQuery);

		var serviceProvider = services.BuildServiceProvider();
		var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

		var handler = new QueueProcessorJobHandler(scopeFactory);
		var payload = new QueueProcessorPayload
		{
			FilePath = "test",
			ToPath = "to",
			ChangeTypes = WatcherChangeTypes.Changed
		};
		var payloadJson = JsonSerializer.Serialize(payload);

		await handler.ExecuteAsync(payloadJson, CancellationToken.None);

		Assert.HasCount(1, synchronize.Inputs);
	}
}
