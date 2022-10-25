using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models;
using starsky.foundation.database.Query;
using starsky.foundation.injection;
using starsky.foundation.platform.Helpers;

namespace starsky.foundation.database.DataProtection;

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
				.Where(p => p.Xml != null).ToList()
				.Select(x => XElement.Parse(x.Xml!)).ToList();
			
			return result;
		}
		catch ( MySqlConnector.MySqlException exception )
		{
			// MySqlConnector.MySqlException (0x80004005): Table 'starsky.DataProtectionKeys' doesn't exist
			if ( exception.Message.Contains("0x80004005") && exception.Message.Contains("DataProtectionKeys") )
			{
				_dbContext.Database.EnsureCreated();
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
		catch ( Microsoft.EntityFrameworkCore.DbUpdateException )
		{
			RetryHelper.Do(
				LocalDefaultQuery, TimeSpan.FromSeconds(5), 3);
		}
	}
}
