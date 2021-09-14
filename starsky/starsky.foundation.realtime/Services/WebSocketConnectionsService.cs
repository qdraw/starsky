using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.realtime.Helpers;
using starsky.foundation.realtime.Interfaces;

namespace starsky.foundation.realtime.Services
{
	[Service(typeof(IWebSocketConnectionsService), InjectionLifetime = InjectionLifetime.Singleton)]
	public class WebSocketConnectionsService : IWebSocketConnectionsService
	{
		public WebSocketConnectionsService(IWebLogger logger)
		{
			_logger = logger;
		}
		
		#region Fields
		private readonly ConcurrentDictionary<Guid, WebSocketConnection> _connections = new ConcurrentDictionary<Guid, WebSocketConnection>();
		private readonly IWebLogger _logger;

		#endregion

		#region Methods
		public void AddConnection(WebSocketConnection connection)
		{
			_connections.TryAdd(connection.Id, connection);
		}

		public void RemoveConnection(Guid connectionId)
		{
			_connections.TryRemove(connectionId, out _);
		}

		public Task SendToAllAsync(string message, CancellationToken cancellationToken)
		{
			List<Task> connectionsTasks = new List<Task>();
			foreach (WebSocketConnection connection in _connections.Values)
			{
				try
				{
					connectionsTasks.Add(connection.SendAsync(message, cancellationToken));
				}
				catch ( WebSocketException exception)
				{
					// if the client is closing the socket the wrong way
					_logger.LogInformation(exception, "catch-ed exception socket");
				}
			}

			return Task.WhenAll(connectionsTasks);
		}
		#endregion
	}
}
