using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;
using starsky.foundation.database.Data;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;

namespace starsky.foundation.database.Helpers
{
	public static class RunMigrations
	{
		public static async Task Run(IServiceScope serviceScope, int retryCount = 2)
		{
			var dbContext = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
			var logger = serviceScope.ServiceProvider.GetRequiredService<IWebLogger>();
			var appSettings = serviceScope.ServiceProvider.GetRequiredService<AppSettings>();

			await Run(dbContext,logger,appSettings,retryCount);
		}
		
		public static async Task Run(ApplicationDbContext dbContext, IWebLogger logger, AppSettings appSettings, int retryCount = 2)
		{
			async Task<bool> Migrate()
			{
				if ( appSettings.DatabaseType == AppSettings.DatabaseTypeList.InMemoryDatabase )
				{
					return true;
				}
				
				await dbContext.Database.MigrateAsync();

				if ( appSettings.DatabaseType !=
				     AppSettings.DatabaseTypeList.Mysql ) return true;
				
				var connection = new MySqlConnection(appSettings.DatabaseConnection);
				var databaseFixes =
					new MySqlDatabaseFixes(connection, appSettings);
				await databaseFixes.OpenConnection();
					
				var tableNames = dbContext.Model.GetEntityTypes()
					.Select(t => t.GetTableName())
					.Distinct()
					.ToList();
				await databaseFixes.FixUtf8Encoding(tableNames);
				await databaseFixes.FixAutoIncrement("Notifications");

				return true;
			}
			
			try
			{
				await RetryHelper.DoAsync(Migrate, TimeSpan.FromSeconds(2),retryCount);
			}
			catch (AggregateException exception)
			{
				logger.LogInformation("[RunMigrations] start migration failed");
				logger.LogError(exception.Message);
				logger.LogError(exception.InnerException?.Message);
				logger.LogError("[RunMigrations] end catch-ed");
			}
		}
		

	}
}
