using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.realtime.Helpers;
using starsky.foundation.realtime.Services;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.realtime.Services
{
	[TestClass]
	public class WebSocketConnectionsServiceTest 
	{
		[TestMethod]
		public async Task SendToAllAsync()
		{
			var service = new WebSocketConnectionsService(new FakeIWebLogger());
			var fakeSocket = new FakeWebSocket();
			service.AddConnection(new WebSocketConnection(fakeSocket));

			await service.SendToAllAsync("test", CancellationToken.None);
			
			Assert.IsTrue(fakeSocket.FakeSendItems.LastOrDefault().StartsWith("test"));
		}
	}
}
