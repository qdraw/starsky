using System;
using System.Threading;
using System.Threading.Tasks;
using starsky.feature.realtime.Interface;
using starsky.foundation.database.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.realtime.Interfaces;

namespace starsky.feature.realtime.Services
{
	public class RealtimeConnectionsService : IRealtimeConnectionsService
	{
		private readonly IWebSocketConnectionsService _webSocketConnectionsService;
		private readonly INotificationQuery _notificationQuery;

		public RealtimeConnectionsService(IWebSocketConnectionsService webSocketConnectionsService, INotificationQuery notificationQuery)
		{
			_webSocketConnectionsService = webSocketConnectionsService;
			_notificationQuery = notificationQuery;
		}

		public async Task NotificationToAllAsync<T>(ApiNotificationResponseModel<T> message,
			CancellationToken cancellationToken)
		{
			await _webSocketConnectionsService.SendToAllAsync(message, cancellationToken);
			await _notificationQuery.AddNotification(message);
		}

		public async Task CleanOldMessagesAsync()
		{
			await _notificationQuery.GetOlderThan(DateTime.UtcNow.AddDays(-30));
		}
	}
}

