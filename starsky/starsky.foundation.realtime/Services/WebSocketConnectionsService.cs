using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using starsky.foundation.injection;
using starsky.foundation.realtime.Helpers;
using starsky.foundation.realtime.Interfaces;

namespace starsky.foundation.realtime.Services
{
	[Service(typeof(IWebSocketConnectionsService), InjectionLifetime = InjectionLifetime.Singleton)]
	public class WebSocketConnectionsService : IWebSocketConnectionsService
	{
		#region Fields
		private readonly ConcurrentDictionary<Guid, WebSocketConnection> _connections = new ConcurrentDictionary<Guid, WebSocketConnection>();
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
				connectionsTasks.Add(connection.SendAsync(message, cancellationToken));
			}

			return Task.WhenAll(connectionsTasks);
		}
		#endregion
	}
}
