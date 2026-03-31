using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using starsky.foundation.mountwatch.ServiceInstaller.Helpers;
using starsky.foundation.mountwatch.ServiceInstaller.Interfaces;
using starsky.foundation.platform.Interfaces;

namespace starsky.foundation.mountwatch.ServiceInstaller;

/// <summary>
///     macOS-specific service installer using launchd
/// </summary>
internal class MacOsServiceInstaller(IWebLogger logger) : IOsServiceInstaller
{
	/// <summary>
	///     Install launchd plist on macOS
	/// </summary>
	public async Task<bool> InstallAsync(string executablePath)
	{
		var plistPath = GetMacOsPlistPath();
		var plistContent =
			ServiceInstallerHelper.GenerateMacOsPlist(executablePath,
				WatchServiceName.GetReverseDnsName());

		try
		{
			var directory = Path.GetDirectoryName(plistPath)!;
			Directory.CreateDirectory(directory);
			await File.WriteAllTextAsync(plistPath, plistContent);

			logger.LogInformation(
				"Note: Grant Full Disk Access to the executable in System Preferences.");

			logger.LogInformation($"macOS launchd plist written to {plistPath}");
			return true;
		}
		catch ( Exception ex )
		{
			logger.LogError(ex, $"Failed to install macOS service: {ex.Message}");
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
				await StopAsync();
				File.Delete(plistPath);
				logger.LogInformation($"macOS launchd plist removed from {plistPath}");
			}
			else
			{
				logger.LogInformation($"LaunchAgent not found: {plistPath}");
			}

			return await Task.FromResult(true);
		}
		catch ( Exception ex )
		{
			logger.LogError(ex, $"Failed to uninstall macOS service: {ex.Message}");
			return false;
		}
	}

	/// <summary>
	///     Start launchd service on macOS
	/// </summary>
	public async Task<bool> StartAsync()
	{
		try
		{
			var plistPath = GetMacOsPlistPath();
			var result = await RunProcessAsync("launchctl", $"load {plistPath}");
			if ( result )
			{
				logger.LogInformation(
					$"macOS service started: {WatchServiceName.GetReverseDnsName()}");
			}

			return result;
		}
		catch ( Exception ex )
		{
			logger.LogError(ex, $"Failed to start macOS service: {ex.Message}");
			return false;
		}
	}

	/// <summary>
	///     Stop launchd service on macOS
	/// </summary>
	public async Task<bool> StopAsync()
	{
		try
		{
			var plistPath = GetMacOsPlistPath();
			var result = await RunProcessAsync("launchctl", $"unload {plistPath}");
			if ( result )
			{
				logger.LogInformation(
					$"macOS service stopped: {WatchServiceName.GetReverseDnsName()}");
			}

			return result;
		}
		catch ( Exception ex )
		{
			logger.LogError(ex, $"Failed to stop macOS service: {ex.Message}");
			return false;
		}
	}

	/// <summary>
	///     Run an external process and return whether it succeeded
	/// </summary>
	private static async Task<bool> RunProcessAsync(string fileName, string arguments)
	{
		var processInfo = new ProcessStartInfo
		{
			FileName = fileName,
			Arguments = arguments,
			RedirectStandardOutput = true,
			RedirectStandardError = true,
			UseShellExecute = false,
			CreateNoWindow = true
		};

		using var process = Process.Start(processInfo);
		if ( process == null )
		{
			return false;
		}

		await process.WaitForExitAsync();
		return process.ExitCode == 0;
	}

	/// <summary>
	///     Get the macOS LaunchAgents plist path
	/// </summary>
	internal static string GetMacOsPlistPath()
	{
		var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
		return Path.Combine(home, "Library", "LaunchAgents",
			$"{WatchServiceName.GetReverseDnsName()}.plist");
	}
}
