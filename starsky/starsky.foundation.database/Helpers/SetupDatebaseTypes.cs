using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;
using starsky.foundation.database.Data;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Interfaces;

namespace starsky.foundation.database.Helpers
{
	public sealed class SetupDatabaseTypes
	{
		private readonly AppSettings _appSettings;
		private readonly IServiceCollection? _services;
		private readonly IWebLogger? _logger;

		public SetupDatabaseTypes(AppSettings appSettings, IServiceCollection? services = null,
			IWebLogger? logger = null)
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

		private ServerVersion GetServerVersionMySql()
		{
			try
			{
				return ServerVersion.AutoDetect(
					_appSettings.DatabaseConnection);
			}
			catch ( MySqlException )
			{
				// nothing here
			}

			return new MariaDbServerVersion("10.2");
		}

		/// <summary>
		/// Setup database connection
		/// </summary>
		/// <param name="foundationDatabaseName">Assembly name, used for running migrations</param>
		/// <returns></returns>
		/// <exception cref="AggregateException">Missing arguments</exception>
		internal DbContextOptions<ApplicationDbContext> BuilderDbFactorySwitch(
			string? foundationDatabaseName = "")
		{
			switch ( _appSettings.DatabaseType )
			{
				case ( AppSettings.DatabaseTypeList.Mysql ):

					var mysql = new DbContextOptionsBuilder<ApplicationDbContext>()
						.UseMySql(_appSettings.DatabaseConnection, GetServerVersionMySql(),
							mySqlOptions =>
							{
								mySqlOptions.EnableRetryOnFailure(2);
								if ( !string.IsNullOrWhiteSpace(foundationDatabaseName) )
								{
									mySqlOptions.MigrationsAssembly(foundationDatabaseName);
								}
							});
					return mysql.Options;
				case AppSettings.DatabaseTypeList.InMemoryDatabase:
					var memoryDatabase = new DbContextOptionsBuilder<ApplicationDbContext>()
						.UseInMemoryDatabase(string.IsNullOrEmpty(_appSettings.DatabaseConnection)
							? "starsky"
							: _appSettings.DatabaseConnection);
					return memoryDatabase.Options;
				case AppSettings.DatabaseTypeList.Sqlite:
					var sqlite = new DbContextOptionsBuilder<ApplicationDbContext>()
						.UseSqlite(_appSettings.DatabaseConnection,
							b =>
							{
								if ( !string.IsNullOrWhiteSpace(foundationDatabaseName) )
								{
									b.MigrationsAssembly(foundationDatabaseName);
								}
							});
					return sqlite.Options;
				default:
					throw new AggregateException(nameof(_appSettings.DatabaseType));
			}
		}

		/// <summary>
		/// Setup database connection
		/// use boot parameters to run with EF Migrations and a direct connection
		/// ENABLE_DEFAULT_DATABASE: SQLite
		/// ENABLE_MYSQL_DATABASE: MySql
		/// In runtime those parameters are not needed and not useful.
		/// </summary>
		/// <param name="foundationDatabaseName">Assembly name, used for migrations</param>
		/// <exception cref="AggregateException">services is null</exception>
		public void BuilderDb(string? foundationDatabaseName = "")
		{
			if ( _services == null )
			{
				throw new AggregateException("services is missing");
			}

			if ( _logger != null && _appSettings.IsVerbose() )
			{
				_logger.LogInformation($"Database connection: {_appSettings.DatabaseConnection}");
			}

			_services.AddScoped(_ =>
				new ApplicationDbContext(BuilderDbFactorySwitch(foundationDatabaseName)));
		}
	}
}
