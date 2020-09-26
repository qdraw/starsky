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

		public WebSocketCloseStatus? CloseStatus { get; private set; } = null;

		public string CloseStatusDescription { get; private set; } = null;
		#endregion

		#region Events
		public event EventHandler<string> ReceiveText;

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
			return _webSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, cancellationToken);
		}

		private async Task<WebSocketCloseStatus?> GetMessage(WebSocketReceiveResult webSocketReceiveResult)
		{
			try
			{
				while ( webSocketReceiveResult.MessageType != WebSocketMessageType.Close )
				{

					WebSocketReceiveResult result;

					string receivedMessage = "";
					var message = new ArraySegment<byte>(new byte[4096]);
					do
					{
						Console.WriteLine(webSocketReceiveResult.MessageType == WebSocketMessageType.Close);

						result = await _webSocket.ReceiveAsync(message, CancellationToken.None);
						if ( result.MessageType != WebSocketMessageType.Text )
							break;
						var messageBytes =
							message.Skip(message.Offset).Take(result.Count).ToArray();
						receivedMessage += Encoding.UTF8.GetString(messageBytes);
				        
					} 
					while ( !result.EndOfMessage );

					if ( receivedMessage == "{}" || string.IsNullOrEmpty(receivedMessage) )
						continue;

					OnReceiveText(receivedMessage);
					Console.WriteLine("Received: {0}", receivedMessage);
				}

			}
			catch ( WebSocketException wsex ) when ( wsex.WebSocketErrorCode ==
			                                         WebSocketError.InvalidState )
			{
				return WebSocketCloseStatus.NormalClosure;
			}

			return webSocketReceiveResult?.CloseStatus;
		}

		public async Task ReceiveMessagesUntilCloseAsync()
		{
			try
			{
				byte[] receivePayloadBuffer = new byte[_receivePayloadBufferSize];
				WebSocketReceiveResult webSocketReceiveResult =
					await _webSocket.ReceiveAsync(new ArraySegment<byte>(receivePayloadBuffer),
						CancellationToken.None);

				CloseStatus = await GetMessage(webSocketReceiveResult);
				CloseStatusDescription = webSocketReceiveResult.CloseStatusDescription;
			}
			catch ( WebSocketException wsex ) when ( wsex.WebSocketErrorCode ==
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
