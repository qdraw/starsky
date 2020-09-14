using System;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using starsky.foundation.injection;
using starsky.foundation.sockets.Interfaces;
using starsky.foundation.sockets.Models;

namespace starsky.foundation.sockets.Services
{
	[Service(typeof(ICustomWebSocketMessageHandler), InjectionLifetime = InjectionLifetime.Singleton)]
	public class CustomWebSocketMessageHandler : ICustomWebSocketMessageHandler
	{
		
		private readonly JsonSerializerOptions _serializerOptions = new JsonSerializerOptions
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		};
		
		public async Task SendInitialMessages(CustomWebSocket userWebSocket)
		{
			WebSocket webSocket = userWebSocket.WebSocket;
			
			var msg = new CustomWebSocketMessage
			{
				Id = userWebSocket.Id,
				RequestId = null,
				Data = null
			};

			string serialisedMessage = JsonSerializer.Serialize(msg,_serializerOptions);
			byte[] bytes = Encoding.ASCII.GetBytes(serialisedMessage);
			await webSocket.SendAsync(new ArraySegment<byte>(bytes, 0, bytes.Length), 
				WebSocketMessageType.Text, true, CancellationToken.None);
		}


		public async Task HandleMessage(WebSocketReceiveResult result, byte[] buffer, CustomWebSocket userWebSocket, 
			ICustomWebSocketFactory wsFactory)
		{
			string msg = Encoding.ASCII.GetString(buffer);
			try
			{
				var message = JsonSerializer.Deserialize<CustomWebSocketMessage>(msg);
				// message.type as anytype
				await BroadcastOthers(buffer, userWebSocket, wsFactory);

			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				await userWebSocket.WebSocket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), 
					result.MessageType, result.EndOfMessage, CancellationToken.None);
			}
		}

		public async Task BroadcastOthers(byte[] buffer, CustomWebSocket userWebSocket, ICustomWebSocketFactory wsFactory)
		{
			var others = wsFactory.Others(userWebSocket);
			foreach (var uws in others)
			{
				await uws.WebSocket.SendAsync(new ArraySegment<byte>(buffer, 0, buffer.Length), 
					WebSocketMessageType.Text, true, CancellationToken.None);
			}
		}
		public async Task BroadcastAll(object msg, Guid? requestId,  ICustomWebSocketFactory wsFactory)
		{
			if ( requestId == null ) throw new ArgumentNullException(nameof(requestId));
			if ( msg == null ) throw new ArgumentNullException(nameof(msg));

			var webSocketMessage = new CustomWebSocketMessage {Data = msg, RequestId = requestId};
			
			string serialisedMessage = JsonSerializer.Serialize(webSocketMessage, _serializerOptions);
			byte[] bytes = Encoding.ASCII.GetBytes(serialisedMessage);
			await BroadcastAll(bytes, wsFactory);
		}

		public async Task BroadcastAll(byte[] buffer, ICustomWebSocketFactory wsFactory)
		{
			var all = wsFactory.All();
			foreach (var uws in all)
			{
				try
				{
					await uws.WebSocket.SendAsync(
						new ArraySegment<byte>(buffer, 0, buffer.Length), 
						WebSocketMessageType.Text, true, CancellationToken.None);
				}
				catch ( Exception e )
				{
					wsFactory.Remove(uws.Id);
					Console.WriteLine(e);
				}

			}
		}
	}
	
}
