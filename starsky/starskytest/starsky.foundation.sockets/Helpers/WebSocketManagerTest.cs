using System;
using System.Security.Claims;
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

		private ClaimsPrincipal FakeUser()
		{
			// Fake: context.User.Identity.IsAuthenticated
			return new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
				new Claim(ClaimTypes.NameIdentifier, "Fake"),
				new Claim(ClaimTypes.Name, "gunnar@somecompany.com")
				// other required and custom claims
			},"TestAuthentication"));
		}
		
		[TestMethod]
		public async Task StatusNotLoggedIn()
		{
			var context = new DefaultHttpContext();
			context.Request.Path = "/realtime/status";
			
			await new WebSocketManager(next: async (innerHttpContext) =>
			  {
				  await innerHttpContext.Response.WriteAsync("test response body");
			  }).Invoke(context.HttpContext, new FakeIRealtimeWebSocketFactory(),
				new FakeIWebSocketMessageHandler(), new AppSettings());

			Assert.AreEqual(401,context.Response.StatusCode );
		}
		
		[TestMethod]
		public async Task StatusUserLoggedIn()
		{
			var context = new DefaultHttpContext{User = FakeUser()};
			context.Request.Path = "/realtime/status";
			
			await new WebSocketManager(next: async (innerHttpContext) =>
			{
				await innerHttpContext.Response.WriteAsync("test response body");
			}).Invoke(context.HttpContext, new FakeIRealtimeWebSocketFactory(),
				new FakeIWebSocketMessageHandler(), new AppSettings());

			Assert.AreEqual(200,context.Response.StatusCode );
		}
		
		[TestMethod]
		public async Task StatusFeatureToggleOff()
		{
			var context = new DefaultHttpContext{User = FakeUser()};
			context.Request.Path = "/realtime/status";
			
			await new WebSocketManager(next: async (innerHttpContext) =>
			{
				await innerHttpContext.Response.WriteAsync("test response body");
			}).Invoke(context.HttpContext, new FakeIRealtimeWebSocketFactory(),
				new FakeIWebSocketMessageHandler(), new AppSettings
				{
					Realtime = false
				});

			Assert.AreEqual(403,context.Response.StatusCode );
		}
		
		[TestMethod]
		public async Task Sta11111111tusNotLoggedIn()
		{
			var context = new DefaultHttpContext();
			context.Request.Path = "/realtime/status";
			
			await new WebSocketManager(next: async (innerHttpContext) =>
			{
				await innerHttpContext.Response.WriteAsync("test response body");
			}).Invoke(context.HttpContext, new FakeIRealtimeWebSocketFactory(),
				new FakeIWebSocketMessageHandler(), new AppSettings());

			Assert.AreEqual(401,context.Response.StatusCode );
		}
		
		[TestMethod]
		public async Task Socket_UserLoggedIn()
		{
			var context = new DefaultHttpContext{User = FakeUser()};
			context.Request.Path = "/realtime";
			// context.WebSockets.IsWebSocketRequest = true;
				
			await new WebSocketManager(next: async (innerHttpContext) =>
			{
				await innerHttpContext.Response.WriteAsync("test response body");
			}).Invoke(context.HttpContext, new FakeIRealtimeWebSocketFactory(),
				new FakeIWebSocketMessageHandler(), new AppSettings());

			Assert.AreEqual(200,context.Response.StatusCode );
		}
		
		[TestMethod]
		public async Task S11111tatusFeatureToggleOff()
		{
			var context = new DefaultHttpContext{User = FakeUser()};
			context.Request.Path = "/realtime/status";
			
			await new WebSocketManager(next: async (innerHttpContext) =>
			{
				await innerHttpContext.Response.WriteAsync("test response body");
			}).Invoke(context.HttpContext, new FakeIRealtimeWebSocketFactory(),
				new FakeIWebSocketMessageHandler(), new AppSettings
				{
					Realtime = false
				});

			Assert.AreEqual(403,context.Response.StatusCode );
		}
	}
}
