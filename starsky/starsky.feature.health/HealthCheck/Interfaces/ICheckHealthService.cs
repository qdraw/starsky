using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using starsky.project.web.ViewModels;

namespace starsky.feature.health.HealthCheck.Interfaces;

public interface ICheckHealthService
{
	Task<HealthReport> CheckHealthWithTimeoutAsync(int timeoutTime = 15000);

	HealthView CreateHealthEntryLog(HealthReport result);
}
