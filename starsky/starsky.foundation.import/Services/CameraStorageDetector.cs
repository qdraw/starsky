using System.Collections.Generic;
using System.IO;
using System.Linq;
using starsky.foundation.import.Interfaces;
using starsky.foundation.injection;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;

namespace starsky.foundation.import.Services;

[Service(typeof(ICameraStorageDetector), InjectionLifetime = InjectionLifetime.Scoped)]
public class CameraStorageDetector(ISelectorStorage selectorStorage) : ICameraStorageDetector
{
	private readonly IStorage _hostStorage =
		selectorStorage.Get(SelectorStorage.StorageServices.HostFilesystem);

	/// <summary>
	///     Get all camera storage root paths
	/// </summary>
	/// <returns>Full FilePaths</returns>
	public IEnumerable<string> FindCameraStorages()
	{
		var drives = DriveInfo.GetDrives().Where(IsCameraStorage);
		return drives.Select(drive => drive.RootDirectory.FullName);
	}

	/// <summary>
	///     Is this drive a camera storage?
	/// </summary>
	/// <param name="drive">Which drive</param>
	/// <returns>true when is a camera storage e.g. sd card</returns>
	public bool IsCameraStorage(DriveInfo? drive)
	{
		// 1. Enumerate mounted volumes (caller responsibility)
		if ( drive == null )
		{
			return false;
		}

		// 2. Filter writable, ready volumes
		if ( !drive.IsReady )
		{
			return false;
		}

		if ( !drive.RootDirectory.Exists )
		{
			return false;
		}

		// 3. File system heuristic (portable, but soft)
		if ( !IsCameraFriendlyFileSystem(drive) )
		{
			return false;
		}

		// 4. DCIM folder (gold standard)
		//  5. Camera-like directory structure (optional but strong)
		var dcimPath = Path.Combine(drive.RootDirectory.FullName, "DCIM");
		return _hostStorage.ExistFolder(dcimPath) ||
		       HasCameraDirectoryStructure(drive.RootDirectory.FullName);
	}

	private static bool IsCameraFriendlyFileSystem(DriveInfo drive)
	{
		var fs = drive.DriveFormat.ToLowerInvariant();

		// Windows: FAT32, exFAT
		// Linux: vfat, exfat
		// macOS: msdos, exfat
		return fs.Contains("fat");
	}

	private bool HasCameraDirectoryStructure(string rootPath)
	{
		try
		{
			// Sony / Panasonic / some drones
			if ( _hostStorage.ExistFolder(Path.Combine(rootPath, "PRIVATE")) )
			{
				return true;
			}

			// Some cameras place DCIM one level down
			var dcimCandidate = Directory
				.EnumerateDirectories(rootPath)
				.Select(Path.GetFileName)
				.Any(name =>
					!string.IsNullOrEmpty(name) &&
					name.Length >= 3 &&
					char.IsDigit(name[0]) &&
					char.IsDigit(name[1]) &&
					char.IsDigit(name[2]));

			return dcimCandidate;
		}
		catch
		{
			return false;
		}
	}
}
