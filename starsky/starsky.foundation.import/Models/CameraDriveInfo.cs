using System;
using System.IO;
using starsky.foundation.storage.Interfaces;

namespace starsky.foundation.import.Models;

public class CameraDriveInfo
{
	public bool IsReady { get; set; }

	public CameraDirectoryInfo RootDirectory { get; set; } = new();
	public string DriveFormat { get; set; } = string.Empty;
}

public static class CameraDriveInfoHelper
{
	private const string ProcMountsPath = "/proc/mounts";

	public static CameraDriveInfo ToCameraDriveInfo(this DriveInfo? driveInfo)
	{
		string driveFormat;
		try
		{
			driveFormat = driveInfo?.DriveFormat ?? string.Empty;
		}
		catch ( Exception )
		{
			driveFormat = string.Empty;
		}

		return new CameraDriveInfo
		{
			IsReady = driveInfo?.IsReady ?? false,
			RootDirectory = new CameraDirectoryInfo
			{
				Exists = driveInfo?.RootDirectory.Exists ?? false,
				FullName = driveInfo?.RootDirectory.FullName ?? string.Empty
			},
			DriveFormat = driveFormat
		};
	}

	/// <summary>
	///     Create CameraDriveInfo from a path (useful for Linux where DriveInfo doesn't work well)
	/// </summary>
	public static CameraDriveInfo ToCameraDriveInfo(IStorage hostStorage, string mountPath)
	{
		// Use IStorage to determine existence so tests can inject FakeIStorage
		var exists = false;
		try
		{
			exists = hostStorage.ExistFolder(mountPath);
		}
		catch
		{
			exists = false;
		}

		return new CameraDriveInfo
		{
			IsReady = true, // Assume ready when constructing from a mount path
			RootDirectory = new CameraDirectoryInfo { Exists = exists, FullName = mountPath },
			DriveFormat = DetectFileSystem(hostStorage, mountPath)
		};
	}

	/// <summary>
	///     Attempt to detect the filesystem type (for Linux)
	/// </summary>
	private static string DetectFileSystem(IStorage hostStorage, string mountPath)
	{
		try
		{
			// Try to read from /proc/mounts if on Linux
			if ( hostStorage.ExistFile(ProcMountsPath) )
			{
				var lines = hostStorage.ReadAllLines(ProcMountsPath);
				foreach ( var line in lines )
				{
					var parts = line.Split([' '], StringSplitOptions.RemoveEmptyEntries);
					if ( parts.Length >= 3 && parts[1] == mountPath )
					{
						return parts[2]; // Third field is filesystem type
					}
				}
			}
		}
		catch
		{
			// Ignore errors
		}

		return string.Empty;
	}
}

public class CameraDirectoryInfo
{
	public bool Exists { get; set; }
	public string FullName { get; set; } = string.Empty;
}
