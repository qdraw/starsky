using System.Collections.Generic;
using System.Linq;
using starsky.foundation.injection;
using starsky.foundation.sockets.Interfaces;
using starsky.foundation.sockets.Models;

namespace starsky.foundation.sockets.Services
{
	[Service(typeof(ICustomWebSocketFactory), InjectionLifetime = InjectionLifetime.Singleton)]
	public class CustomWebSocketFactory : ICustomWebSocketFactory
	{
		private readonly List<CustomWebSocket> _list;

		public CustomWebSocketFactory()
		{
			_list = new List<CustomWebSocket>();
		}

		public void Add(CustomWebSocket uws)
		{
			_list.Add(uws);
		}

		//when disconnect
		public void Remove(string username) 
		{
			_list.Remove(Client(username));
		}

		public List<CustomWebSocket> All()
		{
			return _list;
		}
   
		public List<CustomWebSocket> Others(CustomWebSocket client)
		{
			return _list.Where(c => c.Id != client.Id).ToList();
		}
 
		public CustomWebSocket Client(string username)
		{
			return _list.First(c=>c.Id == username);
		}
	}
}
