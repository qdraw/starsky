using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using starsky.foundation.platform.Helpers;

[assembly: InternalsVisibleTo("starskytest")]
namespace starsky.feature.health.HealthCheck
{
	public class DateAssemblyHealthCheck : IHealthCheck
	{
		/// <summary>
		/// Check the date assembly health status
		/// </summary>
		/// <param name="context">HealthCheckContext</param>
		/// <param name="cancellationToken">to cancel item</param>
		/// <returns></returns>
		// Example:
		// .AddCheck<DateAssemblyHealthCheck>("DateAssemblyHealthCheck")
		public Task<HealthCheckResult> CheckHealthAsync(
			HealthCheckContext context,
			CancellationToken cancellationToken = default(CancellationToken))
		{
			var assemblyDate = DateAssembly.GetBuildDate(Assembly.GetExecutingAssembly());
			return Task.FromResult(assemblyDate.AddDays(-2) > DateTime.UtcNow ? 
				HealthCheckResult.Unhealthy($"Current Date {assemblyDate.AddDays(-2)}>{DateTime.UtcNow} is earlier then the Assembly is build") : 
				HealthCheckResult.Healthy("Current Date is after the Assembly is build :)"));
		}

	}

}
