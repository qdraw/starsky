using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.realtime.Middleware;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.realtime.Middleware
{
	[TestClass]
	public sealed class DisabledWebSocketsMiddlewareTest
	{
		[TestMethod]
		public async Task DisabledWebSocketsMiddleware_Invoke()
		{
			var httpContext = new DefaultHttpContext();
			var disabledWebSocketsMiddleware = new DisabledWebSocketsMiddleware(next: (_) => Task.CompletedTask);
			await disabledWebSocketsMiddleware.Invoke(httpContext);
			Assert.AreEqual(400,httpContext.Response.StatusCode);
		}
		
		[TestMethod]
		public async Task WebSocketConnection_MessageTooBig()
		{
			var httpContext = new FakeWebSocketHttpContext(false);

			var disabledWebSocketsMiddleware = new DisabledWebSocketsMiddleware(next: (_) => Task.CompletedTask);
			await disabledWebSocketsMiddleware.Invoke(httpContext);
			
			var socketManager = httpContext.WebSockets as FakeWebSocketManager;
			Assert.AreEqual(WebSocketCloseStatus.MessageTooBig, 
				(socketManager?.FakeWebSocket as FakeWebSocket)?.FakeCloseOutputAsync.LastOrDefault());
		}
	}
}
