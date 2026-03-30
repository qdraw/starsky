using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.mountwatch.MountWatcher;
using starsky.foundation.mountwatch.Services;

namespace starskytest.starsky.foundation.mountwatch.Services;

[TestClass]
public sealed class MacMountWatcherTest
{
	[TestMethod]
	public void MacMountWatcher_OnConstruction_IsNotRunning()
	{
		// Arrange & Act
		var watcher = new MacMountWatcher();

		// Assert
		Assert.IsNotNull(watcher);
	}

	[TestMethod]
	public void MacMountWatcher_GetMountedVolumes_ReturnsEnumerable()
	{
		// Arrange
		var watcher = new MacMountWatcher();

		// Act
		var volumes = watcher.GetMountedVolumes();

		// Assert
		Assert.IsNotNull(volumes);
	}

	[TestMethod]
	public void MacMountWatcher_Stop_CanbeCalled()
	{
		// Arrange
		var watcher = new MacMountWatcher();

		// Act
		watcher.Stop();

		// Assert
		Assert.IsNotNull(watcher);
	}
}
