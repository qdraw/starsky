using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using starsky.foundation.database.Interfaces;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;

namespace starsky.feature.metaupdate.Helpers
{
	public class AddParentCacheIfNotExist
	{
		private readonly IQuery _query;
		private readonly IWebLogger _logger;

		public AddParentCacheIfNotExist(IQuery query, IWebLogger logger)
		{
			_query = query;
			_logger = logger;
		}
		
		internal async Task<List<string>> AddParentCacheIfNotExistAsync(IEnumerable<string> updatedPaths)
		{
			var parentDirectoryList = new HashSet<string>();

			foreach ( var path in updatedPaths )
			{
				parentDirectoryList.Add(FilenamesHelper.GetParentPath(path));
			}

			var shouldAddParentDirectoriesToCache = parentDirectoryList.Where(parentDirectory => 
				!_query.CacheGetParentFolder(parentDirectory).Item1).ToList();
			if ( !shouldAddParentDirectoriesToCache.Any() ) return new List<string>();

			var databaseQueryResult = await _query.GetAllObjectsAsync(shouldAddParentDirectoriesToCache);
			
			_logger.LogInformation("[AddParentCacheIfNotExist] files added to cache " + 
			                       string.Join(",", shouldAddParentDirectoriesToCache));
			
			foreach ( var directory in shouldAddParentDirectoriesToCache )
			{
				var byDirectory = databaseQueryResult.Where(p => p.ParentDirectory == directory).ToList();
				_query.AddCacheParentItem(directory, byDirectory);
			}
			return shouldAddParentDirectoriesToCache; 
		}
	}
}
