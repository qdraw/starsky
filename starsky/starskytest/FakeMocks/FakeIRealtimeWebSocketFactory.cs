using System.Collections.Generic;
using starsky.foundation.sockets.Interfaces;
using starsky.foundation.sockets.Models;

namespace starskytest.FakeMocks
{
	public class FakeIRealtimeWebSocketFactory : IRealtimeWebSocketFactory
	{
		public void Add(RealtimeWebSocket uws)
		{
		}

		public void Remove(string username)
		{
		}

		public List<RealtimeWebSocket> All()
		{
			return new List<RealtimeWebSocket>();
		}

		public List<RealtimeWebSocket> Others(RealtimeWebSocket client)
		{
			return new List<RealtimeWebSocket>();
		}

		public RealtimeWebSocket Client(string username)
		{
			return new RealtimeWebSocket();
		}
	}
}
