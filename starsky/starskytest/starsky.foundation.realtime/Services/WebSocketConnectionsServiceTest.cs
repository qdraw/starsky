using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.JsonConverter;
using starsky.foundation.platform.Models;
using starsky.foundation.realtime.Helpers;
using starsky.foundation.realtime.Services;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.realtime.Services
{
	[TestClass]
	public sealed class WebSocketConnectionsServiceTest 
	{
		[TestMethod]
		public async Task SendToAllAsync_success()
		{
			var service = new WebSocketConnectionsService(new FakeIWebLogger());
			var fakeSocket = new FakeWebSocket();
			service.AddConnection(new WebSocketConnection(fakeSocket));

			await service.SendToAllAsync("test", CancellationToken.None);
			
			Assert.IsTrue(fakeSocket.FakeSendItems.LastOrDefault()?.StartsWith("test"));
		}
		
		[TestMethod]
		public async Task SendToAllAsync_ExceptionDueNoContent()
		{
			var logger = new FakeIWebLogger();
			var service = new WebSocketConnectionsService(logger);
			var fakeSocket = new FakeWebSocket();
			service.AddConnection(new WebSocketConnection(fakeSocket));

			await service.SendToAllAsync(null!, CancellationToken.None);
			Assert.AreEqual(1,logger.TrackedInformation.Count);
		}
		
		[TestMethod]
		public async Task SendToAllAsync_Model_success()
		{
			var service = new WebSocketConnectionsService(new FakeIWebLogger());
			var fakeSocket = new FakeWebSocket();
			service.AddConnection(new WebSocketConnection(fakeSocket));

			await service.SendToAllAsync(new ApiNotificationResponseModel<string>("test"), CancellationToken.None);

			var json = JsonSerializer.Serialize(
				new ApiNotificationResponseModel<string>("test"),
				DefaultJsonSerializer.CamelCaseNoEnters);
			Assert.AreEqual(json,fakeSocket.FakeSendItems.LastOrDefault());

			Assert.AreEqual("{\"data\":\"test\",\"type\":\"Unknown\"}",fakeSocket.FakeSendItems.LastOrDefault());
		}
	}
}
