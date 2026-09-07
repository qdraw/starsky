using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using starsky.foundation.mountwatch.ServiceInstaller.Helpers;
using starsky.foundation.mountwatch.ServiceInstaller.Interfaces;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;

[assembly: InternalsVisibleTo("starskytest")]

namespace starsky.foundation.mountwatch.ServiceInstaller;

/// <summary>
///     macOS-specific service installer using launchd
/// </summary>
internal class MacOsServiceInstaller(IWebLogger logger) : IOsServiceInstaller
{
	private const string Launchctl = "launchctl";

	private readonly Func<string, string, int[]?, Task<bool>> _runProcessAsync =
		(fileName, args, codes) => new RunProcess(logger).RunProcessAsync(fileName, args, codes);

	private readonly IStorage _storage = new StorageHostFullPathFilesystem(logger);

	internal MacOsServiceInstaller(IWebLogger logger,
		IStorage storage,
		Func<string, string, int[]?, Task<bool>> runProcessAsync) : this(logger)
	{
		_storage = storage;
		_runProcessAsync = runProcessAsync;
	}

	/// <summary>
	///     Install launchd plist on macOS
	/// </summary>
	public async Task<bool> InstallAsync(string executablePath)
	{
		var plistPath = GetMacOsPlistPath();
		var plistContent =
			ServiceInstallerHelper.GenerateMacOsPlist(executablePath,
				new WatchServiceName().GetReverseDnsName());

		try
		{
			var directory = Path.GetDirectoryName(plistPath)!;
			_storage.CreateDirectory(directory);

			using var stream = new MemoryStream(Encoding.UTF8.GetBytes(plistContent));
			await _storage.WriteStreamAsync(stream, plistPath);

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
			if ( _storage.ExistFile(plistPath) )
			{
				await StopAsync();
				_storage.FileDelete(plistPath);
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

			// Best-effort unload before loading: clears any stale launchd registration that
			// may have been left behind when a previous unload silently failed (e.g. during a
			// Sparkle update). Non-zero exit codes are expected and suppressed when the job was
			// not registered; any genuinely unexpected code is still treated as success here
			// because the load that follows will fail with a clear error if needed.
			int[] silentUnloadExitCodes = [0, 1, 2, 3, 4, 5, 36, 113, 125];
			await _runProcessAsync(Launchctl, $"unload {plistPath}", silentUnloadExitCodes);

			var result = await _runProcessAsync(Launchctl, $"load {plistPath}", null);
			if ( result )
			{
				logger.LogInformation(
					$"macOS service started: {new WatchServiceName().GetReverseDnsName()}");
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
			var result =
				await _runProcessAsync(Launchctl, $"unload {plistPath}", null);
			if ( result )
			{
				logger.LogInformation(
					$"macOS service stopped: {new WatchServiceName().GetReverseDnsName()}");
			}

			return result;
		}
		catch ( Exception ex )
		{
			logger.LogError(ex, $"Failed to stop macOS service: {ex.Message}");
			return false;
		}
	}

	public async Task<(bool installed, bool running)> StatusAsync()
	{
		var plistPath = GetMacOsPlistPath();
		var installed = _storage.ExistFile(plistPath);
		bool running;
		try
		{
			// launchctl list <label> returns 0 when loaded
			running = await _runProcessAsync(Launchctl,
				$"list {new WatchServiceName().GetReverseDnsName()}", null);
		}
		catch
		{
			running = false;
		}

		return ( installed, running );
	}

	/// <summary>
	///     Get the macOS LaunchAgents plist path
	/// </summary>
	internal static string GetMacOsPlistPath()
	{
		var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
		return Path.Combine(home, "Library", "LaunchAgents",
			$"{new WatchServiceName().GetReverseDnsName()}.plist");
	}
}
