using System.Collections.Generic;
using System.Linq;
using starsky.foundation.injection;
using starsky.foundation.sockets.Interfaces;
using starsky.foundation.sockets.Models;

namespace starsky.foundation.sockets.Services
{
	[Service(typeof(IRealtimeWebSocketFactory), InjectionLifetime = InjectionLifetime.Singleton)]
	public class RealtimeWebSocketFactory : IRealtimeWebSocketFactory
	{
		private readonly List<RealtimeWebSocket> _list;

		public RealtimeWebSocketFactory()
		{
			_list = new List<RealtimeWebSocket>();
		}

		public void Add(RealtimeWebSocket uws)
		{
			_list.Add(uws);
		}

		//when disconnect
		public void Remove(string username) 
		{
			_list.Remove(Client(username));
		}

		public List<RealtimeWebSocket> All()
		{
			return _list;
		}
   
		public List<RealtimeWebSocket> Others(RealtimeWebSocket client)
		{
			return _list.Where(c => c.Id != client.Id).ToList();
		}
 
		public RealtimeWebSocket Client(string username)
		{
			return _list.First(c=>c.Id == username);
		}
	}
}
