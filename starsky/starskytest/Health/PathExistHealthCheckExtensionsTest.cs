using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Health;

namespace starskytest.Health
{
	[TestClass]
	public class PathExistHealthCheckExtensionsTest
	{
		[TestMethod]
		public void CheckIfServiceExist()
		{
			var services = new ServiceCollection();
			services
				.AddHealthChecks()
				.AddPathExistHealthCheck(
					setup: pathOptions => pathOptions.AddPath("non---exist"),
					name: "Exist_ExifToolPath");
	
			if ( services.All(x => x.ServiceType != typeof(HealthCheckService)) )
			{
				// Service doesn't exist, do something
				throw new ArgumentException("missing service");
			}
		}
	} 
}

