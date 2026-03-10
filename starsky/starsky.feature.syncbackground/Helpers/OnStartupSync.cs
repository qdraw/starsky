using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.realtime.Interfaces;
using starsky.foundation.settings.Enums;
using starsky.foundation.settings.Formats;
using starsky.foundation.settings.Interfaces;
using starsky.foundation.sync.SyncInterfaces;
using starsky.foundation.sync.WatcherBackgroundService;
using starsky.foundation.worker.Helpers;
using starsky.foundation.worker.Models;

namespace starsky.feature.syncbackground.Helpers;

public class OnStartupSync
{
	public const string JobType = "Sync.OnStartup.v1";
	private readonly AppSettings _appSettings;
	private readonly IDiskWatcherBackgroundTaskQueue _backgroundTaskQueue;
	private readonly IWebLogger _logger;
	private readonly IServiceScopeFactory _serviceScopeFactory;
	private readonly ISettingsService _settingsService;
	private readonly ISynchronize _synchronize;

	/// <summary>
	/// </summary>
	/// <param name="serviceScopeFactory">req: IRealtimeConnectionsService</param>
	/// <param name="backgroundTaskQueue"></param>
	/// <param name="appSettings"></param>
	/// <param name="synchronize"></param>
	/// <param name="settingsService"></param>
	/// <param name="logger"></param>
	public OnStartupSync(IServiceScopeFactory serviceScopeFactory,
		IDiskWatcherBackgroundTaskQueue backgroundTaskQueue, AppSettings appSettings,
		ISynchronize synchronize, ISettingsService settingsService, IWebLogger logger)
	{
		_serviceScopeFactory = serviceScopeFactory;
		_backgroundTaskQueue = backgroundTaskQueue;
		_appSettings = appSettings;
		_synchronize = synchronize;
		_settingsService = settingsService;
		_logger = logger;
	}

	public async Task StartUpSync()
	{
		await _backgroundTaskQueue.QueueJobAsync(new BackgroundTaskQueueJob
		{
			MetaData = nameof(StartUpSync),
			TraceParentId = null,
			PriorityLane = ProcessTaskQueue.PriorityLaneDiskWatcher,
			QueueName = nameof(IDiskWatcherBackgroundTaskQueue),
			JobType = JobType,
			PayloadJson = JsonSerializer.Serialize(new OnStartupSyncPayload())
		});
	}

	public async Task StartUpSyncTask()
	{
		if ( _appSettings.SyncOnStartup != true )
		{
			return;
		}

		var lastUpdatedValue = await _settingsService.GetSetting<DateTime>(
			SettingsType.LastSyncBackgroundDateTime);

		await _synchronize.Sync("/", PushToSockets, lastUpdatedValue.ToLocalTime());

		await _settingsService.AddOrUpdateSetting(
			SettingsType.LastSyncBackgroundDateTime,
			DateTime.UtcNow.ToDefaultSettingsFormat());

		_logger.LogInformation("Sync on startup done");
	}

	internal async Task PushToSockets(List<FileIndexItem> updatedList)
	{
		using var scope = _serviceScopeFactory.CreateScope();
		var webSocketConnectionsService =
			scope.ServiceProvider.GetRequiredService<IWebSocketConnectionsService>();
		var notificationQuery = scope.ServiceProvider.GetRequiredService<INotificationQuery>();
		var webSocketResponse =
			new ApiNotificationResponseModel<List<FileIndexItem>>(updatedList,
				ApiNotificationType.OnStartupSyncBackgroundSync);

		await webSocketConnectionsService.SendToAllAsync(webSocketResponse, CancellationToken.None);
		await notificationQuery.AddNotification(webSocketResponse);
	}
}

public sealed class OnStartupSyncPayload
{
}
