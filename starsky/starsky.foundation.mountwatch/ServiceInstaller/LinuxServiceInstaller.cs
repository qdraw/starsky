using System;
using System.IO;
using System.Threading.Tasks;
using starsky.foundation.mountwatch.ServiceInstaller.Helpers;
using starsky.foundation.mountwatch.ServiceInstaller.Interfaces;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;

namespace starsky.foundation.mountwatch.ServiceInstaller;

/// <summary>
///     Linux-specific service installer using systemd
/// </summary>
internal class LinuxServiceInstaller(IWebLogger logger) : IOsServiceInstaller
{
	private readonly IStorage _storage = new StorageHostFullPathFilesystem(logger);

	internal LinuxServiceInstaller(IWebLogger logger, IStorage storage) : this(logger)
	{
		_storage = storage;
	}
	/// <summary>
	///     Install systemd service on Linux
	/// </summary>
	public async Task<bool> InstallAsync(string executablePath)
	{
		var servicePath = $"/etc/systemd/system/{WatchServiceName.GetSystemDName()}.service";
		var serviceContent = ServiceInstallerHelper.GenerateLinuxSystemdUnit(executablePath);

		try
		{
			using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(serviceContent));
			var success = await _storage.WriteStreamAsync(stream, servicePath);
			if ( !success )
			{
				// Try user-level systemd if system-level fails
				return await InstallUserAsync(executablePath);
			}

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
		if ( _storage.ExistFile(systemPath) )
		{
			_storage.FileDelete(systemPath);
			logger.LogInformation($"systemd unit removed: {systemPath}");
			logger.LogInformation("Run: sudo systemctl daemon-reload");
			logger.LogInformation($"Linux systemd unit removed: {systemPath}");
			deleted = true;
		}
		
		if ( _storage.ExistFile(userPath) )
		{
			_storage.FileDelete(userPath);
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

		return true;
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
			_storage.CreateDirectory(userSystemdDir);
			using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(serviceContent));
			var success = await _storage.WriteStreamAsync(stream, servicePath);
			
			if ( !success )
			{
				logger.LogError($"Failed to write service file to {servicePath}");
				return false;
			}

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
