using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace starsky.foundation.import.Helpers;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public static class MacOsFileSystemHelper
{
	private const int MFSNAMELEN = 16;

	[DllImport("libc", SetLastError = true)]
	[SuppressMessage("Globalization", "CA2101:Specify marshaling for P/Invoke string arguments")]
	[SuppressMessage("Interoperability",
		"SYSLIB1054:Use \'LibraryImportAttribute\' instead of \'DllImportAttribute\' to generate P/Invoke marshalling code at compile time")]
	private static extern int statfs(string path, out StatFs buf);

	public static string GetFileSystem(string path)
	{
		return statfs(path, out var stat) != 0
			? throw new Win32Exception(Marshal.GetLastWin32Error())
			: stat.f_fstypename; // e.g. "apfs", "hfs", "msdos", "exfat"
	}

	[StructLayout(LayoutKind.Sequential)]
	private struct StatFs
	{
		public uint f_bsize;
		public int f_iosize;
		public ulong f_blocks;
		public ulong f_bfree;
		public ulong f_bavail;
		public ulong f_files;
		public ulong f_ffree;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
		public int[] f_fsid;

		public uint f_owner;
		public uint f_type;
		public uint f_flags;
		public uint f_fssubtype;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = MFSNAMELEN)]
		public string f_fstypename;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1024)]
		public string f_mntonname;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1024)]
		public string f_mntfromname;

		public uint f_reserved;
	}
}
