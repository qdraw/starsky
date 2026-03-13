using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models;
using starsky.foundation.sync.SyncServices;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.sync.SyncServices;

[TestClass]
public sealed class ManualBackgroundSyncJobHandlerTest
{
	[TestMethod]
	public void JobType_ShouldMatchExpected()
	{
		var service = new FakeIManualBackgroundSyncService(new Dictionary<string, FileIndexItem.ExifStatus>());
		var handler = new ManualBackgroundSyncJobHandler(service);
		Assert.AreEqual(ManualBackgroundSyncService.JobType, handler.JobType);
	}

	[TestMethod]
	public async Task ExecuteAsync_MissingPayload_ShouldThrowArgumentException()
	{
		var service = new FakeIManualBackgroundSyncService(new Dictionary<string, FileIndexItem.ExifStatus>());
		var handler = new ManualBackgroundSyncJobHandler(service);
		
		await Assert.ThrowsExactlyAsync<ArgumentException>(async () => 
			await handler.ExecuteAsync(null, CancellationToken.None));
			
		await Assert.ThrowsExactlyAsync<ArgumentException>(async () => 
			await handler.ExecuteAsync("  ", CancellationToken.None));
	}

	[TestMethod]
	public async Task ExecuteAsync_InvalidPayload_ShouldThrowJsonException()
	{
		var service = new FakeIManualBackgroundSyncService(new Dictionary<string, FileIndexItem.ExifStatus>());
		var handler = new ManualBackgroundSyncJobHandler(service);
		
		await Assert.ThrowsExactlyAsync<JsonException>(async () => 
			await handler.ExecuteAsync("{ invalid }", CancellationToken.None));
	}

	[TestMethod]
	public async Task ExecuteAsync_ImplementationMismatch_ShouldThrowInvalidOperationException()
	{
		var service = new FakeIManualBackgroundSyncService(new Dictionary<string, FileIndexItem.ExifStatus>());
		var handler = new ManualBackgroundSyncJobHandler(service);
		var payload = JsonSerializer.Serialize(new ManualBackgroundSyncPayload { SubPath = "/" });
		
		await Assert.ThrowsExactlyAsync<InvalidOperationException>(async () => 
			await handler.ExecuteAsync(payload, CancellationToken.None));
	}

	[TestMethod]
	public async Task ExecuteAsync_CorrectImplementation_ShouldCallService()
	{
		var synchronize = new FakeISynchronize();
		var query = new FakeIQuery();
		var socketUpdateService = new SocketSyncUpdateService(
			new FakeIWebSocketConnectionsService(), 
			new FakeINotificationQuery(), 
			new FakeIWebLogger());
		
		var services = new ServiceCollection();
		services.AddMemoryCache();
		var serviceProvider = services.BuildServiceProvider();
		var cache = serviceProvider.GetRequiredService<IMemoryCache>();
		
		var logger = new FakeIWebLogger();
		var bgTaskQueue = new FakeIUpdateBackgroundTaskQueue();
		
		var concreteService = new ManualBackgroundSyncService(
			synchronize, query, socketUpdateService, cache, logger, bgTaskQueue);
		
		var handler = new ManualBackgroundSyncJobHandler(concreteService);
		var payload = JsonSerializer.Serialize(new ManualBackgroundSyncPayload { SubPath = "/test" });
		
		await handler.ExecuteAsync(payload, CancellationToken.None);
		
		Assert.AreEqual("/test", synchronize.Inputs[0].Item1);
	}
}
