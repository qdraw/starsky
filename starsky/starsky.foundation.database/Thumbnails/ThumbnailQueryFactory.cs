#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using starsky.foundation.database.Helpers;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Interfaces;

namespace starsky.foundation.database.Thumbnails
{
	public sealed class ThumbnailQueryFactory
	{
		private readonly SetupDatabaseTypes? _setupDatabaseTypes;
		private readonly IThumbnailQuery? _thumbnailQuery;
		private readonly IWebLogger? _logger;

		public ThumbnailQueryFactory(SetupDatabaseTypes? setupDatabaseTypes, IThumbnailQuery? thumbnailQuery, IWebLogger? logger)
		{
			_setupDatabaseTypes = setupDatabaseTypes;
			_thumbnailQuery = thumbnailQuery;
			_logger = logger;
		}
		
		public IThumbnailQuery? ThumbnailQuery()
		{
			if ( _thumbnailQuery == null ) return null!;
			var context = _setupDatabaseTypes?.BuilderDbFactory();
			if ( _thumbnailQuery.GetType() == typeof(ThumbnailQuery) && context != null && _logger != null)
			{
				return new ThumbnailQuery(context);
			}

			// FakeIQuery should skip creation
			var isAnyContentIncluded = _thumbnailQuery.GetReflectionFieldValue<List<ThumbnailItem>?>("_content")?.Any();
			if ( isAnyContentIncluded == true )
			{
				_logger?.LogInformation("FakeIThumbnailQuery _content detected");
				return _thumbnailQuery;
			}
			
			return Activator.CreateInstance(_thumbnailQuery.GetType(), context) as IThumbnailQuery;
		}
	}
}

