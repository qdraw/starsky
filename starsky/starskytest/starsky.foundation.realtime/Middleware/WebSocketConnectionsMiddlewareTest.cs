using System;
using System.Collections.Generic;
using System.Linq;
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
			var _ = new WebSocketConnectionsMiddleware(null,
				null, new WebSocketConnectionsService());
			// expect exception
		}
		
		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void NullService()
		{
			var _ = new WebSocketConnectionsMiddleware(null,
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
		private class FakeWebSocketManager : WebSocketManager
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
		
		[TestMethod]
		public async Task WebSocketConnection()
		{
			var httpContext = new FakeWebSocketHttpContext();

			var disabledWebSocketsMiddleware = new WebSocketConnectionsMiddleware(null,
				new WebSocketConnectionsOptions(), new WebSocketConnectionsService());
			await disabledWebSocketsMiddleware.Invoke(httpContext);

			var socketManager = httpContext.WebSockets as FakeWebSocketManager;

			if ( !( socketManager?.FakeWebSocket is FakeWebSocket ) ) throw new  NullReferenceException(nameof(socketManager));
			
			Assert.AreEqual(WebSocketCloseStatus.NormalClosure,
				( socketManager.FakeWebSocket as FakeWebSocket ).FakeCloseOutputAsync
				.LastOrDefault());
		}
		
		[TestMethod]
		public async Task WebSocketConnection_UserNotLoggedIn()
		{
			var httpContext = new FakeWebSocketHttpContext(false);

			var disabledWebSocketsMiddleware = new WebSocketConnectionsMiddleware(null,
				new WebSocketConnectionsOptions(), new WebSocketConnectionsService());
			await disabledWebSocketsMiddleware.Invoke(httpContext);
			
			var socketManager = httpContext.WebSockets as FakeWebSocketManager;
			Assert.AreEqual(WebSocketCloseStatus.PolicyViolation, 
				(socketManager.FakeWebSocket as FakeWebSocket).FakeCloseOutputAsync.LastOrDefault());
		}

		[TestMethod]
		public async Task WebSocketConnectionValidateOrigin()
		{
			var httpContext = new DefaultHttpContext();
			httpContext.Request.Headers["Origin"] = "fake";
			
			var disabledWebSocketsMiddleware = new WebSocketConnectionsMiddleware(null,
				new WebSocketConnectionsOptions
				{
					AllowedOrigins = new HashSet<string>{"google"}
				}, new WebSocketConnectionsService());
			await disabledWebSocketsMiddleware.Invoke(httpContext);

			Assert.AreEqual(403,httpContext.Response.StatusCode);
		}

	}
}
