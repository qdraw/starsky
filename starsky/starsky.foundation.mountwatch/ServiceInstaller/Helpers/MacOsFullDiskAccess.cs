using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using starsky.foundation.platform.Architecture;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;

namespace starsky.foundation.mountwatch.ServiceInstaller.Helpers;

public class MacOsFullDiskAccess(ISelectorStorage selectorStorage, IWebLogger logger)
{
	public const string MacOsPrivacySettingsUri =
		"x-apple.systempreferences:com.apple.preference.security?Privacy_AllFiles";

	private readonly Func<OSPlatform> _platformResolver =
		OperatingSystemHelper.GetPlatform;

	private readonly IStorage hostStorage =
		selectorStorage.Get(SelectorStorage.StorageServices.HostFilesystem);

	public bool? CheckMacOsFullDiskAccessOnStartup()
	{
		if ( _platformResolver() != OSPlatform.OSX )
		{
			return null;
		}

		if ( CanReadVolumesDirectory(out var probeException) )
		{
			logger.LogInformation("macOS /Volumes access check passed");
			return true;
		}

		const string message = "Unable to read /Volumes. " +
		                       "Mount events may not be visible until Full Disk Access is granted.";
		logger.LogError($"{message} Error: {probeException?.Message}");

		if ( OpenFullDiskAccessSettings() )
		{
			return false;
		}

		logger.LogInformation($"Run manually: open {MacOsPrivacySettingsUri}");
		return false;
	}


	private static bool OpenFullDiskAccessSettings()
	{
		try
		{
			Process.Start(new ProcessStartInfo
			{
				FileName = "open", Arguments = MacOsPrivacySettingsUri, UseShellExecute = false
			});
			return true;
		}
		catch
		{
			return false;
		}
	}

	private bool CanReadVolumesDirectory(out Exception? exception)
	{
		exception = null;

		try
		{
			const string volumesPath = "/Volumes";
			if ( !hostStorage.ExistFolder(volumesPath) )
			{
				return true;
			}

			_ = hostStorage.GetDirectories(volumesPath).Take(1).ToList();
			return true;
		}
		catch ( Exception ex )
		{
			exception = ex;
			return false;
		}
	}
}
