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
#if DEBUG
		[Timeout(4000)]
#else
		[Timeout(10000)]
#endif
		public void DiskWatcherQueuedHostedServiceTest_ExecuteAsync_StartAsync_Test()
		{
			var logger = new FakeIWebLogger();
			var service = new DiskWatcherQueuedHostedService(
				new FakeDiskWatcherUpdateBackgroundTaskQueue(),
				logger, new AppSettings());
			
			CancellationTokenSource source = new CancellationTokenSource();
			CancellationToken token = source.Token;
			source.Cancel(); // <- cancel before start

			// "StartAsync" is protected, so we need to use reflection
			MethodInfo? dynMethod = service.GetType().GetMethod("ExecuteAsync", 
				BindingFlags.NonPublic | BindingFlags.Instance);
			if ( dynMethod == null )
				throw new Exception("missing ExecuteAsync");
			dynMethod.Invoke(service, new object[]
			{
				token
			});
			
			Assert.IsTrue(logger.TrackedInformation.LastOrDefault().Item2?.Contains("Queued Hosted Service"));
			source.Dispose();
		}
		
		[TestMethod]
#if DEBUG
		[Timeout(2000)]
#else
		[Timeout(10000)]
#endif
		public async Task DiskWatcherQueuedHostedService_End_StopAsync_Test()
		{
			var logger = new FakeIWebLogger();
			var service = new DiskWatcherQueuedHostedService(
				new FakeDiskWatcherUpdateBackgroundTaskQueue(),
				logger, new AppSettings());
			
			var source = new CancellationTokenSource();
			var token = source.Token;
			await source.CancelAsync(); // <- cancel before start

			await service.StopAsync(token);
			
			Assert.IsTrue(logger.TrackedInformation.LastOrDefault().Item2?.Contains("is stopping"));
			source.Dispose();
		}
	}
}
