using System;
using System.Collections.Generic;
using System.Reflection;
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
			0,
			"/");

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
			29,
			"/");

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
	[OSCondition(OperatingSystems.Windows)]
	public void GetFileSystem_Windows()
	{
		Assert.ThrowsExactly<DllNotFoundException>(() =>
			new MacOsFileSystemHelper(() => OSPlatform.OSX).GetFileSystem("/")
		);
	}

	[TestMethod]
	[OSCondition(OperatingSystems.Linux)]
	public void GetFileSystem_Linux()
	{
		// on linux this function does not give a result
		// because the native method will fail to load,
		// ensure it at least returns empty string rather than throwing
		var result = new MacOsFileSystemHelper(() => OSPlatform.OSX
		).GetFileSystem("/");
		Assert.IsEmpty(result);
	}

	[TestMethod]
	[OSCondition(OperatingSystems.Linux)]
	public void GetFileSystem_InvalidPath_EntryPointNotFoundException_Linux()
	{
		Assert.ThrowsExactly<EntryPointNotFoundException>(() =>
			new MacOsFileSystemHelper(() => OSPlatform.OSX
			).GetFileSystem("--invalid--this-should-never-succeed-path--"));
	}

	[TestMethod]
	[DataRow("/Volumes/Camera/", "/Volumes/Camera/")]
	[DataRow("//", "/")] // Multiple leading slashes collapse to root
	[DataRow("tmp/path", "/tmp/path/")]
	[DataRow("/", "/")]
	[DataRow("", "/")]
	[DataRow("  ", "/")]
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
		// true when under /Volumes and matched mountpoint is parent (race case)
		var should =
			MacOsFileSystemHelper.ShouldRetryForTransientRootAlias("/Volumes/X", "apfs", 0,
				"/");
		Assert.IsTrue(should);

		// false when not under /Volumes/
		Assert.IsFalse(MacOsFileSystemHelper.ShouldRetryForTransientRootAlias("/tmp/X", "apfs", 0));

		// false when fs is non-apfs and exact mountpoint already matches target
		Assert.IsFalse(
			MacOsFileSystemHelper.ShouldRetryForTransientRootAlias("/Volumes/X", "exfat", 0,
				"/Volumes/X"));

		// false when attempt is the last one
		Assert.IsFalse(
			MacOsFileSystemHelper.ShouldRetryForTransientRootAlias("/Volumes/X", "apfs", 29,
				"/"));
	}

	[TestMethod]
	public void ShouldRetryForTransientRootAlias_SystemVolumeAlias_ReturnsFalse()
	{
		var shouldRetry = MacOsFileSystemHelper.ShouldRetryForTransientRootAlias(
			"/Volumes/Macintosh HD",
			"apfs",
			0,
			null,
			_ => "/");

		Assert.IsFalse(shouldRetry);
	}

	[TestMethod]
	public void GetFileSystem_TransientApfsThenMsdos_RetriesAndReturnsMsdos()
	{
		var callCount = 0;
		var sleepCount = 0;
		var helper = new MacOsFileSystemHelper(
			() => OSPlatform.OSX,
			_ =>
			{
				callCount++;
				return callCount < 3 ? "apfs" : "msdos";
			},
			null,
			_ => { sleepCount++; },
			_ => "/Volumes/ULTRAPLU130");

		var fs = helper.GetFileSystem("/Volumes/ULTRAPLU130");

		Assert.AreEqual("msdos", fs);
		Assert.AreEqual(3, callCount);
		Assert.AreEqual(2, sleepCount);
	}

	[TestMethod]
	public void GetFileSystem_SystemVolumeAliasApfs_DoesNotRetry()
	{
		var callCount = 0;
		var sleepCount = 0;
		var helper = new MacOsFileSystemHelper(
			() => OSPlatform.OSX,
			_ =>
			{
				callCount++;
				return "apfs";
			},
			null,
			_ => { sleepCount++; },
			_ => "/");

		var fs = helper.GetFileSystem("/Volumes/Macintosh HD");

		Assert.AreEqual("apfs", fs);
		Assert.AreEqual(1, callCount);
		Assert.AreEqual(0, sleepCount);
	}

	[TestMethod]
	public void ShouldRetryForTransientRootAlias_MountPointNotExact_RetriesEvenWhenNotApfs()
	{
		var shouldRetry = MacOsFileSystemHelper.ShouldRetryForTransientRootAlias(
			"/Volumes/ULTRAPLU130",
			"msdos",
			0,
			"/",
			value => value);

		Assert.IsTrue(shouldRetry);
	}

	[TestMethod]
	public void MountRace_PMHOME_ParentApfsThenExactMsdos_MatchesExpectedRetryFlow()
	{
		const string path = "/Volumes/PMHOME";

		// attempt 1: only root mount visible => parent APFS match
		var attempt1Entries = new List<MacOsFileSystemHelper.MountTableEntry> { new("/", "apfs") };
		var attempt1Resolved = MacOsFileSystemHelper.ResolveMountEntryForPath(path,
			attempt1Entries,
			value => value);
		var attempt1Retry = MacOsFileSystemHelper.ShouldRetryForTransientRootAlias(path,
			attempt1Resolved.FileSystemType,
			0,
			attempt1Resolved.MountPoint,
			value => value);

		Assert.AreEqual("/", attempt1Resolved.MountPoint);
		Assert.AreEqual("apfs", attempt1Resolved.FileSystemType);
		Assert.IsTrue(attempt1Retry);

		// attempt 2: target mount appears as MSDOS => no retry
		var attempt2Entries = new List<MacOsFileSystemHelper.MountTableEntry>
		{
			new("/", "apfs"), new(path, "msdos")
		};
		var attempt2Resolved = MacOsFileSystemHelper.ResolveMountEntryForPath(path,
			attempt2Entries,
			value => value);
		var attempt2Retry = MacOsFileSystemHelper.ShouldRetryForTransientRootAlias(path,
			attempt2Resolved.FileSystemType,
			1,
			attempt2Resolved.MountPoint,
			value => value);

		Assert.AreEqual(path, attempt2Resolved.MountPoint);
		Assert.AreEqual("msdos", attempt2Resolved.FileSystemType);
		Assert.IsFalse(attempt2Retry);
	}

	[TestMethod]
	public void MountRace_ULTRAPLU130_ParentApfsForSeveralAttempts_ThenExactMsdos()
	{
		const string path = "/Volumes/ULTRAPLU130";
		var parentOnlyEntries =
			new List<MacOsFileSystemHelper.MountTableEntry> { new("/", "apfs") };

		for ( var attempt = 1; attempt <= 4; attempt++ )
		{
			var resolved = MacOsFileSystemHelper.ResolveMountEntryForPath(path,
				parentOnlyEntries,
				value => value);
			var shouldRetry = MacOsFileSystemHelper.ShouldRetryForTransientRootAlias(path,
				resolved.FileSystemType,
				attempt,
				resolved.MountPoint,
				value => value);

			Assert.AreEqual("/", resolved.MountPoint);
			Assert.AreEqual("apfs", resolved.FileSystemType);
			Assert.IsTrue(shouldRetry);
		}

		var exactEntries = new List<MacOsFileSystemHelper.MountTableEntry>
		{
			new("/", "apfs"), new(path, "msdos")
		};
		var finalResolved = MacOsFileSystemHelper.ResolveMountEntryForPath(path,
			exactEntries,
			value => value);
		var finalRetry = MacOsFileSystemHelper.ShouldRetryForTransientRootAlias(path,
			finalResolved.FileSystemType,
			5,
			finalResolved.MountPoint,
			value => value);

		Assert.AreEqual(path, finalResolved.MountPoint);
		Assert.AreEqual("msdos", finalResolved.FileSystemType);
		Assert.IsFalse(finalRetry);
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

		Assert.IsInstanceOfType(mountEx!,
			RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
				? typeof(DllNotFoundException)
				: typeof(EntryPointNotFoundException));
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

	[TestMethod]
	public void IsExactMountPointMatch_Empty()
	{
		var result = MacOsFileSystemHelper.IsExactMountPointMatch(
			string.Empty,
			string.Empty);
		Assert.IsFalse(result,
			"IsExactMountPointMatch for / should not be empty");
	}

	[TestMethod]
	public void ParseEntries_ReadsMultipleEntries_FromUnmanagedBuffer()
	{
		// Arrange
		var statFsType = typeof(MacOsFileSystemHelper).GetNestedType("StatFs",
			BindingFlags.NonPublic | BindingFlags.Instance);
		Assert.IsNotNull(statFsType, "StatFs nested type not found");

		var mountField = statFsType.GetField("f_mntonname",
			BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
		var fsField = statFsType.GetField("f_fstypename",
			BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
		Assert.IsNotNull(mountField, "f_mntonname field not found");
		Assert.IsNotNull(fsField, "f_fstypename field not found");

		var structSize = Marshal.SizeOf(statFsType);
		const int count = 2;
		var buffer = Marshal.AllocHGlobal(structSize * count);
		try
		{
			// First entry
			var inst1 = Activator.CreateInstance(statFsType);
			mountField.SetValue(inst1, "/Volumes/DriveA");
			fsField.SetValue(inst1, "exfat");
			Marshal.StructureToPtr(inst1!, buffer, false);

			// Second entry
			var inst2 = Activator.CreateInstance(statFsType);
			mountField.SetValue(inst2, "/Volumes/DriveB");
			fsField.SetValue(inst2, "ntfs");
			var secondPtr = IntPtr.Add(buffer, structSize);
			Marshal.StructureToPtr(inst2!, secondPtr, false);

			// Act
			var entries = MacOsFileSystemHelper.ParseEntries(buffer, count);

			// Assert
			Assert.HasCount(count, entries);
			Assert.AreEqual("/Volumes/DriveA", entries[0].MountPoint);
			Assert.AreEqual("exfat", entries[0].FileSystemType);
			Assert.AreEqual("/Volumes/DriveB", entries[1].MountPoint);
			Assert.AreEqual("ntfs", entries[1].FileSystemType);
		}
		finally
		{
			Marshal.FreeHGlobal(buffer);
		}
	}

	[TestMethod]
	public void ParseEntries_HandlesEmptyStrings_Correctly()
	{
		var statFsType = typeof(MacOsFileSystemHelper).GetNestedType("StatFs",
			BindingFlags.NonPublic | BindingFlags.Instance);
		Assert.IsNotNull(statFsType);

		var mountField = statFsType.GetField("f_mntonname",
			BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
		var fsField = statFsType.GetField("f_fstypename",
			BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
		Assert.IsNotNull(mountField);
		Assert.IsNotNull(fsField);

		var structSize = Marshal.SizeOf(statFsType);
		const int count = 1;
		var buffer = Marshal.AllocHGlobal(structSize * count);
		try
		{
			var inst = Activator.CreateInstance(statFsType);
			// set empty strings
			mountField.SetValue(inst, string.Empty);
			fsField.SetValue(inst, string.Empty);
			Marshal.StructureToPtr(inst!, buffer, false);

			var entries = MacOsFileSystemHelper.ParseEntries(buffer, count);
			Assert.HasCount(1, entries);
			Assert.AreEqual(string.Empty, entries[0].MountPoint);
			Assert.AreEqual(string.Empty, entries[0].FileSystemType);
		}
		finally
		{
			Marshal.FreeHGlobal(buffer);
		}
	}
}
