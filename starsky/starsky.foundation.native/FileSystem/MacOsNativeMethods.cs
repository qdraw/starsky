using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;

namespace starsky.foundation.native.FileSystem;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public static class MacOsNativeMethods
{
	// From macOS docs: statfs(2)
	internal const int MFSTYPENAMELEN = 16; // length of fs type name including null
	internal const int MAXPATHLEN = 1024; // length of buffer for returned name

	// Cached once per process start — architecture does not change at runtime.
	private static readonly bool IsArm64 =
		RuntimeInformation.ProcessArchitecture == Architecture.Arm64;

	// On x64 macOS, statfs/getmntinfo are aliased to $INODE64 variants that expose
	// the 64-bit inode struct. On ARM64 macOS the plain symbols already use this
	// layout and the $INODE64 variants do not exist. We declare both and dispatch
	// at runtime to avoid EntryPointNotFoundException on either architecture.

	[DllImport("libc", EntryPoint = "statfs$INODE64", SetLastError = true)]
	[SuppressMessage("Globalization", "CA2101:Specify marshaling " +
	                                  "for P/Invoke string arguments")]
	[SuppressMessage("Interoperability",
		"SYSLIB1054:Use 'LibraryImportAttribute' instead of " +
		"'DllImportAttribute' to generate P/Invoke marshalling code at compile time")]
	private static extern int statfs_inode64(string path, out StatFs buf);

	[DllImport("libc", EntryPoint = "statfs", SetLastError = true)]
	[SuppressMessage("Globalization", "CA2101:Specify marshaling " +
	                                  "for P/Invoke string arguments")]
	[SuppressMessage("Interoperability",
		"SYSLIB1054:Use 'LibraryImportAttribute' instead of " +
		"'DllImportAttribute' to generate P/Invoke marshalling code at compile time")]
	private static extern int statfs_plain(string path, out StatFs buf);

	[DllImport("libc", EntryPoint = "getmntinfo$INODE64", SetLastError = true)]
	[SuppressMessage("Interoperability",
		"SYSLIB1054:Use 'LibraryImportAttribute' instead of " +
		"'DllImportAttribute' to generate P/Invoke marshalling code at compile time")]
	private static extern int getmntinfo_inode64(out IntPtr mntbufp, int flags);

	[DllImport("libc", EntryPoint = "getmntinfo", SetLastError = true)]
	[SuppressMessage("Interoperability",
		"SYSLIB1054:Use 'LibraryImportAttribute' instead of " +
		"'DllImportAttribute' to generate P/Invoke marshalling code at compile time")]
	private static extern int getmntinfo_plain(out IntPtr mntbufp, int flags);

	public static int statfs(string path, out StatFs buf)
	{
		return IsArm64 ? statfs_plain(path, out buf) : statfs_inode64(path, out buf);
	}

	public static int getmntinfo(out IntPtr mntbufp, int flags)
	{
		return IsArm64
			? getmntinfo_plain(out mntbufp, flags)
			: getmntinfo_inode64(out mntbufp, flags);
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct Fsid
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
	[SuppressMessage("Usage", "S6640: Make sure that using \"unsafe\" is safe here")]
	public unsafe struct StatFs
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

		public string FileSystemType
		{
			get
			{
				fixed ( byte* buffer = f_fstypename )
				{
					return DecodeNullTerminatedUtf8(buffer, MFSTYPENAMELEN);
				}
			}
		}

		public string MountPoint
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
