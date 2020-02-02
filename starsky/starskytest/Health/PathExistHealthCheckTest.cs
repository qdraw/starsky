using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Health;
using starskytest.FakeCreateAn;

namespace starskytest.Health
{
	[TestClass]
	public class PathExistHealthCheckTest
	{
		[TestMethod]
		public async Task RunSuccessful()
		{
			var pathExistOptions = new PathExistOptions();
			pathExistOptions.AddPath(new CreateAnImage().BasePath);

			var healthCheck = new HealthCheckContext();
			var result = await new PathExistHealthCheck(pathExistOptions).CheckHealthAsync(healthCheck);
			Assert.AreEqual(HealthStatus.Healthy,result.Status);
		}
		
		[TestMethod]
		public async Task RunFailNonExistPath()
		{
			var pathExistOptions = new PathExistOptions();
			pathExistOptions.AddPath("000000000000----non-exist");

			var healthCheck = new HealthCheckContext {Registration = new HealthCheckRegistration("te",new PathExistHealthCheck(pathExistOptions), null,null )};
			var result = await new PathExistHealthCheck(pathExistOptions).CheckHealthAsync(healthCheck);
			Assert.AreEqual(HealthStatus.Unhealthy,result.Status);
		}
	}
}
