using System;
using System.Threading.Tasks;
using starsky.foundation.injection;
using starsky.foundation.sockets.Interfaces;

namespace starsky.foundation.sockets.Services
{
	[Service(typeof(ISockets), InjectionLifetime = InjectionLifetime.Singleton)]
	public class SocketsService : ISockets
	{
		private readonly IRealtimeWebSocketFactory _socketFactory;
		private readonly IWebSocketMessageHandler _messageHandler;
		
		public SocketsService(IRealtimeWebSocketFactory socketFactory, 
			IWebSocketMessageHandler messageHandler)
		{
			_socketFactory = socketFactory;
			_messageHandler = messageHandler;
		}

		public async Task BroadcastAll(Guid? requestId, object message)
		{
			await _messageHandler.BroadcastAll(message, requestId, _socketFactory);
		}
	}
}
