using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Helpers;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.platform.Helpers;

[TestClass]
public class StopWatchLoggerTest
{
	[TestMethod]
	public void StopWatchLogger_IsRunning()
	{
		var stopWatchLogger = new StopWatchLogger(new FakeIWebLogger());
		var stopwatch = StopWatchLogger.StartUpdateReplaceStopWatch();
		stopWatchLogger.StopUpdateReplaceStopWatch("name", "f", true, stopwatch, false);
		Assert.AreEqual(true,stopwatch.IsRunning);
		stopwatch.Stop();
	}
	
	[TestMethod]
	public void StopWatchLogger_IsNotRunning()
	{
		var stopWatchLogger = new StopWatchLogger(new FakeIWebLogger());
		var stopwatch = StopWatchLogger.StartUpdateReplaceStopWatch();
		stopWatchLogger.StopUpdateReplaceStopWatch("name", "f", true, stopwatch);
		Assert.AreEqual(false,stopwatch.IsRunning);
	}
}
