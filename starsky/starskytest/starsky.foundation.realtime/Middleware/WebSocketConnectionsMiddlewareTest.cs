using System;
using System.Collections.Generic;
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
}
