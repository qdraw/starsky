using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.realtime.Middleware;
using starsky.foundation.realtime.Model;
using starsky.foundation.realtime.Services;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.realtime.Middleware;

[TestClass]
public sealed class WebSocketConnectionsMiddlewareTest
{
	[TestMethod]
	[SuppressMessage("Performance",
		"CA1806:Do not ignore method results",
		Justification = "Should fail when null in constructor")]
	[SuppressMessage("ReSharper",
		"ObjectCreationAsStatement")]
	public void NullOptions()
	{
		// Act & Assert
		var exception = Assert.ThrowsExactly<ArgumentNullException>(() =>
			new WebSocketConnectionsMiddleware(null!,
				null!, new WebSocketConnectionsService(),
				new FakeIWebLogger()));

		// Additional assertion (optional)
		Assert.AreEqual("options", exception.ParamName);
	}

	[TestMethod]
	public void NullService()
	{
		// Act & Assert
		var exception = Assert.ThrowsExactly<ArgumentNullException>(() =>
		{
			_ = new WebSocketConnectionsMiddleware(null!,
				new WebSocketConnectionsOptions(), null!, new FakeIWebLogger());
		});

		// Additional assertion (optional)
		Assert.AreEqual("connectionsService", exception.ParamName);
	}

	[TestMethod]
	public async Task Invoke_BadRequest_NotAWebSocket()
	{
		var httpContext = new DefaultHttpContext();
		var disabledWebSocketsMiddleware = new WebSocketConnectionsMiddleware(null!,
			new WebSocketConnectionsOptions(),
			new WebSocketConnectionsService(), new FakeIWebLogger());
		await disabledWebSocketsMiddleware.Invoke(httpContext);
		Assert.AreEqual(400, httpContext.Response.StatusCode);
	}

	[TestMethod]
	[DataRow(true, WebSocketCloseStatus.NormalClosure)]
	[DataRow(false, WebSocketCloseStatus.PolicyViolation)]
	public async Task WebSocketConnection_UserAuthenticationPaths(bool userLoggedIn,
		WebSocketCloseStatus expectedCloseStatus)
	{
		var httpContext = new FakeWebSocketHttpContext(userLoggedIn);

		var disabledWebSocketsMiddleware = new WebSocketConnectionsMiddleware(null!,
			new WebSocketConnectionsOptions(),
			new WebSocketConnectionsService(), new FakeIWebLogger());
		await disabledWebSocketsMiddleware.Invoke(httpContext);

		var socketManager = httpContext.WebSockets as FakeWebSocketManager;
		Assert.AreEqual(expectedCloseStatus,
			( socketManager?.FakeWebSocket as FakeWebSocket )?.FakeCloseOutputAsync
			.LastOrDefault());
	}

	[TestMethod]
	public async Task WebSocketConnectionValidateOrigin()
	{
		var httpContext = new DefaultHttpContext();
		httpContext.Request.Headers.Origin = "fake";

		var disabledWebSocketsMiddleware = new WebSocketConnectionsMiddleware(null!,
			new WebSocketConnectionsOptions { AllowedOrigins = ["google"] },
			new WebSocketConnectionsService(), new FakeIWebLogger());
		await disabledWebSocketsMiddleware.Invoke(httpContext);

		Assert.AreEqual(403, httpContext.Response.StatusCode);
	}

	[TestMethod]
	public async Task WebSocketConnection_NoAllowedOrigins_ForeignOriginIsForbidden()
	{
		var httpContext = new DefaultHttpContext
		{
			Request = { Host = new HostString("photos.example.test") }
		};
		httpContext.Request.Headers.Origin = "https://evil.example.test";

		var middleware = new WebSocketConnectionsMiddleware(null!,
			new WebSocketConnectionsOptions(),
			new WebSocketConnectionsService(), new FakeIWebLogger());
		await middleware.Invoke(httpContext);

		Assert.AreEqual(403, httpContext.Response.StatusCode);
	}

	[TestMethod]
	[DataRow("", "photos.example.test", true)]
	[DataRow("https://photos.example.test", "photos.example.test", true)]
	[DataRow("http://photos.example.test:4000", "photos.example.test", true)]
	[DataRow("https://PHOTOS.example.test", "photos.example.test", true)]
	[DataRow("https://evil.example.test", "photos.example.test", false)]
	[DataRow("https://photos.example.test.evil.test", "photos.example.test", false)]
	[DataRow("null", "photos.example.test", false)]
	[DataRow("fake", "photos.example.test", false)]
	public void IsSameHost_Cases(string origin, string requestHost, bool expected)
	{
		Assert.AreEqual(expected, WebSocketConnectionsMiddleware.IsSameHost(origin, requestHost));
	}

	[TestMethod]
	public async Task WebSocketConnection_NoCloseStatus_DoesNotCloseOutput()
	{
		var httpContext = new FakeWebSocketHttpContext();
		var socketManager = httpContext.WebSockets as FakeWebSocketManager;
		Assert.IsNotNull(socketManager);
		var fakeWebSocket = new FakeWebSocketWithoutCloseStatus();
		socketManager.FakeWebSocket = fakeWebSocket;

		var disabledWebSocketsMiddleware = new WebSocketConnectionsMiddleware(null!,
			new WebSocketConnectionsOptions(),
			new WebSocketConnectionsService(), new FakeIWebLogger());
		await disabledWebSocketsMiddleware.Invoke(httpContext);

		Assert.IsEmpty(fakeWebSocket.FakeCloseOutputAsync);
	}

	private sealed class FakeWebSocketWithoutCloseStatus : FakeWebSocket
	{
#pragma warning disable 1998
		public override async Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer,
			CancellationToken cancellationToken)
#pragma warning restore 1998
		{
			return new WebSocketReceiveResult(0, WebSocketMessageType.Close, true);
		}
	}

	/// <summary>
	///     Repro/regression test for the WebSocketConnectionsService memory leak: only
	///     WebSocketError.InvalidState (in GetMessage) and ConnectionClosedPrematurely (in
	///     ReceiveMessagesUntilCloseAsync) are caught. Any other WebSocketError code propagates
	///     past the middleware's RemoveConnection call and leaks the connection into the
	///     singleton _connections dictionary forever.
	/// </summary>
	[TestMethod]
	public async Task WebSocketConnection_UnhandledErrorCodeDuringReceive_LeaksConnection()
	{
		var httpContext = new FakeWebSocketHttpContext();
		var socketManager = httpContext.WebSockets as FakeWebSocketManager;
		Assert.IsNotNull(socketManager);
		var fakeWebSocket = new FakeWebSocket { ReceiveAsyncErrorType = WebSocketError.Faulted };
		socketManager.FakeWebSocket = fakeWebSocket;

		var connectionsService = new WebSocketConnectionsService();
		var middleware = new WebSocketConnectionsMiddleware(null!,
			new WebSocketConnectionsOptions(), connectionsService, new FakeIWebLogger());

		await Assert.ThrowsExactlyAsync<WebSocketException>(
			() => middleware.Invoke(httpContext));

		var connectionsField = typeof(WebSocketConnectionsService)
			.GetField("_connections",
				System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
		var connections =
			( System.Collections.IDictionary ) connectionsField.GetValue(connectionsService)!;

		Assert.IsEmpty(connections,
			"An unhandled WebSocketError code during receive must not leak the connection " +
			"into the singleton _connections dictionary");
	}
}
