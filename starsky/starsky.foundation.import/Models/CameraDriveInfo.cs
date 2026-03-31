using System;
using System.IO;

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
		var driveFormat = string.Empty;
		try
		{
			driveFormat = driveInfo?.DriveFormat ?? string.Empty;
		}
		catch ( Exception )
		{
			// Catch all exceptions: UnauthorizedAccessException (access denied),
			// IOException (I/O error), or any other exception from native interop
			// when querying invalid device files like /dev/sda, /dev/sdb
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
}

public class CameraDirectoryInfo
{
	public bool Exists { get; set; }
	public string FullName { get; set; } = string.Empty;
}
