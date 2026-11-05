using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Tenancy;
using starsky.foundation.sync.WatcherInterfaces;

namespace starsky.foundation.sync.WatcherServices;

/// <summary>
///     Run DiskWatcher as singleton (once) in the background of the app
/// </summary>
[Service(typeof(IHostedService), InjectionLifetime = InjectionLifetime.Singleton)]
public sealed class DiskWatcherBackgroundService : BackgroundService
{
	private readonly AppSettings _appSettings;
	private readonly IDiskWatcher _diskWatcher;
	private readonly IWebLogger _logger;
	private readonly ITenantRegistry? _tenantRegistry;
	private readonly ITenantSettingsProvider? _tenantSettingsProvider;

	public DiskWatcherBackgroundService(IDiskWatcher diskWatcher, AppSettings appSettings,
		IWebLogger logger, ITenantRegistry? tenantRegistry = null,
		ITenantSettingsProvider? tenantSettingsProvider = null)
	{
		_tenantRegistry = tenantRegistry;
		_tenantSettingsProvider = tenantSettingsProvider;
		_diskWatcher = diskWatcher;
		_appSettings = appSettings;
		_logger = logger;
	}

	/// <summary>
	///     This method is triggered by BackgroundService
	/// </summary>
	/// <param name="stoppingToken">ignored but required</param>
	/// <returns>Task/nothing</returns>
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		_logger.LogInformation(( _appSettings.UseDiskWatcher != false
				? "UseDiskWatcher is enabled"
				: "UseDiskWatcher is disabled" ) + $" on {Environment.MachineName}");
		if ( _appSettings.ApplicationType != AppSettings.StarskyAppType.WebController
		     || _appSettings.UseDiskWatcher == false )
		{
			return;
		}

		StartWatchers();

		if ( _tenantRegistry == null || _tenantSettingsProvider == null )
		{
			return;
		}

		// tenants can be created while the app runs: start the watchers of new tenants
		using var timer = new PeriodicTimer(ReconcileInterval);
		try
		{
			while ( await timer.WaitForNextTickAsync(stoppingToken) )
			{
				StartWatchers();
			}
		}
		catch ( OperationCanceledException )
		{
			// the app is stopping
		}
	}

	internal TimeSpan ReconcileInterval { get; set; } = TimeSpan.FromSeconds(30);

	/// <summary>
	///     Each tenant is watched in its own folder, and its changes are synced as that tenant.
	///     A folder is only watched once, so this can be repeated
	/// </summary>
	internal void StartWatchers()
	{
		// the default tenant first: it is the primary watcher (with retry)
		_diskWatcher.Watcher(_appSettings.StorageFolder);

		foreach ( var physicalPath in _appSettings.StorageFolderMappings.Values )
		{
			_diskWatcher.Watcher(physicalPath);
		}

		if ( _tenantRegistry == null || _tenantSettingsProvider == null )
		{
			return;
		}

		foreach ( var tenantId in _tenantRegistry.ActiveTenantIds
			         .Where(p => p != TenantIdHelper.DefaultTenantId) )
		{
			var settings = _tenantSettingsProvider.GetSettings(tenantId);
			_diskWatcher.Watcher(settings.StorageFolder, tenantId);
			foreach ( var physicalPath in settings.StorageFolderMappings.Values )
			{
				_diskWatcher.Watcher(physicalPath, tenantId);
			}
		}
	}
}
