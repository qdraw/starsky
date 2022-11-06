using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.database.Data;
using starsky.foundation.database.Query;
using starsky.foundation.injection;
using starsky.foundation.platform.Helpers;

namespace starsky.foundation.database.DataProtection
{
	[Service(typeof(IXmlRepository), InjectionLifetime = InjectionLifetime.Scoped)]
	public class SqlXmlRepository : IXmlRepository
	{
		private readonly ApplicationDbContext _dbContext;
		private readonly IServiceScopeFactory _scopeFactory;
    
		public SqlXmlRepository(ApplicationDbContext dbContext, IServiceScopeFactory scopeFactory)
		{
			_dbContext = dbContext;
			_scopeFactory = scopeFactory;
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
				if ( exception is not MySqlConnector.MySqlException &&
				     exception is not Microsoft.Data.Sqlite.SqliteException ) throw;
    			
				// MySqlConnector.MySqlException (0x80004005): Table 'starsky.DataProtectionKeys' doesn't exist
				// or Microsoft.Data.Sqlite.SqliteException (0x80004005): SQLite Error 1: 'no such table: DataProtectionKeys
				if ( exception.Message.Contains("0x80004005") && exception.Message.Contains("DataProtectionKeys") )
				{
					_dbContext.Database.Migrate();
				}
				return new List<XElement>();
			}
		}
    
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
			catch ( DbUpdateException )
			{
				var retryInterval = _dbContext.GetType().FullName?.Contains("test") == true ? 
					TimeSpan.FromSeconds(0) : TimeSpan.FromSeconds(5);
				RetryHelper.Do(
					LocalDefaultQuery, retryInterval, 2);
			}
		}
	}
}
