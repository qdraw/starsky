using System;
using System.Threading.Tasks;
using starsky.foundation.injection;
using starsky.foundation.sockets.Interfaces;

namespace starsky.foundation.sockets.Services
{
	[Service(typeof(ISockets), InjectionLifetime = InjectionLifetime.Singleton)]
	public class SocketsService : ISockets
	{
		private readonly ICustomWebSocketFactory _socketFactory;
		private readonly ICustomWebSocketMessageHandler _messageHandler;
		
		public SocketsService(ICustomWebSocketFactory socketFactory, 
			ICustomWebSocketMessageHandler messageHandler)
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
