using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.mountwatch.Interfaces;
using starsky.foundation.mountwatch.MountWatcher;
using starsky.foundation.mountwatch.Services;

namespace starskytest.starsky.foundation.mountwatch.Services;

[TestClass]
public sealed class MountWatcherFactoryTest
{
	[TestMethod]
	public void MountWatcherFactory_CreateMountWatcher_ReturnsValidWatcher()
	{
		// Arrange
		var factory = new MountWatcherFactory();

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
		var factory = new MountWatcherFactory();

		// Act
		var watcher1 = factory.CreateMountWatcher();
		var watcher2 = factory.CreateMountWatcher();

		// Assert
		Assert.IsNotNull(watcher1);
		Assert.IsNotNull(watcher2);
		Assert.AreNotSame(watcher1, watcher2);
	}
}
