using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using starsky.foundation.import.Interfaces;
using starsky.foundation.import.Models;
using starsky.foundation.mountwatch.Interfaces;
using starsky.foundation.mountwatch.ServiceInstaller;
using starsky.foundation.mountwatch.ServiceInstaller.Helpers;
using starsky.foundation.mountwatch.ServiceInstaller.Interfaces;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;

namespace starsky.foundation.mountwatch.Services;

/// <summary>
///     CLI service for mount watching and automatic importing
/// </summary>
public class MountWatcherCli
{
	private const int DuplicateCheckWindowSeconds = 60;
	private const string InstallArg = "--install";
	private const string UninstallArg = "--uninstall";

	private readonly AppSettings _appSettings;
	private readonly IConsole _console;
	private readonly IImport _importService;
	private readonly IWebLogger _logger;
	private readonly IMountDetector _mountDetector;
	private readonly IMountWatcherFactory _mountWatcherFactory;
	private readonly HashSet<string> _processedPaths = new();
	private readonly IServiceInstaller _serviceInstaller;

	public MountWatcherCli(
		IImport importService,
		AppSettings appSettings,
		IConsole console,
		IWebLogger logger,
		IMountDetector mountDetector,
		IMountWatcherFactory mountWatcherFactory,
		IServiceInstaller serviceInstaller)
	{
		_importService = importService;
		_appSettings = appSettings;
		_console = console;
		_logger = logger;
		_mountDetector = mountDetector;
		_mountWatcherFactory = mountWatcherFactory;
		_serviceInstaller = serviceInstaller;
	}

	/// <summary>
	///     Start the mount watcher and listen for camera mounts.
	///     Handles --install / --uninstall before starting the watcher loop.
	/// </summary>
	public async Task<bool> StartWatcher(string[] args)
	{
		_logger.LogInformation("Starting mount watcher service");

		_appSettings.Verbose = ArgsHelper.NeedVerbose(args);
		_appSettings.ApplicationType = AppSettings.StarskyAppType.MountWatcher;

		if ( ArgsHelper.NeedHelp(args) )
		{
			ShowHelp();
			return true;
		}

		if ( NeedInstall(args) )
		{
			var execPath = GetCurrentExecutablePath();
			_logger.LogInformation($"Detected executable path: {execPath}");
			var installResult = await _serviceInstaller.InstallAsync(execPath);
			if ( installResult )
			{
				await _serviceInstaller.StartAsync();
			}

			return installResult;
		}

		if ( NeedUninstall(args) )
		{
			return await _serviceInstaller.UninstallAsync();
		}

		return RunWatcher();
	}

	/// <summary>
	///     Returns true if --install is present in args
	/// </summary>
	public static bool NeedInstall(string[] args)
	{
		return args.Any(a => a.Equals(InstallArg, StringComparison.OrdinalIgnoreCase));
	}

	/// <summary>
	///     Returns true if --uninstall is present in args
	/// </summary>
	public static bool NeedUninstall(string[] args)
	{
		return args.Any(a => a.Equals(UninstallArg, StringComparison.OrdinalIgnoreCase));
	}

	/// <summary>
	///     Get the full path to the running executable
	/// </summary>
	private static string GetCurrentExecutablePath()
	{
		return Environment.ProcessPath
		       ?? Assembly.GetExecutingAssembly().Location;
	}

	/// <summary>
	///     Display CLI help
	/// </summary>
	private void ShowHelp()
	{
		_console.WriteLine("Starsky Mount Watcher - automatically imports from camera storage");
		_console.WriteLine("");
		_console.WriteLine("Usage:");
		_console.WriteLine("  starskymountwatchercli [options]");
		_console.WriteLine("");
		_console.WriteLine("Options:");
		_console.WriteLine(
			"  --install       Install as OS service (launchd/systemd/Windows Service)");
		_console.WriteLine("  --uninstall     Remove the OS service");
		_console.WriteLine("  --verbose, -v   Enable verbose logging");
		_console.WriteLine("  --help, -h      Show this help");
		_console.WriteLine("");
		_console.WriteLine("Service setup:");
		if ( OperatingSystem.IsMacOS() )
		{
			_console.WriteLine($"  macOS plist: {MacOsServiceInstaller.GetMacOsPlistPath()}");
			_console.WriteLine("  Load: launchctl load <plist>");
			_console.WriteLine("  Requires: Full Disk Access in System Preferences");
			_console.WriteLine("  To grant Full Disk Access:");
			_console.WriteLine(
				"    open x-apple.systempreferences:com.apple.preference.security?Privacy_AllFiles");
			_console.WriteLine("  Logs: ~/Library/Logs/starsky/mountwatcher.log");
		}
		else if ( OperatingSystem.IsLinux() )
		{
			_console.WriteLine("  systemd: /etc/systemd/system/starsky-mountwatcher.service");
			_console.WriteLine("  Enable: sudo systemctl enable starsky-mountwatcher");
			_console.WriteLine("  Start:  sudo systemctl start starsky-mountwatcher");
			_console.WriteLine($"  Logs: {ServiceInstallerHelper.GetLinuxLogHint()}");
		}
		else if ( OperatingSystem.IsWindows() )
		{
			_console.WriteLine("  Windows Service: sc create \"nl.qdraw.mountwatcher\" ...");
			_console.WriteLine("  Start: sc start nl.qdraw.mountwatcher");
			_console.WriteLine("  Logs: Windows Event Viewer -> Windows Logs -> Application");
			_console.WriteLine("        Source: nl.qdraw.mountwatcher");
		}
	}

	/// <summary>
	///     Core watcher loop: listen for mounts
	/// </summary>
	private bool RunWatcher()
	{
		var watcher = _mountWatcherFactory.CreateMountWatcher();
		watcher.MountDetected += OnMountDetected;

		try
		{
			_console.WriteLine("Mount watcher started. Listening for camera mounts...");
			_logger.LogInformation("Mount watcher initialized and listening");
			watcher.Start();
		}
		catch ( Exception ex )
		{
			_logger.LogError($"Mount watcher failed: {ex.Message}");
			return false;
		}

		return true;
	}

	/// <summary>
	///     Handle mount detected event
	/// </summary>
	private void OnMountDetected(object? sender, MountDetectedEventArgs e)
	{
		if ( string.IsNullOrWhiteSpace(e.MountPath) )
		{
			return;
		}

		_logger.LogInformation($"Mount detected: {e.MountPath}");

		try
		{
			// Check for camera storage
			if ( !_mountDetector.HasCameraStorage(e.MountPath) )
			{
				if ( _appSettings.IsVerbose() )
				{
					_logger.LogInformation($"No camera storage found on {e.MountPath}");
				}

				return;
			}

			_logger.LogInformation($"Camera storage detected on {e.MountPath}");

			// Get camera storage paths
			var cameraPaths = _mountDetector.GetCameraStoragePaths(e.MountPath).ToList();
			if ( cameraPaths.Count == 0 )
			{
				_logger.LogInformation($"No camera paths found on {e.MountPath}");
				return;
			}

			// Prevent duplicate processing
			foreach ( var cameraPath in cameraPaths )
			{
				if ( !_processedPaths.Add(cameraPath) )
				{
					if ( _appSettings.IsVerbose() )
					{
						_logger.LogInformation($"Camera path already processed: {cameraPath}");
					}

					continue;
				}

				_ = Task.Run(async () => await RunImporter(cameraPath));
			}
		}
		catch ( Exception ex )
		{
			_logger.LogError($"Error handling mount detected event: {ex.Message}");
		}
	}

	/// <summary>
	///     Run the importer for a camera path
	/// </summary>
	private async Task RunImporter(string cameraPath)
	{
		try
		{
			_logger.LogInformation($"Starting import from {cameraPath}");
			_console.WriteLine($"Importing from {cameraPath}");

			var importSettings = new ImportSettingsModel
			{
				RecursiveDirectory = true, IndexMode = true, DeleteAfter = false
			};

			var result = await _importService.Importer(
				new List<string> { cameraPath },
				importSettings);
			_logger.LogInformation($"Import completed for {cameraPath}: " +
			                       $"{result.Count} items processed");
		}
		catch ( Exception ex )
		{
			_logger.LogError($"Import failed for {cameraPath}: {ex.Message}");
		}
		finally
		{
			// Remove from processed paths after delay to allow re-import if needed
			_ = Task.Delay(TimeSpan.FromSeconds(DuplicateCheckWindowSeconds))
				.ContinueWith(_ => _processedPaths.Remove(cameraPath));
		}
	}
}
