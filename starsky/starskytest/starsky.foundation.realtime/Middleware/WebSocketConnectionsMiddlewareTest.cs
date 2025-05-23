using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.WebSockets;
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
	public async Task WebSocketConnection()
	{
		var httpContext = new FakeWebSocketHttpContext();

		var disabledWebSocketsMiddleware = new WebSocketConnectionsMiddleware(null!,
			new WebSocketConnectionsOptions(),
			new WebSocketConnectionsService(), new FakeIWebLogger());
		await disabledWebSocketsMiddleware.Invoke(httpContext);

		var socketManager = httpContext.WebSockets as FakeWebSocketManager;

		if ( !( socketManager?.FakeWebSocket is FakeWebSocket ) )
		{
			throw new NullReferenceException(nameof(socketManager));
		}

		Assert.AreEqual(WebSocketCloseStatus.NormalClosure,
			( socketManager.FakeWebSocket as FakeWebSocket )!.FakeCloseOutputAsync
			.LastOrDefault());
	}

	[TestMethod]
	public async Task WebSocketConnection_UserNotLoggedIn()
	{
		var httpContext = new FakeWebSocketHttpContext(false);

		var disabledWebSocketsMiddleware = new WebSocketConnectionsMiddleware(null!,
			new WebSocketConnectionsOptions(),
			new WebSocketConnectionsService(), new FakeIWebLogger());
		await disabledWebSocketsMiddleware.Invoke(httpContext);

		var socketManager = httpContext.WebSockets as FakeWebSocketManager;
		Assert.AreEqual(WebSocketCloseStatus.PolicyViolation,
			( socketManager?.FakeWebSocket as FakeWebSocket )?.FakeCloseOutputAsync
			.LastOrDefault());
	}

	[TestMethod]
	public async Task WebSocketConnectionValidateOrigin()
	{
		var httpContext = new DefaultHttpContext();
		httpContext.Request.Headers.Origin = "fake";

		var disabledWebSocketsMiddleware = new WebSocketConnectionsMiddleware(null!,
			new WebSocketConnectionsOptions { AllowedOrigins = new HashSet<string> { "google" } },
			new WebSocketConnectionsService(), new FakeIWebLogger());
		await disabledWebSocketsMiddleware.Invoke(httpContext);

		Assert.AreEqual(403, httpContext.Response.StatusCode);
	}
}
