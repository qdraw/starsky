using System.Collections.Generic;
using starsky.foundation.sockets.Models;

namespace starsky.foundation.sockets.Interfaces
{
	public interface ICustomWebSocketFactory
	{
		void Add(CustomWebSocket uws);
		void Remove(string username);
		List<CustomWebSocket> All();
		List<CustomWebSocket> Others(CustomWebSocket client);
		CustomWebSocket Client(string username);
	}
}
