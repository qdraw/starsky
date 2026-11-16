using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.health.HealthCheck;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Services;
using starsky.foundation.platform.Tenancy;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.health.HealthCheck;

[TestClass]
public sealed class TenantHealthCheckTest
{
	private readonly string _root =
		Path.Combine(Path.GetTempPath(), "starsky-tenant-health-" + Guid.NewGuid().ToString("N"));

	public TestContext TestContext { get; set; } = null!;

	[TestCleanup]
	public void Cleanup()
	{
		TenantAmbient.Reset();
		if ( Directory.Exists(_root) )
		{
			Directory.Delete(_root, true);
		}
	}

	private async Task<HealthReport> Check()
	{
		var appSettings = new AppSettings
		{
			StorageFolder = Path.Combine(_root, "default", "storage"),
			ThumbnailTempFolder = Path.Combine(_root, "default", "thumbs"),
			TempFolder = Path.Combine(_root, "default", "temp"),
			TenantsRoot = Path.Combine(_root, "tenants"),
			DatabaseType = AppSettings.DatabaseTypeList.InMemoryDatabase
		};
		// only the default tenant has folders, the folders of acme are never created
		Directory.CreateDirectory(appSettings.StorageFolder);
		Directory.CreateDirectory(appSettings.ThumbnailTempFolder);
		Directory.CreateDirectory(appSettings.TempFolder);

		var services = new ServiceCollection();
		services.AddSingleton<IWebLogger, WebLogger>();
		services.AddLogging();
		services.AddSingleton<ITenantSettingsProvider>(new TenantSettingsProvider(appSettings,
			new FakeTenantRegistry("default", "acme")));
		new SetupHealthCheck(appSettings, services).BuilderHealth();

		await using var provider = services.BuildServiceProvider();
		return await provider.GetRequiredService<HealthCheckService>()
			.CheckHealthAsync(r => r.Name.StartsWith("Exist_"), TestContext.CancellationTokenSource.Token);
	}

	[TestMethod]
	public async Task Exist_DefaultTenant_Healthy()
	{
		var report = await Check();

		Assert.AreEqual(HealthStatus.Healthy,
			report.Entries["Exist_StorageFolder"].Status);
	}

	[TestMethod]
	public async Task Exist_OtherTenantWithMissingFolders_Unhealthy()
	{
		TenantAmbient.Set("acme");

		var report = await Check();

		Assert.AreEqual(HealthStatus.Unhealthy,
			report.Entries["Exist_StorageFolder"].Status);
		Assert.IsTrue(report.Entries.Keys.Contains("Exist_TempFolder"));
	}
}
