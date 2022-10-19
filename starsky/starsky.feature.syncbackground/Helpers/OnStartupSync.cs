using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Models;
using starsky.foundation.realtime.Interfaces;
using starsky.foundation.settings.Enums;
using starsky.foundation.settings.Formats;
using starsky.foundation.settings.Interfaces;
using starsky.foundation.sync.SyncInterfaces;

namespace starsky.feature.syncbackground.Helpers;

public class OnStartupSync
{
	private readonly ISynchronize _synchronize;
	private readonly ISettingsService _settingsService;
	private readonly IServiceScopeFactory _serviceScopeFactory;
	private readonly AppSettings _appSettings;

	/// <summary>
	/// 
	/// </summary>
	/// <param name="serviceScopeFactory">req: IRealtimeConnectionsService</param>
	/// <param name="appSettings"></param>
	/// <param name="synchronize"></param>
	/// <param name="settingsService"></param>
	public OnStartupSync(IServiceScopeFactory serviceScopeFactory, AppSettings appSettings,
		ISynchronize synchronize, ISettingsService settingsService)
	{
		_serviceScopeFactory = serviceScopeFactory;
		_appSettings = appSettings;
		_synchronize = synchronize;
		_settingsService = settingsService;
	}

	public async Task StartUpSync()
	{
		if ( _appSettings.SyncOnStartup != true  )
		{
			return;
		}
		
		var lastUpdatedValue = await _settingsService.GetSetting<DateTime>(
			SettingsType.LastSyncBackgroundDateTime);
				
		await _synchronize.Sync("/", PushToSockets, lastUpdatedValue.ToLocalTime());

		await _settingsService.AddOrUpdateSetting(
			SettingsType.LastSyncBackgroundDateTime,
			DateTime.UtcNow.ToString(SettingsFormats.LastSyncBackgroundDateTime, CultureInfo.InvariantCulture));
	}
	
	internal async Task PushToSockets(List<FileIndexItem> updatedList)
	{
		using var scope = _serviceScopeFactory.CreateScope();
		var webSocketConnectionsService = scope.ServiceProvider.GetRequiredService<IWebSocketConnectionsService>();
		var notificationQuery = scope.ServiceProvider.GetRequiredService<INotificationQuery>();
		var webSocketResponse =
			new ApiNotificationResponseModel<List<FileIndexItem>>(updatedList, ApiNotificationType.OnStartupSyncBackgroundSync);

		await webSocketConnectionsService.SendToAllAsync(webSocketResponse, CancellationToken.None);
		await notificationQuery.AddNotification(webSocketResponse);
	}	

}
