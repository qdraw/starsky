using System;
using System.Net.WebSockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.JsonConverter;
using starsky.foundation.platform.Models;
using starsky.foundation.realtime.Helpers;
using starsky.foundation.realtime.Interfaces;
using starsky.foundation.realtime.Model;

namespace starsky.foundation.realtime.Middleware;

public sealed class WebSocketConnectionsMiddleware
{
	private readonly IWebSocketConnectionsService _connectionsService;
	private readonly IWebLogger _logger;
	private readonly WebSocketConnectionsOptions _options;


	public WebSocketConnectionsMiddleware(RequestDelegate _,
		WebSocketConnectionsOptions options,
		IWebSocketConnectionsService connectionsService, IWebLogger logger)
	{
		_options = options ?? throw new ArgumentNullException(nameof(options));
		_connectionsService = connectionsService ??
		                      throw new ArgumentNullException(nameof(connectionsService));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}


	public async Task Invoke(HttpContext context)
	{
		if ( ValidateOrigin(context) )
		{
			if ( context.WebSockets.IsWebSocketRequest )
			{
				var webSocket = await context.WebSockets.AcceptWebSocketAsync();

				if ( context.User.Identity?.IsAuthenticated == false )
				{
					// Status Code 1008 PolicyViolation
					await webSocket.CloseOutputAsync(WebSocketCloseStatus.PolicyViolation,
						"Please login first", CancellationToken.None);
					return;
				}

				var webSocketConnection =
					new WebSocketConnection(webSocket, _logger, _options.ReceivePayloadBufferSize);

				async void OnWebSocketConnectionOnNewConnection(object? sender,
					EventArgs message)
				{
					await Task.Delay(150);
					try
					{
						var welcomeMessage = new ApiNotificationResponseModel<HeartbeatModel>(
							new HeartbeatModel(null)) { Type = ApiNotificationType.Welcome };
						await webSocketConnection.SendAsync(JsonSerializer.Serialize(
							welcomeMessage,
							DefaultJsonSerializer.CamelCaseNoEnters), CancellationToken.None);
					}
					catch ( WebSocketException )
					{
						// if the client is closing the socket the wrong way
					}
				}

				webSocketConnection.NewConnection += OnWebSocketConnectionOnNewConnection;

				_connectionsService.AddConnection(webSocketConnection);

				await webSocketConnection.ReceiveMessagesUntilCloseAsync();

				if ( webSocketConnection.CloseStatus.HasValue )
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
		return _options.AllowedOrigins == null || _options.AllowedOrigins.Count == 0 ||
		       _options.AllowedOrigins.Contains(context.Request.Headers.Origin
			       .ToString());
	}
}
