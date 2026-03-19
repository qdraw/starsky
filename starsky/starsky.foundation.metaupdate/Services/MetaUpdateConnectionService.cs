using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.injection;
using starsky.foundation.metaupdate.Interfaces;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Models;
using starsky.foundation.realtime.Interfaces;

namespace starsky.foundation.metaupdate.Services;

[Service(typeof(IMetaUpdateConnectionService), InjectionLifetime = InjectionLifetime.Scoped)]
public class MetaUpdateConnectionService : IMetaUpdateConnectionService
{
	private readonly IWebSocketConnectionsService _connectionsService;
	private readonly INotificationQuery _notificationQuery;

	public MetaUpdateConnectionService(IServiceScopeFactory scopeFactory)
	{
		_connectionsService = scopeFactory.CreateScope()
			.ServiceProvider.GetRequiredService<IWebSocketConnectionsService>();

		_notificationQuery = scopeFactory.CreateScope()
			.ServiceProvider.GetRequiredService<INotificationQuery>();
	}

	internal MetaUpdateConnectionService(IWebSocketConnectionsService connectionsService,
		INotificationQuery notificationQuery)
	{
		_connectionsService = connectionsService;
		_notificationQuery = notificationQuery;
	}

	public async Task<ApiNotificationResponseModel<List<FileIndexItem>>> UpdateWebSocketTaskRun(
		List<FileIndexItem> fileIndexResultsList)
	{
		var webSocketResponse =
			new ApiNotificationResponseModel<List<FileIndexItem>>(fileIndexResultsList,
				ApiNotificationType.MetaUpdate);

		await _connectionsService.SendToAllAsync(webSocketResponse,
			CancellationToken.None);

		await _notificationQuery.AddNotification(webSocketResponse);
		return webSocketResponse;
	}
}
