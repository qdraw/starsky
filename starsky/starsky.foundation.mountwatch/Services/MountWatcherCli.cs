using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using starsky.foundation.geo.GeoDownload.Interfaces;
using starsky.foundation.import.Interfaces;
using starsky.foundation.import.Services;
using starsky.foundation.mountwatch.Interfaces;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.writemeta.Interfaces;

namespace starsky.foundation.mountwatch.Services;

/// <summary>
///     CLI service for mount watching and automatic importing
/// </summary>
public class MountWatcherCli
{
	private const int DuplicateCheckWindowSeconds = 60;
	private readonly AppSettings _appSettings;
	private readonly ICameraStorageDetector _cameraStorageDetector;
	private readonly IConsole _console;
	private readonly IExifToolDownload _exifToolDownload;
	private readonly IGeoFileDownload _geoFileDownload;
	private readonly IImport _importService;
	private readonly IWebLogger _logger;
	private readonly IMountDetector _mountDetector;
	private readonly IMountWatcherFactory _mountWatcherFactory;
	private readonly HashSet<string> _processedPaths = new();

	public MountWatcherCli(
		IImport importService,
		AppSettings appSettings,
		IConsole console,
		IWebLogger logger,
		IExifToolDownload exifToolDownload,
		IGeoFileDownload geoFileDownload,
		IMountDetector mountDetector,
		IMountWatcherFactory mountWatcherFactory,
		ICameraStorageDetector cameraStorageDetector)
	{
		_importService = importService;
		_appSettings = appSettings;
		_console = console;
		_logger = logger;
		_exifToolDownload = exifToolDownload;
		_geoFileDownload = geoFileDownload;
		_mountDetector = mountDetector;
		_mountWatcherFactory = mountWatcherFactory;
		_cameraStorageDetector = cameraStorageDetector;
	}

	/// <summary>
	///     Start the mount watcher and listen for camera mounts
	/// </summary>
	public async Task<bool> StartWatcher(string[] args)
	{
		_logger.LogInformation("Starting mount watcher service");

		_appSettings.Verbose = ArgsHelper.NeedVerbose(args);
		_appSettings.ApplicationType = AppSettings.StarskyAppType.MountWatcher;

		try
		{
			await _exifToolDownload.DownloadExifTool(_appSettings.IsWindows);
			await _geoFileDownload.DownloadAsync();
		}
		catch ( Exception ex )
		{
			_logger.LogError($"Failed to download dependencies: {ex.Message}");
			return false;
		}

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

			var importArgs = new[] { cameraPath, "--recursive", "--verbose" };

			var importCli = new ImportCli(
				_importService,
				_appSettings,
				_console,
				_logger,
				_exifToolDownload,
				_geoFileDownload,
				_cameraStorageDetector);

			var result = await importCli.Importer(importArgs);
			_logger.LogInformation($"Import completed for {cameraPath}: {result}");
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
