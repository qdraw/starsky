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
		if ( !ValidateOrigin(context) )
		{
			context.Response.StatusCode = StatusCodes.Status403Forbidden;
			return;
		}

		if ( !context.WebSockets.IsWebSocketRequest )
		{
			context.Response.StatusCode = StatusCodes.Status400BadRequest;
			return;
		}

		await HandleWebSocketRequestAsync(context);
	}

	private async Task HandleWebSocketRequestAsync(HttpContext context)
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

		// Capture before the HttpContext can be disposed by the pipeline.
		var requestAborted = context.RequestAborted;
		webSocketConnection.NewConnection += (_, _) =>
			SendWelcomeMessageAsync(webSocketConnection, requestAborted);

		_connectionsService.AddConnection(webSocketConnection);
		await webSocketConnection.ReceiveMessagesUntilCloseAsync();
		await CloseWebSocketIfNeededAsync(webSocketConnection, webSocket);
		_connectionsService.RemoveConnection(webSocketConnection.Id);
	}

	private static async void SendWelcomeMessageAsync(WebSocketConnection webSocketConnection,
		CancellationToken requestAborted)
	{
		try
		{
			await Task.Delay(150, requestAborted);
			var welcomeMessage = new ApiNotificationResponseModel<HeartbeatModel>(
				new HeartbeatModel(null)) { Type = ApiNotificationType.Welcome };
			await webSocketConnection.SendAsync(JsonSerializer.Serialize(
				welcomeMessage, DefaultJsonSerializer.CamelCaseNoEnters), requestAborted);
		}
		catch ( WebSocketException )
		{
			// if the client is closing the socket the wrong way
		}
		catch ( OperationCanceledException )
		{
			// client disconnected before the welcome message could be sent
		}
	}

	private static async Task CloseWebSocketIfNeededAsync(WebSocketConnection webSocketConnection,
		WebSocket webSocket)
	{
		if ( !webSocketConnection.CloseStatus.HasValue )
		{
			return;
		}

		await webSocket.CloseOutputAsync(webSocketConnection.CloseStatus.Value,
			webSocketConnection.CloseStatusDescription, CancellationToken.None);
	}

	private bool ValidateOrigin(HttpContext context)
	{
		return _options.AllowedOrigins == null || _options.AllowedOrigins.Count == 0 ||
		       _options.AllowedOrigins.Contains(context.Request.Headers.Origin
			       .ToString());
	}
}
