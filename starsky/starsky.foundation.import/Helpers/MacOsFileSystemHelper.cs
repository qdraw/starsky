using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Threading;
using starsky.foundation.platform.Architecture;

namespace starsky.foundation.import.Helpers;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class MacOsFileSystemHelper
{
	private const int MFSNAMELEN = 16;
	private const int MNAMELEN = 1024;
	private const int MountTableRetryCount = 5;
	private const int MountTableRetryDelayMs = 75;
	private readonly Func<OSPlatform> _platformResolver = OperatingSystemHelper.GetPlatform;

	public MacOsFileSystemHelper()
	{
	}

	internal MacOsFileSystemHelper(Func<OSPlatform> platformResolver)
	{
		_platformResolver = platformResolver;
	}

	[DllImport("libc", SetLastError = true)]
	[SuppressMessage("Globalization", "CA2101:Specify marshaling " +
	                                  "for P/Invoke string arguments")]
	[SuppressMessage("Interoperability",
		"SYSLIB1054:Use \'LibraryImportAttribute\' instead of " +
		"\'DllImportAttribute\' to generate P/Invoke marshalling code at compile time")]
	private static extern int statfs(string path, out StatFs buf);

	[DllImport("libc", SetLastError = true)]
	[SuppressMessage("Interoperability",
		"SYSLIB1054:Use 'LibraryImportAttribute' instead of " +
		"'DllImportAttribute' to generate P/Invoke marshalling code at compile time")]
	private static extern int getmntinfo(out IntPtr mntbufp, int flags);

	public string GetFileSystem(string path)
	{
		EnsureMacOs();

		Exception? mountTableException = null;

		for ( var attempt = 0; attempt < MountTableRetryCount; attempt++ )
		{
			try
			{
				var fs = GetFileSystemViaMountTable(path);
				if ( !ShouldRetryForTransientRootAlias(path, fs, attempt) )
				{
					return fs;
				}

				Thread.Sleep(MountTableRetryDelayMs);
			}
			catch ( Exception ex )
			{
				mountTableException = ex;
				if ( attempt < MountTableRetryCount - 1 )
				{
					Thread.Sleep(MountTableRetryDelayMs);
				}
			}
		}

		try
		{
			return GetFileSystemViaStatFs(path);
		}
		catch
		{
			if ( mountTableException != null )
			{
				throw mountTableException;
			}

			throw;
		}
	}

	internal static bool ShouldRetryForTransientRootAlias(string path, string fileSystem,
		int attempt)
	{
		return attempt < MountTableRetryCount - 1 &&
		       path.StartsWith("/Volumes/", StringComparison.OrdinalIgnoreCase) &&
		       fileSystem.Equals("apfs", StringComparison.OrdinalIgnoreCase);
	}

	public string GetFileSystemViaMountTable(string path)
	{
		EnsureMacOs();

		var mountEntries = GetMountTableEntries();
		return ResolveFileSystemForPath(path, mountEntries, RealPathHelper.GetRealPath);
	}

	internal string GetFileSystemViaStatFs(string path)
	{
		EnsureMacOs();

		return statfs(path, out var stat) != 0
			? throw new Win32Exception(Marshal.GetLastWin32Error())
			: stat.f_fstypename;
	}

	private void EnsureMacOs()
	{
		if ( _platformResolver() == OSPlatform.OSX )
		{
			return;
		}

		throw new PlatformNotSupportedException(
			"MacOsFileSystemHelper only supports macOS.");
	}

	internal static string ResolveFileSystemForPath(string path,
		IEnumerable<MountTableEntry> mountEntries,
		Func<string, string>? realPathResolver = null)
	{
		realPathResolver ??= value => value;
		var targetPath = NormalizeForPrefix(realPathResolver(path));

		string? bestMatchFs = null;
		var bestMatchLength = -1;

		foreach ( var entry in mountEntries )
		{
			if ( string.IsNullOrWhiteSpace(entry.MountPoint) ||
			     string.IsNullOrWhiteSpace(entry.FileSystemType) )
			{
				continue;
			}

			var mountPoint = NormalizeForPrefix(realPathResolver(entry.MountPoint));
			if ( !targetPath.StartsWith(mountPoint, StringComparison.Ordinal) ||
			     mountPoint.Length <= bestMatchLength )
			{
				continue;
			}

			bestMatchLength = mountPoint.Length;
			bestMatchFs = entry.FileSystemType;
		}

		return bestMatchFs ?? throw new InvalidOperationException(
			$"No matching mount point found for path '{path}'");
	}

	internal static List<MountTableEntry> GetMountTableEntries()
	{
		var count = getmntinfo(out var ptr, 0);
		return count <= 0
			? throw new Win32Exception(Marshal.GetLastWin32Error())
			: ParseEntries(ptr, count);
	}

	internal static List<MountTableEntry> ParseEntries(IntPtr mntbufp, int count)
	{
		var entries = new List<MountTableEntry>(count);
		var structSize = Marshal.SizeOf<StatFs>();

		for ( var i = 0; i < count; i++ )
		{
			var current = IntPtr.Add(mntbufp, i * structSize);
			var stat = Marshal.PtrToStructure<StatFs>(current);
			entries.Add(new MountTableEntry(
				stat.f_mntonname,
				stat.f_fstypename));
		}

		return entries;
	}

	internal static string NormalizeForPrefix(string path)
	{
		if ( string.IsNullOrWhiteSpace(path) )
		{
			return "/";
		}

		var normalized = path.Trim();
		if ( normalized.Length > 1 )
		{
			normalized = normalized.TrimEnd('/');
		}

		if ( !normalized.StartsWith('/') )
		{
			normalized = "/" + normalized;
		}

		// If the normalized path is the root, return a single '/'
		if ( normalized == "/" )
		{
			return "/";
		}

		return normalized + "/";
	}

	internal readonly record struct MountTableEntry(string MountPoint, string FileSystemType);

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

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = MNAMELEN)]
		public string f_mntonname;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = MNAMELEN)]
		public string f_mntfromname;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
		public uint[] f_reserved;
	}
}
