using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using starsky.foundation.sockets.Interfaces;
using starsky.foundation.sockets.Models;

namespace starsky.foundation.sockets.Helpers
{
	public class CustomWebSocketManager
	{
		private readonly RequestDelegate _next;

		public CustomWebSocketManager(RequestDelegate next)
		{
			_next = next;
		}

		public async Task Invoke(HttpContext context, ICustomWebSocketFactory wsFactory,
			ICustomWebSocketMessageHandler wsmHandler)
		{
			if ( context.Request.Path != "/api/websocket" )
			{
				await _next(context);
				return;
			}

			if ( !context.WebSockets.IsWebSocketRequest )
			{
				context.Response.StatusCode = 400;
				return;
			}
			
			// TODO ENABLE

			// if ( !context.User.Identity.IsAuthenticated )
			// {
			// 	context.Response.StatusCode = 401;
			// 	return;
			// }

			WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
			var userWebSocket = new CustomWebSocket()
			{
				WebSocket = webSocket, 
				Id = $"user_{Guid.NewGuid()}"  
			};
			wsFactory.Add(userWebSocket);
			
			await wsmHandler.SendInitialMessages(userWebSocket);
			
			await Listen(userWebSocket, wsFactory, wsmHandler);

			await _next(context);
		}

		private async Task Listen(CustomWebSocket userWebSocket,
			ICustomWebSocketFactory wsFactory, ICustomWebSocketMessageHandler wsmHandler)
		{
			WebSocket webSocket = userWebSocket.WebSocket;
			var buffer = new byte[1024 * 4];
			WebSocketReceiveResult result =
				await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer),
					CancellationToken.None);
			while ( !result.CloseStatus.HasValue )
			{
				await wsmHandler.HandleMessage(result, buffer, userWebSocket, wsFactory);
				buffer = new byte[1024 * 4];
				result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer),
					CancellationToken.None);
			}

			wsFactory.Remove(userWebSocket.Id);
			await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription,
				CancellationToken.None);
		}
	}
}
