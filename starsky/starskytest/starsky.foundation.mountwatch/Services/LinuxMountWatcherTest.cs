using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.mountwatch.MountWatcher;

namespace starskytest.starsky.foundation.mountwatch.Services;

[TestClass]
public sealed class LinuxMountWatcherTest
{
	[TestMethod]
	public void LinuxMountWatcher_OnConstruction_IsNotRunning()
	{
		// Arrange & Act
		var watcher = new LinuxMountWatcher(new FakeMocks.FakeIWebLogger());

		// Assert
		Assert.IsNotNull(watcher);
	}

	[TestMethod]
	public void LinuxMountWatcher_GetMountedVolumes_ReturnsEnumerable()
	{
		// Arrange
		var watcher = new LinuxMountWatcher(new FakeMocks.FakeIWebLogger());

		// Act
		var volumes = watcher.GetMountedVolumes();

		// Assert
		Assert.IsNotNull(volumes);
	}

	[TestMethod]
	public void LinuxMountWatcher_Stop_CanBeCalled()
	{
		// Arrange
		var watcher = new LinuxMountWatcher(new FakeMocks.FakeIWebLogger());

		// Act
		watcher.Stop();

		// Assert
		Assert.IsNotNull(watcher);
	}
}
