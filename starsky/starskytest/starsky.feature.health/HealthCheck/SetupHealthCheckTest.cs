using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.health.HealthCheck;
using starsky.foundation.database.Data;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Services;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.health.HealthCheck;

[TestClass]
public sealed class SetupHealthCheckTest
{
	[TestMethod]
	public void HealthCheckService_LoggerMissing_InvalidOperationException()
	{
		var services = new ServiceCollection();
		// logger is not defined here (as designed)

		Assert.ThrowsException<InvalidOperationException>(() =>
			new SetupHealthCheck(new AppSettings(), services).BuilderHealth());
	}

	[TestMethod]
	public void HealthCheckServiceMysql_LoggerMissing_InvalidOperationException()
	{
		var services = new ServiceCollection();
		// logger is not defined here (as designed)

		Assert.ThrowsException<InvalidOperationException>(() => new SetupHealthCheck(
				new AppSettings { DatabaseType = AppSettings.DatabaseTypeList.Mysql }, services)
			.BuilderHealth());
	}

	[TestMethod]
	public async Task HealthCheckServiceMysql_ReturnStatus()
	{
		var services = new ServiceCollection();
		services.AddSingleton<IWebLogger, WebLogger>();
		services.AddLogging();
		services.AddHealthChecks();
		var efServiceProvider = new ServiceCollection().AddEntityFrameworkInMemoryDatabase()
			.BuildServiceProvider();
		services
			.AddDbContext<ApplicationDbContext>(b =>
				b.UseInMemoryDatabase("HealthCheckServiceMysql_ReturnStatus")
					.UseInternalServiceProvider(efServiceProvider));

		new SetupHealthCheck(
				new AppSettings { DatabaseType = AppSettings.DatabaseTypeList.Mysql }, services)
			.BuilderHealth();

		var serviceProvider = services.BuildServiceProvider();
		var service = serviceProvider.GetRequiredService<HealthCheckService>();

		var result = await service.CheckHealthAsync();

		Assert.AreEqual(1, result.Entries.Count(p => p.Key == "mysql"));
		Assert.AreEqual(HealthStatus.Unhealthy,
			result.Entries.FirstOrDefault(p => p.Key == "mysql").Value.Status);
		Assert.AreEqual(0, result.Entries.Count(p => p.Key == "sqlite"));
	}

	[TestMethod]
	public async Task HealthCheckServiceInMemoryDatabase_ReturnStatus()
	{
		var services = new ServiceCollection();
		services.AddSingleton<IWebLogger, WebLogger>();
		services.AddLogging();
		services.AddHealthChecks();
		var efServiceProvider = new ServiceCollection().AddEntityFrameworkInMemoryDatabase()
			.BuildServiceProvider();
		services
			.AddDbContext<ApplicationDbContext>(b =>
				b.UseInMemoryDatabase("HealthCheckServiceInMemoryDatabase_ReturnStatus")
					.UseInternalServiceProvider(efServiceProvider));

		new SetupHealthCheck(
				new AppSettings { DatabaseType = AppSettings.DatabaseTypeList.InMemoryDatabase },
				services)
			.BuilderHealth();

		var serviceProvider = services.BuildServiceProvider();
		var service = serviceProvider.GetRequiredService<HealthCheckService>();

		var result = await service.CheckHealthAsync();

		Assert.AreEqual(0, result.Entries.Count(p => p.Key == "mysql"));
		Assert.AreEqual(0, result.Entries.Count(p => p.Key == "sqlite"));
	}

	[TestMethod]
	public async Task HealthCheckServiceSqlite_ReturnStatus()
	{
		var services = new ServiceCollection();
		services.AddSingleton<IWebLogger, WebLogger>();
		services.AddLogging();
		services.AddHealthChecks();
		var efServiceProvider = new ServiceCollection().AddEntityFrameworkInMemoryDatabase()
			.BuildServiceProvider();
		services
			.AddDbContext<ApplicationDbContext>(b =>
				b.UseInMemoryDatabase("HealthCheckServiceSqlite_ReturnStatus")
					.UseInternalServiceProvider(efServiceProvider));

		new SetupHealthCheck(
				new AppSettings { DatabaseType = AppSettings.DatabaseTypeList.Sqlite },
				services)
			.BuilderHealth();

		var serviceProvider = services.BuildServiceProvider();
		var service = serviceProvider.GetRequiredService<HealthCheckService>();

		var result = await service.CheckHealthAsync();

		Assert.AreEqual(1, result.Entries.Count(p => p.Key == "sqlite"));
		Assert.AreEqual(HealthStatus.Unhealthy,
			result.Entries.FirstOrDefault(p => p.Key == "sqlite").Value.Status);
		Assert.AreEqual(0, result.Entries.Count(p => p.Key == "mysql"));
	}

	[TestMethod]
	public void HealthCheckService_AggregateException()
	{
		var appSettings = new AppSettings { Verbose = true };
		AppSettingsReflection.Modify(appSettings, "get_DatabaseType", 8);
		var services = new ServiceCollection();
		services.AddSingleton<IWebLogger, FakeIWebLogger>();

		// expect exception database type is not found
		Assert.ThrowsException<AggregateException>(() =>
			new SetupHealthCheck(appSettings, services).BuilderHealth());
	}
}
