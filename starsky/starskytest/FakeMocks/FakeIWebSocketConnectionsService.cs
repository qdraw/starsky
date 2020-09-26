using System;
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

		public Task SendToAllAsync(string message, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}
	}
}
