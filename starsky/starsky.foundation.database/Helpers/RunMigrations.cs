using System;
using System.Collections.Generic;
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
			var dbContext = serviceScope.ServiceProvider.GetService<ApplicationDbContext>();
			var logger = serviceScope.ServiceProvider.GetService<IWebLogger>();
			var appSettings = serviceScope.ServiceProvider.GetService<AppSettings>();

			await Run(dbContext,logger,appSettings,retryCount);
		}
		
		public static async Task Run(ApplicationDbContext dbContext, IWebLogger logger, AppSettings appSettings, int retryCount = 2)
		{
			async Task<bool> Migrate()
			{
				await dbContext.Database.MigrateAsync();

				if ( appSettings.DatabaseType == AppSettings.DatabaseTypeList.Mysql )
				{
					var connection = new MySqlConnection(appSettings.DatabaseConnection);
					await new FixMySqlAutoIncrement(connection,appSettings).AutoIncrement("Notifications");
				}

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
				logger.LogError("end catch-ed");
			}
		}
		

	}
}
