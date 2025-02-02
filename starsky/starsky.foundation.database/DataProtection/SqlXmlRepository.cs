using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models;
using starsky.foundation.database.Query;
using starsky.foundation.injection;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;

namespace starsky.foundation.database.DataProtection;

/// <summary>
///     @see:
///     https://nicolas.guelpa.me/blog/2017/01/11/dotnet-core-data-protection-keys-repository.html
///     @see:
///     https://docs.microsoft.com/en-us/aspnet/core/security/data-protection/introduction?view=aspnetcore-5.0
///     @see:
///     https://medium.com/@_kbremner/storing-asp-net-core-data-protection-keys-in-the-database-f284d13897b8
///     @see: https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/web-farm
/// </summary>
[Service(typeof(IXmlRepository), InjectionLifetime = InjectionLifetime.Scoped)]
public class SqlXmlRepository : IXmlRepository
{
	private readonly ApplicationDbContext _dbContext;
	private readonly IWebLogger _logger;
	private readonly IServiceScopeFactory _scopeFactory;

	public SqlXmlRepository(ApplicationDbContext dbContext, IServiceScopeFactory scopeFactory,
		IWebLogger logger)
	{
		_dbContext = dbContext;
		_scopeFactory = scopeFactory;
		_logger = logger;
	}

	public IReadOnlyCollection<XElement> GetAllElements()
	{
		try
		{
			var result = _dbContext.DataProtectionKeys
				.Where(p => p.Xml != null).AsEnumerable()
				.Select(x => XElement.Parse(x.Xml!)).ToList();

			return result;
		}
		catch ( Exception exception )
		{
			if ( exception is not RetryLimitExceededException &&
			     exception is not MySqlException &&
			     exception is not SqliteException )
			{
				throw;
			}

			// MySqlConnector.MySqlException (0x80004005): Table 'starsky.DataProtectionKeys' doesn't exist
			// or Microsoft.Data.Sqlite.SqliteException (0x80004005): SQLite Error 1: 'no such table: DataProtectionKeys
			if ( !exception.Message.Contains("0x80004005") &&
			     !exception.Message.Contains(
				     "no such table: DataProtectionKeys") )
			{
				return new List<XElement>();
			}

			_logger.LogInformation("run migration: dotnet ef database update");
			_dbContext.Database.Migrate();
			return new List<XElement>();
		}
	}

	/// <summary>
	///     This function crashes usually on the first run
	/// </summary>
	/// <param name="element">Xml element</param>
	/// <param name="friendlyName">name of item</param>
	public void StoreElement(XElement element, string friendlyName)
	{
		bool LocalDefault(ApplicationDbContext ctx)
		{
			ctx.DataProtectionKeys.Add(new DataProtectionKey
			{
				Xml = element.ToString(SaveOptions.DisableFormatting),
				FriendlyName = friendlyName
			});
			ctx.SaveChanges();
			return true;
		}

		bool LocalDefaultQuery()
		{
			var context = new InjectServiceScope(_scopeFactory).Context();
			return LocalDefault(context);
		}

		try
		{
			LocalDefault(_dbContext);
		}
		catch ( Exception exception )
		{
			if ( exception is not DbUpdateException &&
			     exception is not RetryLimitExceededException &&
			     exception is not MySqlException &&
			     exception is not SqliteException )
			{
				throw;
			}

			var retryInterval = _dbContext.GetType().FullName?.Contains("test") == true
				? TimeSpan.FromSeconds(0)
				: TimeSpan.FromSeconds(5);

			try
			{
				RetryHelper.Do(
					LocalDefaultQuery, retryInterval, 2);
			}
			catch ( AggregateException aggregateException )
			{
				_logger.LogError(aggregateException,
					"[SqlXmlRepository] catch-ed AggregateException");
			}
		}
	}
}
