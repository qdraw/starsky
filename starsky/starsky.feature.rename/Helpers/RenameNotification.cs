using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Models;
using starsky.foundation.realtime.Interfaces;

namespace starsky.feature.rename.Helpers;

/// <summary>
///     Tell the other clients (websocket and notification list) that files are renamed
/// </summary>
public static class RenameNotification
{
	public static async Task SendAsync(List<FileIndexItem> renamed,
		INotificationQuery notificationQuery, IWebSocketConnectionsService connectionsService)
	{
		var webSocketResponse =
			new ApiNotificationResponseModel<List<FileIndexItem>>(renamed,
				ApiNotificationType.Rename);

		await notificationQuery.AddNotification(webSocketResponse);
		await connectionsService.SendToAllAsync(webSocketResponse, CancellationToken.None);
	}
}
