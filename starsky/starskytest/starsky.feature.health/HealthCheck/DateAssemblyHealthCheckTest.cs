using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.health.HealthCheck;

namespace starskytest.starsky.feature.health.HealthCheck;

[TestClass]
public sealed class DateAssemblyHealthCheckTest
{
	[TestMethod]
	public async Task RunCheckHealthAsync()
	{
		var healthCheck = new HealthCheckContext();
		var result = await new DateAssemblyHealthCheck().CheckHealthAsync(healthCheck);

		if ( result.Status == HealthStatus.Unhealthy )
		{
			Console.WriteLine(result.Description);
		}

		Assert.AreEqual(HealthStatus.Healthy, result.Status);
	}
}
