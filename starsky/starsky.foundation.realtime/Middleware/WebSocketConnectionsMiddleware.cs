using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using starsky.foundation.realtime.Helpers;
using starsky.foundation.realtime.Interfaces;
using starsky.foundation.realtime.Model;

namespace starsky.foundation.realtime.Middleware
{
	public class WebSocketConnectionsMiddleware
	{
		#region Fields
		private readonly WebSocketConnectionsOptions _options;
		private readonly IWebSocketConnectionsService _connectionsService;
		#endregion

		#region Constructor
		public WebSocketConnectionsMiddleware(RequestDelegate _, WebSocketConnectionsOptions options, 
			IWebSocketConnectionsService connectionsService)
		{
			_options = options ?? throw new ArgumentNullException(nameof(options));
			_connectionsService = connectionsService ?? throw new ArgumentNullException(nameof(connectionsService));
		}
		#endregion
		
		#region Methods
		public async Task Invoke(HttpContext context)
		{
			if (ValidateOrigin(context))
			{
				if (context.WebSockets.IsWebSocketRequest)
				{
					WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                    
					if ( !context.User.Identity.IsAuthenticated)
					{
						await webSocket.CloseOutputAsync(WebSocketCloseStatus.PolicyViolation, 
							"Please login first", CancellationToken.None);
						return;
					}
                    
					WebSocketConnection webSocketConnection = new WebSocketConnection(webSocket, _options.ReceivePayloadBufferSize);
					webSocketConnection.ReceiveText += async (sender, message) =>
					{
						await webSocketConnection.SendAsync(message, CancellationToken.None);
					};

					_connectionsService.AddConnection(webSocketConnection);

					await webSocketConnection.ReceiveMessagesUntilCloseAsync();

					if (webSocketConnection.CloseStatus.HasValue)
					{
						await webSocket.CloseOutputAsync(webSocketConnection.CloseStatus.Value, 
							webSocketConnection.CloseStatusDescription, CancellationToken.None);
					}

					_connectionsService.RemoveConnection(webSocketConnection.Id);
				}
				else
				{
					context.Response.StatusCode = StatusCodes.Status400BadRequest;
				}
			}
			else
			{
				context.Response.StatusCode = StatusCodes.Status403Forbidden;
			}
		}

		private bool ValidateOrigin(HttpContext context)
		{
			return (_options.AllowedOrigins == null) || (_options.AllowedOrigins.Count == 0) || (
				_options.AllowedOrigins.Contains(context.Request.Headers["Origin"].ToString()));
		}

		#endregion
	}
}
