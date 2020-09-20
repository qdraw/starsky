using System;
using System.Linq;
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
	[Service(typeof(IWebSocketMessageHandler), InjectionLifetime = InjectionLifetime.Singleton)]
	public class WebSocketMessageHandler : IWebSocketMessageHandler
	{
		
		private readonly JsonSerializerOptions _serializerOptions = new JsonSerializerOptions
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		};
		
		public async Task SendInitialMessages(RealtimeWebSocket userWebSocket)
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


		/// <summary>
		/// Handle incomming messages
		/// </summary>
		/// <param name="result">incomming message</param>
		/// <param name="buffer"></param>
		/// <param name="userWebSocket">user </param>
		/// <returns></returns>
		public async Task HandleMessage(WebSocketReceiveResult result, byte[] buffer, RealtimeWebSocket userWebSocket)
		{
			var msg = Encoding.ASCII.GetString(buffer);
			if ( msg.StartsWith("ping"))
			{
				string serialisedMessage = JsonSerializer.Serialize("pong_" + DateTime.UtcNow, _serializerOptions);
				byte[] bytes = Encoding.ASCII.GetBytes(serialisedMessage);

				await userWebSocket.WebSocket.SendAsync(
					new ArraySegment<byte>(bytes, 0, bytes.Length),
					WebSocketMessageType.Text, true, CancellationToken.None);
			}
		}

		public async Task BroadcastOthers(byte[] buffer, RealtimeWebSocket userWebSocket, IRealtimeWebSocketFactory wsFactory)
		{
			var others = wsFactory.Others(userWebSocket);
			foreach (var uws in others)
			{
				await uws.WebSocket.SendAsync(new ArraySegment<byte>(buffer, 0, buffer.Length), 
					WebSocketMessageType.Text, true, CancellationToken.None);
			}
		}
		public async Task BroadcastAll(object msg, Guid? requestId,  IRealtimeWebSocketFactory wsFactory)
		{
			if ( requestId == null ) throw new ArgumentNullException(nameof(requestId));
			if ( msg == null ) throw new ArgumentNullException(nameof(msg));

			var webSocketMessage = new CustomWebSocketMessage {Data = msg, RequestId = requestId};
			
			string serialisedMessage = JsonSerializer.Serialize(webSocketMessage, _serializerOptions);
			byte[] bytes = Encoding.ASCII.GetBytes(serialisedMessage);
			await BroadcastAll(bytes, wsFactory);
		}

		public async Task BroadcastAll(byte[] buffer, IRealtimeWebSocketFactory wsFactory)
		{
			var all = wsFactory.All();
			foreach ( var uws in all.Where(uws => uws?.WebSocket != null) )
			{
				if ( uws.WebSocket.State == WebSocketState.Open )
				{
					await uws.WebSocket.SendAsync(
						new ArraySegment<byte>(buffer, 0, buffer.Length), 
						WebSocketMessageType.Text, true, CancellationToken.None);
				}
			}
		}
	}
	
}
