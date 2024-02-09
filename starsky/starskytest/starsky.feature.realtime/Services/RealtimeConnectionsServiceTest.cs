using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.realtime.Services;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.realtime.Services
{
	[TestClass]
	public sealed class RealtimeConnectionsServiceTest
	{
		[TestMethod]
		public async Task RealtimeConnectionsService_ShouldPassThough()
		{
			var fakeIWebSocketConnectionsService = new FakeIWebSocketConnectionsService();
			var fakeINotificationQuery = new FakeINotificationQuery();
			var service = new RealtimeConnectionsService(fakeIWebSocketConnectionsService,
				fakeINotificationQuery, new FakeIWebLogger());
			await service.NotificationToAllAsync(
				new ApiNotificationResponseModel<string>(), CancellationToken.None);

			Assert.AreEqual(1, fakeIWebSocketConnectionsService.FakeSendToAllAsync.Count);
			Assert.AreEqual(1, fakeINotificationQuery.FakeContent.Count);
		}

		[TestMethod]
		public async Task CleanOldMessagesAsync_Exception_Handle_Test()
		{
			var fakeIWebSocketConnectionsService = new FakeIWebSocketConnectionsService();
			var fakeINotificationQuery = new FakeINotificationQuery(new Exception("t"));
			var service = new RealtimeConnectionsService(fakeIWebSocketConnectionsService,
				fakeINotificationQuery, new FakeIWebLogger());
			await service.NotificationToAllAsync(
				new ApiNotificationResponseModel<string>(),
				CancellationToken.None);

			await service.CleanOldMessagesAsync();
			// service has thrown an exception so the remove is ignored
			Assert.AreEqual(1, fakeINotificationQuery.FakeContent.Count);
		}
	}
}
