using System;
using System.Reflection;
using System.Reflection.Emit;
using Microsoft.EntityFrameworkCore;
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
		[ExpectedException(typeof(AggregateException))]
		public void BuilderDb_fail()
		{
			var appSettings = new AppSettings {Verbose = true};
			new SetupDatabaseTypes(appSettings).BuilderDb();
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

		[TestMethod]
		public void EnableDatabaseTracking_shouldEnable()
		{
			var console = new FakeConsoleWrapper();
			var appSettings = new AppSettings {
				Verbose = true,
				DatabaseType = AppSettings.DatabaseTypeList.InMemoryDatabase,
				ApplicationInsightsInstrumentationKey = "any",
				ApplicationInsightsDatabaseTracking = true
			};
			var services = new ServiceCollection();
			
			var memoryDatabase = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase("test123");
			var result = new SetupDatabaseTypes(appSettings, services, console).EnableDatabaseTracking(memoryDatabase);
			Assert.IsTrue(result);
		}
		
		[TestMethod]
		public void EnableDatabaseTracking_shouldDisable()
		{
			var console = new FakeConsoleWrapper();
			var appSettings = new AppSettings {
				Verbose = true,
				DatabaseType = AppSettings.DatabaseTypeList.InMemoryDatabase,
				ApplicationInsightsInstrumentationKey = string.Empty, // <-- No Key
				ApplicationInsightsDatabaseTracking = true
			};
			var services = new ServiceCollection();
			var result = new SetupDatabaseTypes(appSettings, services, console).EnableDatabaseTracking(null);
			Assert.IsFalse(result);
		}
	}
}
