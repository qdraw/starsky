using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Models;
using starsky.foundation.sync.WatcherBackgroundService;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.sync.WatcherBackgroundService
{
	[TestClass]
	public sealed class DiskWatcherQueuedHostedServiceTest
	{
		[TestMethod]
		[Timeout(300)]
		public void ExecuteAsync_StartAsync_Test()
		{
			var logger = new FakeIWebLogger();
			var service = new DiskWatcherQueuedHostedService(
				new FakeDiskWatcherUpdateBackgroundTaskQueue(),
				logger, new AppSettings());
			
			CancellationTokenSource source = new CancellationTokenSource();
			CancellationToken token = source.Token;
			source.Cancel(); // <- cancel before start

			MethodInfo dynMethod = service.GetType().GetMethod("ExecuteAsync", 
				BindingFlags.NonPublic | BindingFlags.Instance);
			if ( dynMethod == null )
				throw new Exception("missing ExecuteAsync");
			dynMethod.Invoke(service, new object[]
			{
				token
			});
			
			Assert.IsTrue(logger.TrackedInformation.LastOrDefault().Item2.Contains("Queued Hosted Service"));
		}
		
		[TestMethod]
		[Timeout(1000)]
		public async Task DiskWatcherQueuedHostedService_End_StopAsync_Test()
		{
			var logger = new FakeIWebLogger();
			var service = new DiskWatcherQueuedHostedService(
				new FakeDiskWatcherUpdateBackgroundTaskQueue(),
				logger, new AppSettings());
			
			var source = new CancellationTokenSource();
			var token = source.Token;
			source.Cancel(); // <- cancel before start

			await service.StopAsync(token);
			
			Assert.IsTrue(logger.TrackedInformation.LastOrDefault().Item2.Contains("is stopping"));
		}


	}
}
