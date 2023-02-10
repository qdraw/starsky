using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.syncbackground.Helpers;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.realtime.Interfaces;
using starsky.foundation.settings.Enums;
using starsky.foundation.settings.Interfaces;
using starsky.foundation.sync.SyncInterfaces;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.syncbackground.Helpers
{
	[TestClass]
	public sealed class OnStartupSyncTest
	{
		private static IServiceScopeFactory GetNewScope()
		{
			var services = new ServiceCollection();
			services.AddSingleton<IWebSocketConnectionsService, FakeIWebSocketConnectionsService>();
			services.AddSingleton<INotificationQuery, FakeINotificationQuery>();
			services.AddSingleton<AppSettings>();
			services.AddSingleton<ISynchronize, FakeISynchronize>();
			services.AddSingleton<ISettingsService, FakeISettingsService>();
			services.AddSingleton<IWebLogger, FakeIWebLogger>();

			var serviceProvider = services.BuildServiceProvider();
			return serviceProvider.GetRequiredService<IServiceScopeFactory>();
		}
		
		[TestMethod]
		public async Task StartUpSync_DoesStoreAfterWards()
		{
			var scope = GetNewScope();
			var appSettings = scope.CreateScope().ServiceProvider.GetRequiredService<AppSettings>();
			var synchronize = scope.CreateScope().ServiceProvider.GetRequiredService<ISynchronize>();
			var settingsService = scope.CreateScope().ServiceProvider.GetRequiredService<ISettingsService>();
			var logger = scope.CreateScope().ServiceProvider.GetRequiredService<IWebLogger>();

			var startupSync = new OnStartupSync(scope, appSettings, synchronize, settingsService,logger);
			await startupSync.StartUpSync();

			var setting = await settingsService.GetSetting<DateTime>(SettingsType
				.LastSyncBackgroundDateTime);
			
			Assert.IsNotNull(setting);
			Assert.AreEqual(DateTime.UtcNow.Day, setting.ToUniversalTime().Day);
			Assert.AreEqual(DateTime.UtcNow.Hour, setting.ToUniversalTime().Hour);
			Assert.IsTrue((synchronize as FakeISynchronize)!.Inputs.Any(p => p.Item1 == "/"));
		}
		
		[TestMethod]
		public async Task StartUpSync_TurnedOff()
		{
			var scope = GetNewScope();
			var appSettings = scope.CreateScope().ServiceProvider.GetRequiredService<AppSettings>();
			var synchronize = scope.CreateScope().ServiceProvider.GetRequiredService<ISynchronize>();
			var settingsService = scope.CreateScope().ServiceProvider.GetRequiredService<ISettingsService>();
			var logger = scope.CreateScope().ServiceProvider.GetRequiredService<IWebLogger>();

			appSettings.SyncOnStartup = false;
			var startupSync = new OnStartupSync(scope, appSettings, synchronize, settingsService,logger);
			
			// Assert
			await startupSync.StartUpSync();

			var setting = await settingsService.GetSetting<DateTime>(SettingsType
				.LastSyncBackgroundDateTime);
			
			Assert.IsFalse((synchronize as FakeISynchronize)!.Inputs.Any(p => p.Item1 == "/"));
			
			Assert.AreEqual(1, setting.Year);
			Assert.AreEqual(1, setting.Month);
			Assert.AreEqual(1, setting.Day);
		}

		[TestMethod]
		public async Task StartUpSync_Sockets()
		{
			var scope = GetNewScope();
			var appSettings = scope.CreateScope().ServiceProvider.GetRequiredService<AppSettings>();
			var synchronize = scope.CreateScope().ServiceProvider.GetRequiredService<ISynchronize>();
			var settingsService = scope.CreateScope().ServiceProvider.GetRequiredService<ISettingsService>();
			var socketService = scope.CreateScope().ServiceProvider.GetRequiredService<IWebSocketConnectionsService>();
			var logger = scope.CreateScope().ServiceProvider.GetRequiredService<IWebLogger>();

			var startupSync = new OnStartupSync(scope, appSettings, synchronize, settingsService,logger);
			await startupSync.PushToSockets(new List<FileIndexItem>());
			var result =
				( socketService as
					FakeIWebSocketConnectionsService )!.FakeSendToAllAsync.Any();
			Assert.IsTrue(result);
		}
	}


}
