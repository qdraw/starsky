using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Models;
using starsky.foundation.realtime.Services;
using starsky.foundation.sync.WatcherBackgroundService;
using starsky.foundation.worker.Helpers;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.worker.Helpers;

[TestClass]
public class ProcessTaskQueueTest
{
	
	[TestMethod]
	public void RoundUp_ShouldBeLowerThanValue()
	{
		var t = ProcessTaskQueue.RoundUp(new AppSettings
		{
			UseDiskWatcherIntervalInMilliseconds = 20000
		});
		Assert.IsTrue(t.Item1.TotalMilliseconds <= 20000);
	}
		
	[TestMethod]
	public void RoundUp_WrongInput()
	{
		var t = ProcessTaskQueue.RoundUp(new AppSettings
		{
			UseDiskWatcherIntervalInMilliseconds = 0
		});
		Assert.AreEqual(0, t.Item1.TotalMilliseconds);
	}

	[TestMethod]
	[Timeout(10000)]
	public async Task ProcessBatchedLoopAsyncTest_NothingIn()
	{
		CancellationTokenSource source = new CancellationTokenSource();
		CancellationToken token = source.Token;

		var fakeService = new FakeDiskWatcherUpdateBackgroundTaskQueue();
		var logger = new FakeIWebLogger();
		var appSettings = new AppSettings
		{
			UseDiskWatcherIntervalInMilliseconds = 0
		};
		
		source.CancelAfter(TimeSpan.FromSeconds(2));
		
		await ProcessTaskQueue.ProcessBatchedLoopAsync(fakeService, logger, appSettings,token);
		
		Assert.IsFalse(fakeService.DequeueAsyncCounter != 0);
		Assert.AreEqual(0, fakeService.DequeueAsyncCounter);
	}
	
	[TestMethod]
	[Timeout(10000)]
	public async Task ProcessBatchedLoopAsyncTest_NothingIn_CanceledDuringWait()
	{
		CancellationTokenSource source = new CancellationTokenSource();
		CancellationToken token = source.Token;

		var fakeService = new FakeDiskWatcherUpdateBackgroundTaskQueue();
		var logger = new FakeIWebLogger();
		var appSettings = new AppSettings
		{
			UseDiskWatcherIntervalInMilliseconds = 11
		};
		
		source.CancelAfter(TimeSpan.FromSeconds(2));
		
		// canceled in await Task.Delay
		await ProcessTaskQueue.ProcessBatchedLoopAsync(fakeService, logger, appSettings,token);
		
		Assert.IsFalse(fakeService.DequeueAsyncCounter != 0);
		Assert.AreEqual(0, fakeService.DequeueAsyncCounter);
	}
	
	[TestMethod]
	[Timeout(10000)]
	public async Task ProcessBatchedLoopAsyncTest_ItemsIn()
	{
		CancellationTokenSource source = new CancellationTokenSource();
		CancellationToken token = source.Token;

		var fakeService = new FakeDiskWatcherUpdateBackgroundTaskQueue(2);
		var logger = new FakeIWebLogger();
		var appSettings = new AppSettings
		{
			UseDiskWatcherIntervalInMilliseconds = 0
		};
		
		source.CancelAfter(TimeSpan.FromSeconds(2));
		
		await ProcessTaskQueue.ProcessBatchedLoopAsync(fakeService, logger, appSettings,token);
		
		Assert.IsTrue(fakeService.DequeueAsyncCounter != 0);
		Assert.AreEqual(2, fakeService.DequeueAsyncCounter);
	}
}
