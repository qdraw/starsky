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
		return new CameraDriveInfo
		{
			IsReady = driveInfo?.IsReady ?? false,
			RootDirectory = new CameraDirectoryInfo
			{
				Exists = driveInfo?.RootDirectory.Exists ?? false,
				FullName = driveInfo?.RootDirectory.FullName ?? string.Empty
			},
			DriveFormat = driveInfo?.DriveFormat ?? string.Empty
		};
	}
}

public class CameraDirectoryInfo
{
	public bool Exists { get; set; }
	public string FullName { get; set; } = string.Empty;
}
