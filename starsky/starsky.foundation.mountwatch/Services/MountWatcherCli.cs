using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using starsky.foundation.import.Interfaces;
using starsky.foundation.import.Models;
using starsky.foundation.mountwatch.MountWatcher.Interfaces;
using starsky.foundation.mountwatch.ServiceInstaller;
using starsky.foundation.mountwatch.ServiceInstaller.Helpers;
using starsky.foundation.mountwatch.ServiceInstaller.Interfaces;
using starsky.foundation.platform.Architecture;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;

namespace starsky.foundation.mountwatch.Services;

/// <summary>
///     CLI service for mount watching and automatic importing
/// </summary>
public class MountWatcherCli
{
	private const string InstallArg = "--install";
	private const string UninstallArg = "--uninstall";
    private const string StatusArg = "--status";


	private readonly AppSettings _appSettings;
	private readonly IConsole _console;
	private readonly IImport _importService;
	private readonly IWebLogger _logger;
	private readonly IMountWatcherFactory _mountWatcherFactory;

	private readonly Func<OSPlatform> _platformResolver =
		OperatingSystemHelper.GetPlatform;

	private readonly IServiceInstaller _serviceInstaller;
	private readonly ICameraStorageDetector _storageDetector;

	public MountWatcherCli(
		IImport importService,
		AppSettings appSettings,
		IConsole console,
		IWebLogger logger,
		ICameraStorageDetector storageDetector,
		IMountWatcherFactory mountWatcherFactory,
		IServiceInstaller serviceInstaller)
	{
		_importService = importService;
		_appSettings = appSettings;
		_console = console;
		_logger = logger;
		_storageDetector = storageDetector;
		_mountWatcherFactory = mountWatcherFactory;
		_serviceInstaller = serviceInstaller;
	}

	// Constructor with platform resolver for testing
	public MountWatcherCli(
		IImport importService,
		AppSettings appSettings,
		IConsole console,
		IWebLogger logger,
		ICameraStorageDetector storageDetector,
		IMountWatcherFactory mountWatcherFactory,
		IServiceInstaller serviceInstaller,
		Func<OSPlatform>? platformResolver)
		: this(importService, appSettings, console, logger, storageDetector, mountWatcherFactory,
			serviceInstaller)
	{
		if ( platformResolver != null )
		{
			_platformResolver = platformResolver;
		}
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

		if ( NeedStatus(args) )
		{
			var (installed, running) = await _serviceInstaller.StatusAsync();
			_console.WriteLine($"Service installed: {installed}");
			_console.WriteLine($"Service running: {running}");
			_logger.LogInformation($"Service status - installed: {installed}, running: {running}");
			return true;
		}

		_serviceInstaller.PreflightChecks();

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
	/// Returns true if --status is present in args
	/// </summary>
	public static bool NeedStatus(string[] args)
	{
		return args.Any(a => a.Equals(StatusArg, StringComparison.OrdinalIgnoreCase));
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

		_console.WriteLine($" Storage: {_appSettings.StorageFolder}");

		if ( _platformResolver() == OSPlatform.OSX )
		{
			_console.WriteLine($"  macOS plist: {MacOsServiceInstaller.GetMacOsPlistPath()}");
			_console.WriteLine("  Load: launchctl load <plist>");
			_console.WriteLine("  Requires: Full Disk Access in System Preferences");
			_console.WriteLine("  To grant Full Disk Access:");
			_console.WriteLine($"    open {MacOsFullDiskAccess.MacOsPrivacySettingsUri}");
			_console.WriteLine("  Logs: ~/Library/Logs/starsky/mountwatcher.log");
		}
		else if ( _platformResolver() == OSPlatform.Linux )
		{
			_console.WriteLine(
				$"  systemd: /etc/systemd/system/{new WatchServiceName().GetSystemDName()}.service");
			_console.WriteLine(
				$"  Enable: sudo systemctl enable {new WatchServiceName().GetSystemDName()}");
			_console.WriteLine(
				$"  Start:  sudo systemctl start {new WatchServiceName().GetSystemDName()}");
			_console.WriteLine($"  Logs: {WatchServiceName.GetLinuxLogHint()}");
		}
		else if ( _platformResolver() == OSPlatform.Windows )
		{
			_console.WriteLine(
				$"  Windows Service: sc create \"{new WatchServiceName().GetReverseDnsName()}\" ...");
			_console.WriteLine($"  Start: sc start {new WatchServiceName().GetReverseDnsName()}");
			_console.WriteLine("  Logs: Windows Event Viewer -> Windows Logs -> Application");
			_console.WriteLine($"        Source: {new WatchServiceName().GetReverseDnsName()}");
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
	internal void OnMountDetected(object? sender, MountDetectedEventArgs eventArgs)
	{
		if ( string.IsNullOrWhiteSpace(eventArgs.MountPath) )
		{
			return;
		}

		var mountPath = NormalizeMountPath(eventArgs.MountPath);
		_logger.LogInformation($"Mount detected: {mountPath}");

		_ = Task.Run(async () => await HandleMountDetectedAsync(mountPath));
	}

	internal static string NormalizeMountPath(string mountPath)
	{
		var normalized = mountPath.Trim();
		if ( normalized.Length <= 1 )
		{
			return normalized;
		}

		return normalized.TrimEnd('/');
	}

	private async Task HandleMountDetectedAsync(string mountPath)
	{
		try
		{
			if ( !_storageDetector.IsCameraStorage(mountPath) )
			{
				_logger.LogDebug($"No camera storage found on {mountPath}");
				return;
			}

			_logger.LogDebug($"Run import on {mountPath}");
			await RunImporter(mountPath);
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
				RecursiveDirectory = true,
				IndexMode = true, 
				DeleteAfter = _appSettings.ImportMountWatcher.DeleteAfter
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
	}
}
