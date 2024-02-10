using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.worker.Metrics;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.worker.Metrics;

[TestClass]
public class ThumbnailBackgroundQueuedMetricsTest
{
	[TestMethod]
	public void Thumbnail_ObserveValue_Returns_Correct_Value()
	{
		// Arrange
		const int expectedObserveValue = 10; // Change this to your expected value
		var meterFactoryStub = new FakeIMeterFactory();
		var metrics = new ThumbnailBackgroundQueuedMetrics(meterFactoryStub)
		{
			Value = expectedObserveValue
		};

		// Act
		var observedValue = metrics.ObserveValue();

		// Assert
		Assert.AreEqual(expectedObserveValue, observedValue);
	}
}
