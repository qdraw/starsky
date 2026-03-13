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
public class TrashConnectionService(
	IWebSocketConnectionsService webSocketConnectionsService,
	INotificationQuery notificationQuery)
	: ITrashConnectionService
{
	public static List<FileIndexItem> StatusUpdate(
		List<FileIndexItem> moveToTrash,
		bool isSystemTrash)
	{
		var status = isSystemTrash
			? FileIndexItem.ExifStatus.NotFoundSourceMissing
			: FileIndexItem.ExifStatus.Deleted;

		foreach ( var item in moveToTrash )
		{
			item.Status = status;
		}

		return moveToTrash;
	}

	public async Task<List<FileIndexItem>> ConnectionServiceAsync(List<FileIndexItem> moveToTrash,
		bool isSystemTrash)
	{
		moveToTrash = StatusUpdate(moveToTrash, isSystemTrash);

		var webSocketResponse = new ApiNotificationResponseModel<List<FileIndexItem>>(
			moveToTrash, ApiNotificationType.MoveToTrash);
		await webSocketConnectionsService.SendToAllAsync(webSocketResponse, CancellationToken.None);
		await notificationQuery.AddNotification(webSocketResponse);
		return moveToTrash;
	}
}
