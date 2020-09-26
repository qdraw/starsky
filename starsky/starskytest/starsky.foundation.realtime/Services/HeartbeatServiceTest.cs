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
	public class HeartbeatServiceTest 
	{
		[TestMethod]
		public async Task StartAsync()
		{
			var connectionService = new FakeIWebSocketConnectionsService();
			var service = new HeartbeatService(connectionService);
			
			CancellationTokenSource source = new CancellationTokenSource();
			CancellationToken token = source.Token;
			
			await service.StartAsync(token);
			Assert.IsTrue(connectionService.FakeSendToAllAsync.LastOrDefault().Contains("time"));

			source.Cancel();
		}
		
		[TestMethod]
		public async Task StartAsync_CancelBeforeStart()
		{
			var connectionService = new FakeIWebSocketConnectionsService();
			var service = new HeartbeatService(connectionService);
			
			CancellationTokenSource source = new CancellationTokenSource();
			CancellationToken token = source.Token;
			
			source.Cancel();
			await service.StartAsync(token);
			
			Assert.AreEqual(0,connectionService.FakeSendToAllAsync.Count);
		}
		
		[TestMethod]
		public async Task StartAsyncStop()
		{
			var connectionService = new FakeIWebSocketConnectionsService();
			var service = new HeartbeatService(connectionService);
			
			CancellationTokenSource source = new CancellationTokenSource();
			CancellationToken token = source.Token;
			
			await service.StartAsync(token);
			await service.StopAsync(token);

			Assert.IsTrue(connectionService.FakeSendToAllAsync.LastOrDefault().Contains("time"));
		}
	}
}
