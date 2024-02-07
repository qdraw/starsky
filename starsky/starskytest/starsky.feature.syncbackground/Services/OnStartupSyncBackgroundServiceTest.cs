using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.realtime.Interface;
using starsky.feature.syncbackground.Services;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.settings.Enums;
using starsky.foundation.settings.Interfaces;
using starsky.foundation.sync.SyncInterfaces;
using starsky.foundation.sync.WatcherBackgroundService;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.syncbackground.Services
{
	[TestClass]
	public sealed class OnStartupSyncBackgroundServiceTest
	{
		private static IServiceScopeFactory GetNewScope()
		{
			var services = new ServiceCollection();
			services.AddSingleton<IRealtimeConnectionsService, FakeIRealtimeConnectionsService>();
			services.AddSingleton<AppSettings>();
			services.AddSingleton<ISynchronize, FakeISynchronize>();
			services.AddSingleton<IWebLogger, FakeIWebLogger>();
			services.AddSingleton<ISettingsService, FakeISettingsService>();
			services
				.AddSingleton<IDiskWatcherBackgroundTaskQueue,
					FakeDiskWatcherUpdateBackgroundTaskQueue>();
			var serviceProvider = services.BuildServiceProvider();
			return serviceProvider.GetRequiredService<IServiceScopeFactory>();
		}

		[TestMethod]
		public async Task OnStartupSyncBackgroundService_DoesStoreAfterWards()
		{
			var scope = GetNewScope();
			var synchronize =
				scope.CreateScope().ServiceProvider.GetRequiredService<ISynchronize>();
			var settingsService = scope.CreateScope().ServiceProvider
				.GetRequiredService<ISettingsService>();
			var startupSync = new OnStartupSyncBackgroundService(scope);
			await startupSync.StartAsync(CancellationToken.None);

			var setting = await settingsService.GetSetting<DateTime>(SettingsType
				.LastSyncBackgroundDateTime);

			Assert.IsNotNull(setting);
			Assert.AreEqual(DateTime.UtcNow.Day, setting.ToUniversalTime().Day);
			Assert.AreEqual(DateTime.UtcNow.Hour, setting.ToUniversalTime().Hour);
			Assert.IsTrue(( synchronize as FakeISynchronize )!.Inputs.Exists(p => p.Item1 == "/"));
		}
	}
}
