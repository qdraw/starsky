using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.database.Data;
using starsky.foundation.platform.Models;

namespace starsky.foundation.database.Helpers
{
	public class SetupDatabaseTypes
	{
		private readonly AppSettings _appSettings;
		private readonly ServiceCollection _services;

		public SetupDatabaseTypes(AppSettings appSettings, ServiceCollection services)
		{
			_appSettings = appSettings;
			_services = services;
		}
		public void BuilderDb()
		{
			switch (_appSettings.DatabaseType)
			{
				case (AppSettings.DatabaseTypeList.Mysql):
					_services.AddDbContext<ApplicationDbContext>(options => options.UseMySql(_appSettings.DatabaseConnection));
					break;
				case AppSettings.DatabaseTypeList.InMemoryDatabase:
					_services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase("starsky"));
					break;
				case AppSettings.DatabaseTypeList.Sqlite:
					_services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(_appSettings.DatabaseConnection));
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}
