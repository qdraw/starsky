using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.import.Models;
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
	public void IsCameraStorage_WithCameraDriveInfo_TrueCases()
	{
		var fakeStorageSelector =
			new FakeSelectorStorage(new FakeIStorage(new List<string> { "/", "/DCIM" }));
		var detector = new CameraStorageDetector(fakeStorageSelector);

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
			new FakeSelectorStorage(new FakeIStorage(new List<string> { "/", "/PRIVATE" }));
		detector = new CameraStorageDetector(fakeStorageSelector);
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
			new FakeSelectorStorage(new FakeIStorage(new List<string> { "/" }));
		var detector = new CameraStorageDetector(fakeStorageSelector);

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
			RootDirectory = new CameraDirectoryInfo
			{
				Exists = true, 
				FullName = "/"
			},
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
		var detector = new CameraStorageDetector(fakeStorageSelector);

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
		var detector = new CameraStorageDetector(fakeStorageSelector);

		var result = detector.HasCameraDirectoryStructure("/");
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void HasCameraDirectoryStructure_NumericDirectories_OnlyDigits()
	{
		var fakeStorage = new FakeIStorage(new List<string> { "/", "/123", "/456", "/789" });
		var fakeStorageSelector = new FakeSelectorStorage(fakeStorage);
		var detector = new CameraStorageDetector(fakeStorageSelector);
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
		var detector = new CameraStorageDetector(fakeStorageSelector);
		var result = detector.HasCameraDirectoryStructure("/");

		Assert.IsTrue(result); // 123A should match, 12A should not, A123 should not, A12 should not
	}

	[TestMethod]
	public void HasCameraDirectoryStructure_NumericDirectories_TooShort()
	{
		var fakeStorage = new FakeIStorage(["/", "/12", "/1", "/A"]);
		var fakeStorageSelector = new FakeSelectorStorage(fakeStorage);
		var detector = new CameraStorageDetector(fakeStorageSelector);
		var result = detector.HasCameraDirectoryStructure("/");

		Assert.IsFalse(result);
	}

	[TestMethod]
	public void HasCameraDirectoryStructure_NumericDirectories_EmptyList()
	{
		var fakeStorage = new FakeIStorage(["/"]);
		var fakeStorageSelector = new FakeSelectorStorage(fakeStorage);
		var detector = new CameraStorageDetector(fakeStorageSelector);
		var result = detector.HasCameraDirectoryStructure("/");

		Assert.IsFalse(result);
	}
}
