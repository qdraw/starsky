using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.health.HealthCheck;
using starskytest.FakeCreateAn;

namespace starskytest.starsky.feature.health.HealthCheck
{
	[TestClass]
	public class DiskStorageHealthCheckExtensionsTest
	{
		[TestMethod]
		public void CheckIfServiceExist()
		{
			var services = new ServiceCollection();
			services
				.AddHealthChecks()
				.AddDiskStorageHealthCheck(diskOptions => { DiskOptionsPercentageSetup.Setup(new CreateAnImage().BasePath,diskOptions); },
					name: "ThumbnailTempFolder");
	
			if ( services.All(x => x.ServiceType != typeof(HealthCheckService)) )
			{
				// Service doesn't exist, do something
				throw new ArgumentException("missing service");
			}
		}
	} 
}
