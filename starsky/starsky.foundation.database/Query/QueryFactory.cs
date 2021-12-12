using System;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.database.Helpers;
using starsky.foundation.database.Interfaces;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Services;

namespace starsky.foundation.database.Query
{
	public class QueryFactory
	{
		private readonly SetupDatabaseTypes _setupDatabaseTypes;
		private readonly IQuery _query;
		private readonly IMemoryCache _cache;
		private readonly AppSettings _appSettings;
		private readonly IWebLogger _logger;

		public QueryFactory(SetupDatabaseTypes setupDatabaseTypes, IQuery query, IMemoryCache cache, AppSettings appSettings, IWebLogger logger)
		{
			_setupDatabaseTypes = setupDatabaseTypes;
			_query = query;
			_cache  = cache;
			_appSettings = appSettings;
			_logger = logger;
		}
		
		public IQuery Query()
		{
			var context = _setupDatabaseTypes.BuilderDbFactory();
			if ( _query.GetType() == typeof(Query) )
			{

				return new Query(context, _cache, _appSettings, null, _logger);
			}
			return Activator.CreateInstance(_query.GetType(), context, _cache, _appSettings, null, _logger) as IQuery;
		}
	}
}
