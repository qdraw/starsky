using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.injection;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.realtime.Interfaces;
using starsky.foundation.sync.SyncInterfaces;

namespace starsky.foundation.sync.SyncServices;

[Service(typeof(ISocketSyncUpdateService), InjectionLifetime = InjectionLifetime.Scoped)]
public class SocketSyncUpdateService : ISocketSyncUpdateService
{
	private readonly IWebSocketConnectionsService _connectionsService;
	private readonly INotificationQuery _notificationQuery;
	private readonly IWebLogger _logger;

	public SocketSyncUpdateService(IWebSocketConnectionsService connectionsService, INotificationQuery notificationQuery, IWebLogger logger)
	{
		_connectionsService = connectionsService;
		_notificationQuery = notificationQuery;
		_logger = logger;
	}

	/// <summary>
	/// Used by manual sync
	/// </summary>
	/// <param name="changedFiles"></param>
	public async Task PushToSockets(List<FileIndexItem> changedFiles)
	{
		var webSocketResponse =
			new ApiNotificationResponseModel<List<FileIndexItem>>(FilterBefore(changedFiles), ApiNotificationType.ManualBackgroundSync);
		await _notificationQuery.AddNotification(webSocketResponse);

		try
		{
			await _connectionsService.SendToAllAsync(webSocketResponse, CancellationToken.None);
		}
		catch ( WebSocketException exception )
		{
			// The WebSocket is in an invalid state: 'Aborted' when the client disconnects
			_logger.LogError("[ManualBackgroundSyncService] catch-ed WebSocketException: " + exception.Message, exception);
		}
	}

	internal static List<FileIndexItem> FilterBefore(IEnumerable<FileIndexItem> syncData)
	{
		return syncData.Where(p => p.FilePath != "/").ToList();
	}
}
