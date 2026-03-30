using System;
using System.Linq;
using System.Threading.Tasks;
using starsky.foundation.database.Models;
using starsky.foundation.import.Interfaces;
using starsky.foundation.import.Models;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;

namespace starsky.foundation.import.Services;

[Service(typeof(ICameraMountWatcherService), InjectionLifetime = InjectionLifetime.Singleton)]
public class CameraMountWatcherService(IMountEventSource mountEventSource,
	ICameraStorageDetector cameraStorageDetector,
	IImporterCliRunner importerCliRunner,
	IImport importService,
	IMountWatcherLogWriter logWriter,
	IWebLogger logger,
	AppSettings appSettings) : ICameraMountWatcherService
{
	private const int DefaultDebounceSeconds = 15;
	private readonly object _sync = new();
	private DateTime _lastImportUtc = DateTime.MinValue;
	private DateTime? _lastImportDoneUtc;
	private int _totalImportsTriggered;
	private string? _lastResult;
	private string? _lastUsedProcessModel;
	private bool _subscribed;

	public MountWatcherStatusModel GetStatus()
	{
		lock ( _sync )
		{
			return new MountWatcherStatusModel
			{
				Enabled = appSettings.MountWatcherEnabled,
				Running = mountEventSource.IsRunning,
				LastImportUtc = _lastImportDoneUtc,
				TotalImportsTriggered = _totalImportsTriggered,
				LastResult = _lastResult,
				ConfiguredProcessModel = appSettings.MountWatcherProcessModel,
				LastUsedProcessModel = _lastUsedProcessModel
			};
		}
	}

	public async Task<MountWatcherStatusModel> StartAsync()
	{
		if ( !appSettings.MountWatcherEnabled )
		{
			_lastResult = "Mount watcher disabled in app settings";
			await logWriter.WriteAsync("watcher-start-skipped", new { reason = _lastResult });
			return GetStatus();
		}

		if ( mountEventSource.IsRunning )
		{
			EnsureSubscribed();
			_lastResult = "already-running";
			await logWriter.WriteAsync("watcher-start-skipped", new { reason = _lastResult });
			return GetStatus();
		}

		if ( !mountEventSource.Start() )
		{
			_lastResult = "Mount watcher not available on this OS";
			await logWriter.WriteAsync("watcher-start-failed", new { reason = _lastResult });
			return GetStatus();
		}

		EnsureSubscribed();
		_lastResult = "running";
		await logWriter.WriteAsync("watcher-started", new { });
		return GetStatus();
	}

	public async Task<MountWatcherStatusModel> StopAsync()
	{
		if ( !mountEventSource.IsRunning )
		{
			_lastResult = "already-stopped";
			await logWriter.WriteAsync("watcher-stop-skipped", new { reason = _lastResult });
			return GetStatus();
		}

		mountEventSource.Stop();
		_lastResult = "stopped";
		await logWriter.WriteAsync("watcher-stopped", new { });
		return GetStatus();
	}

	private void EnsureSubscribed()
	{
		lock ( _sync )
		{
			if ( _subscribed )
			{
				return;
			}

			mountEventSource.MountAppeared += OnMountAppeared;
			_subscribed = true;
		}
	}

	private void OnMountAppeared(MountAppearedEventModel eventModel)
	{
		_ = HandleMountAppearedAsync(eventModel);
	}

	private async Task HandleMountAppearedAsync(MountAppearedEventModel eventModel)
	{
		try
		{
			var debounceSeconds = DefaultDebounceSeconds;
			if ( DateTime.UtcNow.Subtract(_lastImportUtc).TotalSeconds < debounceSeconds )
			{
				await logWriter.WriteAsync("mount-ignored", new { reason = "debounce" });
				return;
			}

			var cameraRoots = cameraStorageDetector.FindCameraStorages().ToList();
			if ( !string.IsNullOrWhiteSpace(eventModel.MountRootPath) )
			{
				cameraRoots = cameraRoots
					.Where(path => path.Equals(eventModel.MountRootPath,
						StringComparison.OrdinalIgnoreCase))
					.ToList();
			}

			if ( cameraRoots.Count == 0 )
			{
				await logWriter.WriteAsync("mount-ignored", new { reason = "no-dcim" });
				return;
			}

			_lastImportUtc = DateTime.UtcNow;
			await logWriter.WriteAsync("camera-detected", new { roots = cameraRoots });

			var result = await ExecuteImportAsync(cameraRoots);
			_lastImportDoneUtc = DateTime.UtcNow;
			_lastResult = result.Message;
			_lastUsedProcessModel = result.Model;
			_totalImportsTriggered++;

			await logWriter.WriteAsync("importer-finished", new
			{
				result.Success,
				result.ExitCode,
				result.Model,
				result.Message
			});
		}
		catch ( Exception ex )
		{
			logger.LogError(ex, "[CameraMountWatcherService] mount handling failed");
			_lastResult = ex.Message;
			await logWriter.WriteAsync("importer-failed", new { exception = ex.Message });
		}
	}

	private async Task<(bool Success, int ExitCode, string Message, string Model)> ExecuteImportAsync(
		System.Collections.Generic.IReadOnlyList<string> cameraRoots)
	{
		return appSettings.MountWatcherProcessModel switch
		{
			AppSettings.MountWatcherImportProcessModel.Cli =>
				await ExecuteCliAsync(),
			AppSettings.MountWatcherImportProcessModel.InProcess =>
				await ExecuteInProcessAsync(cameraRoots),
			_ => await ExecuteAutoAsync(cameraRoots)
		};
	}

	private async Task<(bool Success, int ExitCode, string Message, string Model)> ExecuteAutoAsync(
		System.Collections.Generic.IReadOnlyList<string> cameraRoots)
	{
		var inProcess = await ExecuteInProcessAsync(cameraRoots);
		if ( inProcess.Success )
		{
			return inProcess;
		}

		await logWriter.WriteAsync("importer-fallback", new { from = "in-process", to = "cli" });
		return await ExecuteCliAsync();
	}

	private async Task<(bool Success, int ExitCode, string Message, string Model)> ExecuteCliAsync()
	{
		var result = await importerCliRunner.RunCameraImportAsync();
		return (result.Success, result.ExitCode, result.Message, "cli");
	}

	private async Task<(bool Success, int ExitCode, string Message, string Model)> ExecuteInProcessAsync(
		System.Collections.Generic.IReadOnlyList<string> cameraRoots)
	{
		try
		{
			var importSettings = new ImportSettingsModel
			{
				DeleteAfter = true,
				RecursiveDirectory = true,
				IndexMode = true,
				Origin = "mount-watcher"
			};
			var result = await importService.Importer(cameraRoots, importSettings);
			var success = result.Any(item => item.Status == ImportStatus.Ok ||
			                                 item.Status == ImportStatus.IgnoredAlreadyImported);
			return (success, success ? 0 : 1, success ? "ok" : "in-process import failed",
				"in-process");
		}
		catch ( Exception ex )
		{
			logger.LogError(ex, "[CameraMountWatcherService] in-process import failed");
			return (false, 1, ex.Message, "in-process");
		}
	}
}








