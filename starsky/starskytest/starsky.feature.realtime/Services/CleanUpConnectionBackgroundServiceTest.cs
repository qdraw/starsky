using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.realtime.Interface;
using starsky.feature.realtime.Services;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.realtime.Services;

[TestClass]
public class CleanUpConnectionBackgroundServiceTest
{
	private readonly IRealtimeConnectionsService _realtimeConnectionsService;
	private readonly FakeIWebLogger _console;
	private readonly IServiceScopeFactory _serviceScopeFactory;

	public CleanUpConnectionBackgroundServiceTest()
	{
		var services = new ServiceCollection();
		services.AddSingleton<AppSettings>();
		services.AddSingleton<BackgroundService, CleanUpConnectionBackgroundService>();
		services.AddSingleton<IRealtimeConnectionsService, FakeIRealtimeConnectionsService>();
		services.AddSingleton<IWebLogger, FakeIWebLogger>();

		var serviceProvider = services.BuildServiceProvider();
		_serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
		_realtimeConnectionsService = serviceProvider.GetRequiredService<IRealtimeConnectionsService>();
			
		var webLogger = serviceProvider.GetRequiredService<IWebLogger>();
		_console = webLogger as FakeIWebLogger;
	}
	
	[TestMethod]
	public async Task StartAsync_IsRemoved_HappyFlow()
	{
		var service = _realtimeConnectionsService as FakeIRealtimeConnectionsService;
		service!.FakeSendToAllAsync = new List<Tuple<string, DateTime>>{new Tuple<string, DateTime>("1", DateTime.UnixEpoch)};
		Assert.AreEqual(1, service!.FakeSendToAllAsync.Count);

		await new CleanUpConnectionBackgroundService(_serviceScopeFactory).StartAsync(new CancellationToken());
		Assert.AreEqual(0, service!.FakeSendToAllAsync.Count);
	}

}
