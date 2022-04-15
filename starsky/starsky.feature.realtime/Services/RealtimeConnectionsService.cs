using System;
using System.Threading;
using System.Threading.Tasks;
using starsky.feature.realtime.Interface;
using starsky.foundation.database.Interfaces;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.realtime.Interfaces;

namespace starsky.feature.realtime.Services
{
	[Service(typeof(IRealtimeConnectionsService), InjectionLifetime = InjectionLifetime.Scoped)]
	public class RealtimeConnectionsService : IRealtimeConnectionsService
	{
		private readonly IWebSocketConnectionsService _webSocketConnectionsService;
		private readonly INotificationQuery _notificationQuery;
		private readonly IWebLogger _logger;

		public RealtimeConnectionsService(IWebSocketConnectionsService webSocketConnectionsService, INotificationQuery notificationQuery, IWebLogger logger)
		{
			_webSocketConnectionsService = webSocketConnectionsService;
			_notificationQuery = notificationQuery;
			_logger = logger;
		}

		public async Task NotificationToAllAsync<T>(ApiNotificationResponseModel<T> message,
			CancellationToken cancellationToken)
		{
			await _webSocketConnectionsService.SendToAllAsync(message, cancellationToken);
			await _notificationQuery.AddNotification(message);
		}

		public async Task CleanOldMessagesAsync()
		{
			try
			{
				var messages = await _notificationQuery.GetOlderThan(DateTime.UtcNow.AddDays(-30));
				await _notificationQuery.RemoveAsync(messages);
			}
			catch ( Exception e )
			{
				//_logger.LogError(e, "RealtimeConnectionsService CleanOldMessagesAsync");
			}

		}
	}
}

