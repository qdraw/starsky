using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using starsky.foundation.mountwatch.ServiceInstaller.Helpers;
using starsky.foundation.mountwatch.ServiceInstaller.Interfaces;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;

namespace starsky.foundation.mountwatch.ServiceInstaller;

/// <summary>
///     Linux-specific service installer using systemd
/// </summary>
internal class LinuxServiceInstaller(IWebLogger logger) : IOsServiceInstaller
{
	private const string SystemctlCmd = "systemctl";

	private readonly Func<string, string, Task<bool>> _runProcessAsync =
		(fileName, args) => new RunProcess(logger).RunProcessAsync(fileName, args);

	private readonly IStorage _storage = new StorageHostFullPathFilesystem(logger);
	private readonly UnixSecurity _unixSecurity = new();

	internal LinuxServiceInstaller(IWebLogger logger, IStorage storage) : this(logger)
	{
		_storage = storage;
	}

	// Internal constructor for tests to inject a custom runProcess delegate
	internal LinuxServiceInstaller(IWebLogger logger, IStorage storage,
		Func<string, string, Task<bool>> runProcessAsync, UnixSecurity unixSecurity) : this(logger,
		storage)
	{
		_runProcessAsync = runProcessAsync;
		_unixSecurity = unixSecurity;
	}

	/// <summary>
	///     Install systemd service on Linux
	/// </summary>
	public async Task<bool> InstallAsync(string executablePath)
	{
		var serviceContent = ServiceInstallerHelper.GenerateLinuxSystemdUnit(executablePath);

		if ( !_unixSecurity.IsRunningAsRoot() )
		{
			return await InstallUserAsync(executablePath);
		}

		logger.LogInformation($"systemd unit installed: {executablePath}");
		logger.LogInformation("To enable and start:");
		logger.LogInformation($"  sudo {SystemctlCmd} daemon-reload");
		logger.LogInformation(
			$"  sudo {SystemctlCmd} enable {new WatchServiceName().GetSystemDName()}");
		logger.LogInformation(
			$"  sudo {SystemctlCmd} start {new WatchServiceName().GetSystemDName()}");

		logger.LogInformation($"Linux systemd unit written to {executablePath}");

		var servicePath = $"/etc/systemd/system/{new WatchServiceName().GetSystemDName()}.service";
		using var stream = new MemoryStream(Encoding.UTF8.GetBytes(serviceContent));
		return await _storage.WriteStreamAsync(stream, servicePath);
	}

	/// <summary>
	///     Uninstall systemd service on Linux
	/// </summary>
	public async Task<bool> UninstallAsync()
	{
		await StopAsync();
		var systemPath = $"/etc/systemd/system/{new WatchServiceName().GetSystemDName()}.service";
		var userPath = Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
			".config", "systemd", "user", $"{new WatchServiceName().GetSystemDName()}.service");

		var deleted = false;
		if ( _storage.ExistFile(systemPath) )
		{
			_storage.FileDelete(systemPath);
			logger.LogInformation($"systemd unit removed: {systemPath}");
			logger.LogInformation($"Run: sudo {SystemctlCmd} daemon-reload");
			logger.LogInformation($"Linux systemd unit removed: {systemPath}");
			deleted = true;
		}

		if ( _storage.ExistFile(userPath) )
		{
			_storage.FileDelete(userPath);
			logger.LogInformation($"systemd user unit removed: {userPath}");
			logger.LogInformation($"Run: {SystemctlCmd} --user daemon-reload");
			logger.LogInformation($"Linux systemd user unit removed: {userPath}");
			deleted = true;
		}

		if ( !deleted )
		{
			logger.LogInformation(
				$"No systemd service found for {new WatchServiceName().GetSystemDName()}");
		}

		return true;
	}

	/// <summary>
	///     Start systemd service on Linux
	/// </summary>
	public async Task<bool> StartAsync()
	{
		return await StartStopAsync(true);
	}

	/// <summary>
	///     Stop systemd service on Linux
	/// </summary>
	public async Task<bool> StopAsync()
	{
		return await StartStopAsync(false);
	}

	public async Task<(bool installed, bool running)> StatusAsync()
	{
		var systemPath = $"/etc/systemd/system/{new WatchServiceName().GetSystemDName()}.service";
		var userPath = Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
			".config", "systemd", "user", $"{new WatchServiceName().GetSystemDName()}.service");

		var installed = _storage.ExistFile(systemPath) || _storage.ExistFile(userPath);
		bool running;
		try
		{
			// systemctl is-active --quiet returns 0 when active
			running = await _runProcessAsync(SystemctlCmd,
				$"is-active --quiet {new WatchServiceName().GetSystemDName()}");
			if ( !running )
			{
				// Try user-level
				running = await _runProcessAsync(SystemctlCmd,
					$"--user is-active --quiet {new WatchServiceName().GetSystemDName()}");
			}
		}
		catch
		{
			running = false;
		}

		return ( installed, running );
	}

	private async Task<bool> StartStopAsync(bool isStart)
	{
		var command = isStart ? "start" : "stop";
		try
		{
			// Try system-level first (without sudo to avoid privilege prompts in tests)
			var result = await _runProcessAsync(SystemctlCmd,
				$"{command} {new WatchServiceName().GetSystemDName()}");
			if ( !result )
			{
				// Fallback to user-level
				result = await _runProcessAsync(SystemctlCmd,
					$"--user {command} {new WatchServiceName().GetSystemDName()}");
			}

			if ( result )
			{
				logger.LogInformation(
					$"Linux service {command}: {new WatchServiceName().GetSystemDName()}");
			}

			return result;
		}
		catch ( Exception ex )
		{
			logger.LogError(ex, $"Failed to {command} Linux service: {ex.Message}");
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
			Path.Combine(userSystemdDir, $"{new WatchServiceName().GetSystemDName()}.service");
		var serviceContent = ServiceInstallerHelper.GenerateLinuxSystemdUnit(executablePath);

		try
		{
			_storage.CreateDirectory(userSystemdDir);
			using var stream = new MemoryStream(Encoding.UTF8.GetBytes(serviceContent));
			var success = await _storage.WriteStreamAsync(stream, servicePath);

			if ( !success )
			{
				logger.LogError($"Failed to write service file to {servicePath}");
				return false;
			}

			logger.LogInformation($"systemd user unit installed: {servicePath}");
			logger.LogInformation("To enable and start:");
			logger.LogInformation($"  {SystemctlCmd} --user daemon-reload");
			logger.LogInformation(
				$"  {SystemctlCmd} --user enable {new WatchServiceName().GetSystemDName()}");
			logger.LogInformation(
				$"  {SystemctlCmd} --user start {new WatchServiceName().GetSystemDName()}");

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
