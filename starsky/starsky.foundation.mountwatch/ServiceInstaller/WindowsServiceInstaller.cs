using System;
using System.Diagnostics;
using System.Threading.Tasks;
using starsky.foundation.mountwatch.ServiceInstaller.Interfaces;
using starsky.foundation.platform.Interfaces;

namespace starsky.foundation.mountwatch.ServiceInstaller;

/// <summary>
///     Windows-specific service installer using sc.exe
/// </summary>
internal class WindowsServiceInstaller : IOsServiceInstaller
{
	private const string ServiceName = "nl.qdraw.mountwatcher";
	private const string ServiceDisplayName = "Starsky Mount Watcher";

	private readonly IConsole _console;
	private readonly IWebLogger _logger;

	public WindowsServiceInstaller(IConsole console, IWebLogger logger)
	{
		_console = console;
		_logger = logger;
	}

	/// <summary>
	///     Install Windows Service using sc.exe
	/// </summary>
	public async Task<bool> InstallAsync(string executablePath)
	{
		try
		{
			// Add quotes for the binPath as well
			var result = await RunProcessAsync("sc.exe",
				$"create \"{ServiceName}\" binPath= \"\\\"{executablePath}\\\"\" " +
				$"DisplayName= \"{ServiceDisplayName}\" start= auto");

			if ( result )
			{
				_console.WriteLine($"Windows Service installed: {ServiceName}");
				_console.WriteLine($"To start: sc start {ServiceName}");
				_logger.LogInformation($"Windows Service installed: {ServiceName}");
			}
			else
			{
				_logger.LogError($"Failed to install Windows Service: {ServiceName}");
			}

			return result;
		}
		catch ( Exception ex )
		{
			_logger.LogError(ex, $"Failed to install Windows service: {ex.Message}");
			return false;
		}
	}

	/// <summary>
	///     Uninstall Windows Service
	/// </summary>
	public async Task<bool> UninstallAsync()
	{
		try
		{
			await StopAsync();
			var result = await RunProcessAsync("sc.exe", $"delete \"{ServiceName}\"");

			if ( result )
			{
				_console.WriteLine($"Windows Service removed: {ServiceName}");
				_logger.LogInformation($"Windows Service removed: {ServiceName}");
			}
			else
			{
				_logger.LogError($"Failed to remove Windows Service: {ServiceName}");
			}

			return result;
		}
		catch ( Exception ex )
		{
			_logger.LogError(ex, $"Failed to uninstall Windows service: {ex.Message}");
			return false;
		}
	}

	/// <summary>
	///     Start Windows Service
	/// </summary>
	public async Task<bool> StartAsync()
	{
		try
		{
			var result = await RunProcessAsync("sc.exe", $"start \"{ServiceName}\"");
			if ( !result )
			{
				_logger.LogInformation($"Retrying to start Windows Service: {ServiceName} after 2 seconds...");
				await Task.Delay(2000);
				result = await RunProcessAsync("sc.exe", $"start \"{ServiceName}\"");
			}

			if ( result )
			{
				_console.WriteLine($"Windows Service started: {ServiceName}");
				_logger.LogInformation($"Windows Service started: {ServiceName}");
			}
			else
			{
				_logger.LogError($"Failed to start Windows Service: {ServiceName}");
			}

			return result;
		}
		catch ( Exception ex )
		{
			_logger.LogError(ex, $"Failed to start Windows service: {ex.Message}");
			return false;
		}
	}

	/// <summary>
	///     Stop Windows Service
	/// </summary>
	public async Task<bool> StopAsync()
	{
		try
		{
			var result = await RunProcessAsync("sc.exe", $"stop \"{ServiceName}\"");
			if ( result )
			{
				_console.WriteLine($"Windows Service stopped: {ServiceName}");
				_logger.LogInformation($"Windows Service stopped: {ServiceName}");
			}

			return result;
		}
		catch ( Exception ex )
		{
			_logger.LogError(ex, $"Failed to stop Windows service: {ex.Message}");
			return false;
		}
	}

	/// <summary>
	///     Run an external process and return whether it succeeded
	/// </summary>
	private async Task<bool> RunProcessAsync(string fileName, string arguments)
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

		var output = await process.StandardOutput.ReadToEndAsync();
		var error = await process.StandardError.ReadToEndAsync();

		await process.WaitForExitAsync();

		if ( process.ExitCode != 0 )
		{
			_logger.LogError(
				$"Process {fileName} {arguments} failed with exit code {process.ExitCode}\nOutput: {output}\nError: {error}");
		}

		return process.ExitCode == 0;
	}
}
