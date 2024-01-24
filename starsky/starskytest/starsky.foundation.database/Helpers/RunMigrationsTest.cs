using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySqlConnector;
using starsky.foundation.database.Data;
using starsky.foundation.database.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.database.Helpers
{
	[TestClass]
	public sealed class RunMigrationsTest
	{
		[TestMethod]
		public async Task Test()
		{
			IServiceCollection services = new ServiceCollection();
			services.AddSingleton<IWebLogger, FakeIWebLogger>();

			var efServiceProvider = new ServiceCollection().AddEntityFrameworkInMemoryDatabase().BuildServiceProvider();
			services
				.AddDbContext<ApplicationDbContext>(b =>
					b.UseInMemoryDatabase("test1234").UseInternalServiceProvider(efServiceProvider));
			services.AddSingleton<AppSettings>();

			var serviceProvider = services.BuildServiceProvider();
			var serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
			var appSettings = serviceProvider.GetRequiredService<AppSettings>();

			appSettings.DatabaseType = AppSettings.DatabaseTypeList.Mysql;
			
			Assert.IsNotNull(serviceScopeFactory);
			await RunMigrations.Run(serviceScopeFactory.CreateScope(),1);
			// expect exception: Relational-specific methods can only be used when the context is using a relational database provider.
		}

		[SuppressMessage("Usage", "S6602: First or Default")]
		[SuppressMessage("Usage", "S3398: move function")]
		private static MySqlException CreateMySqlException(string message)
		{
			// MySqlErrorCode errorCode, string? sqlState, string message, Exception? innerException

			var ctorLIst =
				typeof(MySqlException).GetConstructors(
					BindingFlags.Instance |
					BindingFlags.NonPublic | BindingFlags.InvokeMethod);
			var ctor = ctorLIst.FirstOrDefault(p => 
				p.ToString() == "Void .ctor(MySqlConnector.MySqlErrorCode, System.String, System.String, System.Exception)" );
				
			var instance =
				( MySqlException? ) ctor?.Invoke(new object[]
				{
					MySqlErrorCode.AccessDenied,
					"test",
					message,
					new Exception()
				});
			return instance!;
		}
		
		private class AppDbMySqlException : ApplicationDbContext
		{
			public AppDbMySqlException(DbContextOptions options) : base(options)
			{
			}

			public override DatabaseFacade Database => throw CreateMySqlException("");

		}

		[TestMethod]
		public async Task MySqlException()
		{
			var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase(databaseName: "MovieListDatabase")
				.Options;
			
			Assert.IsNotNull(options);
			await RunMigrations.Run(
				new AppDbMySqlException(options), new FakeIWebLogger(),new AppSettings{DatabaseType = AppSettings.DatabaseTypeList.Mysql},1);
			
			// should not crash
		}
		
		[TestMethod]
		public async Task MysqlFixes_ShouldReturnTrue_AfterFixesAreApplied()
		{
			// Arrange
			var appSettings = new AppSettings
			{
				DatabaseType = AppSettings.DatabaseTypeList.Mysql,
				DatabaseConnection = "server=localhost;database=mydatabase;user=root;password=mypassword"
			};
			var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase(databaseName: "MovieListDatabase")
				.Options;
			var dbContext = new ApplicationDbContext(options);
			var connection = new MySqlConnection(appSettings.DatabaseConnection);

			// Act
			var result = await RunMigrations.MysqlFixes(connection, appSettings, dbContext,new FakeIWebLogger());

			// Assert
			Assert.IsTrue(result);
		}
		
	}
}
