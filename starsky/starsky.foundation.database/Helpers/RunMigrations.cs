using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.database.Data;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;

namespace starsky.foundation.database.Helpers
{
	public static class RunMigrations
	{
		public static async Task Run(IServiceScope serviceScope)
		{
			var dbContext = serviceScope.ServiceProvider.GetService<ApplicationDbContext>();
			var logger = serviceScope.ServiceProvider.GetService<IWebLogger>();

			await Run(dbContext,logger);
		}

		public static async Task Run(ApplicationDbContext dbContext, IWebLogger logger)
		{
			logger.LogInformation("[RunMigrations] start migration");
			async Task<bool> Migrate()
			{
				await dbContext.Database.MigrateAsync();
				return true;
			}
			
			try
			{
				await RetryHelper.DoAsync(Migrate, TimeSpan.FromSeconds(2),2);
			}
			catch (AggregateException exception)
			{
				logger.LogError(exception.Message);
				logger.LogError("end catch-ed");
			}
		}
	}
}
