using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Models;
using starsky.foundation.sync.WatcherServices;
using starskytest.ExtensionMethods;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.sync.WatcherServices;

[TestClass]
public sealed class DiskWatcherBackgroundServiceTest
{
	[TestMethod]
	public async Task StartAsync_Enabled()
	{
		var diskWatcher = new FakeDiskWatcher();
		var appSettings = new AppSettings { UseDiskWatcher = true };
		var service =
			new DiskWatcherBackgroundService(diskWatcher, appSettings, new FakeIWebLogger());

		var dynMethod = service.GetType().GetMethod("ExecuteAsync",
			BindingFlags.NonPublic | BindingFlags.Instance)!;
		await dynMethod.InvokeAsync(service, CancellationToken.None);

		Assert.AreEqual(appSettings.StorageFolder, diskWatcher.AddedItems.FirstOrDefault());
	}

	[TestMethod]
	public async Task StartAsync_FeatureToggleDisabled()
	{
		var diskWatcher = new FakeDiskWatcher();
		var appSettings = new AppSettings { UseDiskWatcher = false };
		var service =
			new DiskWatcherBackgroundService(diskWatcher, appSettings, new FakeIWebLogger());

		var dynMethod = service.GetType().GetMethod("ExecuteAsync",
			BindingFlags.NonPublic | BindingFlags.Instance)!;
		await dynMethod.InvokeAsync(service, CancellationToken.None);

		Assert.IsEmpty(diskWatcher.AddedItems);
	}

	[TestMethod]
	public async Task StartAsync_WatchesMappedPhysicalPaths()
	{
		var diskWatcher = new FakeDiskWatcher();
		var appSettings = new AppSettings
		{
			UseDiskWatcher = true,
			StorageFolderMappings = new Dictionary<string, string>
			{
				{ "/archive", "/data/archive" },
				{ "/old", "/data/old" }
			}
		};
		var service =
			new DiskWatcherBackgroundService(diskWatcher, appSettings, new FakeIWebLogger());

		var dynMethod = service.GetType().GetMethod("ExecuteAsync",
			BindingFlags.NonPublic | BindingFlags.Instance)!;
		await dynMethod.InvokeAsync(service, CancellationToken.None);

		Assert.Contains("/data/archive", diskWatcher.AddedItems);
		Assert.Contains("/data/old", diskWatcher.AddedItems);
	}
}
