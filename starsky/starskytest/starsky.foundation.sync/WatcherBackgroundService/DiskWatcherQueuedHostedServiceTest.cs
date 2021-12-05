using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.sync.WatcherBackgroundService;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.sync.WatcherBackgroundService
{
	[TestClass]
	public class DiskWatcherQueuedHostedServiceTest
	{
		[TestMethod]
		[Timeout(300)]
		public void ExecuteAsyncTest()
		{
			var logger = new FakeIWebLogger();
			var service = new DiskWatcherQueuedHostedService(
				new FakeIBackgroundTaskQueue(),
				logger, new FakeTelemetryService());
			
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
	}
}
