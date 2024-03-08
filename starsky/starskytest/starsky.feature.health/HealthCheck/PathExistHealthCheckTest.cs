using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.health.HealthCheck;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.health.HealthCheck
{
	[TestClass]
	public sealed class PathExistHealthCheckTest
	{
		[TestMethod]
		public async Task RunSuccessful()
		{
			var pathExistOptions = new PathExistOptions();
			pathExistOptions.AddPath(new CreateAnImage().BasePath);

			var healthCheck = new HealthCheckContext
			{
				Registration = new HealthCheckRegistration("te",
					new PathExistHealthCheck(pathExistOptions, new FakeIWebLogger()), null, null)
			};
			var result =
				await new PathExistHealthCheck(pathExistOptions, new FakeIWebLogger())
					.CheckHealthAsync(healthCheck);
			Assert.AreEqual(HealthStatus.Healthy, result.Status);
		}

		[TestMethod]
		public async Task RunFailNonExistPath()
		{
			var pathExistOptions = new PathExistOptions();
			pathExistOptions.AddPath("000000000000----non-exist");

			var healthCheck = new HealthCheckContext
			{
				Registration = new HealthCheckRegistration("te",
					new PathExistHealthCheck(pathExistOptions, new FakeIWebLogger()), null, null)
			};
			var result =
				await new PathExistHealthCheck(pathExistOptions, new FakeIWebLogger())
					.CheckHealthAsync(healthCheck);
			Assert.AreEqual(HealthStatus.Unhealthy, result.Status);
		}

		[TestMethod]
		public async Task RunFail_No_Input()
		{
			var pathExistOptions = new PathExistOptions();
			var healthCheck = new HealthCheckContext
			{
				Registration = new HealthCheckRegistration("te",
					new PathExistHealthCheck(pathExistOptions, new FakeIWebLogger()), null, null)
			};
			var result =
				await new PathExistHealthCheck(pathExistOptions, new FakeIWebLogger())
					.CheckHealthAsync(healthCheck);
			Assert.AreEqual(HealthStatus.Unhealthy, result.Status);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public async Task RunFail_Null_Input()
		{
			var healthCheck = new HealthCheckContext
			{
				Registration = new HealthCheckRegistration("te",
					new PathExistHealthCheck(null!, new FakeIWebLogger()),
					null, null)
			};
			await new PathExistHealthCheck(null!, new FakeIWebLogger()).CheckHealthAsync(
				healthCheck);
			// expect ArgumentNullException:
		}
	}
}
