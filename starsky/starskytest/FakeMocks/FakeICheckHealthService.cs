using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using starsky.feature.health.HealthCheck.Interfaces;
using starsky.feature.health.HealthCheck.Service;
using starsky.foundation.platform.Extensions;
using starsky.foundation.platform.Interfaces;
using starsky.project.web.ViewModels;

namespace starskytest.FakeMocks;

public class FakeICheckHealthService : ICheckHealthService
{
	private readonly FakeHealthCheckService _fakeHealthCheckService;
	private readonly IWebLogger _logger;

	public FakeICheckHealthService(IWebLogger logger,
		FakeHealthCheckService? fakeHealthCheckService)
	{
		_fakeHealthCheckService = fakeHealthCheckService ?? new FakeHealthCheckService(false);
		_logger = logger;
	}

	public async Task<HealthReport> CheckHealthWithTimeoutAsync(int timeoutTime = 15000)
	{
		return await _fakeHealthCheckService.CheckHealthAsync().TimeoutAfter(timeoutTime);
	}

	public HealthView CreateHealthEntryLog(HealthReport result)
	{
		return new CheckHealthService(null!, _logger).CreateHealthEntryLog(result);
	}
}
