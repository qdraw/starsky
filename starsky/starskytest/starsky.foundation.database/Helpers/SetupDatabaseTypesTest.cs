using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Data;
using starsky.foundation.database.Helpers;
using starsky.foundation.platform.Models;

namespace starskytest.starsky.foundation.database.Helpers
{
	[TestClass]
	public class SetupDatabaseTypesTest
	{
		[TestMethod]
		public void CheckIfMysqlScopeIsThere()
		{
			var services = new ServiceCollection();
			new SetupDatabaseTypes(new AppSettings
			{
				DatabaseType = AppSettings.DatabaseTypeList.Mysql
			}, services).BuilderDb();
			
			var serviceProvider = services.BuildServiceProvider();
			var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();
			Assert.IsNotNull(dbContext);
		}
		
		[TestMethod]
		public void CheckIfSqliteScopeIsThere()
		{
			var services = new ServiceCollection();
			new SetupDatabaseTypes(new AppSettings
			{
				DatabaseType = AppSettings.DatabaseTypeList.Sqlite
			}, services).BuilderDb();
			
			var serviceProvider = services.BuildServiceProvider();
			var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();
			Assert.IsNotNull(dbContext);
		}
	}
}
