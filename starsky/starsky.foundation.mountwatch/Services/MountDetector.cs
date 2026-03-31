using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using starsky.foundation.injection;
using starsky.foundation.mountwatch.Interfaces;

namespace starsky.foundation.mountwatch.Services;

/// <summary>
///     Detects camera storage on mounted volumes
/// </summary>
[Service(typeof(IMountDetector), InjectionLifetime = InjectionLifetime.Scoped)]
public class MountDetector : IMountDetector
{
	private static readonly string[] CameraStorageFolders = ["DCIM", "dcim", "DCIM/", "dcim/"];

	/// <summary>
	///     Check if a mount path contains camera storage (e.g., DCIM)
	/// </summary>
	public bool HasCameraStorage(string mountPath)
	{
		if ( string.IsNullOrWhiteSpace(mountPath) )
		{
			return false;
		}

		try
		{
			var directoryInfo = new DirectoryInfo(mountPath);
			if ( !directoryInfo.Exists )
			{
				return false;
			}

			var directories = directoryInfo.GetDirectories();
			return directories.Any(d => CameraStorageFolders.Contains(d.Name));
		}
		catch ( UnauthorizedAccessException )
		{
			return false;
		}
		catch ( IOException )
		{
			return false;
		}
	}

	/// <summary>
	///     Get all camera storage paths on a mount
	/// </summary>
	public IEnumerable<string> GetCameraStoragePaths(string mountPath)
	{
		if ( string.IsNullOrWhiteSpace(mountPath) )
		{
			return [];
		}

		try
		{
			var directoryInfo = new DirectoryInfo(mountPath);
			if ( !directoryInfo.Exists )
			{
				return [];
			}

			var cameraPaths = new List<string>();
			var directories = directoryInfo.GetDirectories();

			foreach ( var dir in directories )
			{
				if ( CameraStorageFolders.Contains(dir.Name, StringComparer.OrdinalIgnoreCase) )
				{
					cameraPaths.Add(dir.FullName);
				}
			}

			return cameraPaths;
		}
		catch ( UnauthorizedAccessException )
		{
			return [];
		}
		catch ( IOException )
		{
			return [];
		}
	}
}
