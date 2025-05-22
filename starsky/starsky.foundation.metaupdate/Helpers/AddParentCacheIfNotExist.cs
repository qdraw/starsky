using starsky.foundation.database.Interfaces;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;

namespace starsky.foundation.metaupdate.Helpers
{
	public class AddParentCacheIfNotExist(IQuery query, IWebLogger logger)
	{
		internal async Task<List<string>> AddParentCacheIfNotExistAsync(IEnumerable<string> updatedPaths)
		{
			var parentDirectoryList = new HashSet<string>();

			foreach ( var path in updatedPaths )
			{
				parentDirectoryList.Add(FilenamesHelper.GetParentPath(path));
			}

			var shouldAddParentDirectoriesToCache = parentDirectoryList.Where(parentDirectory =>
				!query.CacheGetParentFolder(parentDirectory).Item1).ToList();

			if ( shouldAddParentDirectoriesToCache.Count == 0 )
			{
				return new List<string>();
			}

			var databaseQueryResult = await query.GetAllObjectsAsync(shouldAddParentDirectoriesToCache);

			logger.LogInformation("[AddParentCacheIfNotExist] files added to cache " +
								   string.Join(",", shouldAddParentDirectoriesToCache));

			foreach ( var directory in shouldAddParentDirectoriesToCache )
			{
				var byDirectory = databaseQueryResult.Where(p => p.ParentDirectory == directory).ToList();
				query.AddCacheParentItem(directory, byDirectory);
			}
			return shouldAddParentDirectoriesToCache;
		}
	}
}
