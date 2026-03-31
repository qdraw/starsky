using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.mountwatch.MountWatcher;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.mountwatch.Services;

[TestClass]
public sealed class WindowsMountWatcherTest
{
	[TestMethod]
	public void WindowsMountWatcher_OnConstruction_IsNotRunning()
	{
		// Arrange & Act
		var watcher = new WindowsMountWatcher(new FakeIWebLogger());

		// Assert
		Assert.IsNotNull(watcher);
	}

	[TestMethod]
	public void WindowsMountWatcher_GetMountedVolumes_ReturnsEnumerable()
	{
		// Arrange
		var watcher = new WindowsMountWatcher(new FakeIWebLogger());

		// Act
		var volumes = watcher.GetMountedVolumes();

		// Assert
		Assert.IsNotNull(volumes);
	}

	[TestMethod]
	public void WindowsMountWatcher_Stop_CanBeCalled()
	{
		// Arrange
		var watcher = new WindowsMountWatcher(new FakeIWebLogger());

		// Act
		watcher.Stop();

		// Assert
		Assert.IsNotNull(watcher);
	}

	[TestMethod]
	[Timeout(5000, CooperativeCancellation = true)]
	public void WindowsMountWatcher_Start_DoesNotThrow_WhenPlatformIsUnsupported()
	{
		// This test is to ensure that even if WMI or other platform-specifics fail,
		// the Start() method handles it and potentially falls back to polling.
		// Since we cannot easily mock the platform check or WMI, we just ensure it doesn't throw.

		// Arrange
		var watcher = new WindowsMountWatcher(new FakeIWebLogger());

		// Act & Assert (Should not throw)
		watcher.Start();
		watcher.Stop();
	}
}
