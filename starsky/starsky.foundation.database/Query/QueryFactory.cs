using System;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
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
		private readonly IServiceScopeFactory? _serviceScopeFactory;
		private readonly IWebLogger? _logger;

		public QueryFactory(SetupDatabaseTypes? setupDatabaseTypes, IQuery? query,
			IMemoryCache? cache, AppSettings? appSettings,
			IServiceScopeFactory? serviceScopeFactory, IWebLogger? logger)
		{
			_setupDatabaseTypes = setupDatabaseTypes;
			_query = query;
			_cache = cache;
			_appSettings = appSettings;
			_serviceScopeFactory = serviceScopeFactory;
			_logger = logger;
		}

		public IQuery? Query()
		{
			if ( _query == null ) return null!;
			var context = _setupDatabaseTypes?.BuilderDbFactory();
			if ( _query.GetType() == typeof(Query) && context != null && _appSettings != null &&
			     _logger != null )
			{
				return new Query(context, _appSettings, _serviceScopeFactory!, _logger, _cache);
			}

			// FakeIQuery should skip creation
			var isAnyContentIncluded =
				_query.GetReflectionFieldValue<List<FileIndexItem>?>("_content")?.Count >= 1;
			if ( !isAnyContentIncluded )
			{
				// ApplicationDbContext context, 
				// 	AppSettings appSettings,
				// IServiceScopeFactory scopeFactory, 
				// 	IWebLogger logger, IMemoryCache memoryCache = null

				return Activator.CreateInstance(_query.GetType(), context,
					_appSettings, _serviceScopeFactory, _logger,
					_cache) as IQuery;
			}

			_logger?.LogInformation("FakeIQuery _content detected");
			return _query;
		}
	}
}
