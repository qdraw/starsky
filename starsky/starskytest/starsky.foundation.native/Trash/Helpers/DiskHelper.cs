using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace starskytest.starsky.foundation.native.Trash.Helpers;

[SuppressMessage("Globalization",
	"CA2101:Specify marshaling for P/Invoke string arguments")]
[SuppressMessage("Interoperability",
	"SYSLIB1054:Use \'LibraryImportAttribute\' instead of \'DllImportAttribute\' to generate P/Invoke marshalling code at compile time")]
public static class DiskHelper
{
	[DllImport("libc", SetLastError = true)]
	private static extern int statfs(string path, out Statfs buf);

	public static bool IsOnMainDisk(string filePath)
	{
		if ( string.IsNullOrEmpty(filePath) )
		{
			throw new ArgumentNullException(nameof(filePath));
		}

		if ( statfs(filePath, out var stat) == 0 )
		{
			return stat.f_mntonname is "/" or "/System/Volumes/Data";
		}

		var errorCode = Marshal.GetLastWin32Error();
		throw new InvalidOperationException(
			$"statfs failed for path: {filePath}. Error code: {errorCode}");
	}

	public static string GetMountPoint(string filePath)
	{
		if ( statfs(filePath, out var stat) == 0 )
		{
			return stat.f_mntonname;
		}

		var errorCode = Marshal.GetLastWin32Error();
		throw new InvalidOperationException(
			$"statfs failed for path: {filePath}. Error code: {errorCode}");
	}

	[StructLayout(LayoutKind.Sequential)]
	private struct Statfs
	{
		public uint f_bsize;
		public uint f_iosize;
		public ulong f_blocks;
		public ulong f_bfree;
		public ulong f_bavail;
		public ulong f_files;
		public ulong f_ffree;
		public fsid_t f_fsid;
		public uint f_owner;
		public uint f_type;
		public uint f_flags;
		public uint f_fssubtype;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
		public string f_fstypename;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1024)]
		public string f_mntonname;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1024)]
		public string f_mntfromname;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
		public byte[] f_reserved;
	}

	[StructLayout(LayoutKind.Sequential)]
	private struct fsid_t
	{
		public int val1;
		public int val2;
	}
}
