using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.realtime.Interface;
using starsky.feature.realtime.Services;
using starsky.feature.syncbackground.Helpers;
using starsky.feature.webftppublish.Helpers;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Models;
using starsky.foundation.realtime.Enums;
using starsky.foundation.realtime.Interfaces;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.sync.SyncInterfaces;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.syncbackground.Helpers
{
	[TestClass]
	public class OnStartupSyncTest
	{
		private readonly AppSettings _appSettings;
		private readonly FakeIFtpWebRequestFactory _webRequestFactory;

		public OnStartupSyncTest()
		{
		}

		private static IServiceScopeFactory GetNewScope()
		{
			var services = new ServiceCollection();
			services.AddSingleton<IRealtimeConnectionsService, FakeIRealtimeConnectionsService>();
			services.AddSingleton<AppSettings>();
			services.AddSingleton<ISynchronize, FakeISynchronize>();
			services.AddSingleton<ISettingsService, FakeISettingsService>();
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
			var startupSync = new OnStartupSync(scope, appSettings, synchronize, settingsService);
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
			appSettings.SyncOnStartup = false;
			var startupSync = new OnStartupSync(scope, appSettings, synchronize, settingsService);
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
			var realtimeConnectionsService = scope.CreateScope().ServiceProvider.GetRequiredService<IRealtimeConnectionsService>();

			var startupSync = new OnStartupSync(scope, appSettings, synchronize, settingsService);
			await startupSync.PushToSockets(new List<FileIndexItem>());
			var result =
				( realtimeConnectionsService as
					FakeIRealtimeConnectionsService )!.FakeSendToAllAsync.Any();
			Assert.IsTrue(result);
		}
	}


}
