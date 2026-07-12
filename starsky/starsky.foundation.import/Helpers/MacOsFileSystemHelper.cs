using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Threading;
using starsky.foundation.native.DiskFileSystemType;
using starsky.foundation.platform.Architecture;

namespace starsky.foundation.import.Helpers;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class MacOsFileSystemHelper
{
	private const int MountTableRetryCount = 30;
	private const int MountTableRetryDelayMs = 100;
	private readonly Func<string, string>? _mountTableResolver;
	private readonly Func<OSPlatform> _platformResolver = OperatingSystemHelper.GetPlatform;
	private readonly Func<string, string> _realPathResolver = RealPathHelper.GetRealPath;
	private readonly Action<int> _sleep = Thread.Sleep;
	private readonly Func<string, string>? _statFsResolver;

	public MacOsFileSystemHelper()
	{
	}

	internal MacOsFileSystemHelper(Func<OSPlatform> platformResolver)
	{
		_platformResolver = platformResolver;
	}

	internal MacOsFileSystemHelper(Func<OSPlatform> platformResolver,
		Func<string, string>? mountTableResolver,
		Func<string, string>? statFsResolver,
		Action<int>? sleep,
		Func<string, string>? realPathResolver)
	{
		_platformResolver = platformResolver;
		_mountTableResolver = mountTableResolver;
		_statFsResolver = statFsResolver;
		if ( sleep != null )
		{
			_sleep = sleep;
		}

		if ( realPathResolver != null )
		{
			_realPathResolver = realPathResolver;
		}
	}


	public string GetFileSystem(string path)
	{
		EnsureMacOs();

		Exception? mountTableException = null;

		for ( var attempt = 0; attempt < MountTableRetryCount; attempt++ )
		{
			try
			{
				string fs;
				string? matchedMountPoint;
				if ( _mountTableResolver != null )
				{
					fs = _mountTableResolver(path);
					matchedMountPoint = path;
				}
				else
				{
					var mountEntries = GetMountTableEntries();
					var resolved = ResolveMountEntryForPath(path, mountEntries, _realPathResolver);
					fs = resolved.FileSystemType;
					matchedMountPoint = resolved.MountPoint;
				}

				var shouldRetry = ShouldRetryForTransientRootAlias(path, fs, attempt,
					matchedMountPoint,
					_realPathResolver);

				if ( !shouldRetry )
				{
					return fs;
				}

				_sleep(MountTableRetryDelayMs);
			}
			catch ( Exception ex )
			{
				mountTableException = ex;
				if ( attempt < MountTableRetryCount - 1 )
				{
					_sleep(MountTableRetryDelayMs);
				}
			}
		}

		try
		{
			var fallbackFs = _statFsResolver?.Invoke(path) ?? GetFileSystemViaStatFs(path);
			return fallbackFs;
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
		int attempt, string? matchedMountPoint = null,
		Func<string, string>? realPathResolver = null)
	{
		if ( attempt >= MountTableRetryCount - 1 ||
		     !path.StartsWith("/Volumes/", StringComparison.OrdinalIgnoreCase) ||
		     IsSystemVolumeAlias(path, realPathResolver) )
		{
			return false;
		}

		// During mount races we often resolve to a parent APFS mount (e.g. / or /Volumes)
		// before the target mount appears. Retry until target mountpoint is visible.
		return !IsExactMountPointMatch(path, matchedMountPoint, realPathResolver) ||
		       fileSystem.Equals("apfs", StringComparison.OrdinalIgnoreCase);
	}

	internal static bool IsExactMountPointMatch(string path, string? matchedMountPoint,
		Func<string, string>? realPathResolver = null)
	{
		if ( string.IsNullOrWhiteSpace(matchedMountPoint) )
		{
			return false;
		}

		realPathResolver ??= value => value;
		var target = NormalizeForPrefix(realPathResolver(path));
		var matched = NormalizeForPrefix(realPathResolver(matchedMountPoint));
		return string.Equals(target, matched, StringComparison.Ordinal);
	}

	private static bool IsSystemVolumeAlias(string path,
		Func<string, string>? realPathResolver = null)
	{
		realPathResolver ??= RealPathHelper.GetRealPath;
		return NormalizeForPrefix(realPathResolver(path)) == "/";
	}

	internal string GetFileSystemViaMountTable(string path)
	{
		EnsureMacOs();

		var mountEntries = GetMountTableEntries();
		return ResolveMountEntryForPath(path, mountEntries, _realPathResolver).FileSystemType;
	}

	internal string GetFileSystemViaStatFs(string path)
	{
		EnsureMacOs();

		return MacOsNativeMethods.statfs(path, out var stat) != 0
			? throw new Win32Exception(Marshal.GetLastWin32Error())
			: stat.FileSystemType;
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
		return ResolveMountEntryForPath(path, mountEntries, realPathResolver).FileSystemType;
	}

	internal static MountTableEntry ResolveMountEntryForPath(string path,
		IEnumerable<MountTableEntry> mountEntries,
		Func<string, string>? realPathResolver = null)
	{
		realPathResolver ??= value => value;
		var targetPath = NormalizeForPrefix(realPathResolver(path));

		MountTableEntry? bestMatch = null;
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
			bestMatch = entry;
		}

		return bestMatch ?? throw new InvalidOperationException(
			$"No matching mount point found for path '{path}'");
	}

	internal static List<MountTableEntry> GetMountTableEntries()
	{
		var count = MacOsNativeMethods.getmntinfo(out var ptr, 0);
		return count <= 0
			? throw new Win32Exception(Marshal.GetLastWin32Error())
			: ParseEntries(ptr, count);
	}

	internal static List<MountTableEntry> ParseEntries(IntPtr mntbufp, int count)
	{
		var entries = new List<MountTableEntry>(count);
		var structSize = Marshal.SizeOf<MacOsNativeMethods.StatFs>();

		for ( var i = 0; i < count; i++ )
		{
			var current = IntPtr.Add(mntbufp, i * structSize);
			var stat = Marshal.PtrToStructure<MacOsNativeMethods.StatFs>(current);
			entries.Add(new MountTableEntry(
				stat.MountPoint,
				stat.FileSystemType));
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
}
