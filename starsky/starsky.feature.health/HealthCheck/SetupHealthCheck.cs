using System;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.database.Data;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Tenancy;

namespace starsky.feature.health.HealthCheck;

public class SetupHealthCheck(AppSettings appSettings, IServiceCollection services)
{
	/// <summary>
	///     The folders of the tenant of the request: a full or missing disk of another tenant
	///     must show up there, not in the default tenant
	/// </summary>
	private ITenantSettings Tenant(IServiceProvider sp)
	{
		return sp.GetService<ITenantSettingsProvider>()?.GetSettings(TenantAmbient.Current) ??
		       new TenantSettings(TenantIdHelper.DefaultTenantId, appSettings);
	}

	/// <summary>
	///     Enable .NET health checks
	/// </summary>
	/// <exception cref="AggregateException">when your type is not _appSettings.DatabaseType</exception>
	public void BuilderHealth()
	{
		var logger = services.BuildServiceProvider().GetRequiredService<IWebLogger>();

		services.AddHealthChecks()
			.AddDbContextCheck<ApplicationDbContext>()
			.AddTenantDiskStorageHealthCheck(
				(sp, diskOptions) =>
				{
					DiskOptionsPercentageSetup.Setup(Tenant(sp).StorageFolder,
						diskOptions);
				},
				"Storage_StorageFolder")
			.AddTenantDiskStorageHealthCheck(
				(sp, diskOptions) =>
				{
					DiskOptionsPercentageSetup.Setup(Tenant(sp).ThumbnailTempFolder,
						diskOptions);
				},
				"Storage_ThumbnailTempFolder")
			.AddTenantDiskStorageHealthCheck(
				(sp, diskOptions) =>
				{
					DiskOptionsPercentageSetup.Setup(Tenant(sp).TempFolder,
						diskOptions);
				},
				"Storage_TempFolder")
			.AddTenantPathExistHealthCheck(
				(sp, pathOptions) => pathOptions.AddPath(Tenant(sp).StorageFolder),
				name: "Exist_StorageFolder", logger: logger)
			.AddTenantPathExistHealthCheck(
				(sp, pathOptions) => pathOptions.AddPath(Tenant(sp).TempFolder),
				name: "Exist_TempFolder", logger: logger)
			.AddPathExistHealthCheck(
				pathOptions => pathOptions.AddPath(appSettings.ExifToolPath),
				name: "Exist_ExifToolPath", logger: logger)
			.AddTenantPathExistHealthCheck(
				(sp, pathOptions) => pathOptions.AddPath(Tenant(sp).ThumbnailTempFolder),
				name: "Exist_ThumbnailTempFolder", logger: logger)
			.AddCheck<DateAssemblyHealthCheck>("DateAssemblyHealthCheck");

		var healthSqlQuery = "SELECT * FROM `__EFMigrationsHistory` WHERE ProductVersion > 9";

		switch ( appSettings.DatabaseType )
		{
			case AppSettings.DatabaseTypeList.Mysql:
				services.AddHealthChecks().AddMySql(appSettings.DatabaseConnection);
				break;
			case AppSettings.DatabaseTypeList.Sqlite:
				services.AddHealthChecks()
					.AddSqlite(appSettings.DatabaseConnection, healthSqlQuery);
				break;
			case AppSettings.DatabaseTypeList.InMemoryDatabase:
				break;
			default:
				throw new AggregateException("database type does not exist");
		}
	}
}
