using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.realtime.Middleware;
using starsky.foundation.realtime.Model;
using starsky.foundation.realtime.Services;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.realtime.Middleware
{
	[TestClass]
	public class WebSocketConnectionsMiddlewareTest
	{
		
		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void NullOptions()
		{
			new WebSocketConnectionsMiddleware(null,
				null, new WebSocketConnectionsService());
			// expect exception
		}
		
		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void NullService()
		{
			new WebSocketConnectionsMiddleware(null,
				new WebSocketConnectionsOptions(), null);
			// expect exception
		}
		
		[TestMethod]
		public async Task Invoke_BadRequest_NotAWebSocket()
		{
			var httpContext = new DefaultHttpContext();
			var disabledWebSocketsMiddleware = new WebSocketConnectionsMiddleware(null,
				new WebSocketConnectionsOptions(), new WebSocketConnectionsService());
			await disabledWebSocketsMiddleware.Invoke(httpContext);
			Assert.AreEqual(400,httpContext.Response.StatusCode);
		}
		
		private class FakeWebSocketHttpContext : HttpContext
		{
			public FakeWebSocketHttpContext(bool isWebSocketRequest)
			{
				WebSockets = new FakeWebSocketManager(isWebSocketRequest);
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
			public override ClaimsPrincipal User { get; set; } = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>(), "Test"));
			public override WebSocketManager WebSockets { get; }
		}
		private class FakeWebSocketManager : WebSocketManager
		{
			public FakeWebSocketManager(bool isWebSocketRequest)
			{
				IsWebSocketRequest = isWebSocketRequest;
			}
			public async override Task<WebSocket> AcceptWebSocketAsync(string subProtocol)
			{
				return new FakeWebSocket();
			}

			public override bool IsWebSocketRequest { get; }
			public override IList<string> WebSocketRequestedProtocols { get; }
		}
		
		[TestMethod]
		public async Task WebSocketConnection()
		{
			var httpContext = new FakeWebSocketHttpContext(true);

			var disabledWebSocketsMiddleware = new WebSocketConnectionsMiddleware(null,
				new WebSocketConnectionsOptions(), new WebSocketConnectionsService());
			await disabledWebSocketsMiddleware.Invoke(httpContext);

			Assert.IsNull(httpContext.Response);
		}

	}
}
