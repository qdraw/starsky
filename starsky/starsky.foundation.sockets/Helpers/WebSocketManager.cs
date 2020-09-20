using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using starsky.foundation.platform.Models;
using starsky.foundation.sockets.Interfaces;
using starsky.foundation.sockets.Models;

namespace starsky.foundation.sockets.Helpers
{
	public class WebSocketManager
	{
		private readonly RequestDelegate _next;

		public WebSocketManager(RequestDelegate next)
		{
			_next = next;
		}

		public async Task Invoke(HttpContext context, IRealtimeWebSocketFactory wsFactory,
			IWebSocketMessageHandler wsmHandler, AppSettings appSettings)
		{
			if ( context.Request.Path != "/realtime" && context.Request.Path != "/realtime/status"  )
			{
				await _next(context);
				return;
			}

			if ( await StatusUpdate(context,appSettings) )
			{
				return;
			}

			if (!appSettings.Realtime ||  !context.User.Identity.IsAuthenticated )
			{
				context.Response.StatusCode = 403;
				return;
			}


			if ( !context.WebSockets.IsWebSocketRequest )
			{
				context.Response.StatusCode = 400;
				await context.Response.WriteAsync("This request is not a websocket");
				return;
			}
			
			if ( !context.User.Identity.IsAuthenticated )
			{
				context.Response.StatusCode = 401;
				return;
			}

			WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
			var userWebSocket = new RealtimeWebSocket()
			{
				WebSocket = webSocket, 
				Id = $"user_{Guid.NewGuid()}"  
			};
			wsFactory.Add(userWebSocket);
			
			await wsmHandler.SendInitialMessages(userWebSocket);
			
			await Listen(userWebSocket, wsFactory, wsmHandler);

			if ( context.Response.HasStarted )
			{
				// to avoid  StatusCode cannot be set because the response has already started.
				return;
			}
			await _next(context);
		}

		private async Task Listen(RealtimeWebSocket userWebSocket,
			IRealtimeWebSocketFactory wsFactory, IWebSocketMessageHandler wsmHandler)
		{
			try
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

					Console.WriteLine(result);
				}
				wsFactory.Remove(userWebSocket.Id);
				await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription,
					CancellationToken.None);
				
			}
			catch ( WebSocketException e )
			{
				wsFactory.Remove(userWebSocket.Id);
				Console.WriteLine(">>>>>>>>>>>>   WebSocketException");
				Console.WriteLine(e);
			}

		}
		
		
		private async Task<bool> StatusUpdate(HttpContext context, AppSettings appSettings)
		{
			if ( context.Request.Path != "/realtime/status" ) return false;

			if ( !appSettings.Realtime)
			{
				await StatusFeatureToggleDisabled(context);
				return true;
			}
			
			if (context.User.Identity.IsAuthenticated)
			{
				context.Response.StatusCode = 200;
				await context.Response.WriteAsync("\"User is logged in\"");
				return true;
			}

			await StatusUserLoginFirst(context);
			return true;
		}

		private async Task StatusFeatureToggleDisabled(HttpContext context)
		{
			context.Response.StatusCode = 403;
			await context.Response.WriteAsync("\"Feature toggle disabled\"");
		}
		
		private async Task StatusUserLoginFirst(HttpContext context)
		{
			context.Response.StatusCode = 401;
			await context.Response.WriteAsync("\"Login first\"");
		}

	}
}
