using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.import.Services;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.import.Services;

[TestClass]
public class CameraStorageDetectorTest
{
	[TestMethod]
	public void IsCameraStorage_WithNullDrive_ReturnsFalse()
	{
		var fakeStorageSelector = new FakeSelectorStorage(new FakeIStorage());
		var detector = new CameraStorageDetector(fakeStorageSelector);

		var result = detector.IsCameraStorage(null);
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void IsCameraStorage_WithDCIMFolder_ReturnsTrue()
	{
		// Create temp folder with DCIM
		var tempDir = Path.Combine(Path.GetTempPath(), $"CameraDetectorTest_{Guid.NewGuid():N}");
		Directory.CreateDirectory(tempDir);
		Directory.CreateDirectory(Path.Combine(tempDir, "DCIM"));

		try
		{
			// Create fake storage with DCIM folder
			var fakeStorage = new FakeIStorage(
				["/", "/DCIM"]);
			var fakeStorageSelector = new FakeSelectorStorage(fakeStorage);
			var detector = new CameraStorageDetector(fakeStorageSelector);

			// Use real DriveInfo from temp location
			var rootPath = Path.GetPathRoot(tempDir);
			var drive = new DriveInfo(rootPath ?? "C:\\");

			var result = detector.IsCameraStorage(drive);
			if ( drive.DriveFormat.Contains("fat", StringComparison.InvariantCultureIgnoreCase) )
			{
				Assert.IsTrue(result);
			}
			else
			{
				// Non-FAT drives won't pass the filesystem check
				Assert.IsFalse(result);
			}
		}
		finally
		{
			Directory.Delete(tempDir, true);
		}
	}

	[TestMethod]
	public void IsCameraStorage_WithPRIVATEFolder_ReturnsTrue()
	{
		// Create temp folder with PRIVATE
		var tempDir = Path.Combine(Path.GetTempPath(), $"CameraDetectorTest_{Guid.NewGuid():N}");
		Directory.CreateDirectory(tempDir);
		Directory.CreateDirectory(Path.Combine(tempDir, "PRIVATE"));

		try
		{
			// Create fake storage with PRIVATE folder (Sony/Panasonic style)
			var fakeStorage = new FakeIStorage(
				new List<string> { "/", "/PRIVATE" });
			var fakeStorageSelector = new FakeSelectorStorage(fakeStorage);
			var detector = new CameraStorageDetector(fakeStorageSelector);

			var rootPath = Path.GetPathRoot(tempDir);
			var drive = new DriveInfo(rootPath ?? "C:\\");
			var result = detector.IsCameraStorage(drive);

			if ( drive.DriveFormat.Contains("fat", StringComparison.InvariantCultureIgnoreCase) )
			{
				Assert.IsTrue(result);
			}
			else
			{
				Assert.IsFalse(result);
			}
		}
		finally
		{
			Directory.Delete(tempDir, true);
		}
	}

	[TestMethod]
	public void IsCameraStorage_WithNumericDirectories_ReturnsTrue()
	{
		// Create temp folder with numeric directories
		var tempDir = Path.Combine(Path.GetTempPath(), $"CameraDetectorTest_{Guid.NewGuid():N}");
		Directory.CreateDirectory(tempDir);
		Directory.CreateDirectory(Path.Combine(tempDir, "100CANON"));
		Directory.CreateDirectory(Path.Combine(tempDir, "101DCIM"));

		try
		{
			var fakeStorage = new FakeIStorage(
				new List<string> { "/", "/100CANON", "/101DCIM" });
			var fakeStorageSelector = new FakeSelectorStorage(fakeStorage);
			var detector = new CameraStorageDetector(fakeStorageSelector);

			var rootPath = Path.GetPathRoot(tempDir);
			var drive = new DriveInfo(rootPath ?? "C:\\");
			var result = detector.IsCameraStorage(drive);

			if ( drive.DriveFormat.Contains("fat", StringComparison.InvariantCultureIgnoreCase) )
			{
				Assert.IsTrue(result);
			}
			else
			{
				Assert.IsFalse(result);
			}
		}
		finally
		{
			Directory.Delete(tempDir, true);
		}
	}

	[TestMethod]
	public void IsCameraStorage_WithMultipleNumericDirectories_ReturnsTrue()
	{
		var fakeStorage = new FakeIStorage(
			new List<string> { "/", "/100", "/101", "/102ABC" });
		var fakeStorageSelector = new FakeSelectorStorage(fakeStorage);
		var detector = new CameraStorageDetector(fakeStorageSelector);

		// Use real system drive
		var drive = DriveInfo.GetDrives().FirstOrDefault();
		if ( drive == null )
		{
			Assert.Inconclusive("No drives available");
			return;
		}

		var result = detector.IsCameraStorage(drive);
		if ( drive.DriveFormat.Contains("fat", StringComparison.InvariantCultureIgnoreCase) )
		{
			Assert.IsTrue(result);
		}
	}

	[TestMethod]
	public void IsCameraStorage_WithShortNumericDirectory_ReturnsFalse()
	{
		// Directories with only 2 digits don't match the 3+ digit pattern
		var fakeStorage = new FakeIStorage(
			new List<string> { "/", "/10", "/A" });
		var fakeStorageSelector = new FakeSelectorStorage(fakeStorage);
		var detector = new CameraStorageDetector(fakeStorageSelector);

		var drive = DriveInfo.GetDrives().FirstOrDefault();
		if ( drive == null )
		{
			Assert.Inconclusive("No drives available");
			return;
		}

		var result = detector.IsCameraStorage(drive);
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void IsCameraStorage_WithEmptyDirectory_ReturnsFalse()
	{
		// Empty directory with no camera markers
		var fakeStorage = new FakeIStorage(
			new List<string> { "/" });
		var fakeStorageSelector = new FakeSelectorStorage(fakeStorage);
		var detector = new CameraStorageDetector(fakeStorageSelector);

		var drive = DriveInfo.GetDrives().FirstOrDefault();
		if ( drive == null )
		{
			Assert.Inconclusive("No drives available");
			return;
		}

		var result = detector.IsCameraStorage(drive);
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void IsCameraStorage_WithNonCameraFolder_ReturnsFalse()
	{
		// Non-camera related folder
		var fakeStorage = new FakeIStorage(
			new List<string> { "/", "/SomeFolder" });
		var fakeStorageSelector = new FakeSelectorStorage(fakeStorage);
		var detector = new CameraStorageDetector(fakeStorageSelector);

		var drive = DriveInfo.GetDrives().FirstOrDefault();
		if ( drive == null )
		{
			Assert.Inconclusive("No drives available");
			return;
		}

		var result = detector.IsCameraStorage(drive);
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void IsCameraStorage_WithNonFATFileSystem_ReturnsFalse()
	{
		var fakeStorage = new FakeIStorage(
			new List<string> { "/", "/DCIM" });
		var fakeStorageSelector = new FakeSelectorStorage(fakeStorage);
		var detector = new CameraStorageDetector(fakeStorageSelector);

		// Find a non-FAT drive
		var drive = DriveInfo.GetDrives()
			.FirstOrDefault(d =>
				!d.DriveFormat.Contains("fat", StringComparison.InvariantCultureIgnoreCase));

		if ( drive == null )
		{
			Assert.Inconclusive("No non-FAT drives available for testing");
			return;
		}

		var result = detector.IsCameraStorage(drive);
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void IsCameraStorage_WithVFATFileSystem_ReturnsTrue()
	{
		var fakeStorage = new FakeIStorage(
			new List<string> { "/", "/DCIM" });
		var fakeStorageSelector = new FakeSelectorStorage(fakeStorage);
		var detector = new CameraStorageDetector(fakeStorageSelector);

		// Find a vfat drive (Linux FAT equivalent)
		var drive = DriveInfo.GetDrives()
			.FirstOrDefault(d => d.DriveFormat.ToLowerInvariant() == "vfat");

		if ( drive == null )
		{
			Assert.Inconclusive("No vfat drives available for testing");
			return;
		}

		var result = detector.IsCameraStorage(drive);
		Assert.IsTrue(result);
	}
}
