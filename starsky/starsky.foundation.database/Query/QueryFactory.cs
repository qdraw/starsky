#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using starsky.foundation.database.Helpers;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;

namespace starsky.foundation.database.Query
{
	public class QueryFactory
	{
		private readonly SetupDatabaseTypes? _setupDatabaseTypes;
		private readonly IQuery? _query;
		private readonly IMemoryCache? _cache;
		private readonly AppSettings? _appSettings;
		private readonly IWebLogger? _logger;

		public QueryFactory(SetupDatabaseTypes? setupDatabaseTypes, IQuery? query, IMemoryCache? cache, AppSettings? appSettings, IWebLogger? logger)
		{
			_setupDatabaseTypes = setupDatabaseTypes;
			_query = query;
			_cache  = cache;
			_appSettings = appSettings;
			_logger = logger;
		}
		
		public IQuery? Query()
		{
			if ( _query == null ) return null!;
			var context = _setupDatabaseTypes?.BuilderDbFactory();
			if ( _query.GetType() == typeof(Query) )
			{
				return new Query(context, _appSettings, null, _logger, _cache);
			}

			// FakeIQuery should skip creation
			var isAnyContentIncluded = _query.GetReflectionFieldValue<List<FileIndexItem>?>("_content")?.Any();
			if ( isAnyContentIncluded == true )
			{
				_logger?.LogInformation("FakeIQuery _content detected");
				return _query;
			}
		
			// ApplicationDbContext context, 
			// 	AppSettings appSettings,
			// IServiceScopeFactory scopeFactory, 
			// 	IWebLogger logger, IMemoryCache memoryCache = null
			return Activator.CreateInstance(_query.GetType(), context, _appSettings, null, _logger, _cache) as IQuery;
		}
	}
}
