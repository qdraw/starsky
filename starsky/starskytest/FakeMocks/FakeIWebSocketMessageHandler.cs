using System;
using System.Net.WebSockets;
using System.Threading.Tasks;
using starsky.foundation.sockets.Interfaces;
using starsky.foundation.sockets.Models;

namespace starskytest.FakeMocks
{
	public class FakeIWebSocketMessageHandler : IWebSocketMessageHandler
	{
		public Task SendInitialMessages(RealtimeWebSocket userWebSocket)
		{
			throw new NotImplementedException();
		}

		public Task HandleMessage(WebSocketReceiveResult result, byte[] buffer, RealtimeWebSocket userWebSocket)
		{
			throw new NotImplementedException();
		}

		public Task BroadcastOthers(byte[] buffer, RealtimeWebSocket userWebSocket,
			IRealtimeWebSocketFactory wsFactory)
		{
			throw new NotImplementedException();
		}

		public Task BroadcastAll(object msg, Guid? requestId, IRealtimeWebSocketFactory wsFactory)
		{
			throw new NotImplementedException();
		}

		public Task BroadcastAll(byte[] buffer, IRealtimeWebSocketFactory wsFactory)
		{
			throw new NotImplementedException();
		}
	}
}
