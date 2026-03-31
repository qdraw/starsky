using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using starsky.foundation.import.Interfaces;
using starsky.foundation.import.Models;
using starsky.foundation.injection;
using starsky.foundation.platform.Architecture;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;

namespace starsky.foundation.import.Services;

[Service(typeof(ICameraStorageDetector), InjectionLifetime = InjectionLifetime.Scoped)]
public class CameraStorageDetector(ISelectorStorage selectorStorage, IWebLogger logger)
	: ICameraStorageDetector
{
	private readonly IStorage _hostStorage =
		selectorStorage.Get(SelectorStorage.StorageServices.HostFilesystem);

	private readonly OperatingSystemHelper.IsOsPlatformDelegate _isOsPlatformDelegate =
		RuntimeInformation.IsOSPlatform;

	internal CameraStorageDetector(ISelectorStorage selectorStorage, IWebLogger logger,
		OperatingSystemHelper.IsOsPlatformDelegate isOsPlatformDelegate) : this(selectorStorage,
		logger)
	{
		_isOsPlatformDelegate = isOsPlatformDelegate;
	}

	/// <summary>
	///     Get all camera storage root paths
	/// </summary>
	/// <returns>Full FilePaths</returns>
	public IEnumerable<string> FindCameraStorages()
	{
		try
		{
			if ( _isOsPlatformDelegate(OSPlatform.Linux) )
			{
				var linuxDiscovery = new LinuxCameraStorageDiscovery(_hostStorage, logger);
				return linuxDiscovery.FindCameraStorages()
					.Where(IsCameraStorage)
					.ToList();
			}

			var drives = DriveInfo.GetDrives().Where(IsCameraStorage);
			return drives.Select(drive => drive.RootDirectory.FullName);
		}
		catch ( Exception ex )
		{
			logger.LogError(ex, "Failed to enumerate drives during camera storage detection");
			return [];
		}
	}

	public bool IsCameraStorage(string driveRoot)
	{
		if ( string.IsNullOrWhiteSpace(driveRoot) )
		{
			logger.LogError($"Drive root is null or whitespace: '{driveRoot}'");
			return false;
		}

		try
		{
			// On Linux, create CameraDriveInfo from path directly
			if ( _isOsPlatformDelegate(OSPlatform.Linux) )
			{
				var cameraDriveInfo = CameraDriveInfoHelper.ToCameraDriveInfo(driveRoot);
				return IsCameraStorage(cameraDriveInfo);
			}

			// On Windows, use DriveInfo
			var drive = new DriveInfo(driveRoot);
			return IsCameraStorage(drive);
		}
		catch ( Exception ex )
		{
			logger.LogError($"Drive root failed: '{driveRoot}'", ex);
			return false;
		}
	}

	public bool IsCameraStorage(DriveInfo? drive)
	{
		return IsCameraStorage(drive.ToCameraDriveInfo());
	}

	/// <summary>
	///     Is this drive a camera storage?
	/// </summary>
	/// <param name="drive">Which drive</param>
	/// <returns>true when is a camera storage e.g. sd card</returns>
	public bool IsCameraStorage(CameraDriveInfo drive)
	{
		// 2. Filter writable, ready volumes
		if ( !drive.IsReady )
		{
			logger.LogDebug($"[IsCameraStorage] Drive {drive.DriveFormat} is not ready");
			return false;
		}

		if ( !drive.RootDirectory.Exists )
		{
			logger.LogDebug(
				$"[IsCameraStorage] Drive RootDirectory does not Exists: {drive.RootDirectory.FullName}");
			return false;
		}

		// 3. File system heuristic (portable, but soft)
		if ( !IsCameraFriendlyFileSystem(drive.DriveFormat) )
		{
			logger.LogError(
				$"No IsCameraFriendlyFileSystem: {drive.RootDirectory.FullName} {drive.DriveFormat}");
			return false;
		}

		// 4. DCIM folder (gold standard)
		//  5. Camera-like directory structure (optional but strong)
		var dcimPath = Path.Combine(drive.RootDirectory.FullName, "DCIM");
		var dcimLowerCasePath = Path.Combine(drive.RootDirectory.FullName, "dcim");

		var hasDcim = _hostStorage.ExistFolder(dcimPath) ||
		              _hostStorage.ExistFolder(dcimLowerCasePath) ||
		              HasCameraDirectoryStructure(drive.RootDirectory.FullName);

		logger.LogDebug($"[IsCameraStorage] HasDcim: {hasDcim}");
		return hasDcim;
	}

	private static bool IsCameraFriendlyFileSystem(string driveFormat)
	{
		var fs = driveFormat.ToLowerInvariant();

		// Windows: FAT32, exFAT
		// Linux: vfat, exfat
		// macOS: msdos, exfat
		return fs.Contains("fat");
	}

	internal bool HasCameraDirectoryStructure(string rootPath)
	{
		// Sony / Panasonic / some drones
		if ( _hostStorage.ExistFolder(Path.Combine(rootPath, "PRIVATE")) )
		{
			return true;
		}

		// Some cameras place DCIM one level down
		var dcimCandidate = _hostStorage
			.GetDirectoryRecursive(rootPath)
			.Select(value => PathHelper.GetFileName(value.Key))
			.Any(name =>
				!string.IsNullOrEmpty(name) &&
				name.Length >= 3 &&
				char.IsDigit(name[0]) &&
				char.IsDigit(name[1]) &&
				char.IsDigit(name[2]));

		return dcimCandidate;
	}
}
