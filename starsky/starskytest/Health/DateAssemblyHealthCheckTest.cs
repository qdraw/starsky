using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky;
using starsky.Helpers;

namespace starskytest.Health
{
	[TestClass]
	public class DateAssemblyHealthCheckTest
	{
		[TestMethod]
		public async Task RunCheckHealthAsync()
		{
			var healthCheck = new HealthCheckContext();
			var result = await new DateAssemblyHealthCheck().CheckHealthAsync(healthCheck);

			if ( result.Status == HealthStatus.Unhealthy ) Console.WriteLine(result.Description);
			
			Assert.AreEqual(HealthStatus.Healthy,result.Status);
		}
		
		[TestMethod]
		public void DateAssemblyHealthCheck_GetBuildDate()
		{
			// this gets the one from the test assembly
			var date = DateAssemblyHealthCheck.GetBuildDate(Assembly.GetExecutingAssembly());
			Assert.IsTrue(date.Year == 1 );
		}
		
		[TestMethod]
		public void GetBuildDate_StarskyStartup()
		{
			var date = DateAssemblyHealthCheck.GetBuildDate(typeof(Startup).Assembly);
			Assert.IsTrue(date.Year >= 2020 );
		}
		
		[TestMethod]
		public void GetBuildDate_NonExist()
		{
			var date = DateAssemblyHealthCheck.GetBuildDate(typeof(short).Assembly);
			Assert.IsTrue(date.Year == 1 );
		}

	}
}
