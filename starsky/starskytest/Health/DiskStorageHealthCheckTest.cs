using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Health;
using starskycore.Models;

namespace starskytest.Health
{
	[TestClass]
	public class DiskStorageHealthCheckTest
	{
		[TestMethod]
		public async Task RunSuccessful()
		{
			var appSettings = new AppSettings();
			var diskOptions = new DiskStorageOptions();
			new DiskOptionsPercentageSetup().Setup(appSettings.TempFolder,diskOptions,0.000001f);

			var healthCheck = new HealthCheckContext();
			var result = await new DiskStorageHealthCheck(diskOptions).CheckHealthAsync(healthCheck);
			Assert.AreEqual(HealthStatus.Healthy,result.Status);
		}
		
		[TestMethod]
		public async Task RunFailNotEnoughSpace()
		{
			var appSettings = new AppSettings();
			var diskOptions = new DiskStorageOptions();
			
			new DiskOptionsPercentageSetup().Setup(appSettings.TempFolder,diskOptions,0.99f);

			var healthCheck = new HealthCheckContext {Registration = new HealthCheckRegistration("te",new DiskStorageHealthCheck(diskOptions),null,null )};
			var result = await new DiskStorageHealthCheck(diskOptions).CheckHealthAsync(healthCheck);
			Assert.AreEqual(HealthStatus.Unhealthy,result.Status);
			Assert.IsTrue(result.Description.Contains("Minimum configured megabytes for disk"));
		}
		
		[TestMethod]
		public async Task RunFailNonExistDisk()
		{
			var diskOptions = new DiskStorageOptions();
			diskOptions.AddDrive("NonExistDisk:", 10);
			
			var healthCheck = new HealthCheckContext {Registration = new HealthCheckRegistration("te",new DiskStorageHealthCheck(diskOptions),null,null )};
			var result = await new DiskStorageHealthCheck(diskOptions).CheckHealthAsync(healthCheck);
			Assert.AreEqual(HealthStatus.Unhealthy,result.Status);
			Assert.IsTrue(result.Description.Contains("is not present on system"));
		}
	}
}
