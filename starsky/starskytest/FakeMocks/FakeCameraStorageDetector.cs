using System.Collections.Generic;
using System.IO;
using starsky.foundation.import.Interfaces;

namespace starskytest.FakeMocks;

public class FakeCameraStorageDetector(List<string> paths) : ICameraStorageDetector
{
	public IEnumerable<string> FindCameraStorages()
	{
		return paths;
	}

	public bool IsCameraStorage(string driveRoot)
	{
		return paths.Contains(driveRoot);
	}

	public bool IsCameraStorage(DriveInfo? drive)
	{
		return paths.Contains(drive?.RootDirectory.FullName ?? string.Empty);
	}
}
