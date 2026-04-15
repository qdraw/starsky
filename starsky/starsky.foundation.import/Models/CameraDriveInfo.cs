using System;
using System.IO;
using starsky.foundation.import.Helpers;
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
	public static CameraDriveInfo ToCameraDriveInfo(IStorage hostStorage,
		string mountPath)
	{
		var exists = hostStorage.ExistFolder(mountPath);
		return new CameraDriveInfo
		{
			IsReady = true,
			RootDirectory = new CameraDirectoryInfo { Exists = exists, FullName = mountPath },
			DriveFormat = LinuxFileSystemHelper.DetectFileSystem(hostStorage, mountPath)
		};
	}
}

public class CameraDirectoryInfo
{
	public bool Exists { get; set; }
	public string FullName { get; set; } = string.Empty;
}
