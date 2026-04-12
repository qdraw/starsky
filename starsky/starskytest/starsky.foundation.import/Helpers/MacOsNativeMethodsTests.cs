using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.import.Helpers;

namespace starskytest.starsky.foundation.import.Helpers;

[TestClass]
public class MacOsNativeMethodsTests
{
	[TestMethod]
	public void ParseEntries_ReadsMultipleEntries_FromUnmanagedBuffer()
	{
		// Arrange
		var statFsSize = Marshal.SizeOf<MacOsNativeMethods.StatFs>();
		const int count = 2;
		var buffer = Marshal.AllocHGlobal(statFsSize * count);
		try
		{
			// First entry
			CreateTestStatFsInBuffer("/Volumes/DriveA", "exfat", buffer);

			// Second entry
			var secondPtr = IntPtr.Add(buffer, statFsSize);
			CreateTestStatFsInBuffer("/Volumes/DriveB", "ntfs", secondPtr);

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
		var statFsSize = Marshal.SizeOf<MacOsNativeMethods.StatFs>();
		const int count = 1;
		var buffer = Marshal.AllocHGlobal(statFsSize * count);
		try
		{
			// Create entry with empty strings
			CreateTestStatFsInBuffer(string.Empty, string.Empty, buffer);

			// Act
			var entries = MacOsFileSystemHelper.ParseEntries(buffer, count);

			// Assert
			Assert.HasCount(1, entries);
			Assert.AreEqual(string.Empty, entries[0].MountPoint);
			Assert.AreEqual(string.Empty, entries[0].FileSystemType);
		}
		finally
		{
			Marshal.FreeHGlobal(buffer);
		}
	}

	[SuppressMessage("ReSharper", "InconsistentNaming")]
	private static void CreateTestStatFsInBuffer(string mountPoint, string fileSystemType, IntPtr buffer)
	{
		// Zero out the buffer first
		var statFsSize = Marshal.SizeOf<MacOsNativeMethods.StatFs>();
		for ( int i = 0; i < statFsSize; i++ )
		{
			Marshal.WriteByte(buffer, i, 0);
		}

		// Offsets based on struct layout:
		// f_fstypename is at offset 72 (after 4 uints + 8 bytes Fsid)
		// f_mntonname is at offset 88 (after f_fstypename + 16 bytes)
		const int f_fstypename_offset = 72;
		const int f_mntonname_offset = 88;

		// Write filesystem type
		if ( !string.IsNullOrEmpty(fileSystemType) )
		{
			var encoded = Encoding.UTF8.GetBytes(fileSystemType);
			var copyLength = Math.Min(encoded.Length, MacOsNativeMethods.MFSTYPENAMELEN - 1);
			Marshal.Copy(encoded, 0, buffer + f_fstypename_offset, copyLength);
		}

		// Write mount point
		if ( !string.IsNullOrEmpty(mountPoint) )
		{
			var encoded = Encoding.UTF8.GetBytes(mountPoint);
			var copyLength = Math.Min(encoded.Length, MacOsNativeMethods.MAXPATHLEN - 1);
			Marshal.Copy(encoded, 0, buffer + f_mntonname_offset, copyLength);
		}
	}
}
