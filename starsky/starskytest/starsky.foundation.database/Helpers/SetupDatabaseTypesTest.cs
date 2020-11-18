using System;
using System.Reflection;
using System.Reflection.Emit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Data;
using starsky.foundation.database.Helpers;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

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
		public void CheckIfMysqlScopeIsThere_withParam()
		{
			var services = new ServiceCollection();
			new SetupDatabaseTypes(new AppSettings
			{
				DatabaseType = AppSettings.DatabaseTypeList.Mysql
			}, services).BuilderDb("database");
			
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
		
		[TestMethod]
		public void CheckIfSqliteScopeIsThere_WithParam()
		{
			var services = new ServiceCollection();
			new SetupDatabaseTypes(new AppSettings
			{
				DatabaseType = AppSettings.DatabaseTypeList.Sqlite
			}, services).BuilderDb("database");
			
			var serviceProvider = services.BuildServiceProvider();
			var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();
			Assert.IsNotNull(dbContext);
		}

		[TestMethod]
		[ExpectedException(typeof(AggregateException))]
		public void BuilderDbFactorySwitch_fail()
		{
			var appSettings = new AppSettings {Verbose = true};
			// do something that should not be allowed
			AppSettingsReflection.Modify(appSettings, "get_DatabaseType", 8);
			
			var services = new ServiceCollection();

			new SetupDatabaseTypes(appSettings, services).BuilderDbFactorySwitch();
			// expect exception
		}
		
		[TestMethod]
		public void BuilderDb_console()
		{
			var console = new FakeConsoleWrapper();
			var appSettings = new AppSettings {Verbose = true, DatabaseType = AppSettings.DatabaseTypeList.InMemoryDatabase};
			var services = new ServiceCollection();

			new SetupDatabaseTypes(appSettings, services, console).BuilderDb();

			Assert.IsTrue(console.WrittenLines[0].Contains("Database connection:"));
		}
	}
}
