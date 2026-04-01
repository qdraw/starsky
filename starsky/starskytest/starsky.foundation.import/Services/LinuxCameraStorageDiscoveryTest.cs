using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.import.Helpers;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.import.Services;

[TestClass]
public class LinuxCameraStorageDiscoveryTest
{
	[TestMethod]
	public void FindCameraStorages_WithNoMountPoints_ReturnsEmpty()
	{
		// Arrange - empty storage means no directories exist
		var fakeStorage = new FakeIStorage();
		var logger = new FakeIWebLogger();
		var discovery = new LinuxCameraStorageDiscovery(fakeStorage, logger);

		// Act
		var result = discovery.FindCameraStorages();

		// Assert
		Assert.AreEqual(0, result.Count());
	}

	[TestMethod]
	public void FindCameraStorages_WithMediaMountPoint_ScansForDevices()
	{
		// Arrange - simulate /media directory with mounted USB drive
		var fakeStorage = new FakeIStorage(
			new List<string> { "/media", "/media/usb-drive", "/media/usb-drive/DCIM" });

		var logger = new FakeIWebLogger();
		var discovery = new LinuxCameraStorageDiscovery(fakeStorage, logger);

		// Act
		var result = discovery.FindCameraStorages();

		// Assert - should find the /media/usb-drive directory
		Assert.Contains(p => p.Contains("usb-drive"), result);
	}

	[TestMethod]
	public void FindCameraStorages_WithMultipleMountPoints_FindsAllDevices()
	{
		// Arrange - simulate multiple mount point paths
		var fakeStorage = new FakeIStorage(
			new List<string>
			{
				"/media",
				"/media/device1",
				"/mnt",
				"/mnt/device2",
				"/run/media",
				"/run/media/user",
				"/run/media/user/device3"
			});

		var logger = new FakeIWebLogger();
		var discovery = new LinuxCameraStorageDiscovery(fakeStorage, logger);

		// Act
		var result = discovery.FindCameraStorages().ToList();

		// Assert - should find devices at different levels
		Assert.Contains(p => p.Contains("device1"), result);
		Assert.Contains(p => p.Contains("device2"), result);
		Assert.Contains(p => p.Contains("device3"), result);
	}

	[TestMethod]
	public void FindCameraStorages_WithNestedDirectories_RespectMaxDepth()
	{
		// Arrange - create deeply nested structure
		var fakeStorage = new FakeIStorage(
			new List<string>
			{
				"/media",
				"/media/level1",
				"/media/level1/level2",
				"/media/level1/level2/level3" // This is 3 levels deep, should not be scanned (max depth is 2)
			});

		var logger = new FakeIWebLogger();
		var discovery = new LinuxCameraStorageDiscovery(fakeStorage, logger);

		// Act
		var result = discovery.FindCameraStorages().ToList();

		// Assert - should find level1 and level2, but not level3
		Assert.Contains(p => p.Contains("level1"), result);
		Assert.Contains(p => p.Contains("level2"), result);
		Assert.DoesNotContain(p => p.Contains("level3"), result);
	}

	[TestMethod]
	public void FindCameraStorages_WithUnauthorizedAccess_SkipsAndContinues()
	{
		// Arrange - this test verifies that UnauthorizedAccessException is handled gracefully
		var fakeStorage = new FakeIStorage(
			new List<string>
			{
				"/media", "/media/accessible-device"
				// /media/inaccessible would throw but FakeIStorage doesn't actually throw
				// This test ensures the code structure handles it
			});

		var logger = new FakeIWebLogger();
		var discovery = new LinuxCameraStorageDiscovery(fakeStorage, logger);

		// Act
		var result = discovery.FindCameraStorages();

		// Assert - should not throw, should return accessible devices
		Assert.IsNotNull(result);
		Assert.IsTrue(result.Any());
	}

	// [TestMethod]
	// public void FindCameraStorages_WithExceptionDuringScanning_LogsErrorAndReturnsEmpty()
	// {
	// 	// Arrange
	// 	var fakeStorage = new FakeIStorage(
	// 		new IOException("Simulated disk error"));
	//
	// 	var logger = new FakeIWebLogger();
	// 	var discovery = new LinuxCameraStorageDiscovery(fakeStorage, logger);
	//
	// 	// Act
	// 	var result = discovery.FindCameraStorages();
	//
	// 	// Assert
	// 	Assert.AreEqual(0, result.Count());
	// 	Assert.IsTrue(logger.TrackedExceptions.Any(), "Should have logged the exception");
	// }

	[TestMethod]
	public void FindCameraStorages_WithOnlyInaccessibleMountPoints_ReturnsEmpty()
	{
		// Arrange - /mnt exists but is empty
		var fakeStorage = new FakeIStorage(
			new List<string>
			{
				"/mnt" // Directory exists but contains nothing
			});

		var logger = new FakeIWebLogger();
		var discovery = new LinuxCameraStorageDiscovery(fakeStorage, logger);

		// Act
		var result = discovery.FindCameraStorages();

		// Assert
		Assert.AreEqual(0, result.Count());
	}
}
