using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using starsky.foundation.database.Data;
using starsky.foundation.platform.Models;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Pomelo.EntityFrameworkCore.MySql.Storage;
using starsky.foundation.databasetelemetry.Helpers;
using starsky.foundation.databasetelemetry.Services;
using starsky.foundation.platform.Interfaces;

namespace starsky.foundation.database.Helpers
{
	public class SetupDatabaseTypes
	{
		private readonly AppSettings _appSettings;
		private readonly IServiceCollection _services;
		private readonly IWebLogger _logger;

		public SetupDatabaseTypes(AppSettings appSettings, IServiceCollection services = null, IWebLogger logger = null)
		{
			_appSettings = appSettings;
			_services = services;

			// if null get from service collection
			logger ??= _services?.BuildServiceProvider().GetService<IWebLogger>();
			_logger = logger;
		}

		public ApplicationDbContext BuilderDbFactory()
		{
			return new ApplicationDbContext(BuilderDbFactorySwitch());
		}

		internal ServerVersion GetServerVersionMySql()
		{
			try
			{
				return ServerVersion.AutoDetect(
						_appSettings.DatabaseConnection);
			}
			catch ( MySqlException)
			{
				// nothing here
			}
			return new MariaDbServerVersion("10.2");
		}
		
		internal DbContextOptions<ApplicationDbContext> BuilderDbFactorySwitch(string foundationDatabaseName = "")
		{
			switch ( _appSettings.DatabaseType )
			{
				case ( AppSettings.DatabaseTypeList.Mysql ):
					
					var mysql = new DbContextOptionsBuilder<ApplicationDbContext>()
						.UseMySql(_appSettings.DatabaseConnection, GetServerVersionMySql(), mySqlOptions =>
						{
							mySqlOptions.EnableRetryOnFailure(2);
							if ( !string.IsNullOrWhiteSpace(foundationDatabaseName) )
							{
								mySqlOptions.MigrationsAssembly(foundationDatabaseName);
							}
						});
					EnableDatabaseTracking(mysql);
					return mysql.Options;
				case AppSettings.DatabaseTypeList.InMemoryDatabase:
					var memoryDatabase = new DbContextOptionsBuilder<ApplicationDbContext>()
						.UseInMemoryDatabase(string.IsNullOrEmpty(_appSettings.DatabaseConnection) ? "starsky" : _appSettings.DatabaseConnection );
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
					EnableDatabaseTracking(sqlite);
					return sqlite.Options;
				default:
					throw new AggregateException(nameof(_appSettings.DatabaseType));
			}
		}

		private bool IsDatabaseTrackingEnabled()
		{
			return !string.IsNullOrEmpty(_appSettings
				       .ApplicationInsightsInstrumentationKey) && _appSettings.ApplicationInsightsDatabaseTracking == true;
		}

		internal bool EnableDatabaseTracking( DbContextOptionsBuilder<ApplicationDbContext> databaseOptionsBuilder)
		{
			if (!IsDatabaseTrackingEnabled())
			{
				return false;
			}
			databaseOptionsBuilder.AddInterceptors(
				new DatabaseTelemetryInterceptor(
					TelemetryConfigurationHelper.InitTelemetryClient(
						_appSettings.ApplicationInsightsInstrumentationKey, 
						_appSettings.ApplicationType.ToString(),_logger)
					)
				);
			return true;
		}

		public void BuilderDb(string foundationDatabaseName = "")
		{
			if ( _services == null ) throw new AggregateException("services is missing");
			if ( _logger != null && _appSettings.IsVerbose() )
			{
				_logger.LogInformation($"Database connection: {_appSettings.DatabaseConnection}");
			}
			_logger?.LogInformation($"Application Insights Database tracking is {IsDatabaseTrackingEnabled()}" );

#if ENABLE_DEFAULT_DATABASE
				// dirty hack
			_services.AddDbContext<ApplicationDbContext>(options =>
				options.UseSqlite(_appSettings.DatabaseConnection, 
				b =>
				{
					if (! string.IsNullOrWhiteSpace(foundationDatabaseName) )
					{
						b.MigrationsAssembly(foundationDatabaseName);
					}
				}));		
#endif

			_services.AddScoped(provider => new ApplicationDbContext(BuilderDbFactorySwitch(foundationDatabaseName)));
		}

	}
}
