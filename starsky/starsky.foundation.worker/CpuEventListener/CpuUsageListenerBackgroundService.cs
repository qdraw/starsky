using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.worker.CpuEventListener.Interfaces;

namespace starsky.foundation.worker.CpuEventListener;

[Service(typeof(ICpuUsageListenerBackgroundService),
	InjectionLifetime = InjectionLifetime.Singleton)]
public class CpuUsageListenerBackgroundService : ICpuUsageListenerBackgroundService
{
	private readonly IWebLogger _logger;

	public CpuUsageListenerBackgroundService(IWebLogger logger)
	{
		_logger = logger;
	}
	
	private CpuUsageListener? _cpuListener;
	
	public double LastValue => _cpuListener?.LastValue ?? 0;
	
	public Task StartAsync(CancellationToken cancellationToken)
	{
		_cpuListener = new CpuUsageListener(_logger);
		return Task.CompletedTask;
	}

	public Task StopAsync(CancellationToken cancellationToken)
	{
		_cpuListener?.Dispose();
		return Task.CompletedTask;
	}
}
