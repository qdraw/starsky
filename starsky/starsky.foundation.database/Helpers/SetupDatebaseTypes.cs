using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.database.Data;
using starsky.foundation.platform.Models;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Pomelo.EntityFrameworkCore.MySql.Storage;

namespace starsky.foundation.database.Helpers
{
	public class SetupDatabaseTypes
	{
		private readonly AppSettings _appSettings;
		private readonly IServiceCollection _services;

		public SetupDatabaseTypes(AppSettings appSettings, IServiceCollection services)
		{
			_appSettings = appSettings;
			_services = services;
		}

		public ApplicationDbContext BuilderDbFactory()
		{
			return new ApplicationDbContext(BuilderDbFactorySwitch());
		}
		
		private DbContextOptions<ApplicationDbContext> BuilderDbFactorySwitch(string foundationDatabaseName = "")
		{
			if ( _appSettings.Verbose ) Console.WriteLine(_appSettings.DatabaseConnection);

			switch ( _appSettings.DatabaseType )
			{
				case ( AppSettings.DatabaseTypeList.Mysql ):
					var mysql = new DbContextOptionsBuilder<ApplicationDbContext>()
						.UseMySql(_appSettings.DatabaseConnection, mySqlOptions =>
						{
							mySqlOptions.CharSet(CharSet.Utf8Mb4);
							mySqlOptions.CharSetBehavior(CharSetBehavior.AppendToAllColumns);
							mySqlOptions.EnableRetryOnFailure(2);
							if ( !string.IsNullOrWhiteSpace(foundationDatabaseName) )
							{
								mySqlOptions.MigrationsAssembly(foundationDatabaseName);
							}
						});
					return mysql.Options;
				case AppSettings.DatabaseTypeList.InMemoryDatabase:
					var memoryDatabase = new DbContextOptionsBuilder<ApplicationDbContext>()
						.UseInMemoryDatabase("starsky");
					return memoryDatabase.Options;
				case AppSettings.DatabaseTypeList.Sqlite:
					var sqlite = new DbContextOptionsBuilder<ApplicationDbContext>()
						.UseSqlite(_appSettings.DatabaseConnection, 
							b =>
							{
								if (! string.IsNullOrWhiteSpace(foundationDatabaseName) )
								{
									b.MigrationsAssembly(foundationDatabaseName);
								}
							});
					return sqlite.Options;
				default:
					throw new AggregateException(nameof(_appSettings.DatabaseType));
			}
		}

		public void BuilderDb(string foundationDatabaseName = "")
		{
			if ( _appSettings.Verbose ) Console.WriteLine(_appSettings.DatabaseConnection);
			_services.AddScoped(provider => new ApplicationDbContext(BuilderDbFactorySwitch(foundationDatabaseName)));
		}

	}
}
