using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.health.HealthCheck;
using starskytest.FakeCreateAn;

namespace starskytest.starsky.feature.health.HealthCheck
{
	[TestClass]
	public class PathExistHealthCheckTest
	{
		[TestMethod]
		public async Task RunSuccessful()
		{
			var pathExistOptions = new PathExistOptions();
			pathExistOptions.AddPath(new CreateAnImage().BasePath);

			var healthCheck = new HealthCheckContext {Registration = new HealthCheckRegistration("te",new PathExistHealthCheck(pathExistOptions), null,null )};
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
		
		[TestMethod]
		public async Task RunFail_No_Input()
		{
			var pathExistOptions = new PathExistOptions();
			var healthCheck = new HealthCheckContext {Registration = new HealthCheckRegistration("te",new PathExistHealthCheck(pathExistOptions), null,null )};
			var result = await new PathExistHealthCheck(pathExistOptions).CheckHealthAsync(healthCheck);
			Assert.AreEqual(HealthStatus.Unhealthy,result.Status);
		}
	}
}
