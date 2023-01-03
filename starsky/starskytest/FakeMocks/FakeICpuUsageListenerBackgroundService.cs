using System.Threading;
using System.Threading.Tasks;
using starsky.foundation.worker.CpuEventListener.Interfaces;

namespace starskytest.FakeMocks;

public class FakeICpuUsageListenerBackgroundService : ICpuUsageListenerBackgroundService
{
	public FakeICpuUsageListenerBackgroundService(double lastValue = 0)
	{
		LastValue = lastValue;
	}

	public Task StartAsync(CancellationToken cancellationToken)
	{
		return Task.CompletedTask;
	}

	public Task StopAsync(CancellationToken cancellationToken)
	{
		return Task.CompletedTask;
	}

	public double LastValue { get; set; }
}
