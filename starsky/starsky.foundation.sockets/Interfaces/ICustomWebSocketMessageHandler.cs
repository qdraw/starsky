using System;
using System.Net.WebSockets;
using System.Threading.Tasks;
using starsky.foundation.sockets.Models;

namespace starsky.foundation.sockets.Interfaces
{
	public interface ICustomWebSocketMessageHandler
	{
		Task SendInitialMessages(CustomWebSocket userWebSocket);
		Task HandleMessage(WebSocketReceiveResult result, byte[] buffer, CustomWebSocket userWebSocket, ICustomWebSocketFactory wsFactory);
		Task BroadcastOthers(byte[] buffer, CustomWebSocket userWebSocket, ICustomWebSocketFactory wsFactory);

		Task BroadcastAll(object msg, Guid? requestId, ICustomWebSocketFactory wsFactory);
		Task BroadcastAll(byte[] buffer, ICustomWebSocketFactory wsFactory);
	}
}
