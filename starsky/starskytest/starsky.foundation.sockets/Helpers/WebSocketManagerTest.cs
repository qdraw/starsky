using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;
using WebSocketManager = starsky.foundation.sockets.Helpers.WebSocketManager;

namespace starskytest.starsky.foundation.sockets.Helpers
{
	[TestClass]
	public class WebSocketManagerTest
	{
		[TestMethod]
		public async Task Test()
		{
			var context = new DefaultHttpContext();
			_ = new WebSocketManager(next: async (innerHttpContext) =>
			  {
				  await innerHttpContext.Response.WriteAsync("test response body");
			  }).Invoke(context.HttpContext, new FakeIRealtimeWebSocketFactory(),
				new FakeIWebSocketMessageHandler(), new AppSettings());

			Console.WriteLine();
		}
	}
}
