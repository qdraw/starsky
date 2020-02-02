using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Health;
using starsky.Helpers;
using starskycore.Models;

namespace starskytest.Health
{
	[TestClass]
	public class DateAssemblyHealthCheckTest
	{
		[TestMethod]
		public async Task RunSuccesfull()
		{
			var healthCheck = new HealthCheckContext();
			var result = await new DateAssemblyHealthCheck().CheckHealthAsync(healthCheck);
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
		public void GetBuildDate_Starsky()
		{
			var date = DateAssemblyHealthCheck.GetBuildDate(typeof(starsky.Startup).Assembly);
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
