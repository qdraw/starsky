using System;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.database.Data;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;

namespace starsky.feature.health.HealthCheck;

public class SetupHealthCheck(AppSettings appSettings, IServiceCollection services)
{
	/// <summary>
	///     Enable .NET health checks
	/// </summary>
	/// <exception cref="AggregateException">when your type is not _appSettings.DatabaseType</exception>
	public void BuilderHealth()
	{
		var logger = services.BuildServiceProvider().GetRequiredService<IWebLogger>();

		services.AddHealthChecks()
			.AddDbContextCheck<ApplicationDbContext>()
			.AddDiskStorageHealthCheck(
				diskOptions =>
				{
					DiskOptionsPercentageSetup.Setup(appSettings.StorageFolder,
						diskOptions);
				},
				"Storage_StorageFolder")
			.AddDiskStorageHealthCheck(
				diskOptions =>
				{
					DiskOptionsPercentageSetup.Setup(appSettings.ThumbnailTempFolder,
						diskOptions);
				},
				"Storage_ThumbnailTempFolder")
			.AddDiskStorageHealthCheck(
				diskOptions =>
				{
					DiskOptionsPercentageSetup.Setup(appSettings.TempFolder,
						diskOptions);
				},
				"Storage_TempFolder")
			.AddPathExistHealthCheck(
				pathOptions => pathOptions.AddPath(appSettings.StorageFolder),
				name: "Exist_StorageFolder", logger: logger)
			.AddPathExistHealthCheck(
				pathOptions => pathOptions.AddPath(appSettings.TempFolder),
				name: "Exist_TempFolder", logger: logger)
			.AddPathExistHealthCheck(
				pathOptions => pathOptions.AddPath(appSettings.ExifToolPath),
				name: "Exist_ExifToolPath", logger: logger)
			.AddPathExistHealthCheck(
				pathOptions => pathOptions.AddPath(appSettings.ThumbnailTempFolder),
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
