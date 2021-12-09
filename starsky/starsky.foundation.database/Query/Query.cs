using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using MySql.Data.MySqlClient;
using starsky.foundation.database.Data;
using starsky.foundation.database.Helpers;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.injection;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;

namespace starsky.foundation.database.Query
{
	
	[Service(typeof(IQuery), InjectionLifetime = InjectionLifetime.Scoped)]
	public partial class Query : IQuery
    {
        private ApplicationDbContext _context;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IMemoryCache _cache;
        private readonly AppSettings _appSettings;
        private readonly IWebLogger _logger;

        public Query(ApplicationDbContext context, 
            IMemoryCache memoryCache = null, 
            AppSettings appSettings = null,
            IServiceScopeFactory scopeFactory = null, 
            IWebLogger logger = null)
        {
	        _context = context;
            _cache = memoryCache;
            _appSettings = appSettings;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

		/// <summary>
		/// Returns a database object file or folder
		/// </summary>
		/// <param name="filePath">relative database path</param>
		/// <returns>FileIndex-objects with database data</returns>
        public FileIndexItem GetObjectByFilePath(string filePath)
		{
			if ( filePath != "/" ) filePath = PathHelper.RemoveLatestSlash(filePath);
			
            FileIndexItem LocalQuery(ApplicationDbContext context)
            {
	            return context.FileIndex.FirstOrDefault(p => p.FilePath == filePath);
            }
            
            try
            {
	            return LocalQuery(_context);
            }
            catch (ObjectDisposedException e)
            {
	            _logger?.LogInformation("[GetObjectByFilePath] catch-ed ObjectDisposedException", e);
	            return LocalQuery(new InjectServiceScope(_scopeFactory).Context());
            }
        }
		
		internal static string GetObjectByFilePathAsyncCacheName(string subPath)
		{
			return $"_{nameof(GetObjectByFilePathAsyncCacheName)}~{subPath}";
		}
		

		/// <summary>
		/// Returns a database object file or folder
		/// </summary>
		/// <param name="filePath">relative database path</param>
		/// <param name="cacheTime">time to have the cache present</param>
		/// <returns>FileIndex-objects with database data</returns>
		public async Task<FileIndexItem> GetObjectByFilePathAsync(
			string filePath, TimeSpan? cacheTime = null)
		{
			// cache code:
			if ( cacheTime != null && 
			     _appSettings?.AddMemoryCache == true &&
			     _cache.TryGetValue(
				     GetObjectByFilePathAsyncCacheName(filePath), out var data) )
			{
				_logger.LogInformation("Get from cache " + GetObjectByFilePathAsyncCacheName(filePath));
				if ( !(data is FileIndexItem fileIndexItem) ) return null;
				fileIndexItem.Status = FileIndexItem.ExifStatus.OkAndSame;
				return fileIndexItem;
			}
			// end cache

			var result = ( await GetObjectByFilePathQueryAsync(filePath) );

			// cache code:
			if ( cacheTime == null || _appSettings?.AddMemoryCache != true || result == null)
				return result;

			SetGetObjectByFilePathCache(filePath, result.Clone(), cacheTime);

			return result;
		}

		public void SetGetObjectByFilePathCache(string filePath, 
			FileIndexItem result,
			TimeSpan? cacheTime)
		{
			if ( _cache == null || cacheTime == null || result == null )
			{
				_logger.LogInformation("SetGetObjectByFilePathCache not used");
				return;
			}
			_cache.Set(GetObjectByFilePathAsyncCacheName(filePath),
				result, cacheTime.Value );
		}

		private async Task<FileIndexItem> GetObjectByFilePathQueryAsync(
			string filePath)
		{
			if ( filePath != "/" ) filePath = PathHelper.RemoveLatestSlash(filePath);
			var paths = new List<string> {filePath};
			return ( await GetObjectsByFilePathQueryAsync(paths) )
				.FirstOrDefault();
		}
	    
		/// <summary>
		/// Get subPath based on hash (cached hashList view to clear use ResetItemByHash)
		/// </summary>
		/// <param name="fileHash">base32 hash</param>
		/// <returns>subPath (relative to database)</returns>
	    public string GetSubPathByHash(string fileHash)
	    {
		    // The CLI programs uses no cache
		    if( !IsCacheEnabled() ) return QueryGetItemByHash(fileHash);
            
		    // Return values from IMemoryCache
		    var queryHashListCacheName = CachingDbName("hashList", fileHash);

		    // if result is not null return cached value
		    if ( _cache.TryGetValue(queryHashListCacheName, out var cachedSubPath) 
		         && !string.IsNullOrEmpty((string)cachedSubPath)) return ( string ) cachedSubPath;

		    cachedSubPath = QueryGetItemByHash(fileHash);
		    
		    _cache.Set(queryHashListCacheName, cachedSubPath, new TimeSpan(48,0,0));
		    return (string) cachedSubPath;
		}

		/// <summary>
		/// Remove fileHash from hash-list-cache
		/// </summary>
		/// <param name="fileHash">base32 fileHash</param>
	    public void ResetItemByHash(string fileHash)
	    {
		    if( _cache == null || _appSettings?.AddMemoryCache == false) return;
		    
			var queryCacheName = CachingDbName("hashList", fileHash);
			
			if ( _cache.TryGetValue(queryCacheName, out _) )
			{
				_cache.Remove(queryCacheName);
			}
	    }

	    // Return a File Item By it Hash value
        // New added, directory hash now also hashes
        private string QueryGetItemByHash(string fileHash)
        {   
	        try
	        {
		       return _context.FileIndex.TagWith("QueryGetItemByHash").FirstOrDefault(
			        p => p.FileHash == fileHash 
			             && p.IsDirectory != true
		        )?.FilePath;
	        }
	        catch ( ObjectDisposedException )
	        {
		        var context = new InjectServiceScope(_scopeFactory).Context();
		        return context.FileIndex.TagWith("QueryGetItemByHash").FirstOrDefault(
			        p => p.FileHash == fileHash 
			             && p.IsDirectory != true
		        )?.FilePath;
	        }
        }

	    /// <summary>
	    /// Remove the '/' from the end of the url
	    /// </summary>
	    /// <param name="subPath">path</param>
	    /// <returns>removed / at end</returns>
	    [Obsolete("use PathHelper.RemoveLatestSlash()")]
        public string SubPathSlashRemove(string subPath = "/")
        {
            if (string.IsNullOrEmpty(subPath)) return subPath;

            // remove / from end
            if (subPath.Substring(subPath.Length - 1, 1) == "/" && subPath != "/")
            {
                subPath = subPath.Substring(0, subPath.Length - 1);
            }

            return subPath;
        }

        /// <summary>
        /// Get the name of Key in the cache db
        /// </summary>
        /// <param name="functionName">how is the function called</param>
        /// <param name="singleItemDbPath">the path</param>
        /// <returns>an unique key</returns>
        private string CachingDbName(string functionName, string singleItemDbPath)
        {
	        // when is nothing assume its the home item
            if ( string.IsNullOrWhiteSpace(singleItemDbPath) ) singleItemDbPath = "/";
            // For creating an unique name: DetailView_/2018/01/1.jpg
            var uniqueSingleDbCacheNameBuilder = new StringBuilder();
            uniqueSingleDbCacheNameBuilder.Append(functionName + "_" + singleItemDbPath);
            return uniqueSingleDbCacheNameBuilder.ToString();
        }
        
        /// <summary>
        /// Update one single item in the database
        /// For the API/update endpoint
        /// </summary>
        /// <param name="updateStatusContent">content to updated</param>
        /// <returns>this item</returns>
        public async Task<FileIndexItem> UpdateItemAsync(FileIndexItem updateStatusContent)
        {
	        async Task LocalQuery(DbContext context, FileIndexItem fileIndexItem)
	        {
		        //  Update te last edited time manual
		        fileIndexItem.SetLastEdited();
		        context.Attach(fileIndexItem).State = EntityState.Modified;
		        await context.SaveChangesAsync();
		        context.Attach(fileIndexItem).State = EntityState.Detached;
		        CacheUpdateItem(new List<FileIndexItem>{updateStatusContent});
		        SetGetObjectByFilePathCache(fileIndexItem.FilePath, updateStatusContent, TimeSpan.FromMinutes(1));
	        }

	        try
	        {
		        await LocalQuery(_context, updateStatusContent);
	        }
	        catch ( ObjectDisposedException e )
	        {
		        await RetrySaveChangesAsync(updateStatusContent, e);
	        }
	        catch ( DbUpdateConcurrencyException concurrencyException)
	        {
		        SolveConcurrencyExceptionLoop(concurrencyException.Entries);
		        try
		        {
			        await _context.SaveChangesAsync();
		        }
		        catch ( DbUpdateConcurrencyException e)
		        {
			        _logger?.LogInformation(e, "[UpdateItemAsync] save failed after DbUpdateConcurrencyException");
		        }
	        }
            
	        return updateStatusContent;
        }

        /// <summary>
        /// Update item in Database Async
        /// You should update the cache yourself (so this is NOT done)
        /// </summary>
        /// <param name="updateStatusContentList">content to update</param>
        /// <returns>same item</returns>
        public async Task<List<FileIndexItem>> UpdateItemAsync(List<FileIndexItem> updateStatusContentList)
        {
	        if ( !updateStatusContentList.Any() ) return new List<FileIndexItem>();
	        
	        async Task<List<FileIndexItem>> LocalQuery(DbContext context, List<FileIndexItem> fileIndexItems)
	        {
		        foreach ( var item in fileIndexItems )
		        {
			        item.SetLastEdited();
			        context.Attach(item).State = EntityState.Modified;
		        }

		        await context.SaveChangesAsync();
		        
		        foreach ( var item in fileIndexItems )
		        {
			        context.Attach(item).State = EntityState.Detached;
		        }

		        CacheUpdateItem(fileIndexItems);
		        return fileIndexItems;
	        }

	        try
	        {
		        return await LocalQuery(_context, updateStatusContentList);
	        }
	        catch ( ObjectDisposedException )
	        {
		        var context = new InjectServiceScope(_scopeFactory).Context();
		        try
		        {
			        return await LocalQuery(context, updateStatusContentList);
		        }
		        catch ( DbUpdateConcurrencyException concurrencyException)
		        {
			        SolveConcurrencyExceptionLoop(concurrencyException.Entries);
			        return await LocalQuery(context, updateStatusContentList);
		        }
	        }
	        catch ( DbUpdateConcurrencyException concurrencyException)
	        {
		        SolveConcurrencyExceptionLoop(concurrencyException.Entries);
		        try
		        {
			        return await LocalQuery(_context, updateStatusContentList);
		        }
		        catch ( DbUpdateConcurrencyException e)
		        {
			        var items =  await GetObjectsByFilePathQueryAsync(updateStatusContentList
				        .Select(p => p.FilePath).ToList());
			        _logger?.LogInformation($"double error UCL:{updateStatusContentList.Count} Count: {items.Count}", e);
			        return updateStatusContentList;
		        }
	        }
        }


        /// <summary>
        /// Retry when an Exception has occured
        /// </summary>
        /// <param name="updateStatusContent"></param>
        /// <param name="e">Exception</param>
        private async Task RetrySaveChangesAsync(FileIndexItem updateStatusContent, Exception e)
        {
	        _logger?.LogInformation(e,"[RetrySaveChangesAsync] retry catch-ed exception ");
	        _logger?.LogInformation("[RetrySaveChangesAsync] next retry ~>");
	        
	        async Task LocalRetrySaveChangesAsyncQuery()
	        {
		        // InvalidOperationException: A second operation started on this context before a previous operation completed.
		        // https://go.microsoft.com/fwlink/?linkid=2097913
		        await Task.Delay(5);
		        var context = new InjectServiceScope(_scopeFactory).Context();
		        context.Attach(updateStatusContent).State = EntityState.Modified;
		        await context.SaveChangesAsync();
		        context.Attach(updateStatusContent).State = EntityState.Detached; 
	        }

	        try
	        {
		        await LocalRetrySaveChangesAsyncQuery();
	        }
	        catch ( MySqlException mySqlException)
	        {
		        _logger?.LogError(mySqlException,"[RetrySaveChangesAsync] MySqlException catch-ed and retry again");
		        await LocalRetrySaveChangesAsyncQuery();
	        }
	        catch ( DbUpdateConcurrencyException concurrencyException)
	        {
		        SolveConcurrencyExceptionLoop(concurrencyException.Entries);
		        try
		        {
			        _logger?.LogInformation("[RetrySaveChangesAsync] SolveConcurrencyExceptionLoop disposed item");
			        var context = new InjectServiceScope(_scopeFactory).Context();
			        await context.SaveChangesAsync();
		        }
		        catch ( DbUpdateConcurrencyException retry2Exception)
		        {
			        _logger?.LogInformation(retry2Exception, 
				        "[RetrySaveChangesAsync] save failed after DbUpdateConcurrencyException");
		        }
	        }
        }
        
        /// <summary>
        /// Update one single item in the database
        /// For the API/update endpoint
        /// </summary>
        /// <param name="updateStatusContent">content to updated</param>
        /// <returns>this item</returns>
        public FileIndexItem UpdateItem(FileIndexItem updateStatusContent)
        {
	        void LocalUpdateItemQuery(ApplicationDbContext context)
	        {
		        //  Update te last edited time manual
		        updateStatusContent.SetLastEdited();
		        context.Attach(updateStatusContent).State = EntityState.Modified;
				context.SaveChanges();
		        context.Attach(updateStatusContent).State = EntityState.Detached;
		        CacheUpdateItem(new List<FileIndexItem>{updateStatusContent});
	        }
	        
	        try
	        {
		        LocalUpdateItemQuery(_context);
	        }
	        catch ( ObjectDisposedException error)
	        {
		        _logger?.LogInformation(error,"[UpdateItem] catch-ed ObjectDisposedException");
		        var context = new InjectServiceScope(_scopeFactory).Context();
		        LocalUpdateItemQuery(context);
	        }
	        catch (InvalidOperationException)
	        {
		        var context = new InjectServiceScope(_scopeFactory).Context();
		        LocalUpdateItemQuery(context);
	        }
	        catch (DbUpdateConcurrencyException concurrencyException)
	        {
		        SolveConcurrencyExceptionLoop(concurrencyException.Entries);
		        try
		        {
			        _context.SaveChanges();
		        }
		        catch ( DbUpdateConcurrencyException e)
		        {
			        _logger?.LogInformation(e, "[UpdateItem] save failed after DbUpdateConcurrencyException");
		        }
	        }

            return updateStatusContent;
        }
        
        /// <summary>
        /// Update a list of items in the index
        /// Used for the API/update endpoint
        /// </summary>
        /// <param name="updateStatusContentList">list of items to be updated</param>
        /// <returns>the same list, and updated in the database</returns>
        public List<FileIndexItem> UpdateItem(List<FileIndexItem> updateStatusContentList)
        {
	        void LocalQuery(ApplicationDbContext context)
	        {
		        foreach ( var item in updateStatusContentList )
		        {
			        item.SetLastEdited();
			        context.Attach(item).State = EntityState.Modified;
		        }
		        context.SaveChanges();
	        }

	        try
	        {
		        LocalQuery(_context);
	        }
	        catch (ObjectDisposedException)
	        {
		        var context = new InjectServiceScope(_scopeFactory).Context();
		        LocalQuery(context);
	        }
	        catch (InvalidOperationException)
	        {
		        var context = new InjectServiceScope(_scopeFactory).Context();
		        LocalQuery(context);
	        }
	        catch (DbUpdateConcurrencyException concurrencyException)
	        {
		        SolveConcurrencyExceptionLoop(concurrencyException.Entries);
		        try
		        {
			        _context.SaveChanges();
		        }
		        catch ( DbUpdateConcurrencyException e)
		        {
			        _logger?.LogInformation(e, "[UpdateItem] save failed after DbUpdateConcurrencyException");
		        }
	        }
	        
	        CacheUpdateItem(updateStatusContentList);
	        return updateStatusContentList;
        }

        internal void SolveConcurrencyExceptionLoop(
	        IReadOnlyList<EntityEntry> concurrencyExceptionEntries)
        {
	        foreach (var entry in concurrencyExceptionEntries)
	        {
		        SolveConcurrencyException(entry.Entity, entry.CurrentValues,
			        entry.GetDatabaseValues(), entry.Metadata.Name, 
			        entry.OriginalValues.SetValues);
	        }
        }
        
        /// <summary>
        /// Delegate to abstract OriginalValues Setter
        /// </summary>
        /// <param name="propertyValues"> propertyValues</param>
        internal delegate void OriginalValuesSetValuesDelegate(PropertyValues propertyValues);

        /// <summary>
        /// Database concurrency refers to situations in which multiple processes or users access or change the same data in a database at the same time.
        /// @see: https://docs.microsoft.com/en-us/ef/core/saving/concurrency
        /// </summary>
        /// <param name="entryEntity">item</param>
        /// <param name="proposedValues">new update</param>
        /// <param name="databaseValues">old database item</param>
        /// <param name="entryMetadataName">meta name</param>
        /// <param name="entryOriginalValuesSetValues">entry item</param>
        /// <exception cref="NotSupportedException">unknown how to fix</exception>
        internal void SolveConcurrencyException(object entryEntity, 
	        PropertyValues proposedValues, PropertyValues databaseValues, string entryMetadataName, 
	        OriginalValuesSetValuesDelegate entryOriginalValuesSetValues)
        {
	        if ( !( entryEntity is FileIndexItem ) )
		        throw new NotSupportedException(
			        "Don't know how to handle concurrency conflicts for "
			        + entryMetadataName);
	        
	        foreach (var property in proposedValues.Properties)
	        {
		        var proposedValue = proposedValues[property];
		        proposedValues[property] = proposedValue;
	        }

	        // Refresh original values to bypass next concurrency check
	        if ( databaseValues != null )
	        {
		        entryOriginalValuesSetValues(databaseValues);
	        }
        }
        
        /// <summary>
        /// Is Cache enabled, null object or feature toggle disabled
        /// </summary>
        /// <returns>true when enabled</returns>
	    internal bool IsCacheEnabled()
	    {
		    if( _cache == null || _appSettings?.AddMemoryCache == false) return false;
		    return true;
	    }

	    /// <summary>
	    /// Add child item to parent cache
	    /// Private api within Query to add cached items
	    /// Assumes that the parent directory already exist in the cache
	    /// @see: AddCacheParentItem to add parent item
	    /// </summary>
	    /// <param name="updateStatusContent">the content to add</param>
        internal void AddCacheItem(FileIndexItem updateStatusContent)
        {
            // If cache is turned of
            if( _cache == null || _appSettings?.AddMemoryCache == false) return;

            var queryCacheName = CachingDbName(typeof(List<FileIndexItem>).Name, 
                updateStatusContent.ParentDirectory);

            if (!_cache.TryGetValue(queryCacheName, out var objectFileFolders)) return;
            
            var displayFileFolders = (List<FileIndexItem>) objectFileFolders;
            
            displayFileFolders.Add(updateStatusContent);
            // Order by filename
            displayFileFolders = displayFileFolders.OrderBy(p => p.FileName).ToList();
            
            _cache.Remove(queryCacheName);
            _cache.Set(queryCacheName, displayFileFolders, new TimeSpan(1,0,0));
        }

        /// <summary>
        /// Cache API within Query to update cached items
        /// </summary>
        /// <param name="updateStatusContent">items to update</param>
        public void CacheUpdateItem(List<FileIndexItem> updateStatusContent)
        {
            if( _cache == null || _appSettings?.AddMemoryCache == false) return;

            var skippedCacheItems = new HashSet<string>();
			foreach (var item in updateStatusContent.ToList())
			{
				// ToList() > Collection was modified; enumeration operation may not execute.
				var queryCacheName = CachingDbName(typeof(List<FileIndexItem>).Name, 
					item.ParentDirectory);

				if ( !_cache.TryGetValue(queryCacheName,
					out var objectFileFolders) )
				{
					skippedCacheItems.Add(item.ParentDirectory);
					continue;
				}
				
				var displayFileFolders = (List<FileIndexItem>) objectFileFolders;

				// make it a list to avoid enum errors
				displayFileFolders = displayFileFolders.ToList();
				
				var obj = displayFileFolders.FirstOrDefault(p => p.FilePath == item.FilePath);
				if (obj == null) continue;
				displayFileFolders.Remove(obj);

				if ( item.Status == FileIndexItem.ExifStatus.OkAndSame )
				{
					item.Status = FileIndexItem.ExifStatus.Ok;
				}

				// Add here item to cached index
				displayFileFolders.Add(item);
				
				// make it a list to avoid enum errors
				displayFileFolders = displayFileFolders.ToList();
				// Order by filename
				displayFileFolders = displayFileFolders.OrderBy(p => p.FileName).ToList();
				
				_cache.Remove(queryCacheName);
				_cache.Set(queryCacheName, displayFileFolders, new TimeSpan(1,0,0));
			}

			if ( skippedCacheItems.Any() )
			{
				_logger?.LogInformation($"[CacheUpdateItem] skipped: {string.Join(", ", skippedCacheItems)}");
			}
			
        }

        /// <summary>
        /// Cache Only! Private api within Query to remove cached items
        /// This Does remove a SINGLE item from the cache NOT from the database
        /// </summary>
        /// <param name="updateStatusContent"></param>
        public void RemoveCacheItem(List<FileIndexItem> updateStatusContent)
        {
	        if( _cache == null || _appSettings?.AddMemoryCache == false) return;

	        foreach ( var item in updateStatusContent.ToList() )
	        {
		        RemoveCacheItem(item);
	        }
        }
        
        
        /// <summary>
        /// Cache Only! Private api within Query to remove cached items
        /// This Does remove a SINGLE item from the cache NOT from the database
        /// </summary>
        /// <param name="updateStatusContent"></param>
        public void RemoveCacheItem(FileIndexItem updateStatusContent)
        {
            // Add protection for disabled caching
            if( _cache == null || _appSettings?.AddMemoryCache == false) return;

            var queryCacheName = CachingDbName(typeof(List<FileIndexItem>).Name, 
                updateStatusContent.ParentDirectory);

            if (!_cache.TryGetValue(queryCacheName, out var objectFileFolders)) return;
            
            var displayFileFolders = (List<FileIndexItem>) objectFileFolders;
                        // Order by filename
            displayFileFolders = displayFileFolders
	            .Where(p => p.FilePath != updateStatusContent.FilePath)
	            .OrderBy(p => p.FileName).ToList();
            
            _cache.Remove(queryCacheName);
            // generate list again
            _cache.Set(queryCacheName, displayFileFolders, new TimeSpan(1,0,0));
        }

        /// <summary>
        /// Clear the directory name from the cache
        /// </summary>
        /// <param name="directoryName">the path of the directory (there is no parent generation)</param>
        public bool RemoveCacheParentItem(string directoryName)
        {
            // Add protection for disabled caching
            if( _cache == null || _appSettings?.AddMemoryCache == false) return false;
            
            var queryCacheName = CachingDbName(typeof(List<FileIndexItem>).Name, 
                PathHelper.RemoveLatestSlash(directoryName.Clone().ToString()));
            if (!_cache.TryGetValue(queryCacheName, out _)) return false;
            
            _cache.Remove(queryCacheName);
            return true;
        }

        /// <summary>
        /// Add an new Parent Item
        /// </summary>
        /// <param name="directoryName">the path of the directory (there is no parent generation)</param>
        /// <param name="items">the items in the folder</param>
        public bool AddCacheParentItem(string directoryName, List<FileIndexItem> items)
        {
	        // Add protection for disabled caching
	        if( _cache == null || _appSettings?.AddMemoryCache == false) return false;
            
	        var queryCacheName = CachingDbName(typeof(List<FileIndexItem>).Name, 
		        PathHelper.RemoveLatestSlash(directoryName.Clone().ToString()));
            
	        _cache.Set(queryCacheName, items,  
		        new TimeSpan(1,0,0));
	        return true;
        }

	    /// <summary>
	    /// Add a new item to the database
	    /// </summary>
	    /// <param name="updateStatusContent">the item</param>
	    /// <returns>item with id</returns>
        public FileIndexItem AddItem(FileIndexItem updateStatusContent)
        {        
	        if( string.IsNullOrWhiteSpace(updateStatusContent.FileName) 
	            && updateStatusContent.IsDirectory == false) 
		        throw new MissingFieldException("use filename (exception: the root folder can have no name)");

	        try
	        {
		        _context.FileIndex.Add(updateStatusContent);
		        _context.SaveChanges();
	        }
	        catch ( ObjectDisposedException )
	        {
		        var context = new InjectServiceScope(_scopeFactory).Context();
		        context.FileIndex.Add(updateStatusContent);
		        context.SaveChanges();
	        }
	        catch ( DbUpdateConcurrencyException e)
	        {
		        _logger?.LogInformation("AddItem catch-ed DbUpdateConcurrencyException (ignored)", e);
	        }
            
            AddCacheItem(updateStatusContent);

			return updateStatusContent;
        }
	    
	    /// <summary>
	    /// Add a new item to the database
	    /// </summary>
	    /// <param name="fileIndexItem">the item</param>
	    /// <returns>item with id</returns>
	    public virtual async Task<FileIndexItem> AddItemAsync(FileIndexItem fileIndexItem)
	    {
		    async Task<FileIndexItem> LocalDefaultQuery()
		    {
			    var context = new InjectServiceScope(_scopeFactory).Context();
			    return await LocalQuery(context);
		    }

		    async Task<FileIndexItem> LocalQuery(ApplicationDbContext context)
		    {
			    // only in test case fileIndex is null
			    if ( context.FileIndex != null ) await context.FileIndex.AddAsync(fileIndexItem);
			    await context.SaveChangesAsync();
			    // Fix for: The instance of entity type 'Item' cannot be tracked because
			    // another instance with the same key value for {'Id'} is already being tracked
			    context.Entry(fileIndexItem).State = EntityState.Unchanged;
			    AddCacheItem(fileIndexItem);
			    return fileIndexItem;
		    }

		    try
		    {
			    return await LocalQuery(_context);
		    }
		    catch ( Microsoft.Data.Sqlite.SqliteException e)
		    {
			    _logger?.LogInformation(e, "catch-ed SqliteException going to retry 2 times");
			    return await RetryHelper.DoAsync(
				    LocalDefaultQuery, TimeSpan.FromSeconds(2), 2);
		    }
		    catch ( DbUpdateException e)
		    {
			    _logger?.LogInformation(e, "catch-ed DbUpdateException going to retry 2 times");
			    return await RetryHelper.DoAsync(
				    LocalDefaultQuery, TimeSpan.FromSeconds(2), 2);
		    }
		    catch ( ObjectDisposedException )
		    {
			    var context = new InjectServiceScope(_scopeFactory).Context();
			    return await LocalQuery(context);
		    }
	    }

	    private async Task<List<FileIndexItem>> GetParentItems(List<string> pathListShouldExist)
	    {
		    async Task<List<FileIndexItem>> LocalQuery(ApplicationDbContext context)
		    {
			    return await context.FileIndex.Where(p => 
				    pathListShouldExist.Any(f => f == p.FilePath)).ToListAsync();
		    }

		    try
		    {
			    return await LocalQuery(_context);

		    }
		    catch ( ObjectDisposedException)
		    {
			    return await LocalQuery(new InjectServiceScope( _scopeFactory).Context());
		    }
	    }
	    
	    /// <summary>
	    /// Add Sub Path Folder - Parent Folders
	    ///  root(/)
	    ///      /2017  *= index only this folder
	    ///      /2018
	    /// If you use the cmd: $ starskycli -s "/2017"
	    /// the folder '2017' it self is not added 
	    /// and all parent paths are not included
	    /// this class does add those parent folders
	    /// </summary>
	    /// <param name="subPath">subPath as input</param>
	    /// <returns>void</returns>
	    public async Task AddParentItemsAsync(string subPath)
	    {
		    var path = subPath == "/" || string.IsNullOrEmpty(subPath) ? "/" : PathHelper.RemoveLatestSlash(subPath);
		    var pathListShouldExist = Breadcrumbs.BreadcrumbHelper(path).ToList();

		    var indexItems = await GetParentItems(pathListShouldExist);

		    var toAddList = new List<FileIndexItem>();
		    // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
		    foreach ( var pathShouldExist in pathListShouldExist )
		    {
			    if ( !indexItems.Select(p => p.FilePath).Contains(pathShouldExist) )
			    {
				    toAddList.Add(new FileIndexItem(pathShouldExist)
				    {
					    IsDirectory = true,
					    AddToDatabase = DateTime.UtcNow,
					    ColorClass = ColorClassParser.Color.None,
					    Software = pathShouldExist == "/" ? "Root object" : string.Empty
				    });
			    }
		    }

		    await AddRangeAsync(toAddList);
	    }

	    /// <summary>
	    /// Remove a new item from the database (NOT from the file system)
	    /// </summary>
	    /// <param name="updateStatusContent">the FileIndexItem with database data</param>
	    /// <returns></returns>
        public FileIndexItem RemoveItem(FileIndexItem updateStatusContent)
        {
	        void LocalQuery(ApplicationDbContext context)
	        {
		        // Detach first https://stackoverflow.com/a/42475617
		        var local = context.Set<FileIndexItem>()
			        .Local
			        .FirstOrDefault(entry => entry.Id.Equals(updateStatusContent.Id));
		        if (local != null)
		        {
			        context.Entry(local).State = EntityState.Detached;
		        }
		        
		        context.Attach(updateStatusContent).State = EntityState.Deleted;
		        context.FileIndex.Remove(updateStatusContent);
		        context.SaveChanges();
		        context.Attach(updateStatusContent).State = EntityState.Detached;
	        }

	        try
	        {
		        LocalQuery(_context);
	        }
	        catch ( ObjectDisposedException disposedException)
	        {
		        _logger?.LogInformation("catch-ed disposedException:",disposedException);
		        var context = new InjectServiceScope(_scopeFactory).Context();
		        LocalQuery(context);
	        }
	        catch (DbUpdateConcurrencyException concurrencyException)
	        {
		        _logger?.LogInformation("catch-ed concurrencyException:",concurrencyException);
		        try
		        {
			        _context.SaveChanges();
		        }
		        catch ( DbUpdateConcurrencyException e)
		        {
			        _logger?.LogInformation(e, "[RemoveItem] save failed after DbUpdateConcurrencyException");
		        }
	        }
	        
	        // remove parent directory cache
			RemoveCacheItem(updateStatusContent);

			// remove getFileHash Cache
			ResetItemByHash(updateStatusContent.FileHash);
			return updateStatusContent;
	    }
	    
	    /// <summary>
	    /// Remove a new item from the database (NOT from the file system)
	    /// </summary>
	    /// <param name="updateStatusContent">the FileIndexItem with database data</param>
	    /// <returns></returns>
	    public async Task<FileIndexItem> RemoveItemAsync(FileIndexItem updateStatusContent)
	    {
		    async Task<bool> LocalRemoveDefaultQuery()
		    {
			    await LocalRemoveQuery(new InjectServiceScope(_scopeFactory).Context());
			    return true;
		    }

		    async Task LocalRemoveQuery(ApplicationDbContext context)
		    {
			    context.FileIndex?.Remove(updateStatusContent);
			    await context.SaveChangesAsync();
		    }

		    try
		    {
			    await LocalRemoveQuery(_context);
		    }
		    catch ( Microsoft.Data.Sqlite.SqliteException )
		    {
			    // Files that are locked
			    await RetryHelper.DoAsync(LocalRemoveDefaultQuery,
				    TimeSpan.FromSeconds(2), 4);
		    }
		    catch ( ObjectDisposedException )
		    {
			    await LocalRemoveDefaultQuery();
		    }
		    catch ( InvalidOperationException )
		    {
			    await LocalRemoveDefaultQuery();
		    }
		    catch ( DbUpdateConcurrencyException e)
		    {
			    _logger?.LogInformation(e,"[RemoveItemAsync] catch-ed " +
			                              "DbUpdateConcurrencyException (do nothing)");
		    }

		    // remove parent directory cache
		    RemoveCacheItem(updateStatusContent);

		    // remove getFileHash Cache
		    ResetItemByHash(updateStatusContent.FileHash);
		    return updateStatusContent;
	    }
    }
}
