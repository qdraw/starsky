using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Interfaces;
using starsky.foundation.realtime.Interfaces;
using starsky.feature.realtime.Interface;
using starsky.feature.syncbackground.Helpers;
using starsky.feature.syncbackground.Interfaces;
using starsky.feature.syncbackground.Services;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.settings.Enums;
using starsky.foundation.settings.Interfaces;
using starsky.foundation.sync.SyncInterfaces;
using starsky.foundation.sync.WatcherBackgroundService;
using starsky.foundation.worker.Interfaces;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.syncbackground.Services;

[TestClass]
public sealed class OnStartupSyncBackgroundServiceTest
{
	private static IServiceScopeFactory GetNewScope()
	{
		var services = new ServiceCollection();
		services.AddSingleton<IRealtimeConnectionsService, FakeIRealtimeConnectionsService>();
		services.AddSingleton(new AppSettings { SyncOnStartup = true });
		services.AddSingleton<ISynchronize, FakeISynchronize>();
		services.AddSingleton<IWebLogger, FakeIWebLogger>();
		services.AddSingleton<ISettingsService, FakeISettingsService>();
		services.AddSingleton<IOnStartupSync, OnStartupSync>();
		services.AddSingleton<IBackgroundJobHandler, OnStartupSyncJobHandler>();
		services.AddSingleton<INotificationQuery, FakeINotificationQuery>();
		services.AddSingleton<IWebSocketConnectionsService, FakeIWebSocketConnectionsService>();

		// Create a factory first
		var tempProvider = services.BuildServiceProvider();
		var scopeFactory = tempProvider.GetRequiredService<IServiceScopeFactory>();
		// Now register the fake queue with the scope factory
		services.AddSingleton<IDiskWatcherBackgroundTaskQueue>(
			new FakeDiskWatcherUpdateBackgroundTaskQueue(scopeFactory));

		var serviceProvider = services.BuildServiceProvider();
		return serviceProvider.GetRequiredService<IServiceScopeFactory>();
	}

	[TestMethod]
	public async Task OnStartupSyncBackgroundService_DoesStoreAfterWards()
	{
		var services = new ServiceCollection();
		services.AddSingleton<IRealtimeConnectionsService, FakeIRealtimeConnectionsService>();
		services.AddSingleton(new AppSettings { SyncOnStartup = true });
		services.AddSingleton<ISynchronize, FakeISynchronize>();
		services.AddSingleton<IWebLogger, FakeIWebLogger>();
		services.AddSingleton<ISettingsService, FakeISettingsService>();
		services.AddSingleton<IOnStartupSync, OnStartupSync>();
		services.AddSingleton<IBackgroundJobHandler, OnStartupSyncJobHandler>();
		services.AddSingleton<INotificationQuery, FakeINotificationQuery>();
		services.AddSingleton<IWebSocketConnectionsService, FakeIWebSocketConnectionsService>();
		services.AddSingleton<IDiskWatcherBackgroundTaskQueue,
				FakeDiskWatcherUpdateBackgroundTaskQueue>();

		var serviceProvider = services.BuildServiceProvider();
		var finalScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

		var startupSync = new OnStartupSyncBackgroundService(finalScopeFactory);
		await startupSync.StartAsync(CancellationToken.None);

		var settingsService = serviceProvider.GetRequiredService<ISettingsService>();

		var setting = await settingsService.GetSetting<DateTime>(SettingsType
			.LastSyncBackgroundDateTime);

		Assert.AreNotEqual(0, setting.Year);
		Assert.AreNotEqual(0, setting.Month);
		Assert.AreNotEqual(0, setting.Day);


		Assert.AreEqual(DateTime.UtcNow.Day, setting.ToUniversalTime().Day);
		Assert.AreEqual(DateTime.UtcNow.Hour, setting.ToUniversalTime().Hour);
		var synchronize = serviceProvider.GetRequiredService<ISynchronize>() as FakeISynchronize;
		Assert.IsNotNull(synchronize);
		Assert.IsTrue(synchronize.Inputs.Exists(p => p.Item1 == "/"));
	}
}
