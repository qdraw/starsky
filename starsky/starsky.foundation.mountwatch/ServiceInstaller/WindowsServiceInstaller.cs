using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using starsky.foundation.mountwatch.ServiceInstaller.Helpers;
using starsky.foundation.mountwatch.ServiceInstaller.Interfaces;
using starsky.foundation.platform.Interfaces;

[assembly: InternalsVisibleTo("starskytest")]

namespace starsky.foundation.mountwatch.ServiceInstaller;

/// <summary>
///     Windows-specific service installer using sc.exe
/// </summary>
internal class WindowsServiceInstaller(IWebLogger logger) : IOsServiceInstaller
{
	private readonly Func<int, Task> _delayAsync = Task.Delay;

	private readonly Func<string, string, Task<bool>> _runProcessAsync =
		(fileName, args) => new RunProcess(logger).RunProcessAsync(fileName, args);

	// sc.exe stop returns 1060 (service does not exist) or 1062 (service not active);
	// both mean the service is already in the desired stopped state → treat as success.
	private readonly Func<string, string, Task<bool>> _stopProcessAsync =
		(fileName, args) => new RunProcess(logger).RunProcessAsync(fileName, args,
			new[] { 1060, 1062 });

	private readonly string _serviceDisplayName = WatchServiceName.GetDisplayName();
	private readonly string _serviceName = WatchServiceName.GetReverseDnsName();

	internal WindowsServiceInstaller(IWebLogger logger,
		Func<string, string, Task<bool>> runProcessAsync,
		Func<int, Task> delayAsync) : this(logger)
	{
		_runProcessAsync = runProcessAsync;
		_delayAsync = delayAsync;
		_stopProcessAsync = runProcessAsync;
	}

	/// <summary>
	///     Install Windows Service using sc.exe
	/// </summary>
	public async Task<bool> InstallAsync(string executablePath)
	{
		try
		{
			string binPath;
			if ( executablePath.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) )
			{
				const string dotnetPath = "dotnet.exe";
				binPath = $"\\\"{dotnetPath}\\\" \\\"{executablePath}\\\"";
			}
			else
			{
				binPath = $"\\\"{executablePath}\\\"";
			}

			// sc.exe create "service" binPath= "path"
			// Note the space after "binPath=" is mandatory for sc.exe
			var result = await _runProcessAsync("sc.exe",
				$"create \"{_serviceName}\" binPath= \"{binPath}\" " +
				$"DisplayName= \"{_serviceDisplayName}\" start= auto obj= \"LocalSystem\"");

			if ( result )
			{
				logger.LogInformation($"Windows Service installed: {_serviceName}");
				logger.LogInformation($"To start: sc start \"{_serviceName}\"");
			}
			else
			{
				logger.LogError($"Failed to install Windows Service: {_serviceName}");
			}

			return result;
		}
		catch ( Exception ex )
		{
			logger.LogError(ex, $"Failed to install Windows service: {ex.Message}");
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
			var result = await _runProcessAsync("sc.exe", $"delete \"{_serviceName}\"");

			if ( result )
			{
				logger.LogInformation($"Windows Service removed: {_serviceName}");
			}
			else
			{
				logger.LogError($"Failed to remove Windows Service: {_serviceName}");
			}

			return result;
		}
		catch ( Exception ex )
		{
			logger.LogError(ex, $"Failed to uninstall Windows service: {ex.Message}");
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
			var result = await _runProcessAsync("sc.exe", $"start \"{_serviceName}\"");
			if ( !result )
			{
				logger.LogInformation(
					$"Retrying to start Windows Service: {_serviceName} after 2 seconds...");
				await _delayAsync(2000);
				result = await _runProcessAsync("sc.exe", $"start \"{_serviceName}\"");
			}

			if ( result )
			{
				logger.LogInformation($"Windows Service started: {_serviceName}");
			}
			else
			{
				logger.LogError($"Failed to start Windows Service: {_serviceName}");
			}

			return result;
		}
		catch ( Exception ex )
		{
			logger.LogError(ex, $"Failed to start Windows service: {ex.Message}");
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
			var result = await _stopProcessAsync("sc.exe", $"stop \"{_serviceName}\"");
			if ( result )
			{
				logger.LogInformation($"Windows Service stopped: {_serviceName}");
			}

			return result;
		}
		catch ( Exception ex )
		{
			logger.LogError(ex, $"Failed to stop Windows service: {ex.Message}");
			return false;
		}
	}
}
