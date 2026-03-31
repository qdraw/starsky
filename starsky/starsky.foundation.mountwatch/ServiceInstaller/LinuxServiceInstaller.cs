using System;
using System.IO;
using System.Threading.Tasks;
using starsky.foundation.mountwatch.ServiceInstaller.Helpers;
using starsky.foundation.mountwatch.ServiceInstaller.Interfaces;
using starsky.foundation.mountwatch.Services;
using starsky.foundation.platform.Interfaces;

namespace starsky.foundation.mountwatch.ServiceInstaller;

/// <summary>
///     Linux-specific service installer using systemd
/// </summary>
internal class LinuxServiceInstaller : IOsServiceInstaller
{
	private const string SystemdServiceName = "starsky-mountwatcher";

	private readonly IConsole _console;
	private readonly IWebLogger _logger;

	public LinuxServiceInstaller(IConsole console, IWebLogger logger)
	{
		_console = console;
		_logger = logger;
	}

	/// <summary>
	///     Install systemd service on Linux
	/// </summary>
	public async Task<bool> InstallAsync(string executablePath)
	{
		const string servicePath = $"/etc/systemd/system/{SystemdServiceName}.service";
		var serviceContent = ServiceInstallerHelper.GenerateLinuxSystemdUnit(executablePath);

		try
		{
			await File.WriteAllTextAsync(servicePath, serviceContent);

			_console.WriteLine($"systemd unit installed: {servicePath}");
			_console.WriteLine("To enable and start:");
			_console.WriteLine("  sudo systemctl daemon-reload");
			_console.WriteLine($"  sudo systemctl enable {SystemdServiceName}");
			_console.WriteLine($"  sudo systemctl start {SystemdServiceName}");

			_logger.LogInformation($"Linux systemd unit written to {servicePath}");
			return true;
		}
		catch ( UnauthorizedAccessException )
		{
			// Try user-level systemd
			return await InstallUserAsync(executablePath);
		}
		catch ( Exception ex )
		{
			_logger.LogError(ex, $"Failed to install Linux service: {ex.Message}");
			return false;
		}
	}

	/// <summary>
	///     Uninstall systemd service on Linux
	/// </summary>
	public async Task<bool> UninstallAsync()
	{
		await StopAsync();
		var systemPath = $"/etc/systemd/system/{SystemdServiceName}.service";
		var userPath = Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
			".config", "systemd", "user", $"{SystemdServiceName}.service");

		var deleted = false;

		if ( File.Exists(systemPath) )
		{
			File.Delete(systemPath);
			_console.WriteLine($"systemd unit removed: {systemPath}");
			_console.WriteLine("Run: sudo systemctl daemon-reload");
			_logger.LogInformation($"Linux systemd unit removed: {systemPath}");
			deleted = true;
		}

		if ( File.Exists(userPath) )
		{
			File.Delete(userPath);
			_console.WriteLine($"systemd user unit removed: {userPath}");
			_console.WriteLine("Run: systemctl --user daemon-reload");
			_logger.LogInformation($"Linux systemd user unit removed: {userPath}");
			deleted = true;
		}

		if ( !deleted )
		{
			_console.WriteLine($"No systemd service found for {SystemdServiceName}");
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
			var result = await RunProcessAsync("sudo", $"systemctl start {SystemdServiceName}");
			if ( !result )
			{
				// Fallback to user-level
				result = await RunProcessAsync("systemctl", $"--user start {SystemdServiceName}");
			}

			if ( result )
			{
				_console.WriteLine($"Linux service started: {SystemdServiceName}");
				_logger.LogInformation($"Linux service started: {SystemdServiceName}");
			}

			return result;
		}
		catch ( Exception ex )
		{
			_logger.LogError(ex, $"Failed to start Linux service: {ex.Message}");
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
			var result = await RunProcessAsync("sudo", $"systemctl stop {SystemdServiceName}");
			if ( !result )
			{
				// Fallback to user-level
				result = await RunProcessAsync("systemctl", $"--user stop {SystemdServiceName}");
			}

			if ( result )
			{
				_console.WriteLine($"Linux service stopped: {SystemdServiceName}");
				_logger.LogInformation($"Linux service stopped: {SystemdServiceName}");
			}

			return result;
		}
		catch ( Exception ex )
		{
			_logger.LogError(ex, $"Failed to stop Linux service: {ex.Message}");
			return false;
		}
	}

	/// <summary>
	///     Run an external process and return whether it succeeded
	/// </summary>
	private static async Task<bool> RunProcessAsync(string fileName, string arguments)
	{
		var processInfo = new System.Diagnostics.ProcessStartInfo
		{
			FileName = fileName,
			Arguments = arguments,
			RedirectStandardOutput = true,
			RedirectStandardError = true,
			UseShellExecute = false,
			CreateNoWindow = true
		};

		using var process = System.Diagnostics.Process.Start(processInfo);
		if ( process == null )
		{
			return false;
		}

		await process.WaitForExitAsync();
		return process.ExitCode == 0;
	}

	/// <summary>
	///     Install systemd user-level service (fallback without root)
	/// </summary>
	private async Task<bool> InstallUserAsync(string executablePath)
	{
		var userSystemdDir = Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
			".config", "systemd", "user");
		var servicePath = Path.Combine(userSystemdDir, $"{SystemdServiceName}.service");
		var serviceContent = ServiceInstallerHelper.GenerateLinuxSystemdUnit(executablePath);

		try
		{
			Directory.CreateDirectory(userSystemdDir);
			await File.WriteAllTextAsync(servicePath, serviceContent);

			_console.WriteLine($"systemd user unit installed: {servicePath}");
			_console.WriteLine("To enable and start:");
			_console.WriteLine("  systemctl --user daemon-reload");
			_console.WriteLine($"  systemctl --user enable {SystemdServiceName}");
			_console.WriteLine($"  systemctl --user start {SystemdServiceName}");

			_logger.LogInformation($"Linux systemd user unit written to {servicePath}");
			return true;
		}
		catch ( Exception ex )
		{
			_logger.LogError(ex, $"Failed to install Linux user service: {ex.Message}");
			return false;
		}
	}
}
