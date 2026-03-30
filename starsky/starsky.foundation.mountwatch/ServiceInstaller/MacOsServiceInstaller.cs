using System;
using System.IO;
using System.Threading.Tasks;
using starsky.foundation.mountwatch.ServiceInstaller.Interfaces;
using starsky.foundation.mountwatch.Services;
using starsky.foundation.platform.Interfaces;

namespace starsky.foundation.mountwatch.ServiceInstaller;

/// <summary>
///     macOS-specific service installer using launchd
/// </summary>
internal class MacOsServiceInstaller : IOsServiceInstaller
{
	private const string ServiceName = "com.starsky.mountwatcher";
	private readonly IConsole _console;
	private readonly IWebLogger _logger;

	public MacOsServiceInstaller(IConsole console, IWebLogger logger)
	{
		_console = console;
		_logger = logger;
	}

	/// <summary>
	///     Install launchd plist on macOS
	/// </summary>
	public async Task<bool> InstallAsync(string executablePath)
	{
		var plistPath = GetMacOsPlistPath();
		var plistContent = ServiceInstallerHelper.GenerateMacOsPlist(executablePath);

		try
		{
			var directory = Path.GetDirectoryName(plistPath)!;
			Directory.CreateDirectory(directory);
			await File.WriteAllTextAsync(plistPath, plistContent);

			_console.WriteLine($"LaunchAgent installed: {plistPath}");
			_console.WriteLine($"To load now: launchctl load {plistPath}");
			_console.WriteLine(
				"Note: Grant Full Disk Access to the executable in System Preferences.");

			_logger.LogInformation($"macOS launchd plist written to {plistPath}");
			return true;
		}
		catch ( Exception ex )
		{
			_logger.LogError(ex, $"Failed to install macOS service: {ex.Message}");
			return false;
		}
	}

	/// <summary>
	///     Uninstall launchd plist on macOS
	/// </summary>
	public async Task<bool> UninstallAsync()
	{
		var plistPath = GetMacOsPlistPath();

		try
		{
			if ( File.Exists(plistPath) )
			{
				_console.WriteLine($"Stopping service: launchctl unload {plistPath}");
				File.Delete(plistPath);
				_console.WriteLine($"LaunchAgent removed: {plistPath}");
				_logger.LogInformation($"macOS launchd plist removed from {plistPath}");
			}
			else
			{
				_console.WriteLine($"LaunchAgent not found: {plistPath}");
			}

			return await Task.FromResult(true);
		}
		catch ( Exception ex )
		{
			_logger.LogError(ex, $"Failed to uninstall macOS service: {ex.Message}");
			return false;
		}
	}

	/// <summary>
	///     Get the macOS LaunchAgents plist path
	/// </summary>
	internal static string GetMacOsPlistPath()
	{
		var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
		return Path.Combine(home, "Library", "LaunchAgents", $"{ServiceName}.plist");
	}
}
