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
		public static async Task Run(IServiceScope serviceScope, int retryCount = 2)
		{
			var dbContext = serviceScope.ServiceProvider.GetService<ApplicationDbContext>();
			var logger = serviceScope.ServiceProvider.GetService<IWebLogger>();

			await Run(dbContext,logger,retryCount);
		}

		public static async Task Run(ApplicationDbContext dbContext, IWebLogger logger, int retryCount = 2)
		{
			logger.LogInformation("[RunMigrations] start migration");
			async Task<bool> Migrate()
			{
				await dbContext.Database.MigrateAsync();
				return true;
			}
			
			try
			{
				await RetryHelper.DoAsync(Migrate, TimeSpan.FromSeconds(2),retryCount);
			}
			catch (AggregateException exception)
			{
				logger.LogError(exception.Message);
				logger.LogError("end catch-ed");
			}
		}
	}
}
