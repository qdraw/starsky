using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.database.Data;

namespace starsky.foundation.database.Helpers
{
	public static class RunMigrations
	{
		public static async Task Run(IServiceScope serviceScope)
		{
			var dbContext = serviceScope.ServiceProvider.GetService<ApplicationDbContext>();
			await Run(dbContext);
		}

		public static async Task Run(ApplicationDbContext dbContext)
		{
			try
			{
				await dbContext.Database.MigrateAsync();
			}
			catch (MySql.Data.MySqlClient.MySqlException e)
			{
				Console.WriteLine(e);
			}
		}
	}
}
