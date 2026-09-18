using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.connect.Sync;
using starsky.feature.connect.Watchers;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.connect.Watchers;

[TestClass]
public sealed class DiskWatcherSyncLocalChangeSourceTest
{
	[TestMethod]
	public void Created_Event_RaisesLocalFileChangedWithCreatedType()
	{
		var fakeWatcher = new FakeIFileSystemWatcherWrapper();
		using var source = new DiskWatcherSyncLocalChangeSource(fakeWatcher);

		SyncLocalChangeEventArgs? received = null;
		source.LocalFileChanged += (_, e) => received = e;

		fakeWatcher.TriggerOnCreated(new FileSystemEventArgs(
			WatcherChangeTypes.Created, "/tmp", "photo.jpg"));

		Assert.IsNotNull(received);
		Assert.AreEqual(WatcherChangeTypes.Created, received.ChangeType);
		Assert.IsTrue(received.FullPath.EndsWith("photo.jpg", StringComparison.Ordinal));
		Assert.IsNull(received.OldFullPath);
	}

	[TestMethod]
	public void Changed_Event_RaisesLocalFileChangedWithChangedType()
	{
		var fakeWatcher = new FakeIFileSystemWatcherWrapper();
		using var source = new DiskWatcherSyncLocalChangeSource(fakeWatcher);

		SyncLocalChangeEventArgs? received = null;
		source.LocalFileChanged += (_, e) => received = e;

		fakeWatcher.TriggerOnChanged(new FileSystemEventArgs(
			WatcherChangeTypes.Changed, "/tmp", "photo.jpg"));

		Assert.IsNotNull(received);
		Assert.AreEqual(WatcherChangeTypes.Changed, received.ChangeType);
	}

	[TestMethod]
	public void Deleted_Event_RaisesLocalFileChangedWithDeletedType()
	{
		var fakeWatcher = new FakeIFileSystemWatcherWrapper();
		using var source = new DiskWatcherSyncLocalChangeSource(fakeWatcher);

		SyncLocalChangeEventArgs? received = null;
		source.LocalFileChanged += (_, e) => received = e;

		fakeWatcher.TriggerOnDeleted(new RenamedEventArgs(
			WatcherChangeTypes.Deleted, "/tmp", "photo.jpg", "photo.jpg"));

		Assert.IsNotNull(received);
		Assert.AreEqual(WatcherChangeTypes.Deleted, received.ChangeType);
	}

	[TestMethod]
	public void Renamed_Event_IncludesOldFullPath()
	{
		var fakeWatcher = new FakeIFileSystemWatcherWrapper();
		using var source = new DiskWatcherSyncLocalChangeSource(fakeWatcher);

		SyncLocalChangeEventArgs? received = null;
		source.LocalFileChanged += (_, e) => received = e;

		fakeWatcher.TriggerOnRename(new RenamedEventArgs(
			WatcherChangeTypes.Renamed, "/tmp", "new.jpg", "old.jpg"));

		Assert.IsNotNull(received);
		Assert.AreEqual(WatcherChangeTypes.Renamed, received.ChangeType);
		Assert.IsTrue(received.FullPath.EndsWith("new.jpg", StringComparison.Ordinal));
		Assert.IsNotNull(received.OldFullPath);
		Assert.IsTrue(received.OldFullPath.EndsWith("old.jpg", StringComparison.Ordinal));
	}

	[TestMethod]
	public void Dispose_UnsubscribesFromWatcher()
	{
		var fakeWatcher = new FakeIFileSystemWatcherWrapper();
		var source = new DiskWatcherSyncLocalChangeSource(fakeWatcher);

		var eventCount = 0;
		source.LocalFileChanged += (_, _) => eventCount++;

		source.Dispose();

		fakeWatcher.TriggerOnCreated(new FileSystemEventArgs(
			WatcherChangeTypes.Created, "/tmp", "photo.jpg"));

		Assert.AreEqual(0, eventCount, "No events should fire after Dispose.");
	}
}
