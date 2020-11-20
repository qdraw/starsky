using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.health.HealthCheck;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.health.HealthCheck
{
	[TestClass]
	public class SetupHealthCheckTest
	{
	
		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public void HealthCheckService()
		{
			var services = new ServiceCollection();

			new SetupHealthCheck(new AppSettings(), services).BuilderHealth();
			
			var serviceProvider = services.BuildServiceProvider();
			serviceProvider.GetRequiredService<HealthCheckService>();
			// logger not defined
		}
		
		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public void HealthCheckServiceMysql()
		{
			var services = new ServiceCollection();

			new SetupHealthCheck(new AppSettings
			{
				DatabaseType = AppSettings.DatabaseTypeList.Mysql
			}, services).BuilderHealth();
			
			var serviceProvider = services.BuildServiceProvider();
			serviceProvider.GetRequiredService<HealthCheckService>();
			// logger not defined
		}

		[TestMethod]
		[ExpectedException(typeof(AggregateException))]
		public void HealthCheckService_AggregateException()
		{
			var appSettings = new AppSettings {Verbose = true};
			AppSettingsReflection.Modify(appSettings, "get_DatabaseType", 8);
			var services = new ServiceCollection();
			new SetupHealthCheck(appSettings, services).BuilderHealth();
			// expect exception database type is not found
		}
	}
}
