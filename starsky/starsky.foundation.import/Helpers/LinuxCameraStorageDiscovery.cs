using System;
using System.Collections.Generic;
using System.Linq;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.storage.Interfaces;

namespace starsky.foundation.import.Helpers;

/// <summary>
///     Linux-specific camera storage discovery using mount point scanning
/// </summary>
internal class LinuxCameraStorageDiscovery(IStorage hostStorage, IWebLogger logger)
{
	/// <summary>
	///     Find camera storages on Linux using /proc/mounts and directory scanning
	/// </summary>
	public IEnumerable<string> FindCameraStorages()
	{
		var cameraStorages = new List<string>();

		try
		{
			// Common mount points for USB drives and camera cards on Linux
			var commonMountPoints = new[] { "/media", "/mnt", "/run/media" };

			foreach ( var mountPoint in commonMountPoints )
			{
				if ( !hostStorage.ExistFolder(mountPoint) )
				{
					continue;
				}

				// Get direct subdirectories using the storage interface
				var directoryListing = hostStorage.GetDirectoryRecursive(mountPoint);
				foreach ( var dirPath in directoryListing.Select(p => p.Key) )
				{
					// Only take direct children of the mount point (depth 1-2)
					if ( dirPath == mountPoint || !IsDirectChild(mountPoint, dirPath, 2) )
					{
						continue;
					}

					if ( hostStorage.ExistFolder(dirPath) )
					{
						cameraStorages.Add(dirPath);
					}
				}
			}
		}
		catch ( Exception ex )
		{
			logger.LogError(ex, "Failed to find camera storages on Linux");
		}

		return cameraStorages;
	}

	/// <summary>
	///     Check if a path is a direct child of base path (within maxDepth levels)
	/// </summary>
	private static bool IsDirectChild(string basePath, string path, int maxDepth)
	{
		// Remove trailing slashes
		var normalizedBase = basePath.TrimEnd('/');
		var normalizedPath = path.TrimEnd('/');

		if ( !normalizedPath.StartsWith(normalizedBase) )
		{
			return false;
		}

		var relativePath = normalizedPath[normalizedBase.Length..].TrimStart('/');
		var depth = relativePath.Count(c => c == '/') +
		            ( string.IsNullOrEmpty(relativePath) ? 0 : 1 );

		return depth > 0 && depth <= maxDepth;
	}
}
