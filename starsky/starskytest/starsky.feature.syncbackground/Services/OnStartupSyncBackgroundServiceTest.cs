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
		services.AddSingleton<IDiskWatcherBackgroundTaskQueue>(sp =>
			new FakeDiskWatcherUpdateBackgroundTaskQueue(sp.GetRequiredService<IServiceScopeFactory>()));

		var serviceProvider = services.BuildServiceProvider();
		var finalScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

		var startupSync = new OnStartupSyncBackgroundService(finalScopeFactory);
		await startupSync.StartAsync(CancellationToken.None);
		
		var synchronize = serviceProvider.GetRequiredService<ISynchronize>() as FakeISynchronize;
		Assert.IsNotNull(synchronize);

		// Poll the settings service until LastSyncBackgroundDateTime is persisted.
		var settingsService = serviceProvider.GetRequiredService<ISettingsService>();
		DateTime setting = default;
		var sw = System.Diagnostics.Stopwatch.StartNew();
		while (sw.Elapsed < TimeSpan.FromSeconds(5))
		{
			setting = await settingsService.GetSetting<DateTime>(SettingsType.LastSyncBackgroundDateTime);
			if (setting.Year >= 2000)
			{
				break;
			}
			await Task.Delay(50, TestContext.CancellationToken);
		}

		Assert.IsGreaterThanOrEqualTo(2000, setting.Year, "LastSyncBackgroundDateTime was not written in time");
		var delta = (DateTime.UtcNow - setting.ToUniversalTime()).Duration();
		Assert.IsLessThan(TimeSpan.FromMinutes(2), delta, $"LastSyncBackgroundDateTime is not recent: delta={delta}");
		Assert.IsTrue(synchronize.Inputs.Exists(p => p.Item1 == "/"));
	}

	public TestContext TestContext { get; set; }
}
