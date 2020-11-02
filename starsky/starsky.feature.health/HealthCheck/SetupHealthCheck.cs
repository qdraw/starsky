using System;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.database.Data;
using starsky.foundation.platform.Models;

namespace starsky.feature.health.HealthCheck
{
	public class SetupHealthCheck
	{
		private readonly IServiceCollection _services;
		private readonly AppSettings _appSettings;

		public SetupHealthCheck(AppSettings appSettings, IServiceCollection services)
		{
			_services = services;
			_appSettings = appSettings;
		}

		/// <summary>
		/// Enable .NET CORE health checks
		/// </summary>
		/// <exception cref="ArgumentOutOfRangeException">when your type is not _appSettings.DatabaseType</exception>
		public void BuilderHealth()
		{
			_services.AddHealthChecks()
	            .AddDbContextCheck<ApplicationDbContext>()
	            .AddDiskStorageHealthCheck(
		            setup: diskOptions =>
		            {
			            new DiskOptionsPercentageSetup().Setup(_appSettings.StorageFolder,
				            diskOptions);
		            },
		            name: "Storage_StorageFolder")
	            .AddDiskStorageHealthCheck(
		            setup: diskOptions =>
		            {
			            new DiskOptionsPercentageSetup().Setup(_appSettings.ThumbnailTempFolder,
				            diskOptions);
		            },
		            name: "Storage_ThumbnailTempFolder")
	            .AddDiskStorageHealthCheck(
		            setup: diskOptions =>
		            {
			            new DiskOptionsPercentageSetup().Setup(_appSettings.TempFolder,
				            diskOptions);
		            },
		            name: "Storage_TempFolder")
	            .AddPathExistHealthCheck(
		            setup: pathOptions => pathOptions.AddPath(_appSettings.StorageFolder),
		            name: "Exist_StorageFolder")
	            .AddPathExistHealthCheck(
		            setup: pathOptions => pathOptions.AddPath(_appSettings.TempFolder),
		            name: "Exist_TempFolder")
	            .AddPathExistHealthCheck(
		            setup: pathOptions => pathOptions.AddPath(_appSettings.ExifToolPath),
		            name: "Exist_ExifToolPath")
	            .AddPathExistHealthCheck(
		            setup: pathOptions => pathOptions.AddPath(_appSettings.ThumbnailTempFolder),
		            name: "Exist_ThumbnailTempFolder")
	            .AddCheck<DateAssemblyHealthCheck>("DateAssemblyHealthCheck");
            
            var healthSqlQuery = "SELECT * FROM `__EFMigrationsHistory` WHERE ProductVersion > 9";

            switch (_appSettings.DatabaseType)
            {
                case (AppSettings.DatabaseTypeList.Mysql):
                    _services.AddHealthChecks().AddMySql(_appSettings.DatabaseConnection);
                    break;
                case AppSettings.DatabaseTypeList.Sqlite:
	                _services.AddHealthChecks().AddSqlite(_appSettings.DatabaseConnection, healthSqlQuery, "sqlite");
                    break;
                case AppSettings.DatabaseTypeList.InMemoryDatabase:
	                break;
                default:
	                throw new AggregateException("database type does not exist");
            }
            
		}
	}
}
