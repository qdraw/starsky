using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.realtime.Services;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.realtime.Services
{
	[TestClass]
	public class RealtimeConnectionsServiceTest
	{
	
		[TestMethod]
		public void RealtimeConnectionsService_ShouldPassThough()
		{
			var fakeIWebSocketConnectionsService = new FakeIWebSocketConnectionsService();
			var fakeINotificationQuery = new FakeINotificationQuery();
			var service = new RealtimeConnectionsService(fakeIWebSocketConnectionsService, fakeINotificationQuery);
			service.NotificationToAllAsync(
				new ApiNotificationResponseModel<string>(), CancellationToken.None);
		
			Assert.AreEqual(1, fakeIWebSocketConnectionsService.FakeSendToAllAsync.Count);
			Assert.AreEqual(1, fakeINotificationQuery.FakeContent.Count);
		}
	}
}

