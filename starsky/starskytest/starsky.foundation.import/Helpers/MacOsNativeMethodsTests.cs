using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.import.Helpers;

namespace starskytest.starsky.foundation.import.Helpers;

[TestClass]
public class MacOsNativeMethodsTests
{
	private static byte[] StringToByteArray(string str, int maxLength)
	{
		var bytes = new byte[maxLength];
		if ( !string.IsNullOrEmpty(str) )
		{
			var encoded = Encoding.UTF8.GetBytes(str);
			var copyLength = Math.Min(encoded.Length, maxLength - 1);
			Array.Copy(encoded, bytes, copyLength);
		}

		return bytes;
	}

	[TestMethod]
	public void ParseEntries_ReadsMultipleEntries_FromUnmanagedBuffer()
	{
		// Arrange
		var statFsType = typeof(MacOsNativeMethods).GetNestedType("StatFs64",
			BindingFlags.NonPublic | BindingFlags.Instance);
		Assert.IsNotNull(statFsType, "StatFs64 nested type not found");

		var mountField = statFsType.GetField("f_mntonname_raw",
			BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
		var fsField = statFsType.GetField("f_fstypename_raw",
			BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
		Assert.IsNotNull(mountField, "f_mntonname_raw field not found");
		Assert.IsNotNull(fsField, "f_fstypename_raw field not found");

		var structSize = Marshal.SizeOf(statFsType);
		const int count = 2;
		var buffer = Marshal.AllocHGlobal(structSize * count);
		try
		{
			// First entry
			var inst1 = Activator.CreateInstance(statFsType);
			mountField.SetValue(inst1, StringToByteArray("/Volumes/DriveA", MacOsNativeMethods.MAXPATHLEN));
			fsField.SetValue(inst1, StringToByteArray("exfat", MacOsNativeMethods.MFSTYPENAMELEN));
			Marshal.StructureToPtr(inst1!, buffer, false);

			// Second entry
			var inst2 = Activator.CreateInstance(statFsType);
			mountField.SetValue(inst2, StringToByteArray("/Volumes/DriveB", MacOsNativeMethods.MAXPATHLEN));
			fsField.SetValue(inst2, StringToByteArray("ntfs", MacOsNativeMethods.MFSTYPENAMELEN));
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
		var statFsType = typeof(MacOsNativeMethods).GetNestedType("StatFs64",
			BindingFlags.NonPublic | BindingFlags.Instance);
		Assert.IsNotNull(statFsType);

		var mountField = statFsType.GetField("f_mntonname_raw",
			BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
		var fsField = statFsType.GetField("f_fstypename_raw",
			BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
		Assert.IsNotNull(mountField);
		Assert.IsNotNull(fsField);

		var structSize = Marshal.SizeOf(statFsType);
		const int count = 1;
		var buffer = Marshal.AllocHGlobal(structSize * count);
		try
		{
			var inst = Activator.CreateInstance(statFsType);
			// set empty byte arrays
			mountField.SetValue(inst, StringToByteArray(string.Empty, MacOsNativeMethods.MAXPATHLEN));
			fsField.SetValue(inst, StringToByteArray(string.Empty, MacOsNativeMethods.MFSTYPENAMELEN));
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
