using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.mountwatch.Services;

namespace starskytest.starsky.foundation.mountwatch.Services;

[TestClass]
public sealed class MountDetectorTest
{
	[TestMethod]
	public void HasCameraStorage_WithValidDcimFolder_ReturnsTrue()
	{
		// Arrange
		var detector = new MountDetector();
		var tempDir = Path.Combine(Path.GetTempPath(), "test_mount_" + Guid.NewGuid());
		Directory.CreateDirectory(tempDir);
		Directory.CreateDirectory(Path.Combine(tempDir, "DCIM"));

		try
		{
			// Act
			var result = detector.HasCameraStorage(tempDir);

			// Assert
			Assert.IsTrue(result);
		}
		finally
		{
			// Cleanup
			Directory.Delete(tempDir, true);
		}
	}

	[TestMethod]
	public void HasCameraStorage_WithLowercaseDcim_ReturnsTrue()
	{
		// Arrange
		var detector = new MountDetector();
		var tempDir = Path.Combine(Path.GetTempPath(), "test_mount_" + Guid.NewGuid());
		Directory.CreateDirectory(tempDir);
		Directory.CreateDirectory(Path.Combine(tempDir, "dcim"));

		try
		{
			// Act
			var result = detector.HasCameraStorage(tempDir);

			// Assert
			Assert.IsTrue(result);
		}
		finally
		{
			// Cleanup
			Directory.Delete(tempDir, true);
		}
	}

	[TestMethod]
	public void HasCameraStorage_WithNoCameraFolder_ReturnsFalse()
	{
		// Arrange
		var detector = new MountDetector();
		var tempDir = Path.Combine(Path.GetTempPath(), "test_mount_" + Guid.NewGuid());
		Directory.CreateDirectory(tempDir);

		try
		{
			// Act
			var result = detector.HasCameraStorage(tempDir);

			// Assert
			Assert.IsFalse(result);
		}
		finally
		{
			// Cleanup
			Directory.Delete(tempDir, true);
		}
	}

	[TestMethod]
	public void HasCameraStorage_WithNullPath_ReturnsFalse()
	{
		// Arrange
		var detector = new MountDetector();

		// Act
		var result = detector.HasCameraStorage(null!);

		// Assert
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void HasCameraStorage_WithEmptyPath_ReturnsFalse()
	{
		// Arrange
		var detector = new MountDetector();

		// Act
		var result = detector.HasCameraStorage("");

		// Assert
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void GetCameraStoragePaths_WithDcimFolder_ReturnsDcimPath()
	{
		// Arrange
		var detector = new MountDetector();
		var tempDir = Path.Combine(Path.GetTempPath(), "test_mount_" + Guid.NewGuid());
		var dcimDir = Path.Combine(tempDir, "DCIM");
		Directory.CreateDirectory(dcimDir);

		try
		{
			// Act
			var result = detector.GetCameraStoragePaths(tempDir).ToList();

			// Assert
			Assert.HasCount(1, result);
			Assert.EndsWith("DCIM", result[0]);
		}
		finally
		{
			// Cleanup
			Directory.Delete(tempDir, true);
		}
	}

	[TestMethod]
	public void GetCameraStoragePaths_WithMultipleCameraFolders_ReturnsAll()
	{
		// Arrange
		var detector = new MountDetector();
		var tempDir = Path.Combine(Path.GetTempPath(), "test_mount_" + Guid.NewGuid());
		Directory.CreateDirectory(Path.Combine(tempDir, "DCIM"));
		Directory.CreateDirectory(Path.Combine(tempDir, "dcim"));

		try
		{
			// Act
			var result = detector.GetCameraStoragePaths(tempDir).ToList();

			// Assert
			Assert.HasCount(1, result);
		}
		finally
		{
			// Cleanup
			Directory.Delete(tempDir, true);
		}
	}

	[TestMethod]
	public void GetCameraStoragePaths_WithNoCameraFolder_ReturnsEmpty()
	{
		// Arrange
		var detector = new MountDetector();
		var tempDir = Path.Combine(Path.GetTempPath(), "test_mount_" + Guid.NewGuid());
		Directory.CreateDirectory(tempDir);

		try
		{
			// Act
			var result = detector.GetCameraStoragePaths(tempDir).ToList();

			// Assert
			Assert.IsEmpty(result);
		}
		finally
		{
			// Cleanup
			Directory.Delete(tempDir, true);
		}
	}

	[TestMethod]
	public void GetCameraStoragePaths_WithNullPath_ReturnsEmpty()
	{
		// Arrange
		var detector = new MountDetector();

		// Act
		var result = detector.GetCameraStoragePaths(null!).ToList();

		// Assert
		Assert.IsEmpty(result);
	}
}
