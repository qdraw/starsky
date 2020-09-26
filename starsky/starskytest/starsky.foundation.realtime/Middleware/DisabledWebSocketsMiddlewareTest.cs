using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.realtime.Middleware;

namespace starskytest.starsky.foundation.realtime.Middleware
{
	[TestClass]
	public class DisabledWebSocketsMiddlewareTest
	{
		[TestMethod]
		public async Task DisabledWebSocketsMiddleware_Invoke()
		{
			var httpContext = new DefaultHttpContext();
			var disabledWebSocketsMiddleware = new DisabledWebSocketsMiddleware(next: (innerHttpContext) => Task.FromResult(0));
			await disabledWebSocketsMiddleware.Invoke(httpContext);
			Assert.AreEqual(204,httpContext.Response.StatusCode);
		}
	}
}
