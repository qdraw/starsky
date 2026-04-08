using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.import.Helpers;

namespace starskytest.starsky.foundation.import.Helpers;

[TestClass]
public class MacOsFileSystemHelperTest
{
	[TestMethod]
	public void ResolveFileSystemForPath_LongestPrefixMatch_PrefersExternalVolume()
	{
		var entries = new List<MacOsFileSystemHelper.MountTableEntry>
		{
			new("/", "apfs"),
			new("/Volumes/MyCard", "exfat")
		};

		var fs = MacOsFileSystemHelper.ResolveFileSystemForPath(
			"/Volumes/MyCard/DCIM",
			entries,
			path => path);

		Assert.AreEqual("exfat", fs);
	}

	[TestMethod]
	public void ResolveFileSystemForPath_SystemVolumeAliasUnderVolumes_ResolvesToRootWithoutHardcoding()
	{
		var aliasPath = "/Volumes/AnyUserChosenSystemName";
		var entries = new List<MacOsFileSystemHelper.MountTableEntry>
		{
			new("/", "apfs"),
			new(aliasPath, "apfs"),
			new("/Volumes/CameraCard", "exfat")
		};

		string Resolver(string value)
		{
			if ( value.Equals(aliasPath, StringComparison.Ordinal) )
			{
				return "/";
			}

			return value;
		}

		var fsOnAlias = MacOsFileSystemHelper.ResolveFileSystemForPath(aliasPath, entries, Resolver);
		var fsOnCard =
			MacOsFileSystemHelper.ResolveFileSystemForPath("/Volumes/CameraCard/DCIM", entries,
				Resolver);

		Assert.AreEqual("apfs", fsOnAlias);
		Assert.AreEqual("exfat", fsOnCard);
	}

	[TestMethod]
	public void ResolveFileSystemForPath_IgnoresEmptyEntries_AndStillResolves()
	{
		var entries = new List<MacOsFileSystemHelper.MountTableEntry>
		{
			new("", ""),
			new("/", "apfs"),
			new("/Volumes/Card", "fat32")
		};

		var fs = MacOsFileSystemHelper.ResolveFileSystemForPath(
			"/Volumes/Card/DCIM",
			entries,
			path => path);

		Assert.AreEqual("fat32", fs);
	}

	[TestMethod]
	public void ResolveFileSystemForPath_NoMatchingMount_ThrowsInvalidOperationException()
	{
		var entries = new List<MacOsFileSystemHelper.MountTableEntry>
		{
			new("/Volumes/Card", "exfat")
		};

		var didThrow = false;
		try
		{
			_ = MacOsFileSystemHelper.ResolveFileSystemForPath("/private/tmp", entries,
				path => path);
		}
		catch ( InvalidOperationException )
		{
			didThrow = true;
		}

		Assert.IsTrue(didThrow);
	}

	[TestMethod]
	public void ShouldRetryForTransientRootAlias_OnVolumesApfsAndNotFinalAttempt_ReturnsTrue()
	{
		var shouldRetry = MacOsFileSystemHelper.ShouldRetryForTransientRootAlias(
			"/Volumes/CameraCard",
			"apfs",
			0);

		Assert.IsTrue(shouldRetry);
	}

	[TestMethod]
	public void ShouldRetryForTransientRootAlias_NonVolumesPath_ReturnsFalse()
	{
		var shouldRetry = MacOsFileSystemHelper.ShouldRetryForTransientRootAlias(
			"/",
			"apfs",
			0);

		Assert.IsFalse(shouldRetry);
	}

	[TestMethod]
	public void ShouldRetryForTransientRootAlias_FinalAttempt_ReturnsFalse()
	{
		var shouldRetry = MacOsFileSystemHelper.ShouldRetryForTransientRootAlias(
			"/Volumes/CameraCard",
			"apfs",
			4);

		Assert.IsFalse(shouldRetry);
	}

	[TestMethod]
	[OSCondition(OperatingSystems.OSX)]
	public void GetFileSystem_OnRoot_ReturnsNonEmpty_OnMacOnly()
	{
		var fs = MacOsFileSystemHelper.GetFileSystem("/");
		Assert.IsFalse(string.IsNullOrWhiteSpace(fs), "Filesystem for / should not be empty");
		Console.WriteLine($"Root filesystem: {fs}");
	}

	[TestMethod]
	[OSCondition(OperatingSystems.Linux)]
	public void GetFileSystem_OnLinux_ThrowsPlatformNotSupported()
	{
		var didThrow = false;
		try
		{
			_ = MacOsFileSystemHelper.GetFileSystem("/");
		}
		catch ( PlatformNotSupportedException )
		{
			didThrow = true;
		}

		Assert.IsTrue(didThrow);
	}

	[TestMethod]
	[OSCondition(OperatingSystems.Windows)]
	public void GetFileSystem_OnWindows_ThrowsPlatformNotSupported()
	{
		var didThrow = false;
		try
		{
			_ = MacOsFileSystemHelper.GetFileSystem("/");
		}
		catch ( PlatformNotSupportedException )
		{
			didThrow = true;
		}

		Assert.IsTrue(didThrow);
	}
}
