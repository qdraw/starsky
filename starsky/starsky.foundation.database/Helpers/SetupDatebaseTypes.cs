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
		public void BuilderDb(string foundationDatabaseName = "")
		{
			if ( _appSettings.Verbose ) Console.WriteLine(_appSettings.DatabaseConnection);
			switch (_appSettings.DatabaseType)
			{
				case (AppSettings.DatabaseTypeList.Mysql):
					_services.AddDbContext<ApplicationDbContext>(
						options => options.UseMySql(_appSettings.DatabaseConnection, mySqlOptions =>
						{
							mySqlOptions.CharSet(CharSet.Utf8Mb4);
							mySqlOptions.CharSetBehavior(CharSetBehavior.AppendToAllColumns);
							mySqlOptions.EnableRetryOnFailure(2);
							if (! string.IsNullOrWhiteSpace(foundationDatabaseName) )
							{
								mySqlOptions.MigrationsAssembly(foundationDatabaseName);
							}
						})
					);
					break;
				case AppSettings.DatabaseTypeList.InMemoryDatabase:
					_services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase("starsky"));
					break;
				case AppSettings.DatabaseTypeList.Sqlite:
					_services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(_appSettings.DatabaseConnection, 
						b =>
						{
							if (! string.IsNullOrWhiteSpace(foundationDatabaseName) )
							{
								b.MigrationsAssembly(foundationDatabaseName);
							}
						}));
					break;
				default:
					throw new AggregateException(nameof(_appSettings.DatabaseType));
			}
		}
	}
}
