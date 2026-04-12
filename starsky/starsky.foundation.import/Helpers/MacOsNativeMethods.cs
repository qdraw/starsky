using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;

namespace starsky.foundation.import.Helpers;

[SuppressMessage("ReSharper", "InconsistentNaming")]
internal static class MacOsNativeMethods
{
	// From macOS docs: statfs(2)
	internal const int MFSTYPENAMELEN = 16; // length of fs type name including null
	internal const int MAXPATHLEN = 1024; // length of buffer for returned name

	[DllImport("libc", EntryPoint = "statfs", SetLastError = true)]
	[SuppressMessage("Globalization", "CA2101:Specify marshaling " +
	                                  "for P/Invoke string arguments")]
	[SuppressMessage("Interoperability",
		"SYSLIB1054:Use \'LibraryImportAttribute\' instead of " +
		"\'DllImportAttribute\' to generate P/Invoke marshalling code at compile time")]
	internal static extern int statfs(string path, out StatFs buf);

	[DllImport("libc", EntryPoint = "getmntinfo", SetLastError = true)]
	[SuppressMessage("Interoperability",
		"SYSLIB1054:Use 'LibraryImportAttribute' instead of " +
		"'DllImportAttribute' to generate P/Invoke marshalling code at compile time")]
	internal static extern int getmntinfo(out IntPtr mntbufp, int flags);

	[StructLayout(LayoutKind.Sequential)]
	internal struct Fsid
	{
		public int val0;
		public int val1;
	}

	/// <summary>
	///     statfs structure from macOS (10.5+)
	///     See: man statfs(2)
	///     https://developer.apple.com/library/archive/documentation/System/Conceptual/ManPages_iPhoneOS/man2/fstatfs64.2.html
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	internal unsafe struct StatFs
	{
		public uint f_bsize; /* fundamental file system block size */
		public int f_iosize; /* optimal transfer block size */
		public ulong f_blocks; /* total data blocks in file system */
		public ulong f_bfree; /* free blocks in fs */
		public ulong f_bavail; /* free blocks avail to non-superuser */
		public ulong f_files; /* total file nodes in file system */
		public ulong f_ffree; /* free file nodes in fs */

		public Fsid f_fsid; /* file system id */

		public uint f_owner; /* user that mounted the filesystem */
		public uint f_type; /* type of filesystem */
		public uint f_flags; /* copy of mount exported flags */
		public uint f_fssubtype; /* fs sub-type (flavor) */

		public fixed byte f_fstypename[MFSTYPENAMELEN]; /* fs type name */

		public fixed byte f_mntonname[MAXPATHLEN]; /* directory on which mounted */

		public fixed byte f_mntfromname[MAXPATHLEN]; /* mounted filesystem */

		public fixed uint f_reserved[8]; /* For future use */

		internal string FileSystemType
		{
			get
			{
				fixed ( byte* buffer = f_fstypename )
				{
					return DecodeNullTerminatedUtf8(buffer, MFSTYPENAMELEN);
				}
			}
		}

		internal string MountPoint
		{
			get
			{
				fixed ( byte* buffer = f_mntonname )
				{
					return DecodeNullTerminatedUtf8(buffer, MAXPATHLEN);
				}
			}
		}

		private static string DecodeNullTerminatedUtf8(byte* buffer, int capacity)
		{
			var length = 0;
			while ( length < capacity && buffer[length] != 0 )
			{
				length++;
			}

			return length == 0 ? string.Empty : Encoding.UTF8.GetString(buffer, length).Trim();
		}
	}
}
