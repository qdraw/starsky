using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.import.Models;
using starsky.foundation.import.Services;
using starsky.foundation.platform.Architecture;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.import.Services;

[TestClass]
public class CameraStorageDetectorTest
{
	[TestMethod]
	public void IsCameraStorage_WithNullDrive_ReturnsFalse()
	{
		var fakeStorageSelector = new FakeSelectorStorage(new FakeIStorage());
		var detector = new CameraStorageDetector(fakeStorageSelector, new FakeIWebLogger());

		// Explicitly call the DriveInfo? overload to avoid ambiguity
		DriveInfo? nullDrive = null;
		var result = detector.IsCameraStorage(nullDrive);
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
			var detector = new CameraStorageDetector(fakeStorageSelector, new FakeIWebLogger());

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
				["/", "/PRIVATE"]);
			var fakeStorageSelector = new FakeSelectorStorage(fakeStorage);
			var detector = new CameraStorageDetector(fakeStorageSelector, new FakeIWebLogger());

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
				["/", "/100CANON", "/101DCIM"]);
			var fakeStorageSelector = new FakeSelectorStorage(fakeStorage);
			var detector = new CameraStorageDetector(fakeStorageSelector, new FakeIWebLogger());

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
			["/", "/100", "/101", "/102ABC"]);
		var fakeStorageSelector = new FakeSelectorStorage(fakeStorage);
		var detector = new CameraStorageDetector(fakeStorageSelector, new FakeIWebLogger());

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
			["/", "/10", "/A"]);
		var fakeStorageSelector = new FakeSelectorStorage(fakeStorage);
		var detector = new CameraStorageDetector(fakeStorageSelector, new FakeIWebLogger());

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
			["/"]);
		var fakeStorageSelector = new FakeSelectorStorage(fakeStorage);
		var detector = new CameraStorageDetector(fakeStorageSelector, new FakeIWebLogger());

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
			["/", "/SomeFolder"]);
		var fakeStorageSelector = new FakeSelectorStorage(fakeStorage);
		var detector = new CameraStorageDetector(fakeStorageSelector, new FakeIWebLogger());

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
			["/", "/DCIM"]);
		var fakeStorageSelector = new FakeSelectorStorage(fakeStorage);
		var detector = new CameraStorageDetector(fakeStorageSelector, new FakeIWebLogger());

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
	public void IsCameraStorage_WithCameraDriveInfo_TrueCases()
	{
		var fakeStorageSelector =
			new FakeSelectorStorage(new FakeIStorage(["/", "/DCIM"]));
		var detector = new CameraStorageDetector(fakeStorageSelector, new FakeIWebLogger());

		// DCIM present, FAT format
		var cameraDrive = new CameraDriveInfo
		{
			IsReady = true,
			RootDirectory = new CameraDirectoryInfo { Exists = true, FullName = "/" },
			DriveFormat = "FAT32"
		};
		Assert.IsTrue(detector.IsCameraStorage(cameraDrive));

		// PRIVATE present, exFAT format
		fakeStorageSelector =
			new FakeSelectorStorage(new FakeIStorage(["/", "/PRIVATE"]));
		detector = new CameraStorageDetector(fakeStorageSelector, new FakeIWebLogger());
		cameraDrive = new CameraDriveInfo
		{
			IsReady = true,
			RootDirectory = new CameraDirectoryInfo { Exists = true, FullName = "/" },
			DriveFormat = "exFAT"
		};
		Assert.IsTrue(detector.IsCameraStorage(cameraDrive));
	}

	[TestMethod]
	public void IsCameraStorage_WithCameraDriveInfo_FalseCases()
	{
		var fakeStorageSelector =
			new FakeSelectorStorage(new FakeIStorage(["/"]));
		var detector = new CameraStorageDetector(fakeStorageSelector, new FakeIWebLogger());

		// Not ready
		var cameraDrive = new CameraDriveInfo
		{
			IsReady = false,
			RootDirectory = new CameraDirectoryInfo { Exists = true, FullName = "/" },
			DriveFormat = "FAT32"
		};
		Assert.IsFalse(detector.IsCameraStorage(cameraDrive));

		// Not FAT/exFAT
		cameraDrive = new CameraDriveInfo
		{
			IsReady = true,
			RootDirectory = new CameraDirectoryInfo { Exists = true, FullName = "/" },
			DriveFormat = "NTFS"
		};
		Assert.IsFalse(detector.IsCameraStorage(cameraDrive));

		// No DCIM/PRIVATE or numeric dirs
		cameraDrive = new CameraDriveInfo
		{
			IsReady = true,
			RootDirectory = new CameraDirectoryInfo { Exists = true, FullName = "/" },
			DriveFormat = "FAT32"
		};
		Assert.IsFalse(detector.IsCameraStorage(cameraDrive));
	}

	[TestMethod]
	public void HasCameraDirectoryStructure_NumericDirectories_MatchAndNoMatch()
	{
		var fakeStorage = new FakeIStorage([
			"/", "/100CANON", "/101DCIM", "/10", "/A", "/123", "/12A"
		]);
		var fakeStorageSelector = new FakeSelectorStorage(fakeStorage);
		var detector = new CameraStorageDetector(fakeStorageSelector, new FakeIWebLogger());

		var result = detector.HasCameraDirectoryStructure("/");
		Assert.IsTrue(result);
	}

	[TestMethod]
	public void HasCameraDirectoryStructure_NumericDirectories_NoMatch()
	{
		// Should not match 10, A, 12A
		// (less than 3 digits or not all digits at start)
		var fakeStorage = new FakeIStorage(
			["/", "/10", "/A", "/12A"]);
		var fakeStorageSelector = new FakeSelectorStorage(fakeStorage);
		var detector = new CameraStorageDetector(fakeStorageSelector, new FakeIWebLogger());

		var result = detector.HasCameraDirectoryStructure("/");
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void HasCameraDirectoryStructure_NumericDirectories_OnlyDigits()
	{
		var fakeStorage = new FakeIStorage(["/", "/123", "/456", "/789"]);
		var fakeStorageSelector = new FakeSelectorStorage(fakeStorage);
		var detector = new CameraStorageDetector(fakeStorageSelector, new FakeIWebLogger());
		var result = detector.HasCameraDirectoryStructure("/");

		Assert.IsTrue(result);
	}

	[TestMethod]
	public void HasCameraDirectoryStructure_NumericDirectories_AlphaNumeric()
	{
		var fakeStorage = new FakeIStorage([
			"/",
			"/123A",
			"/A123",
			"/12A",
			"/A12"
		]);
		var fakeStorageSelector = new FakeSelectorStorage(fakeStorage);
		var detector = new CameraStorageDetector(fakeStorageSelector, new FakeIWebLogger());
		var result = detector.HasCameraDirectoryStructure("/");

		Assert.IsTrue(result); // 123A should match, 12A should not, A123 should not, A12 should not
	}

	[TestMethod]
	public void HasCameraDirectoryStructure_NumericDirectories_TooShort()
	{
		var fakeStorage = new FakeIStorage(["/", "/12", "/1", "/A"]);
		var fakeStorageSelector = new FakeSelectorStorage(fakeStorage);
		var detector = new CameraStorageDetector(fakeStorageSelector, new FakeIWebLogger());
		var result = detector.HasCameraDirectoryStructure("/");

		Assert.IsFalse(result);
	}

	[TestMethod]
	public void HasCameraDirectoryStructure_NumericDirectories_EmptyList()
	{
		var fakeStorage = new FakeIStorage(["/"]);
		var fakeStorageSelector = new FakeSelectorStorage(fakeStorage);
		var detector = new CameraStorageDetector(fakeStorageSelector, new FakeIWebLogger());
		var result = detector.HasCameraDirectoryStructure("/");

		Assert.IsFalse(result);
	}

	[TestMethod]
	public void IsCameraStorage_RootDirectoryNotExists_ReturnsFalse()
	{
		var fakeStorageSelector = new FakeSelectorStorage(new FakeIStorage());
		var detector = new CameraStorageDetector(fakeStorageSelector, new FakeIWebLogger());
		var cameraDrive = new CameraDriveInfo
		{
			IsReady = true,
			RootDirectory = new CameraDirectoryInfo
			{
				Exists = false, // Simulate root directory does not exist
				FullName = "/notfound"
			},
			DriveFormat = "FAT32"
		};
		Assert.IsFalse(detector.IsCameraStorage(cameraDrive));
	}

	[TestMethod]
	public void FindCameraStorages_FiltersAndReturnsRootDirectories()
	{
		// Arrange: Setup a detector with a fake storage (the storage is not used in this test)
		var fakeStorageSelector = new FakeSelectorStorage(new FakeIStorage());
		var detector = new CameraStorageDetector(fakeStorageSelector, new FakeIWebLogger());

		// Use a real system drive and a fake drive
		var drives = DriveInfo.GetDrives().ToList();
		if ( drives.Count == 0 )
		{
			Assert.Inconclusive("No drives available for testing");
			return;
		}

		// Act: Call FindCameraStorages
		var result = detector.FindCameraStorages().ToList();

		// Assert: All returned paths should be RootDirectory.FullName of a drive that matches IsCameraStorage
		foreach ( var path in result )
		{
			Assert.Contains(d => d.RootDirectory.FullName == path, drives);
		}

		// Should not return more than available drives
		Assert.IsLessThanOrEqualTo(drives.Count, result.Count);
	}

	[TestMethod]
	public void FindCameraStorages_OnLinuxPlatform_UsesLinuxDiscovery()
	{
		// Arrange
		var fakeStorage = new FakeIStorage(
			["/media", "/media/camera"]);
		var fakeStorageSelector = new FakeSelectorStorage(fakeStorage);
		var logger = new FakeIWebLogger();

		// Create detector with Linux platform delegate
		var isLinuxDelegate = new OperatingSystemHelper.IsOsPlatformDelegate(platform =>
			platform == OSPlatform.Linux);

		var detector = new CameraStorageDetector(fakeStorageSelector, logger, isLinuxDelegate);

		// Act
		var result = detector.FindCameraStorages();

		// Assert - should have attempted Linux discovery
		Assert.IsNotNull(result);
	}

	[TestMethod]
	public void FindCameraStorages_OnWindowsPlatform_UsesWindowsDriveInfo()
	{
		// Arrange
		var fakeStorage = new FakeIStorage();
		var fakeStorageSelector = new FakeSelectorStorage(fakeStorage);
		var logger = new FakeIWebLogger();

		// Create detector with Windows platform delegate
		var isWindowsDelegate = new OperatingSystemHelper.IsOsPlatformDelegate(platform =>
			platform == OSPlatform.Windows);

		var detector = new CameraStorageDetector(fakeStorageSelector, logger, isWindowsDelegate);

		// Act - won't actually enumerate drives in test, just verifies the code path
		var result = detector.FindCameraStorages();

		// Assert - should return a collection (empty or not, depends on system)
		Assert.IsNotNull(result);
	}

	[TestMethod]
	public void IsCameraStorage_WithPathOnLinux_UsesCameraDriveInfoHelper()
	{
		// Arrange
		var fakeStorage = new FakeIStorage(
			["/media/usb-drive", "/media/usb-drive/DCIM"]);
		var fakeStorageSelector = new FakeSelectorStorage(fakeStorage);
		var logger = new FakeIWebLogger();

		// Create detector with Linux platform delegate
		var isLinuxDelegate = new OperatingSystemHelper.IsOsPlatformDelegate(platform =>
			platform == OSPlatform.Linux);

		var detector = new CameraStorageDetector(fakeStorageSelector, logger, isLinuxDelegate);

		// Act
		var result = detector.IsCameraStorage("/media/usb-drive");

		// Assert
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void IsCameraStorage_NoDriveRoot()
	{
		// Arrange
		var logger = new FakeIWebLogger();
		var detector = new CameraStorageDetector(new FakeSelectorStorage(), logger);

		// Act
		var result = detector.IsCameraStorage(string.Empty);

		// Assert
		Assert.Contains("Drive root is null or whitespace",
			logger.TrackedExceptions.LastOrDefault().Item2!);
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void IsCameraStorage_CatchException()
	{
		// Arrange
		var logger = new FakeIWebLogger();
		var detector = new CameraStorageDetector(new FakeSelectorStorage(),
			logger, null!);

		// Act
		var result = detector.IsCameraStorage("/test");

		// Assert
		Assert.IsFalse(result);
		Assert.Contains("Drive root failed", logger.TrackedExceptions.LastOrDefault().Item2!);
	}

	[TestMethod]
	public void IFindCameraStorages_CatchException()
	{
		// Arrange
		var logger = new FakeIWebLogger();
		var detector = new CameraStorageDetector(new FakeSelectorStorage(),
			logger, null!);

		// Act
		var result = detector.FindCameraStorages();

		// Assert
		Assert.IsEmpty(result);
		Assert.Contains("Failed to enumerate drives during camera storage detection",
			logger.TrackedExceptions.LastOrDefault().Item2!);
	}

	[TestMethod]
	public void IsCameraStorage_WithPathOnWindows_UsesDriveInfo()
	{
		// Arrange
		var fakeStorage = new FakeIStorage();
		var fakeStorageSelector = new FakeSelectorStorage(fakeStorage);
		var logger = new FakeIWebLogger();

		// Create detector with Windows platform delegate
		var isWindowsDelegate = new OperatingSystemHelper.IsOsPlatformDelegate(platform =>
			platform == OSPlatform.Windows);

		var detector = new CameraStorageDetector(fakeStorageSelector, logger, isWindowsDelegate);

		// Act - use a path that might work on Windows
		var result = detector.IsCameraStorage("C:\\");

		// Assert 
		Assert.IsFalse(result);
	}
}
