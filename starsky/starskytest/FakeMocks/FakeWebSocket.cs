using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace starskytest.FakeMocks
{
	public class FakeWebSocket : WebSocket
	{
		public List<string> FakeSendItems { get; set; } = new List<string>();
		
		public override void Abort()
		{
			throw new NotImplementedException();
		}

		public override Task CloseAsync(WebSocketCloseStatus closeStatus, string statusDescription,
			CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

#pragma warning disable 1998
		public async override Task CloseOutputAsync(WebSocketCloseStatus closeStatus, string statusDescription,
#pragma warning restore 1998
			CancellationToken cancellationToken)
		{
		}

		public override void Dispose()
		{
		}

		private int FakeReceiveAsyncCounter { get; set; }

		public string ReceiveAsyncMessage { get; set; } = "message";

		public WebSocketError ReceiveAsyncErrorType { get; set; } = WebSocketError.InvalidState;

#pragma warning disable 1998
		public override async Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken)
#pragma warning restore 1998
		{
			FakeReceiveAsyncCounter++;
			if ( FakeReceiveAsyncCounter <= 2 )
			{
				new ArraySegment<byte>(Encoding.ASCII.GetBytes(ReceiveAsyncMessage)).CopyTo(buffer);
				return new WebSocketReceiveResult(buffer.Count, WebSocketMessageType.Text, true,
					WebSocketCloseStatus.Empty, "");
			}
			if ( FakeReceiveAsyncCounter == 3 ) return new WebSocketReceiveResult(buffer.Count, WebSocketMessageType.Close, true);
		
			throw new WebSocketException(ReceiveAsyncErrorType);
		}

#pragma warning disable 1998
		public override async Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage,
#pragma warning restore 1998
			CancellationToken cancellationToken)
		{
			FakeSendItems.Add(System.Text.Encoding.Default.GetString(buffer));
		}

		public override WebSocketCloseStatus? CloseStatus { get; }
		public override string CloseStatusDescription { get; }
		public override WebSocketState State { get; }
		public override string SubProtocol { get; }
	}
}
