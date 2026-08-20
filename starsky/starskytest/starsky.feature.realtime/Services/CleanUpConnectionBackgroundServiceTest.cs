using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.realtime.Interface;
using starsky.feature.realtime.Services;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starskytest.ExtensionMethods;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.realtime.Services;

[TestClass]
public sealed class CleanUpConnectionBackgroundServiceTest
{
	private readonly IRealtimeConnectionsService _realtimeConnectionsService;
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
		_realtimeConnectionsService =
			serviceProvider.GetRequiredService<IRealtimeConnectionsService>();
	}

	[TestMethod]
	public async Task StartAsync_IsRemoved_HappyFlow()
	{
		var service = _realtimeConnectionsService as FakeIRealtimeConnectionsService;
		service!.FakeSendToAllAsync = [new Tuple<string, DateTime>("1", DateTime.UnixEpoch)];
		Assert.HasCount(1, service.FakeSendToAllAsync);

		var backgroundService = new CleanUpConnectionBackgroundService(_serviceScopeFactory);
		var dynMethod = backgroundService.GetType().GetMethod("ExecuteAsync",
			BindingFlags.NonPublic | BindingFlags.Instance)!;
		await dynMethod.InvokeAsync(backgroundService, CancellationToken.None);

		Assert.IsEmpty(service.FakeSendToAllAsync);
	}
}
