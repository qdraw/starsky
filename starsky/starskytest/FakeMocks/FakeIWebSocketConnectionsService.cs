using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using starsky.foundation.realtime.Helpers;
using starsky.foundation.realtime.Interfaces;

namespace starskytest.FakeMocks
{
	public class FakeIWebSocketConnectionsService : IWebSocketConnectionsService
	{
		public void AddConnection(WebSocketConnection connection)
		{
			throw new NotImplementedException();
		}

		public void RemoveConnection(Guid connectionId)
		{
			throw new NotImplementedException();
		}

		public List<string> FakeSendToAllAsync { get; set; } = new List<string>();
		
#pragma warning disable 1998
		public async Task SendToAllAsync(string message, CancellationToken cancellationToken)
#pragma warning restore 1998
		{
			FakeSendToAllAsync.Add(message);
		}
	}
}
