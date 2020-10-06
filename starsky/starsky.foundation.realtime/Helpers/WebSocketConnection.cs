using System;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace starsky.foundation.realtime.Helpers
{
	public class WebSocketConnection
	{
		#region Fields
		private readonly WebSocket _webSocket;
		private readonly int _receivePayloadBufferSize;
		#endregion

		#region Properties
		public Guid Id { get; } = Guid.NewGuid();

		public WebSocketCloseStatus? CloseStatus { get; private set; }

		public string CloseStatusDescription { get; private set; }
		#endregion

		#region Events
		public event EventHandler<string> ReceiveText;

		public event EventHandler NewConnection;

		#endregion

		#region Constructor
		public WebSocketConnection(WebSocket webSocket, int receivePayloadBufferSize = 4096)
		{
			_webSocket = webSocket ?? throw new ArgumentNullException(nameof(webSocket));
			_receivePayloadBufferSize = receivePayloadBufferSize;
		}
		#endregion

		#region Methods
		public Task SendAsync(string message, CancellationToken cancellationToken)
		{
			byte[] bytes = Encoding.ASCII.GetBytes(message);
			try
			{
				return _webSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, cancellationToken);
			}
			catch ( WebSocketException e)
			{
				return Task.FromException(e);
			}
		}

		private async Task<WebSocketCloseStatus?> GetMessage(WebSocketReceiveResult webSocketReceiveResult)
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
							break;
						// skip the offset of the message and check the length
						var messageBytes = message.Skip(message.Offset).Take(result.Count).ToArray();
						receivedMessageStringBuilder.Append(Encoding.UTF8.GetString(messageBytes));
					} 
					while ( !result.EndOfMessage );

					var receivedMessage = receivedMessageStringBuilder.ToString();
					if ( receivedMessage == "{}" || string.IsNullOrEmpty(receivedMessage) )
						continue;

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
				byte[] receivePayloadBuffer = new byte[_receivePayloadBufferSize];
				WebSocketReceiveResult webSocketReceiveResult =
					await _webSocket.ReceiveAsync(new ArraySegment<byte>(receivePayloadBuffer),
						CancellationToken.None);

				CloseStatus = await GetMessage(webSocketReceiveResult);
				CloseStatusDescription = webSocketReceiveResult.CloseStatusDescription;
			}
			catch ( WebSocketException webSocketException ) when ( webSocketException.WebSocketErrorCode ==
			                                         WebSocketError.ConnectionClosedPrematurely )
			{
				Console.WriteLine("connection ConnectionClosedPrematurely");
			}
		}

		private void OnReceiveText(string webSocketMessage)
		{
			ReceiveText?.Invoke(this, webSocketMessage);
		}
		#endregion
	}
}
