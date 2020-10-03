using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.realtime.Middleware
{
	public class FakeWebSocketHttpContext : HttpContext
	{
		public FakeWebSocketHttpContext(bool userLoggedIn = true)
		{
			WebSockets = new FakeWebSocketManager(true);
			if ( userLoggedIn )
			{
				User = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>(), "Test"));
				return;
			}
			User = new ClaimsPrincipal(new ClaimsIdentity());
			Response = new DefaultHttpContext().Response;
		}
		public override void Abort() { }
		public override ConnectionInfo Connection { get; }
		public override IFeatureCollection Features { get; }
		public override IDictionary<object, object> Items { get; set; }
		public override HttpRequest Request { get; }
		public override CancellationToken RequestAborted { get; set; }
		public override IServiceProvider RequestServices { get; set; }
		public override HttpResponse Response { get; }
		public override ISession Session { get; set; }
		public override string TraceIdentifier { get; set; }
		public sealed override ClaimsPrincipal User { get; set; }
		public override WebSocketManager WebSockets { get; }
	}
	public class FakeWebSocketManager : WebSocketManager
	{
		public FakeWebSocketManager(bool isWebSocketRequest)
		{
			IsWebSocketRequest = isWebSocketRequest;
		}
		public WebSocket FakeWebSocket { get; set; } = new FakeWebSocket();
			
#pragma warning disable 1998
		// ReSharper disable once ArrangeModifiersOrder
		public async override Task<WebSocket> AcceptWebSocketAsync(string subProtocol)
#pragma warning restore 1998
		{
			return FakeWebSocket;
		}

		public override bool IsWebSocketRequest { get; }
		public override IList<string> WebSocketRequestedProtocols { get; }
	}
}


