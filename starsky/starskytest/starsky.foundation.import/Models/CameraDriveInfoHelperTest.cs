using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.import.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.import.Models;

[TestClass]
public class CameraDriveInfoHelperTest
{
	[TestMethod]
	public void ToCameraDriveInfo_WithProcMounts_ReturnsFilesystem()
	{
		var tempDir = Path.Combine(Path.GetTempPath(),
			"camera_drive_test1" + Guid.NewGuid());

		// Create a fake /proc/mounts containing an entry that maps to tempDir
		var mountLine = $"/dev/sdb1 {tempDir} exfat rw 0 0";
		var bytes = Encoding.UTF8.GetBytes(mountLine + "\n");

		var fakeStorage = new FakeIStorage(
			new List<string> { tempDir },
			new List<string> { "/proc/mounts" },
			new List<byte[]> { bytes }
		);

		var info = CameraDriveInfoHelper.ToCameraDriveInfo(fakeStorage, tempDir);

		Assert.IsTrue(info.IsReady);
		Assert.IsTrue(info.RootDirectory.Exists);
		Assert.AreEqual(tempDir, info.RootDirectory.FullName);
		Assert.AreEqual("exfat", info.DriveFormat);
	}

	[TestMethod]
	public void ToCameraDriveInfo_NoProcMounts_ReturnsEmptyFilesystem()
	{
		var tempDir = Path.Combine(Path.GetTempPath(),
			"camera_drive_test2" + Guid.NewGuid());

		var fakeStorage = new FakeIStorage(
			new List<string> { tempDir }
		);

		var info = CameraDriveInfoHelper.ToCameraDriveInfo(fakeStorage, tempDir);

		Assert.IsTrue(info.IsReady);
		Assert.IsTrue(info.RootDirectory.Exists);
		Assert.AreEqual(string.Empty, info.DriveFormat);
	}

	[TestMethod]
	public void ToCameraDriveInfo_ReadAllLinesThrows_ReturnsEmptyFilesystem()
	{
		var tempDir = Path.Combine(Path.GetTempPath(),
			"camera_drive_test3" + Guid.NewGuid());

		var fakeStorage = new FakeIStorage(
			new List<string> { tempDir },
			exception: new InvalidOperationException("io")
		);

		var info = CameraDriveInfoHelper.ToCameraDriveInfo(fakeStorage, tempDir);

		// When ReadAllLines throws, DetectFileSystem swallows exceptions -> empty string
		Assert.IsTrue(info.IsReady);
		Assert.IsTrue(info.RootDirectory.Exists);
		Assert.AreEqual(string.Empty, info.DriveFormat);
	}

	[TestMethod]
	public void ToCameraDriveInfo_DirectoryDoesNotExist_ReflectsExistsFalse()
	{
		var tempDir = Path.Combine(Path.GetTempPath(),
			"camera_drive_test4" + Guid.NewGuid());

		// Ensure the directory does not exist
		try
		{
			Directory.Delete(tempDir, true);
		}
		catch
		{
			// ignored
		}

		var mountLine = $"/dev/sdb1 {tempDir} vfat rw 0 0";
		var bytes = Encoding.UTF8.GetBytes(mountLine + "\n");

		var fakeStorage = new FakeIStorage(
			null,
			["/proc/mounts"],
			new List<byte[]> { bytes }
		);

		var info = CameraDriveInfoHelper.ToCameraDriveInfo(fakeStorage, tempDir);

		Assert.IsTrue(info.IsReady);
		Assert.IsFalse(info.RootDirectory.Exists);
		Assert.AreEqual("vfat", info.DriveFormat);
	}

	[TestMethod]
	public void DetectFileSystem_CatchException()
	{
		const string mountLine = "/dev/sdb1 /test vfat rw 0 0";
		var bytes = Encoding.UTF8.GetBytes(mountLine + "\n");

		var fakeStorage = new FakeIStorage(
			null,
			["/proc/mounts"],
			new List<byte[]> { bytes },
			null,
			null,
			new Exception()
		);

		var info = CameraDriveInfoHelper.DetectFileSystem(fakeStorage,
			"/test");

		// When ReadAllLines throws, DetectFileSystem swallows exceptions -> empty string
		Assert.AreEqual(string.Empty, info);
	}
	
	[TestMethod]
	public void DetectFileSystem_InvalidMountLine()
	{
		const string mountLine = "/dev/sdb1 /test"; // invalid
		var bytes = Encoding.UTF8.GetBytes(mountLine + "\n");

		var fakeStorage = new FakeIStorage(
			null,
			["/proc/mounts"],
			new List<byte[]> { bytes }
		);

		var info = CameraDriveInfoHelper.DetectFileSystem(fakeStorage,
			"/test");

		Assert.AreEqual(string.Empty, info);
	}
}
