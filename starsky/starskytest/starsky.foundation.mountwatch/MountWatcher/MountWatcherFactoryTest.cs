using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.mountwatch.MountWatcher;
using starsky.foundation.mountwatch.MountWatcher.Interfaces;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.mountwatch.MountWatcher;

[TestClass]
public sealed class MountWatcherFactoryTest
{
	[TestMethod]
	public void MountWatcherFactory_CreateMountWatcher_ReturnsValidWatcher()
	{
		// Arrange
		var factory = new MountWatcherFactory(new FakeSelectorStorage(), new FakeIWebLogger());

		// Act
		var watcher = factory.CreateMountWatcher();

		// Assert
		Assert.IsNotNull(watcher);
		Assert.IsInstanceOfType(watcher, typeof(IMountWatcher));
	}

	[TestMethod]
	public void MountWatcherFactory_CreateMountWatcher_ReturnsDifferentInstance()
	{
		// Arrange
		var factory = new MountWatcherFactory(new FakeSelectorStorage(), new FakeIWebLogger());

		// Act
		var watcher1 = factory.CreateMountWatcher();
		var watcher2 = factory.CreateMountWatcher();

		// Assert
		Assert.IsNotNull(watcher1);
		Assert.IsNotNull(watcher2);
		Assert.AreNotSame(watcher1, watcher2);
	}

	[TestMethod]
	public void MountWatcherFactory_CreateMountWatcher_ReturnsMacWatcher()
	{
		var factory = new MountWatcherFactory(new FakeSelectorStorage(), new FakeIWebLogger(),
			() => OSPlatform.OSX, 10);

		var watcher = factory.CreateMountWatcher();

		Assert.IsInstanceOfType(watcher, typeof(MacMountWatcher));
	}

	[TestMethod]
	public void MountWatcherFactory_CreateMountWatcher_ReturnsWindowsWatcher()
	{
		var factory = new MountWatcherFactory(new FakeSelectorStorage(), new FakeIWebLogger(),
			() => OSPlatform.Windows, 10);

		var watcher = factory.CreateMountWatcher();

		Assert.IsInstanceOfType(watcher, typeof(WindowsMountWatcher));
	}

	[TestMethod]
	public void MountWatcherFactory_CreateMountWatcher_ReturnsLinuxWatcher()
	{
		var factory = new MountWatcherFactory(new FakeSelectorStorage(), new FakeIWebLogger(),
			() => OSPlatform.Linux, 10);

		var watcher = factory.CreateMountWatcher();

		Assert.IsInstanceOfType(watcher, typeof(LinuxMountWatcher));
		Assert.IsInstanceOfType(watcher, typeof(IMountWatcher));
	}

	[TestMethod]
	public void MountWatcherFactory_CreateMountWatcher_UnsupportedOs_Throws()
	{
		var factory = new MountWatcherFactory(new FakeSelectorStorage(), new FakeIWebLogger(),
			() => OSPlatform.Create("Unknown"), 10);

		Assert.ThrowsExactly<NotSupportedException>(factory.CreateMountWatcher);
	}
}
