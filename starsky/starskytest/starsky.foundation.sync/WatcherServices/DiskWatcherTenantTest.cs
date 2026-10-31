using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Tenancy;
using starsky.foundation.sync.WatcherInterfaces;
using starsky.foundation.sync.WatcherServices;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.sync.WatcherServices;

[TestClass]
public sealed class DiskWatcherTenantTest
{
	private readonly string _root =
		Path.Combine(Path.GetTempPath(), "starsky-watcher-" + Guid.NewGuid().ToString("N"));

	[TestCleanup]
	public void Cleanup()
	{
		TenantAmbient.Reset();
		if ( Directory.Exists(_root) )
		{
			Directory.Delete(_root, true);
		}
	}

	private sealed class RecordingQueueProcessor : IQueueProcessor
	{
		public readonly List<(string Path, string AmbientTenant)> Jobs = [];

		public Task QueueJob(string filepath, string? toPath, WatcherChangeTypes changeTypes)
		{
			// the job takes the tenant of the flow that creates it
			Jobs.Add(( filepath, TenantAmbient.Current ));
			return Task.CompletedTask;
		}
	}

	private string Folder(string name)
	{
		var path = Path.Combine(_root, name);
		Directory.CreateDirectory(path);
		return path;
	}

	[TestMethod]
	public void Event_OfAWatcher_QueuesAsThatTenant_AndDoesNotLeakIt()
	{
		var wrapper = new FakeIFileSystemWatcherWrapper();
		var queue = new RecordingQueueProcessor();
		var watcher = new DiskWatcher(wrapper, new FakeIWebLogger(), queue);
		watcher.Watcher(Folder("acme"), "acme");

		wrapper.TriggerOnCreated(new FileSystemEventArgs(WatcherChangeTypes.Created, _root,
			"photo.jpg"));
		wrapper.TriggerOnChanged(new FileSystemEventArgs(WatcherChangeTypes.Changed, _root,
			"photo2.jpg"));

		Assert.HasCount(2, queue.Jobs);
		Assert.IsTrue(queue.Jobs.All(j => j.AmbientTenant == "acme"));
		Assert.AreEqual("default", TenantAmbient.Current);
	}

	[TestMethod]
	public void Event_WithoutTenant_IsTheDefaultTenant()
	{
		var wrapper = new FakeIFileSystemWatcherWrapper();
		var queue = new RecordingQueueProcessor();
		var watcher = new DiskWatcher(wrapper, new FakeIWebLogger(), queue);
		watcher.Watcher(Folder("root"));

		wrapper.TriggerOnCreated(new FileSystemEventArgs(WatcherChangeTypes.Created, _root,
			"photo.jpg"));

		Assert.AreEqual("default", queue.Jobs.Single().AmbientTenant);
	}

	[TestMethod]
	public void Rename_QueuesAsThatTenant()
	{
		var queue = new RecordingQueueProcessor();
		var watcher = new DiskWatcher(new FakeIFileSystemWatcherWrapper(), new FakeIWebLogger(),
			queue);

		watcher.OnRenamed(this,
			new RenamedEventArgs(WatcherChangeTypes.Renamed, _root, "b.jpg", "a.jpg"), "beta");

		Assert.AreEqual("beta", queue.Jobs.Single().AmbientTenant);
	}

	[TestMethod]
	public void SameFolderTwice_IsWatchedOnce()
	{
		var logger = new FakeIWebLogger();
		var watcher = new DiskWatcher(new FakeIFileSystemWatcherWrapper(), logger,
			new RecordingQueueProcessor());
		var folder = Folder("acme");

		watcher.Watcher(folder, "acme");
		watcher.Watcher(folder, "acme");

		Assert.AreEqual(1, logger.TrackedInformation.Count(p =>
			p.Item2?.Contains("[DiskWatcher] started") == true));
	}

	[TestMethod]
	public void FolderThatAppearsLater_IsWatchedByTheNextReconcile()
	{
		var wrapper = new FakeIFileSystemWatcherWrapper();
		var logger = new FakeIWebLogger();
		var watcher = new DiskWatcher(wrapper, logger, new RecordingQueueProcessor());
		var folder = Path.Combine(_root, "later");

		// not there yet: nothing is watched, and the path is not remembered as watched
		watcher.Watcher(folder, "acme");
		Assert.AreEqual(string.Empty, wrapper.Path);

		Directory.CreateDirectory(folder);
		watcher.Watcher(folder, "acme");

		Assert.AreEqual(folder, wrapper.Path);
	}

	[TestMethod]
	public void BackgroundService_WatchesEveryActiveTenantInItsOwnFolder()
	{
		var appSettings = new AppSettings
		{
			StorageFolder = Folder("default-storage"),
			ThumbnailTempFolder = Folder("default-thumbs"),
			TempFolder = Folder("default-temp"),
			TenantsRoot = Path.Combine(_root, "tenants")
		};
		var registry = new FakeTenantRegistry("default", "acme", "beta");
		var provider = new TenantSettingsProvider(appSettings, registry);
		var fakeWatcher = new FakeDiskWatcher();

		new DiskWatcherBackgroundService(fakeWatcher, appSettings, new FakeIWebLogger(), registry,
			provider).StartWatchers();

		Assert.AreEqual(appSettings.StorageFolder, fakeWatcher.AddedItems[0],
			"the default tenant is first: it is the primary watcher");
		CollectionAssert.AreEqual(new[] { "acme", "beta" },
			fakeWatcher.AddedByTenant.Select(p => p.TenantId).ToArray());
		Assert.IsTrue(fakeWatcher.AddedByTenant[0].Path
			.StartsWith(Path.Combine(_root, "tenants", "acme")));
		Assert.IsTrue(fakeWatcher.AddedByTenant[1].Path
			.StartsWith(Path.Combine(_root, "tenants", "beta")));
	}

	[TestMethod]
	public async Task BackgroundService_StartsTheWatcherOfANewTenant()
	{
		var appSettings = new AppSettings
		{
			StorageFolder = Folder("default-storage"),
			ThumbnailTempFolder = Folder("default-thumbs"),
			TempFolder = Folder("default-temp"),
			TenantsRoot = Path.Combine(_root, "tenants")
		};
		var registry = new MutableRegistry();
		var provider = new TenantSettingsProvider(appSettings, registry);
		var fakeWatcher = new FakeDiskWatcher();
		var service = new DiskWatcherBackgroundService(fakeWatcher, appSettings,
			new FakeIWebLogger(), registry, provider)
		{
			ReconcileInterval = TimeSpan.FromMilliseconds(50)
		};

		await service.StartAsync(default);
		registry.Add("acme");
		for ( var i = 0; i < 100 && fakeWatcher.AddedByTenant.Count == 0; i++ )
		{
			await Task.Delay(50);
		}

		await service.StopAsync(default);
		Assert.AreEqual("acme", fakeWatcher.AddedByTenant.FirstOrDefault().TenantId);
	}

	private sealed class MutableRegistry : ITenantRegistry
	{
		private readonly List<string> _active = ["default"];

		public IReadOnlyCollection<string> ActiveTenantIds => _active.ToArray();

		public void Add(string id)
		{
			lock ( _active )
			{
				_active.Add(id);
			}
		}

		public bool IsActive(string tenantId) => _active.Contains(tenantId);

		public TenantRoots? GetRoots(string tenantId) => null;

		public void Invalidate()
		{
		}
	}
}
