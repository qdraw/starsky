using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Models;
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
}
