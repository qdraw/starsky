using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace starskytest.FakeMocks
{
	public class FakeHealthCheckService : HealthCheckService
	{
		private readonly bool _isHealthy;
		
		public FakeHealthCheckService(bool isHealthy)
		{
			_isHealthy = isHealthy;
		}
		
		public override Task<HealthReport> CheckHealthAsync(Func<HealthCheckRegistration, bool> predicate,
			CancellationToken cancellationToken = new CancellationToken())
		{
			var entry = new HealthReportEntry(_isHealthy ? HealthStatus.Healthy : HealthStatus.Unhealthy, "", TimeSpan.Zero, null, null);
			var dictionary = new Dictionary<string, HealthReportEntry>
		    {
				{ "test", entry }
			};
				
			return Task.FromResult(new HealthReport(dictionary, TimeSpan.Zero));
		}
	}
}
