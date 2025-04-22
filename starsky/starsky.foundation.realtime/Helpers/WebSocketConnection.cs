using System;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using starsky.foundation.platform.Interfaces;

namespace starsky.foundation.realtime.Helpers;

public sealed class WebSocketConnection(
	WebSocket webSocket,
	IWebLogger logger,
	int receivePayloadBufferSize = 4096)
{
	private readonly WebSocket _webSocket =
		webSocket ?? throw new ArgumentNullException(nameof(webSocket));

	public Guid Id { get; } = Guid.NewGuid();

	public WebSocketCloseStatus? CloseStatus { get; private set; }

	public string? CloseStatusDescription { get; private set; }

	public event EventHandler<string>? ReceiveText;

	public event EventHandler? NewConnection;

	private static byte[] EncodeToByteArray(string message)
	{
		if ( string.IsNullOrWhiteSpace(message) )
		{
			throw new WebSocketException("[WebSocketConnection] no content in message");
		}

		return Encoding.ASCII.GetBytes(message);
	}

	/// <summary>
	///     Need to check for WebSocketException
	/// </summary>
	/// <param name="message">message</param>
	/// <param name="cancellationToken">cancel token</param>
	/// <returns>Task</returns>
	public async Task SendAsync(string message, CancellationToken cancellationToken)
	{
		try
		{
			await _webSocket.SendAsync(new ArraySegment<byte>(EncodeToByteArray(message)),
				WebSocketMessageType.Text,
				true,
				cancellationToken);
		}
		catch ( Exception exception )
		{
			if ( exception is WebSocketException )
			{
				logger.LogInformation(exception,
					"[WebSocketConnection.SendAsync] Catch-ed WebSocketException");
				return;
			}

			logger.LogError(exception, "[WebSocketConnection.SendAsync] Catch-ed Exception");
		}
	}

	private async Task<WebSocketCloseStatus?> GetMessage(
		WebSocketReceiveResult webSocketReceiveResult)
	{
		try
		{
			while ( webSocketReceiveResult.MessageType != WebSocketMessageType.Close )
			{
				WebSocketReceiveResult result;

				var receivedMessageStringBuilder = new StringBuilder();
				var message = new ArraySegment<byte>(new byte[4096]);
				do
				{
					// Check again to get the content of the buffer
					result = await _webSocket.ReceiveAsync(message, CancellationToken.None);
					if ( result.MessageType != WebSocketMessageType.Text )
					{
						break;
					}

					// skip the offset of the message and check the length
					var messageBytes = message.Skip(message.Offset).Take(result.Count).ToArray();
					receivedMessageStringBuilder.Append(Encoding.UTF8.GetString(messageBytes));
				} while ( !result.EndOfMessage );

				var receivedMessage = receivedMessageStringBuilder.ToString();
				if ( receivedMessage == "{}" || string.IsNullOrEmpty(receivedMessage) )
				{
					continue;
				}

				// there is a public EventHandler
				OnReceiveText(receivedMessage);
			}
		}
		catch ( WebSocketException wsex ) when ( wsex.WebSocketErrorCode ==
		                                         WebSocketError.InvalidState )
		{
			return WebSocketCloseStatus.NormalClosure;
		}

		return webSocketReceiveResult.CloseStatus;
	}

	public async Task ReceiveMessagesUntilCloseAsync()
	{
		try
		{
			NewConnection?.Invoke(this, EventArgs.Empty);
			var receivePayloadBuffer = new byte[receivePayloadBufferSize];
			var webSocketReceiveResult =
				await _webSocket.ReceiveAsync(new ArraySegment<byte>(receivePayloadBuffer),
					CancellationToken.None);

			CloseStatus = await GetMessage(webSocketReceiveResult);
			CloseStatusDescription = webSocketReceiveResult.CloseStatusDescription;
		}
		catch ( WebSocketException webSocketException ) when ( webSocketException
			                                                       .WebSocketErrorCode ==
		                                                       WebSocketError
			                                                       .ConnectionClosedPrematurely )
		{
			logger.LogInformation(
				"[WebSocketConnection.Receive] ConnectionClosedPrematurely (exception is catch-ed) ");
		}
		catch ( OperationCanceledException )
		{
			// Happens when the application closes
			logger.LogInformation(
				"[WebSocketConnection.Receive] OperationCanceledException (exception is catch-ed)");
		}
	}

	private void OnReceiveText(string webSocketMessage)
	{
		ReceiveText?.Invoke(this, webSocketMessage);
	}
}
