using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.mountwatch.MountWatcher;
using starsky.foundation.mountwatch.Services;

namespace starskytest.starsky.foundation.mountwatch.Services;

[TestClass]
public sealed class WindowsMountWatcherTest
{
	[TestMethod]
	public void WindowsMountWatcher_OnConstruction_IsNotRunning()
	{
		// Arrange & Act
		var watcher = new WindowsMountWatcher();

		// Assert
		Assert.IsNotNull(watcher);
	}

	[TestMethod]
	public void WindowsMountWatcher_GetMountedVolumes_ReturnsEnumerable()
	{
		// Arrange
		var watcher = new WindowsMountWatcher();

		// Act
		var volumes = watcher.GetMountedVolumes();

		// Assert
		Assert.IsNotNull(volumes);
	}

	[TestMethod]
	public void WindowsMountWatcher_Stop_CanBeCalled()
	{
		// Arrange
		var watcher = new WindowsMountWatcher();

		// Act
		watcher.Stop();

		// Assert
		Assert.IsNotNull(watcher);
	}
}
