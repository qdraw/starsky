using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Models;
using starsky.foundation.sync.WatcherServices;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.sync.WatcherServices
{
	[TestClass]
	public class DiskWatcherBackgroundServiceTest
	{
		[TestMethod]
		public void StartAsync_Enabled()
		{
			var diskWatcher = new FakeDiskWatcher();
			var appSettings = new AppSettings{UseDiskWatcher = true};
			new DiskWatcherBackgroundService(diskWatcher,appSettings, new FakeIWebLogger()).StartAsync(CancellationToken
				.None);
			Assert.AreEqual(appSettings.StorageFolder, diskWatcher.AddedItems.FirstOrDefault());
		}
		
		[TestMethod]
		public void StartAsync_FeatureToggleDisabled()
		{
			var diskWatcher = new FakeDiskWatcher();
			var appSettings = new AppSettings{UseDiskWatcher = false};
			new DiskWatcherBackgroundService(diskWatcher,appSettings, new FakeIWebLogger()).StartAsync(CancellationToken
				.None);
			Assert.AreEqual(0, diskWatcher.AddedItems.Count);
		}
	}


}
