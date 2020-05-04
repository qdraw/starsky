using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.database.Data;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.injection;
using starsky.foundation.platform.Helpers;
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

        public Query(ApplicationDbContext context, 
            IMemoryCache memoryCache = null, 
            AppSettings appSettings = null,
            IServiceScopeFactory scopeFactory = null)
        {
	        _context = new InjectServiceScope(context, scopeFactory).Context();
            _cache = memoryCache;
            _appSettings = appSettings;
            _scopeFactory = scopeFactory;
        }

	    /// <summary>
		/// Get a list of all files inside an folder
		/// But this uses a database as source
		/// </summary>
		/// <param name="subPath">relative database path</param>
		/// <returns>list of FileIndex-objects</returns>
        public List<FileIndexItem> GetAllFiles(string subPath)
        {
            subPath = SubPathSlashRemove(subPath);

            try
            {
	            return _context.FileIndex.Where
			            (p => !p.IsDirectory && p.ParentDirectory == subPath)
		            .OrderBy(r => r.FileName).ToList();
            }
            catch ( ObjectDisposedException )
            {
	            var context = new InjectServiceScope(null, _scopeFactory).Context();
	            return context.FileIndex.Where
			            (p => !p.IsDirectory && p.ParentDirectory == subPath)
		            .OrderBy(r => r.FileName).ToList();
            }
        }
        
        // Includes sub items in file
        // Used for Orphan Check
        // All files in
        public List<FileIndexItem> GetAllRecursive(
            string subPath = "/")
        {
            subPath = SubPathSlashRemove(subPath);
            
            return _context.FileIndex.Where
                    (p => p.ParentDirectory.Contains(subPath) )
                .OrderBy(r => r.FileName).ToList();
        }

		/// <summary>
		/// Returns a database object file or folder
		/// </summary>
		/// <param name="filePath">relative database path</param>
		/// <returns>FileIndex-objects with database data</returns>
        public FileIndexItem GetObjectByFilePath(string filePath)
        {
            filePath = SubPathSlashRemove(filePath);
            FileIndexItem query;
            try
            {
	            query = _context.FileIndex.FirstOrDefault(p => p.FilePath == filePath);
            }
            catch (ObjectDisposedException)
            {
	            if ( _appSettings != null && _appSettings.Verbose )	 Console.WriteLine("catch ObjectDisposedException");
	            _context = new InjectServiceScope(null, _scopeFactory).Context();
	            query = _context.FileIndex.FirstOrDefault(p => p.FilePath == filePath);
            }
            return query;
        }
	    
		/// <summary>
		/// Get subpath based on hash (cached hashlist view to clear use ResetItemByHash)
		/// </summary>
		/// <param name="fileHash">base32 hash</param>
		/// <returns>subpath (relative to database)</returns>
	    public string GetSubPathByHash(string fileHash)
	    {
		    // The CLI programs uses no cache
		    if( !IsCacheEnabled() ) return QueryGetItemByHash(fileHash);
            
		    // Return values from IMemoryCache
		    var queryCacheName = CachingDbName("hashList", fileHash);

		    // if result is not null return cached value
		    if ( _cache.TryGetValue(queryCacheName, out var cachedSubpath) 
		         && !string.IsNullOrEmpty((string)cachedSubpath)) return ( string ) cachedSubpath;

		    cachedSubpath = QueryGetItemByHash(fileHash);
		    
		    _cache.Set(queryCacheName, cachedSubpath, new TimeSpan(48,0,0));
		    return (string) cachedSubpath;
		}

		/// <summary>
		/// Remove fileHash from hash-list-cache
		/// </summary>
		/// <param name="fileHash">base32 filehash</param>
	    public void ResetItemByHash(string fileHash)
	    {
		    if( _cache == null || _appSettings?.AddMemoryCache == false) return;
		    
			var queryCacheName = CachingDbName("hashList", fileHash);
			
			if ( _cache.TryGetValue(queryCacheName, out var cachedSubpath) )
			{
				_cache.Remove(queryCacheName);
			}
	    }

	    // Return a File Item By it Hash value
        // New added, directory hash now also hashes
        private string QueryGetItemByHash(string fileHash)
        {            
			var query = _context.FileIndex.FirstOrDefault(
				p => p.FileHash == fileHash 
				&& !p.IsDirectory
			);
			return query?.FilePath;
        }

	    // Remove the '/' from the end of the url
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

        private string CachingDbName(string functionName, string singleItemDbPath)
        {
            // For creating an unique name: DetailView_/2018/01/1.jpg
            
            var uniqueSingleDbCacheNameBuilder = new StringBuilder();
            uniqueSingleDbCacheNameBuilder.Append(functionName + "_" + singleItemDbPath);
            return uniqueSingleDbCacheNameBuilder.ToString();
        }


        /// <summary>
        /// Update a list of items in the index
        /// Used for the API/update endpoint
        /// </summary>
        /// <param name="updateStatusContentList">list of items to be updated</param>
        /// <returns>the same list, and updated in the database</returns>
        public List<FileIndexItem> UpdateItem(List<FileIndexItem> updateStatusContentList)
        {
            foreach (var item in updateStatusContentList)
            {
	            //  Update te last edited time manual
	            item.SetLastEdited();
	            
	            // Set state to edit mode
                _context.Attach(item).State = EntityState.Modified;
            }
            
	        _context.SaveChanges();
            
	        CacheUpdateItem(updateStatusContentList);
            return updateStatusContentList;
        }

        /// <summary>
        /// Update one single item in the database
        /// For the API/update endpoint
        /// </summary>
        /// <param name="updateStatusContent">content to updated</param>
        /// <returns>this item</returns>
        public FileIndexItem UpdateItem(FileIndexItem updateStatusContent)
        {
	        //  Update te last edited time manual
	        updateStatusContent.SetLastEdited();
	        try
	        {
				_context.Attach(updateStatusContent).State = EntityState.Modified;
	            _context.SaveChanges();
            }
            catch ( ObjectDisposedException)
            {
	            if ( _appSettings.Verbose ) Console.WriteLine("Retry ObjectDisposedException");
	            _context = new InjectServiceScope(null, _scopeFactory).Context();
	            _context.Attach(updateStatusContent).State = EntityState.Modified;
	            _context.SaveChanges();
            }
            
            CacheUpdateItem(new List<FileIndexItem>{updateStatusContent});
			
	        _context.Attach(updateStatusContent).State = EntityState.Detached;
	        
            return updateStatusContent;
        }

	    public bool IsCacheEnabled()
	    {
		    if( _cache == null || _appSettings?.AddMemoryCache == false) return false;
		    return true;
	    }

	    // Private api within Query to add cached items
        public void AddCacheItem(FileIndexItem updateStatusContent)
        {
            // Add protection for disabeling caching
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

        // Private api within Query to update cached items
        public void CacheUpdateItem(List<FileIndexItem> updateStatusContent)
        {
            if( _cache == null || _appSettings?.AddMemoryCache == false) return;

			foreach (var item in updateStatusContent.ToList())
			{
				// ToList() > Collection was modified; enumeration operation may not execute.
				var queryCacheName = CachingDbName(typeof(List<FileIndexItem>).Name, 
				item.ParentDirectory);
				
				if (!_cache.TryGetValue(queryCacheName, out var objectFileFolders)) return;
				
				var displayFileFolders = (List<FileIndexItem>) objectFileFolders;

				// make it a list to avoid enum errors
				displayFileFolders = displayFileFolders.ToList();
				
				var obj = displayFileFolders.FirstOrDefault(p => p.FilePath == item.FilePath);
				if (obj == null) return;
				displayFileFolders.Remove(obj);
				// Add here item to cached index
				displayFileFolders.Add(item);
				
				// make it a list to avoid enum errors
				displayFileFolders = displayFileFolders.ToList();
				// Order by filename
				displayFileFolders = displayFileFolders.OrderBy(p => p.FileName).ToList();
				
				_cache.Remove(queryCacheName);
				_cache.Set(queryCacheName, displayFileFolders, new TimeSpan(1,0,0));
			}
            
        }
        
        // Private api within Query to remove cached items
        // This Does remove a SINGLE item from the cache NOT from the database
        public void RemoveCacheItem(FileIndexItem updateStatusContent)
        {
            // Add protection for disabeling caching
            if( _cache == null || _appSettings?.AddMemoryCache == false) return;

            var queryCacheName = CachingDbName(typeof(List<FileIndexItem>).Name, 
                updateStatusContent.ParentDirectory);

            if (!_cache.TryGetValue(queryCacheName, out var objectFileFolders)) return;
            
            var displayFileFolders = (List<FileIndexItem>) objectFileFolders;
                        // Order by filename
            displayFileFolders = displayFileFolders.OrderBy(p => p.FileName).ToList();
            
            _cache.Remove(queryCacheName);
            // generate list agian
            _cache.Set(queryCacheName, displayFileFolders, new TimeSpan(1,0,0));
        }

        /// <summary>
        /// Clear the directory name from the cache
        /// </summary>
        /// <param name="directoryName">the path of the directory (there is no parent generation)</param>
        public void RemoveCacheParentItem(string directoryName)
        {
            // Add protection for disabled caching
            if( _cache == null || _appSettings?.AddMemoryCache == false) return;
            
            var queryCacheName = CachingDbName(typeof(List<FileIndexItem>).Name, 
                PathHelper.RemoveLatestSlash(directoryName));
            if (!_cache.TryGetValue(queryCacheName, out var objectFileFolders)) return;
            
            _cache.Remove(queryCacheName);
        }

	    /// <summary>
	    /// Add a new item to the database
	    /// </summary>
	    /// <param name="updateStatusContent">the item</param>
	    /// <returns>item with id</returns>
        public FileIndexItem AddItem(FileIndexItem updateStatusContent)
        {        
	        if( string.IsNullOrWhiteSpace(updateStatusContent.FileName) 
	            && !updateStatusContent.IsDirectory) 
		        throw new MissingFieldException("use filename (exception: the root folder can have no name)");

	        try
	        {
		        _context.FileIndex.Add(updateStatusContent);
		        _context.SaveChanges();
	        }
	        catch (ObjectDisposedException)
	        {
		        var context = new InjectServiceScope(null, _scopeFactory).Context();
		        context.FileIndex.Add(updateStatusContent);
		        context.SaveChanges();
	        }
            
            AddCacheItem(updateStatusContent);

			return updateStatusContent;
        }
	    
	    /// <summary>
	    /// Add a new item to the database
	    /// </summary>
	    /// <param name="updateStatusContent">the item</param>
	    /// <returns>item with id</returns>
	    public virtual async Task<FileIndexItem> AddItemAsync(FileIndexItem updateStatusContent)
	    {
		    try
		    {
			    await _context.FileIndex.AddAsync(updateStatusContent);
			    await _context.SaveChangesAsync();
			    // Fix for: The instance of entity type 'Item' cannot be tracked because
			    // another instance with the same key value for {'Id'} is already being tracked
			    _context.Entry(updateStatusContent).State = EntityState.Unchanged;
		    }
		    catch (ObjectDisposedException)
		    {
			    var context = new InjectServiceScope(null, _scopeFactory).Context();
			    await context.FileIndex.AddAsync(updateStatusContent);
			    await context.SaveChangesAsync();
			    context.Entry(updateStatusContent).State = EntityState.Unchanged;
		    }
            
		    AddCacheItem(updateStatusContent);

		    return updateStatusContent;
	    }
        
	    /// <summary>
	    /// Remove a new item from the database (NOT from the file system)
	    /// </summary>
	    /// <param name="updateStatusContent">the FileIndexItem with database data</param>
	    /// <returns></returns>
        public FileIndexItem RemoveItem(FileIndexItem updateStatusContent)
        {
            _context.FileIndex.Remove(updateStatusContent);
            _context.SaveChanges();

	        // remove parent directory cache
            RemoveCacheItem(updateStatusContent);

	        // remove getFileHash Cache
	        ResetItemByHash(updateStatusContent.FileHash);
            return updateStatusContent;
        }
    }
}
