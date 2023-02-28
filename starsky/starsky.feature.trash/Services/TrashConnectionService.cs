using starsky.feature.trash.Interfaces;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.injection;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Models;
using starsky.foundation.realtime.Interfaces;

namespace starsky.feature.trash.Services;

[Service(typeof(ITrashConnectionService), 
	InjectionLifetime = InjectionLifetime.Scoped)]
public class TrashConnectionService : ITrashConnectionService
{
	private readonly IWebSocketConnectionsService _webSocketConnectionsService;
	private readonly INotificationQuery _notificationQuery;
	
	public TrashConnectionService(IWebSocketConnectionsService webSocketConnectionsService, 
		INotificationQuery notificationQuery)
	{
		_webSocketConnectionsService = webSocketConnectionsService;
		_notificationQuery = notificationQuery;
	}
	
	public async Task<List<FileIndexItem>> ConnectionServiceAsync( List<FileIndexItem> moveToTrash, 
		bool isSystemTrash)
	{
		var status = isSystemTrash
			? FileIndexItem.ExifStatus.NotFoundSourceMissing
			: FileIndexItem.ExifStatus.Deleted;
		
		foreach ( var item in moveToTrash )
		{
			item.Status = status;
		}
		
		var webSocketResponse = new ApiNotificationResponseModel<List<FileIndexItem>>(
			moveToTrash,ApiNotificationType.MoveToTrash);
		await _webSocketConnectionsService.SendToAllAsync(webSocketResponse, CancellationToken.None);
		await _notificationQuery.AddNotification(webSocketResponse);
		return moveToTrash;
	}
}


