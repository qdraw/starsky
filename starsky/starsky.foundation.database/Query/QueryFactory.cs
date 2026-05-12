using System;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.database.Helpers;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;

namespace starsky.foundation.database.Query;

public class QueryFactory(
	SetupDatabaseTypes? setupDatabaseTypes,
	IQuery? query,
	IMemoryCache? cache,
	AppSettings? appSettings,
	IServiceScopeFactory? serviceScopeFactory,
	IWebLogger? logger)
{
	public IQuery? Query()
	{
		if ( query == null )
		{
			return null!;
		}

		var context = setupDatabaseTypes?.BuilderDbFactory();
		if ( query.GetType() == typeof(Query) && context != null && appSettings != null &&
		     logger != null )
		{
			return new Query(context, appSettings, serviceScopeFactory!, logger, cache);
		}

		// FakeIQuery should skip creation
		var isAnyContentIncluded =
			query.GetReflectionFieldValue<List<FileIndexItem>?>("_content")?.Count >= 1;
		if ( !isAnyContentIncluded )
		{
			// ApplicationDbContext context, 
			// 	AppSettings appSettings,
			// IServiceScopeFactory scopeFactory, 
			// 	IWebLogger logger, IMemoryCache memoryCache = null

			return Activator.CreateInstance(query.GetType(), context,
				appSettings, serviceScopeFactory, logger,
				cache) as IQuery;
		}

		logger?.LogInformation("FakeIQuery _content detected");
		return query;
	}
}
