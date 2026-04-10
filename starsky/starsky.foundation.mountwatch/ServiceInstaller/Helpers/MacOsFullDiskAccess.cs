using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using starsky.foundation.platform.Architecture;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;

namespace starsky.foundation.mountwatch.ServiceInstaller.Helpers;

public class MacOsFullDiskAccess
{
	public const string MacOsPrivacySettingsUri =
		"x-apple.systempreferences:com.apple.preference.security?Privacy_AllFiles";

	private readonly IStorage _hostStorage;
	private readonly IWebLogger _logger;
	private readonly Func<OSPlatform> _platformResolver;

	public MacOsFullDiskAccess(ISelectorStorage selectorStorage, IWebLogger logger,
		Func<OSPlatform>? platformResolver = null)
	{
		_logger = logger;
		_platformResolver = platformResolver ?? OperatingSystemHelper.GetPlatform;
		_hostStorage = selectorStorage.Get(SelectorStorage.StorageServices.HostFilesystem);
	}

	internal string OpenCmd { get; set; } = "open";

	public bool? CheckMacOsFullDiskAccessOnStartup()
	{
		if ( _platformResolver() != OSPlatform.OSX )
		{
			return null;
		}

		if ( CanReadVolumesDirectory(out var probeException) )
		{
			_logger.LogInformation("macOS /Volumes access check passed");
			return true;
		}

		const string message = "Unable to read /Volumes. " +
		                       "Mount events may not be visible until Full Disk Access is granted.";
		_logger.LogError($"{message} Error: {probeException?.Message}");

		if ( OpenFullDiskAccessSettings() )
		{
			return false;
		}

		_logger.LogInformation($"Run manually: open {MacOsPrivacySettingsUri}");
		return false;
	}

	[SuppressMessage("Usage", "S4036: Make sure the \"PATH\" " +
	                          "variable only contains fixed, unwriteable directories.")]
	protected virtual bool OpenFullDiskAccessSettings()
	{
		try
		{
			Process.Start(new ProcessStartInfo
			{
				FileName = OpenCmd, Arguments = MacOsPrivacySettingsUri, UseShellExecute = false
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
			if ( !_hostStorage.ExistFolder(volumesPath) )
			{
				return false;
			}

			_ = _hostStorage.GetDirectories(volumesPath).Take(1).ToList();
			return true;
		}
		catch ( Exception ex )
		{
			exception = ex;
			return false;
		}
	}
}
