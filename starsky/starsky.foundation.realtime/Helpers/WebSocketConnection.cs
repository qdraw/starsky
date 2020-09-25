using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace starsky.foundation.realtime.Helpers
{
	public class WebSocketConnection
    {
        #region Fields
        private WebSocket _webSocket;
        private int _receivePayloadBufferSize;
        #endregion

        #region Properties
        public Guid Id { get; } = Guid.NewGuid();

        public WebSocketCloseStatus? CloseStatus { get; private set; } = null;

        public string CloseStatusDescription { get; private set; } = null;
        #endregion

        #region Events
        public event EventHandler<string> ReceiveText;

        public event EventHandler<byte[]> ReceiveBinary;
        #endregion

        #region Constructor
        public WebSocketConnection(WebSocket webSocket, int receivePayloadBufferSize)
        {
            _webSocket = webSocket ?? throw new ArgumentNullException(nameof(webSocket));
            _receivePayloadBufferSize = receivePayloadBufferSize;
        }
        #endregion

        #region Methods
        public Task SendAsync(string message, CancellationToken cancellationToken)
        {
            // return _textSubProtocol.SendAsync(message, _webSocket, _webSocketCompressionProvider, cancellationToken);

            
            Console.WriteLine("111");
            return _webSocket.SendAsync(message, WebSocketMessageType.Text, true, cancellationToken)
        }

        public async Task ReceiveMessagesUntilCloseAsync()
        {
            try
            {
                byte[] receivePayloadBuffer = new byte[_receivePayloadBufferSize];
                WebSocketReceiveResult webSocketReceiveResult = await _webSocket.ReceiveAsync(new ArraySegment<byte>(receivePayloadBuffer), CancellationToken.None);
                while (webSocketReceiveResult.MessageType != WebSocketMessageType.Close)
                {
                    // if (webSocketReceiveResult.MessageType == WebSocketMessageType.Binary)
                    // {
                    //     byte[] webSocketMessage = await _webSocketCompressionProvider.DecompressBinaryMessageAsync(_webSocket, webSocketReceiveResult, receivePayloadBuffer);
                    //     OnReceiveBinary(webSocketMessage);
                    // }
                    // else
                    // {
                    //     string webSocketMessage = await _webSocketCompressionProvider.DecompressTextMessageAsync(_webSocket, webSocketReceiveResult, receivePayloadBuffer);
                    //     OnReceiveText(webSocketMessage);
                    // }

                    webSocketReceiveResult = await _webSocket.ReceiveAsync(new ArraySegment<byte>(receivePayloadBuffer), CancellationToken.None);
                }

                CloseStatus = webSocketReceiveResult.CloseStatus.Value;
                CloseStatusDescription = webSocketReceiveResult.CloseStatusDescription;
            }
            catch (WebSocketException wsex) when (wsex.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely)
            { }
        }

        private void OnReceiveText(string webSocketMessage)
        {
            // string message = _textSubProtocol.Read(webSocketMessage);

            ReceiveText?.Invoke(this, webSocketMessage);
        }

        private void OnReceiveBinary(byte[] webSocketMessage)
        {
            ReceiveBinary?.Invoke(this, webSocketMessage);
        }
        #endregion
    }
}
