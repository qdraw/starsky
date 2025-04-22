using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.JsonConverter;
using starsky.foundation.platform.Models;
using starsky.foundation.realtime.Helpers;
using starsky.foundation.realtime.Interfaces;

namespace starsky.foundation.realtime.Services;

[Service(typeof(IWebSocketConnectionsService), InjectionLifetime = InjectionLifetime.Singleton)]
public sealed class WebSocketConnectionsService : IWebSocketConnectionsService
{
	private readonly ConcurrentDictionary<Guid, WebSocketConnection> _connections = new();
	private readonly IWebLogger _logger;

	public WebSocketConnectionsService(IWebLogger logger)
	{
		_logger = logger;
	}

	public void AddConnection(WebSocketConnection connection)
	{
		_connections.TryAdd(connection.Id, connection);
	}

	public void RemoveConnection(Guid connectionId)
	{
		_connections.TryRemove(connectionId, out _);
	}

	public async Task SendToAllAsync(string message, CancellationToken cancellationToken)
	{
		try
		{
			var connectionsTasks = new List<Task>();
			connectionsTasks.AddRange(_connections.Values.Select(connection =>
				connection.SendAsync(message, cancellationToken)));
			await Task.WhenAll(connectionsTasks);
		}
		catch ( Exception ex )
		{
			_logger.LogInformation(ex, "[SendToAllAsync] Exception during Task.WhenAll " +
			                     "for WebSocket sends");
		}
	}

	public Task SendToAllAsync<T>(ApiNotificationResponseModel<T> message,
		CancellationToken cancellationToken)
	{
		var stringMessage = JsonSerializer.Serialize(message,
			DefaultJsonSerializer.CamelCaseNoEnters);
		return SendToAllAsync(stringMessage, cancellationToken);
	}
}
