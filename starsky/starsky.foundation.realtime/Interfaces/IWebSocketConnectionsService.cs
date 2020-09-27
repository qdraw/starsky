using System;
using System.Threading;
using System.Threading.Tasks;
using starsky.foundation.realtime.Helpers;

namespace starsky.foundation.realtime.Interfaces
{
	public interface IWebSocketConnectionsService
	{
		void AddConnection(WebSocketConnection connection);

		void RemoveConnection(Guid connectionId);

		Task SendToAllAsync(string message, CancellationToken cancellationToken);
	}
}
