using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using starsky.feature.syncbackground.Models;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.injection;
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

/// <summary>
/// </summary>
/// <param name="serviceScopeFactory">req: IRealtimeConnectionsService</param>
/// <param name="backgroundTaskQueue"></param>
/// <param name="appSettings"></param>
/// <param name="synchronize"></param>
/// <param name="settingsService"></param>
/// <param name="logger"></param>
[Service(typeof(IOnStartupSync), InjectionLifetime = InjectionLifetime.Scoped)]
public class OnStartupSync(
	IServiceScopeFactory serviceScopeFactory,
	IDiskWatcherBackgroundTaskQueue backgroundTaskQueue,
	AppSettings appSettings,
	ISynchronize synchronize,
	ISettingsService settingsService,
	IWebLogger logger) : IOnStartupSync
{
	public const string JobType = "Sync.OnStartup.v1";

	public async Task StartUpSyncTask()
	{
		if ( appSettings.SyncOnStartup != true )
		{
			return;
		}

		var lastUpdatedValue = await settingsService.GetSetting<DateTime>(
			SettingsType.LastSyncBackgroundDateTime);

		await synchronize.Sync("/", PushToSockets, lastUpdatedValue.ToLocalTime());

		await settingsService.AddOrUpdateSetting(
			SettingsType.LastSyncBackgroundDateTime,
			DateTime.UtcNow.ToDefaultSettingsFormat());

		logger.LogInformation("Sync on startup done");
	}

	public async Task StartUpSync()
	{
		await backgroundTaskQueue.QueueJobAsync(new BackgroundTaskQueueJob
		{
			TraceParentId = Activity.Current?.Id,
			PriorityLane = ProcessTaskQueue.PriorityLaneDiskWatcher,
			JobType = JobType,
			PayloadJson = JsonSerializer.Serialize(new OnStartupSyncPayload())
		});
	}

	internal async Task PushToSockets(List<FileIndexItem> updatedList)
	{
		using var scope = serviceScopeFactory.CreateScope();
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

public interface IOnStartupSync
{
	Task StartUpSyncTask();
}
