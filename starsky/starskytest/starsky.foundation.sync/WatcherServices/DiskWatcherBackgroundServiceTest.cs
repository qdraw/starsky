using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Models;
using starsky.foundation.sync.Services;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.sync.WatcherServices
{
	[TestClass]
	public class DiskWatcherBackgroundServiceTest
	{
		[TestMethod]
		public void StartAsync()
		{
			var diskWatcher = new FakeDiskWatcher();
			var appSettings = new AppSettings();
			new DiskWatcherBackgroundService(diskWatcher,appSettings).StartAsync(CancellationToken
				.None);
			Assert.AreEqual(appSettings.StorageFolder, diskWatcher.AddedItems.FirstOrDefault());
		}
	}


}
