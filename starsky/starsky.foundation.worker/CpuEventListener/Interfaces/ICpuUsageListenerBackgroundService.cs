using Microsoft.Extensions.Hosting;

namespace starsky.foundation.worker.CpuEventListener.Interfaces;

public interface ICpuUsageListenerBackgroundService : IHostedService
{
	/// <summary>
	/// Last CPU usage
	/// </summary>
	double CpuUsageMean { get; }
}
