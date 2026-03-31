using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.storage.Interfaces;

namespace starsky.foundation.import.Services;

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

				// Recursively scan subdirectories (usually mounted devices are 1-2 levels deep)
				var devices = GetMountedDevicesInDirectory(mountPoint, 2);
				foreach ( var device in devices )
				{
					cameraStorages.Add(device);
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
	///     Recursively get potentially mounted device directories
	/// </summary>
	private IEnumerable<string> GetMountedDevicesInDirectory(string basePath, int maxDepth)
	{
		var devices = new List<string>();

		if ( maxDepth <= 0 )
		{
			return devices;
		}

		try
		{
			var subDirectories = Directory.GetDirectories(basePath);
			foreach ( var dir in subDirectories )
			{
				try
				{
					// Check if this directory looks like a mounted device
					if ( IsLikelyMountPoint(dir) )
					{
						devices.Add(dir);
					}

					// Recurse deeper
					devices.AddRange(GetMountedDevicesInDirectory(dir, maxDepth - 1));
				}
				catch ( UnauthorizedAccessException )
				{
					// Skip directories we can't access
				}
			}
		}
		catch ( Exception ex )
		{
			logger.LogDebug($"Error scanning directory {basePath}: {ex.Message}");
		}

		return devices;
	}

	/// <summary>
	///     Check if a directory is likely a mounted device
	/// </summary>
	private bool IsLikelyMountPoint(string path)
	{
		try
		{
			// Check if directory exists and is accessible
			if ( !hostStorage.ExistFolder(path) )
			{
				return false;
			}

			// Check if we can read its contents
			_ = Directory.EnumerateFileSystemEntries(path).FirstOrDefault();

			return true;
		}
		catch
		{
			return false;
		}
	}
}
