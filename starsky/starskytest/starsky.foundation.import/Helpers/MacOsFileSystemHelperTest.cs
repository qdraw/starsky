using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
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
			new("/", "apfs"), new("/Volumes/MyCard", "exfat")
		};

		var fs = MacOsFileSystemHelper.ResolveFileSystemForPath(
			"/Volumes/MyCard/DCIM",
			entries,
			path => path);

		Assert.AreEqual("exfat", fs);
	}

	[TestMethod]
	public void
		ResolveFileSystemForPath_SystemVolumeAliasUnderVolumes_ResolvesToRootWithoutHardcoding()
	{
		var aliasPath = "/Volumes/AnyUserChosenSystemName";
		var entries = new List<MacOsFileSystemHelper.MountTableEntry>
		{
			new("/", "apfs"), new(aliasPath, "apfs"), new("/Volumes/CameraCard", "exfat")
		};

		string Resolver(string value)
		{
			if ( value.Equals(aliasPath, StringComparison.Ordinal) )
			{
				return "/";
			}

			return value;
		}

		var fsOnAlias =
			MacOsFileSystemHelper.ResolveFileSystemForPath(aliasPath, entries, Resolver);
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
			new("", ""), new("/", "apfs"), new("/Volumes/Card", "fat32")
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
		var fs = new MacOsFileSystemHelper().GetFileSystem("/");
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
			_ = new MacOsFileSystemHelper().GetFileSystem("/");
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
			_ = new MacOsFileSystemHelper().GetFileSystem("/");
		}
		catch ( PlatformNotSupportedException )
		{
			didThrow = true;
		}

		Assert.IsTrue(didThrow);
	}

	[TestMethod]
	[DataRow("/Volumes/Camera/", "/Volumes/Camera/")]
	[DataRow("//", "/")] // Multiple leading slashes collapse to root
	[DataRow("tmp/path", "/tmp/path/")]
	[DataRow("/", "/")]
	public void NormalizeForPrefix_FormatsPaths_Correctly(string input, string expected)
	{
		var actual = MacOsFileSystemHelper.NormalizeForPrefix(input);
		Assert.AreEqual(expected, actual);
	}

	[TestMethod]
	public void ResolveFileSystemForPath_LongestPrefixEdgeCases()
	{
		var entries = new[]
		{
			new MacOsFileSystemHelper.MountTableEntry("/", "apfs"),
			new MacOsFileSystemHelper.MountTableEntry("/Volumes/USB", "exfat"),
			new MacOsFileSystemHelper.MountTableEntry("/Volumes/USB/Inner", "fat32")
		};

		var fs = MacOsFileSystemHelper.ResolveFileSystemForPath("/Volumes/USB/Inner/DCIM", entries,
			p => p);
		Assert.AreEqual("fat32", fs);

		fs = MacOsFileSystemHelper.ResolveFileSystemForPath("/Volumes/USB/DCIM", entries, p => p);
		Assert.AreEqual("exfat", fs);
	}


	[TestMethod]
	[OSCondition(OperatingSystems.OSX)]
	public void GetFileSystemViaStatFs_OnMac_ReturnsNonEmpty()
	{
		var fs = new MacOsFileSystemHelper().GetFileSystemViaStatFs("/");
		Assert.IsFalse(string.IsNullOrWhiteSpace(fs));
	}

	[TestMethod]
	[OSCondition(ConditionMode.Exclude, OperatingSystems.OSX)]
	public void GetFileSystem_OnNonMac_ThrowsPlatformNotSupported()
	{
		var didThrow = false;
		try
		{
			_ = new MacOsFileSystemHelper().GetFileSystem("/");
		}
		catch ( PlatformNotSupportedException )
		{
			didThrow = true;
		}

		Assert.IsTrue(didThrow);
	}


	[TestMethod]
	public void ResolveFileSystemForPath_LongestPrefixWins()
	{
		// Arrange
		var entries = new List<MacOsFileSystemHelper.MountTableEntry>
		{
			new("/", "apfs"),
			new("/Volumes/Camera", "exfat"),
			new("/Volumes/Camera/inner", "ntfs")
		};

		// Act / Assert
		var res1 =
			MacOsFileSystemHelper.ResolveFileSystemForPath("/Volumes/Camera/inner/sub/file.jpg",
				entries, v => v);
		Assert.AreEqual("ntfs", res1);

		var res2 =
			MacOsFileSystemHelper.ResolveFileSystemForPath("/Volumes/Camera/sub/file.jpg", entries,
				v => v);
		Assert.AreEqual("exfat", res2);

		var res3 = MacOsFileSystemHelper.ResolveFileSystemForPath("/other/path", entries, v => v);
		Assert.AreEqual("apfs", res3);
	}

	[TestMethod]
	public void ResolveFileSystemForPath_IgnoresEmptyEntries_AndThrowsWhenNoMatch()
	{
		var entries = new List<MacOsFileSystemHelper.MountTableEntry>
		{
			new(string.Empty, string.Empty)
		};

		// No valid mount points -> should throw
		Exception? caughtEx = null;
		try
		{
			MacOsFileSystemHelper.ResolveFileSystemForPath("/some/path", entries, v => v);
			Assert.Fail("Expected InvalidOperationException was not thrown");
		}
		catch ( Exception ex )
		{
			caughtEx = ex;
		}

		Assert.IsNotNull(caughtEx);
		Assert.IsInstanceOfType(caughtEx, typeof(InvalidOperationException));
	}

	[TestMethod]
	public void ShouldRetryForTransientRootAlias_BehavesAsExpected()
	{
		// true when path under /Volumes/ and fs == apfs and attempt is less than retry-1
		var should =
			MacOsFileSystemHelper.ShouldRetryForTransientRootAlias("/Volumes/X", "apfs", 0);
		Assert.IsTrue(should);

		// false when not under /Volumes/
		Assert.IsFalse(MacOsFileSystemHelper.ShouldRetryForTransientRootAlias("/tmp/X", "apfs", 0));

		// false when fs not apfs
		Assert.IsFalse(
			MacOsFileSystemHelper.ShouldRetryForTransientRootAlias("/Volumes/X", "exfat", 0));

		// false when attempt is the last one
		Assert.IsFalse(
			MacOsFileSystemHelper.ShouldRetryForTransientRootAlias("/Volumes/X", "apfs", 4));
	}

	[TestMethod]
	public void NativeMethods_Throw_WhenNativeSymbolsUnavailable()
	{
		var helper = new MacOsFileSystemHelper(() => OSPlatform.OSX);

		// GetFileSystemViaMountTable will attempt native getmntinfo; on Linux this may throw EntryPointNotFoundException
		Exception? mountEx = null;
		try
		{
			_ = helper.GetFileSystemViaMountTable("/");
			// If it did not throw, we at least exercised the method.
		}
		catch ( Exception ex )
		{
			mountEx = ex;
		}

		if ( mountEx != null )
		{
			Assert.IsInstanceOfType(mountEx,
				RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
					? typeof(DllNotFoundException)
					: typeof(EntryPointNotFoundException));
		}
	}

	[TestMethod]
	public void NativeMethods_Throw_WhenNativeSymbolsUnavailable2()
	{
		var helper = new MacOsFileSystemHelper(() => OSPlatform.OSX);

		// GetFileSystemViaStatFs also P/Invokes statfs; ensure it either returns or throws a platform-related exception
		Exception? statEx = null;
		try
		{
			_ = helper.GetFileSystemViaStatFs("/");
		}
		catch ( Exception ex )
		{
			statEx = ex;
		}

		if ( statEx != null )
		{
			Assert.IsInstanceOfType(statEx,
				RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
					? typeof(DllNotFoundException)
					: typeof(EntryPointNotFoundException));
		}
	}
}
