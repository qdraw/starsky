using starsky.foundation.worker.CpuEventListener.Interfaces;

namespace starskytest.FakeMocks;

public class FakeICpuUsageListener : ICpuUsageListener
{
	public FakeICpuUsageListener(double lastValue = 0)
	{
		CpuUsageMean = lastValue;
	}

	public double CpuUsageMean { get; set; }
}
