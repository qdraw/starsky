using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.import.Helpers;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.import.Helpers;

[TestClass]
public class LinuxFileSystemHelperTests
{
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

		var info = LinuxFileSystemHelper.DetectFileSystem(fakeStorage,
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

		var info = LinuxFileSystemHelper.DetectFileSystem(fakeStorage,
			"/test");

		Assert.AreEqual(string.Empty, info);
	}
}
