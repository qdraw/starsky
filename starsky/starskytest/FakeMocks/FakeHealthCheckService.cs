using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace starskytest.FakeMocks
{
	public class FakeHealthCheckService : HealthCheckService
	{
		public FakeHealthCheckService()
		{
			
		}
		
		public override Task<HealthReport> CheckHealthAsync(Func<HealthCheckRegistration, bool> predicate,
			CancellationToken cancellationToken = new CancellationToken())
		{
			var entry = new HealthReportEntry(HealthStatus.Healthy, "", TimeSpan.Zero, null, null);
			var dictionary = new Dictionary<string, HealthReportEntry>
		    {
				{ "test", entry }
			};
				
			return Task.FromResult(new HealthReport(dictionary, TimeSpan.Zero));
		}
	}
}
