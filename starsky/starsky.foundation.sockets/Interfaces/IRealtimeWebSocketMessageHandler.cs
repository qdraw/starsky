using System;
using System.Net.WebSockets;
using System.Threading.Tasks;
using starsky.foundation.sockets.Models;

namespace starsky.foundation.sockets.Interfaces
{
	public interface IRealtimeWebSocketMessageHandler
	{
		Task SendInitialMessages(RealtimeWebSocket userWebSocket);
		Task HandleMessage(WebSocketReceiveResult result, byte[] buffer, RealtimeWebSocket userWebSocket, IRealtimeWebSocketFactory wsFactory);
		Task BroadcastOthers(byte[] buffer, RealtimeWebSocket userWebSocket, IRealtimeWebSocketFactory wsFactory);

		Task BroadcastAll(object msg, Guid? requestId, IRealtimeWebSocketFactory wsFactory);
		Task BroadcastAll(byte[] buffer, IRealtimeWebSocketFactory wsFactory);
	}
}
