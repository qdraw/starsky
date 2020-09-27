using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.realtime.Extentions;
using starsky.foundation.realtime.Interfaces;
using starsky.foundation.realtime.Model;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.realtime.Extensions
{
	[TestClass]
	public class WebSocketConnectionsMiddlewareExtensionsTest
	{
		
		[TestMethod]
		public async Task CreateDefaultBuilder_TestAppWebSocket_NotFailing()
		{
			var host = WebHost.CreateDefaultBuilder()
				.UseUrls("http://localhost:9824")
				.ConfigureServices(services =>
				{
					services.AddSingleton<IWebSocketConnectionsService, FakeIWebSocketConnectionsService>();
				})
				.Configure(app =>
				{
					app.MapWebSocketConnections("/test", new WebSocketConnectionsOptions(), false);
					app.MapWebSocketConnections("/test1", new WebSocketConnectionsOptions());
				})
				.Build();
			
			await host.StartAsync();

			// it should not fail, 
			var fakeService = host.Services.GetService<IWebSocketConnectionsService>();
			Assert.IsNotNull(fakeService);
			
			await host.StopAsync();
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ExpectedException_ArgumentNullException()
		{
			var app = null as IApplicationBuilder;
			app.MapWebSocketConnections("/test1", new WebSocketConnectionsOptions());
		}
	}
}
