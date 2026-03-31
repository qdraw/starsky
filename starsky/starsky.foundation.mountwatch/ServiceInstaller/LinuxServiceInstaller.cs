using System;
using System.IO;
using System.Threading.Tasks;
using starsky.foundation.mountwatch.ServiceInstaller.Helpers;
using starsky.foundation.mountwatch.ServiceInstaller.Interfaces;
using starsky.foundation.platform.Interfaces;

namespace starsky.foundation.mountwatch.ServiceInstaller;

/// <summary>
///     Linux-specific service installer using systemd
/// </summary>
internal class LinuxServiceInstaller(IWebLogger logger) : IOsServiceInstaller
{
	/// <summary>
	///     Install systemd service on Linux
	/// </summary>
	public async Task<bool> InstallAsync(string executablePath)
	{
		var servicePath = $"/etc/systemd/system/{WatchServiceName.GetSystemDName()}.service";
		var serviceContent = ServiceInstallerHelper.GenerateLinuxSystemdUnit(executablePath);

		try
		{
			await File.WriteAllTextAsync(servicePath, serviceContent);

			logger.LogInformation($"systemd unit installed: {servicePath}");
			logger.LogInformation("To enable and start:");
			logger.LogInformation("  sudo systemctl daemon-reload");
			logger.LogInformation($"  sudo systemctl enable {WatchServiceName.GetSystemDName()}");
			logger.LogInformation($"  sudo systemctl start {WatchServiceName.GetSystemDName()}");

			logger.LogInformation($"Linux systemd unit written to {servicePath}");
			return true;
		}
		catch ( UnauthorizedAccessException )
		{
			// Try user-level systemd
			return await InstallUserAsync(executablePath);
		}
		catch ( Exception ex )
		{
			logger.LogError(ex, $"Failed to install Linux service: {ex.Message}");
			return false;
		}
	}

	/// <summary>
	///     Uninstall systemd service on Linux
	/// </summary>
	public async Task<bool> UninstallAsync()
	{
		await StopAsync();
		var systemPath = $"/etc/systemd/system/{WatchServiceName.GetSystemDName()}.service";
		var userPath = Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
			".config", "systemd", "user", $"{WatchServiceName.GetSystemDName()}.service");

		var deleted = false;

		if ( File.Exists(systemPath) )
		{
			File.Delete(systemPath);
			logger.LogInformation($"systemd unit removed: {systemPath}");
			logger.LogInformation("Run: sudo systemctl daemon-reload");
			logger.LogInformation($"Linux systemd unit removed: {systemPath}");
			deleted = true;
		}

		if ( File.Exists(userPath) )
		{
			File.Delete(userPath);
			logger.LogInformation($"systemd user unit removed: {userPath}");
			logger.LogInformation("Run: systemctl --user daemon-reload");
			logger.LogInformation($"Linux systemd user unit removed: {userPath}");
			deleted = true;
		}

		if ( !deleted )
		{
			logger.LogInformation(
				$"No systemd service found for {WatchServiceName.GetSystemDName()}");
		}

		return await Task.FromResult(true);
	}

	/// <summary>
	///     Start systemd service on Linux
	/// </summary>
	public async Task<bool> StartAsync()
	{
		try
		{
			// Try system-level first
			var result = await new RunProcess(logger).RunProcessAsync("sudo",
				$"systemctl start {WatchServiceName.GetSystemDName()}");
			if ( !result )
			{
				// Fallback to user-level
				result = await new RunProcess(logger).RunProcessAsync("systemctl",
					$"--user start {WatchServiceName.GetSystemDName()}");
			}

			if ( result )
			{
				logger.LogInformation(
					$"Linux service started: {WatchServiceName.GetSystemDName()}");
			}

			return result;
		}
		catch ( Exception ex )
		{
			logger.LogError(ex, $"Failed to start Linux service: {ex.Message}");
			return false;
		}
	}

	/// <summary>
	///     Stop systemd service on Linux
	/// </summary>
	public async Task<bool> StopAsync()
	{
		try
		{
			// Try system-level first
			var result = await new RunProcess(logger).RunProcessAsync("sudo",
				$"systemctl stop {WatchServiceName.GetSystemDName()}");
			if ( !result )
			{
				// Fallback to user-level
				result = await new RunProcess(logger).RunProcessAsync("systemctl",
					$"--user stop {WatchServiceName.GetSystemDName()}");
			}

			if ( result )
			{
				logger.LogInformation(
					$"Linux service stopped: {WatchServiceName.GetSystemDName()}");
			}

			return result;
		}
		catch ( Exception ex )
		{
			logger.LogError(ex, $"Failed to stop Linux service: {ex.Message}");
			return false;
		}
	}

	/// <summary>
	///     Install systemd user-level service (fallback without root)
	/// </summary>
	private async Task<bool> InstallUserAsync(string executablePath)
	{
		var userSystemdDir = Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
			".config", "systemd", "user");
		var servicePath =
			Path.Combine(userSystemdDir, $"{WatchServiceName.GetSystemDName()}.service");
		var serviceContent = ServiceInstallerHelper.GenerateLinuxSystemdUnit(executablePath);

		try
		{
			Directory.CreateDirectory(userSystemdDir);
			await File.WriteAllTextAsync(servicePath, serviceContent);

			logger.LogInformation($"systemd user unit installed: {servicePath}");
			logger.LogInformation("To enable and start:");
			logger.LogInformation("  systemctl --user daemon-reload");
			logger.LogInformation($"  systemctl --user enable {WatchServiceName.GetSystemDName()}");
			logger.LogInformation($"  systemctl --user start {WatchServiceName.GetSystemDName()}");

			logger.LogInformation($"Linux systemd user unit written to {servicePath}");
			return true;
		}
		catch ( Exception ex )
		{
			logger.LogError(ex, $"Failed to install Linux user service: {ex.Message}");
			return false;
		}
	}
}
