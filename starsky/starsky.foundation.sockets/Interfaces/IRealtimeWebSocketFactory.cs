using System.Collections.Generic;
using starsky.foundation.sockets.Models;

namespace starsky.foundation.sockets.Interfaces
{
	public interface IRealtimeWebSocketFactory
	{
		void Add(RealtimeWebSocket uws);
		void Remove(string username);
		List<RealtimeWebSocket> All();
		List<RealtimeWebSocket> Others(RealtimeWebSocket client);
		RealtimeWebSocket Client(string username);
	}
}
