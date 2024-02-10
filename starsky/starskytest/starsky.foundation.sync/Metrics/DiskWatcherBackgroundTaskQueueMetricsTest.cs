using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.sync.Metrics;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.sync.Metrics;

[TestClass]
public class DiskWatcherBackgroundTaskQueueMetricsTests
{
	[TestMethod]
	public void DiskWatcher_ObserveValue_Returns_Correct_Value()
	{
		// Arrange
		const int expectedValue = 10; // Change this to your expected value
		var meterFactoryStub = new FakeIMeterFactory();
		var metrics = new DiskWatcherBackgroundTaskQueueMetrics(meterFactoryStub)
		{
			Value = expectedValue
		};

		// Act
		var observedValue = metrics.ObserveValue();

		// Assert
		Assert.AreEqual(expectedValue, observedValue);
	}
}
