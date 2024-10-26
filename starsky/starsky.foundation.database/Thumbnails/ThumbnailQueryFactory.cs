using System;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.database.Helpers;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Interfaces;

namespace starsky.foundation.database.Thumbnails;

public sealed class ThumbnailQueryFactory
{
	private readonly IWebLogger _logger;
	private readonly IMemoryCache? _memoryCache;
	private readonly IServiceScopeFactory? _serviceScopeFactory;
	private readonly SetupDatabaseTypes? _setupDatabaseTypes;
	private readonly IThumbnailQuery? _thumbnailQuery;

	public ThumbnailQueryFactory(SetupDatabaseTypes? setupDatabaseTypes,
		IServiceScopeFactory? serviceScopeFactory, IThumbnailQuery? thumbnailQuery,
		IWebLogger logger, IMemoryCache? memoryCache)
	{
		_setupDatabaseTypes = setupDatabaseTypes;
		_serviceScopeFactory = serviceScopeFactory;
		_thumbnailQuery = thumbnailQuery;
		_logger = logger;
		_memoryCache = memoryCache;
	}

	public IThumbnailQuery? ThumbnailQuery()
	{
		if ( _thumbnailQuery == null )
		{
			return null;
		}

		var context = _setupDatabaseTypes?.BuilderDbFactory();
		if ( _thumbnailQuery.GetType() == typeof(ThumbnailQuery) && context != null &&
		     _memoryCache != null )
		{
			return new ThumbnailQuery(context, _serviceScopeFactory, _logger, _memoryCache);
		}

		// FakeIQuery should skip creation
		var isAnyContentIncluded =
			_thumbnailQuery.GetReflectionFieldValue<List<ThumbnailItem>?>("_content")?.Count != 0;

		if ( !isAnyContentIncluded )
		{
			return Activator.CreateInstance(_thumbnailQuery.GetType(),
				context, _serviceScopeFactory, _logger) as IThumbnailQuery;
		}

		_logger.LogInformation("FakeIThumbnailQuery _content detected");
		return _thumbnailQuery;
	}
}
