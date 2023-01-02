using Microsoft.Extensions.Hosting;

namespace starsky.foundation.worker.CpuEventListener.Interfaces;

public interface ICpuUsageListenerBackgroundService : IHostedService
{
	double LastValue { get; }
}
