using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace starsky.foundation.import.Helpers;

[SuppressMessage("ReSharper", "InconsistentNaming")]
internal static class MacOsNativeMethods
{
	// From macOS docs: statfs(2)
	internal const int MFSTYPENAMELEN = 16; // length of fs type name including null
	internal const int MAXPATHLEN = 1024;   // length of buffer for returned name

	[DllImport("libc", SetLastError = true)]
	[SuppressMessage("Globalization", "CA2101:Specify marshaling " +
	                                  "for P/Invoke string arguments")]
	[SuppressMessage("Interoperability",
		"SYSLIB1054:Use \'LibraryImportAttribute\' instead of " +
		"\'DllImportAttribute\' to generate P/Invoke marshalling code at compile time")]
	internal static extern int statfs(string path, out StatFs buf);

	[DllImport("libc", SetLastError = true)]
	[SuppressMessage("Interoperability",
		"SYSLIB1054:Use 'LibraryImportAttribute' instead of " +
		"'DllImportAttribute' to generate P/Invoke marshalling code at compile time")]
	internal static extern int getmntinfo(out IntPtr mntbufp, int flags);

	/// <summary>
	/// statfs structure from macOS (10.5+)
	/// See: man statfs(2)
	/// https://developer.apple.com/library/archive/documentation/System/Conceptual/ManPages_iPhoneOS/man2/fstatfs64.2.html
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	internal struct StatFs
	{
		public uint f_bsize;        /* fundamental file system block size */
		public int f_iosize;        /* optimal transfer block size */
		public ulong f_blocks;      /* total data blocks in file system */
		public ulong f_bfree;       /* free blocks in fs */
		public ulong f_bavail;      /* free blocks avail to non-superuser */
		public ulong f_files;       /* total file nodes in file system */
		public ulong f_ffree;       /* free file nodes in fs */

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
		public int[] f_fsid;        /* file system id */

		public uint f_owner;        /* user that mounted the filesystem */
		public uint f_type;         /* type of filesystem */
		public uint f_flags;        /* copy of mount exported flags */
		public uint f_fssubtype;    /* fs sub-type (flavor) */

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = MFSTYPENAMELEN)]
		public byte[] f_fstypename_raw;     /* fs type name */

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = MAXPATHLEN)]
		public byte[] f_mntonname_raw;      /* directory on which mounted */

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = MAXPATHLEN)]
		public byte[] f_mntfromname_raw;    /* mounted filesystem */

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
		public uint[] f_reserved;   /* For future use */
	}
}
