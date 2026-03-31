using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.mountwatch.MountWatcher;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.mountwatch.Services;

[TestClass]
public sealed class MacMountWatcherTest
{
	private static MacMountWatcher CreateSut()
	{
		return new MacMountWatcher(new FakeIWebLogger());
	}

	[TestMethod]
	public void MacMountWatcher_OnConstruction_IsNotRunning()
	{
		var watcher = CreateSut();
		Assert.IsNotNull(watcher);
	}

	[TestMethod]
	public void MacMountWatcher_GetMountedVolumes_ReturnsEnumerable()
	{
		var watcher = CreateSut();
		var volumes = watcher.GetMountedVolumes();
		Assert.IsNotNull(volumes);
	}

	[TestMethod]
	public void MacMountWatcher_Stop_CanbeCalled()
	{
		var watcher = CreateSut();
		watcher.Stop();
		Assert.IsNotNull(watcher);
	}

	[TestMethod]
	public void DetectNewExternalMounts_NewExternalVolume_ReturnsPath()
	{
		var watcher = CreateSut();
		var detected = watcher.DetectNewExternalMounts([
			"/",
			"/Volumes/SD_CARD"
		]);

		CollectionAssert.AreEqual(new List<string> { "/Volumes/SD_CARD" }, detected);
	}

	[TestMethod]
	public void DetectNewExternalMounts_IgnoresRootAndNonExternal()
	{
		var watcher = CreateSut();
		var detected = watcher.DetectNewExternalMounts([
			"/",
			"/tmp/some-dir",
			"/Volumes/CAMERA"
		]);

		CollectionAssert.AreEqual(new List<string> { "/Volumes/CAMERA" }, detected);
	}

	[TestMethod]
	public void DetectNewExternalMounts_RepeatedSnapshot_DoesNotDuplicate()
	{
		var watcher = CreateSut();
		var first = watcher.DetectNewExternalMounts([
			"/Volumes/CAMERA"
		]);
		var second = watcher.DetectNewExternalMounts([
			"/Volumes/CAMERA"
		]);

		CollectionAssert.AreEqual(new List<string> { "/Volumes/CAMERA" }, first);
		CollectionAssert.AreEqual(new List<string>(), second);
	}

	[TestMethod]
	public void UpdateKnownExternalMounts_RemovedMount_CanBeDetectedAgain()
	{
		var watcher = CreateSut();
		_ = watcher.DetectNewExternalMounts([
			"/Volumes/CAMERA"
		]);

		watcher.UpdateKnownExternalMounts([
			"/"
		]);

		var detectedAgain = watcher.DetectNewExternalMounts([
			"/Volumes/CAMERA"
		]);

		CollectionAssert.AreEqual(new List<string> { "/Volumes/CAMERA" }, detectedAgain);
	}

	[TestMethod]
	public void DetectNewExternalMounts_EjectAndReinsertSamePath_DetectedAgainWithoutDisappearCallback()
	{
		var watcher = CreateSut();

		var first = watcher.DetectNewExternalMounts([
			"/Volumes/SD_CARD"
		]);

		var afterEject = watcher.DetectNewExternalMounts([
			"/"
		]);

		var reinsert = watcher.DetectNewExternalMounts([
			"/Volumes/SD_CARD"
		]);

		CollectionAssert.AreEqual(new List<string> { "/Volumes/SD_CARD" }, first);
		CollectionAssert.AreEqual(new List<string>(), afterEject);
		CollectionAssert.AreEqual(new List<string> { "/Volumes/SD_CARD" }, reinsert);
	}
}
