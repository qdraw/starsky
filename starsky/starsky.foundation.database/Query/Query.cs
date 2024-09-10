using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;
using starsky.foundation.database.Data;
using starsky.foundation.database.Helpers;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.injection;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;

namespace starsky.foundation.database.Query;

[Service(typeof(IQuery), InjectionLifetime = InjectionLifetime.Scoped)]
// ReSharper disable once RedundantExtendsListEntry
public partial class Query : IQuery
{
	private readonly AppSettings _appSettings;
	private readonly IMemoryCache? _cache;
	private readonly IWebLogger _logger;
	private readonly IServiceScopeFactory? _scopeFactory;
	private ApplicationDbContext _context;

	public Query(ApplicationDbContext context,
		AppSettings appSettings,
		IServiceScopeFactory? scopeFactory,
		IWebLogger logger, IMemoryCache? memoryCache = null)
	{
		_context = context;
		_cache = memoryCache;
		_appSettings = appSettings;
		_scopeFactory = scopeFactory;
		_logger = logger;
	}

	/// <summary>
	///     Returns a database object file or folder
	/// </summary>
	/// <param name="filePath">relative database path</param>
	/// <returns>FileIndex-objects with database data</returns>
	public FileIndexItem? GetObjectByFilePath(string filePath)
	{
		if ( filePath != "/" ) filePath = PathHelper.RemoveLatestSlash(filePath);

		FileIndexItem? LocalQuery(ApplicationDbContext context)
		{
			var item = context.FileIndex.FirstOrDefault(p => p.FilePath == filePath);
			if ( item != null ) item.Status = FileIndexItem.ExifStatus.Ok;
			return item;
		}

		try
		{
			return LocalQuery(_context);
		}
		catch ( ObjectDisposedException e )
		{
			_logger.LogInformation("[GetObjectByFilePath] catch-ed ObjectDisposedException", e);
			return LocalQuery(new InjectServiceScope(_scopeFactory).Context());
		}
	}

	/// <summary>
	///     Returns a database object file or folder
	/// </summary>
	/// <param name="filePath">relative database path</param>
	/// <param name="cacheTime">time to have the cache present</param>
	/// <returns>FileIndex-objects with database data</returns>
	public async Task<FileIndexItem?> GetObjectByFilePathAsync(
		string filePath, TimeSpan? cacheTime = null)
	{
		// cache code:
		if ( cacheTime != null &&
		     _appSettings.AddMemoryCache == true &&
		     _cache != null &&
		     _cache.TryGetValue(
			     GetObjectByFilePathAsyncCacheName(filePath), out var data) )
		{
			if ( !( data is FileIndexItem fileIndexItem ) ) return null;
			fileIndexItem.Status = FileIndexItem.ExifStatus.OkAndSame;
			return fileIndexItem;
		}
		// end cache

		var result = await GetObjectByFilePathQueryAsync(filePath);

		// cache code:
		if ( cacheTime == null || _appSettings.AddMemoryCache != true || result == null )
			return result;

		SetGetObjectByFilePathCache(filePath, result.Clone(), cacheTime);

		return result;
	}

	public void SetGetObjectByFilePathCache(string filePath,
		FileIndexItem result,
		TimeSpan? cacheTime)
	{
		// ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
		if ( _cache == null || cacheTime == null || result == null )
		{
			_logger.LogDebug("SetGetObjectByFilePathCache not used");
			return;
		}

		_cache.Set(GetObjectByFilePathAsyncCacheName(filePath),
			result, cacheTime.Value);
	}

	public async Task<string?> GetSubPathByHashAsync(string fileHash)
	{
		// The CLI programs uses no cache
		if ( !IsCacheEnabled() || _cache == null ) return await QueryGetItemByHashAsync(fileHash);

		// Return values from IMemoryCache
		var queryHashListCacheName = CachingDbName("hashList", fileHash);

		// if result is not null return cached value
		if ( _cache.TryGetValue(queryHashListCacheName, out var cachedSubPath)
		     && !string.IsNullOrEmpty(( string? )cachedSubPath) ) return ( string )cachedSubPath;

		cachedSubPath = await QueryGetItemByHashAsync(fileHash);

		_cache.Set(queryHashListCacheName, cachedSubPath, new TimeSpan(48, 0, 0));
		return ( string? )cachedSubPath;
	}

	/// <summary>
	///     Remove fileHash from hash-list-cache
	/// </summary>
	/// <param name="fileHash">base32 fileHash</param>
	public void ResetItemByHash(string? fileHash)
	{
		if ( _cache == null || _appSettings.AddMemoryCache == false ) return;

		var queryCacheName = CachingDbName("hashList", fileHash);

		if ( _cache.TryGetValue(queryCacheName, out _) ) _cache.Remove(queryCacheName);
	}

	/// <summary>
	///     Update one single item in the database
	///     For the API/update endpoint
	/// </summary>
	/// <param name="updateStatusContent">content to updated</param>
	/// <returns>this item</returns>
	public async Task<FileIndexItem> UpdateItemAsync(FileIndexItem updateStatusContent)
	{
		async Task LocalQuery(ApplicationDbContext context, FileIndexItem fileIndexItem)
		{
			context.Attach(fileIndexItem).State = EntityState.Modified;
			await context.SaveChangesAsync();
			context.Attach(fileIndexItem).State = EntityState.Detached;
			// Clone to avoid reference when cache exists
			CacheUpdateItem(new List<FileIndexItem> { updateStatusContent.Clone() });
			if ( _appSettings.Verbose == true )
				// Ef core changes debug
				_logger.LogDebug(context.ChangeTracker.DebugView.LongView);

			// object cache path is used to avoid updates
			SetGetObjectByFilePathCache(fileIndexItem.FilePath!, updateStatusContent,
				TimeSpan.FromMinutes(1));
		}

		try
		{
			await LocalQuery(_context, updateStatusContent);
		}
		catch ( ObjectDisposedException e )
		{
			await RetryQueryUpdateSaveChangesAsync(updateStatusContent, e,
				"UpdateItemAsync ObjectDisposedException");
		}
		catch ( InvalidOperationException e )
		{
			// System.InvalidOperationException: Can't replace active reader.
			await RetryQueryUpdateSaveChangesAsync(updateStatusContent, e,
				$"UpdateItemAsync InvalidOperationException {updateStatusContent.FilePath}", 2000);
		}
		catch ( DbUpdateConcurrencyException concurrencyException )
		{
			SolveConcurrency.SolveConcurrencyExceptionLoop(concurrencyException.Entries);
			try
			{
				await _context.SaveChangesAsync();
			}
			catch ( DbUpdateConcurrencyException e )
			{
				_logger.LogInformation(e,
					"[UpdateItemAsync] save failed after DbUpdateConcurrencyException");
			}
		}
		catch ( MySqlException exception )
		{
			// Skip if Duplicate entry
			// MySqlConnector.MySqlException (0x80004005): Duplicate entry for key 'PRIMARY'
			if ( !exception.Message.Contains("Duplicate") ) throw;
			_logger.LogError(exception,
				$"[UpdateItemAsync] Skipped MySqlException Duplicate entry for key {updateStatusContent.FilePath}");
		}

		return updateStatusContent;
	}

	/// <summary>
	///     Update item in Database Async
	///     You should update the cache yourself (so this is NOT done)
	/// </summary>
	/// <param name="updateStatusContentList">content to update</param>
	/// <returns>same item</returns>
	public async Task<List<FileIndexItem>> UpdateItemAsync(
		List<FileIndexItem> updateStatusContentList)
	{
		if ( updateStatusContentList.Count == 0 ) return new List<FileIndexItem>();

		async Task<List<FileIndexItem>> LocalQuery(DbContext context,
			List<FileIndexItem> fileIndexItems)
		{
			foreach ( var item in fileIndexItems )
				try
				{
					context.Attach(item).State = EntityState.Modified;
				}
				catch ( InvalidOperationException )
				{
					// System.InvalidOperationException: The property 'FileIndexItem.Id' has a temporary value while attempting to change the entity's state to 'Modified'
					// Issue #994
				}

			await context.SaveChangesAsync();

			foreach ( var item in fileIndexItems )
				context.Attach(item).State = EntityState.Detached;

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
			catch ( DbUpdateConcurrencyException concurrencyException )
			{
				SolveConcurrency.SolveConcurrencyExceptionLoop(concurrencyException.Entries);
				return await LocalQuery(context, updateStatusContentList);
			}
		}
		catch ( DbUpdateConcurrencyException concurrencyException )
		{
			SolveConcurrency.SolveConcurrencyExceptionLoop(concurrencyException.Entries);
			try
			{
				return await LocalQuery(_context, updateStatusContentList);
			}
			catch ( DbUpdateConcurrencyException e )
			{
				var items = await GetObjectsByFilePathQueryAsync(updateStatusContentList
					.Where(p => p.FilePath != null)
					.Select(p => p.FilePath).ToList()!);
				_logger.LogInformation(
					$"double error UCL:{updateStatusContentList.Count} Count: {items.Count}", e);
				return updateStatusContentList;
			}
		}
	}

	/// <summary>
	///     Cache API within Query to update cached items and implicit add items to list
	/// </summary>
	/// <param name="updateStatusContent">items to update</param>
	public void CacheUpdateItem(List<FileIndexItem> updateStatusContent)
	{
		if ( _cache == null || _appSettings.AddMemoryCache == false ) return;

		var skippedCacheItems = new HashSet<string>();
		foreach ( var item in updateStatusContent.ToList() )
		{
			if ( item.Status == FileIndexItem.ExifStatus.OkAndSame ||
			     item.Status == FileIndexItem.ExifStatus.Default )
				item.Status = FileIndexItem.ExifStatus.Ok;

			// ToList() > Collection was modified; enumeration operation may not execute.
			var queryCacheName = CachingDbName(nameof(FileIndexItem),
				item.ParentDirectory!);

			if ( !_cache.TryGetValue(queryCacheName,
				    out var objectFileFolders) )
			{
				skippedCacheItems.Add(item.ParentDirectory!);
				continue;
			}

			objectFileFolders ??= new List<FileIndexItem>();
			var displayFileFolders = ( List<FileIndexItem> )objectFileFolders;

			// make it a list to avoid enum errors
			displayFileFolders = displayFileFolders.ToList();

			var obj = displayFileFolders.Find(p => p.FilePath == item.FilePath);
			if ( obj != null )
				// remove add again
				displayFileFolders.Remove(obj);

			if ( item.Status ==
			     FileIndexItem.ExifStatus.Ok ) // ExifStatus.default is already changed
				// Add here item to cached index
				displayFileFolders.Add(item);

			// make it a list to avoid enum errors
			displayFileFolders = displayFileFolders.ToList();
			// Order by filename
			displayFileFolders = displayFileFolders.OrderBy(p => p.FileName).ToList();

			_cache.Remove(queryCacheName);
			_cache.Set(queryCacheName, displayFileFolders, new TimeSpan(1, 0, 0));
		}

		if ( skippedCacheItems.Count >= 1 && _appSettings.Verbose == true )
			_logger.LogInformation(
				$"[CacheUpdateItem] skipped: {string.Join(", ", skippedCacheItems)}");
	}

	/// <summary>
	///     Cache Only! Private api within Query to remove cached items
	///     This Does remove a SINGLE item from the cache NOT from the database
	/// </summary>
	/// <param name="updateStatusContent"></param>
	public void RemoveCacheItem(List<FileIndexItem> updateStatusContent)
	{
		if ( _cache == null || _appSettings.AddMemoryCache == false ) return;

		foreach ( var item in updateStatusContent.ToList() ) RemoveCacheItem(item);
	}

	/// <summary>
	///     Clear the directory name from the cache
	/// </summary>
	/// <param name="directoryName">the path of the directory (there is no parent generation)</param>
	public bool RemoveCacheParentItem(string directoryName)
	{
		// Add protection for disabled caching
		if ( _cache == null || _appSettings.AddMemoryCache == false ) return false;

		var queryCacheName = CachingDbName(nameof(FileIndexItem),
			PathHelper.RemoveLatestSlash(directoryName.Clone().ToString()!));
		if ( !_cache.TryGetValue(queryCacheName, out _) ) return false;

		_cache.Remove(queryCacheName);
		return true;
	}

	/// <summary>
	///     Add an new Parent Item
	/// </summary>
	/// <param name="directoryName">the path of the directory (there is no parent generation)</param>
	/// <param name="items">the items in the folder</param>
	public bool AddCacheParentItem(string directoryName, List<FileIndexItem> items)
	{
		// Add protection for disabled caching
		if ( _cache == null || _appSettings.AddMemoryCache == false ) return false;

		var queryCacheName = CachingDbName(nameof(FileIndexItem),
			PathHelper.RemoveLatestSlash(directoryName.Clone().ToString()!));

		_cache.Set(queryCacheName, items,
			new TimeSpan(1, 0, 0));
		return true;
	}

	/// <summary>
	///     Add a new item to the database
	/// </summary>
	/// <param name="fileIndexItem">the item</param>
	/// <returns>item with id</returns>
	[SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract")]
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
			context.Entry(fileIndexItem).State = EntityState.Detached;
			AddCacheItem(fileIndexItem);
			return fileIndexItem;
		}

		try
		{
			return await LocalQuery(_context);
		}
		catch ( SqliteException e )
		{
			_logger.LogInformation(e,
				$"[AddItemAsync] catch-ed SqliteException going to retry 2 times {fileIndexItem.FilePath}");
			return await RetryHelper.DoAsync(
				LocalDefaultQuery, TimeSpan.FromSeconds(2), 2);
		}
		catch ( DbUpdateException e )
		{
			_logger.LogInformation(e,
				$"[AddItemAsync] catch-ed DbUpdateException going to retry 2 times {fileIndexItem.FilePath}");
			return await RetryHelper.DoAsync(
				LocalDefaultQuery, TimeSpan.FromSeconds(2), 2);
		}
		catch ( InvalidOperationException e ) // or ObjectDisposedException
		{
			_logger.LogInformation(e,
				$"[AddItemAsync] catch-ed InvalidOperationException going to retry 2 times {fileIndexItem.FilePath}");
			return await RetryHelper.DoAsync(
				LocalDefaultQuery, TimeSpan.FromSeconds(2), 2);
		}
	}

	/// <summary>
	///     Add Sub Path Folder - Parent Folders
	///     root(/)
	///     /2017  *= index only this folder
	///     /2018
	///     If you use the cmd: $ starskycli -s "/2017"
	///     the folder '2017' it self is not added
	///     and all parent paths are not included
	///     this class does add those parent folders
	/// </summary>
	/// <param name="subPath">subPath as input</param>
	/// <returns>List</returns>
	public async Task<List<FileIndexItem>> AddParentItemsAsync(string subPath)
	{
		var path = subPath == "/" || string.IsNullOrEmpty(subPath)
			? "/"
			: PathHelper.RemoveLatestSlash(subPath);
		var pathListShouldExist = Breadcrumbs.BreadcrumbHelper(path).ToList();

		var indexItems = await GetParentItems(pathListShouldExist);

		var toAddList = new List<FileIndexItem>();
		// ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
		foreach ( var pathShouldExist in pathListShouldExist )
			if ( !indexItems.Select(p => p.FilePath).Contains(pathShouldExist) )
				toAddList.Add(new FileIndexItem(pathShouldExist)
				{
					IsDirectory = true,
					AddToDatabase = DateTime.UtcNow,
					ColorClass = ColorClassParser.Color.None,
					Software = pathShouldExist == "/" ? "Root object" : string.Empty,
					Status = FileIndexItem.ExifStatus.Ok
				});

		await AddRangeAsync(toAddList);
		return toAddList;
	}

	/// <summary>
	///     Use only when new Context item is created manually, otherwise there is only 1 context
	/// </summary>
	public async Task DisposeAsync()
	{
		await _context.DisposeAsync();
	}

	internal static string GetObjectByFilePathAsyncCacheName(string subPath)
	{
		return $"_{nameof(GetObjectByFilePathAsyncCacheName)}~{subPath}";
	}

	private async Task<FileIndexItem?> GetObjectByFilePathQueryAsync(
		string filePath)
	{
		if ( filePath != "/" ) filePath = PathHelper.RemoveLatestSlash(filePath);
		var paths = new List<string> { filePath };
		return ( await GetObjectsByFilePathQueryAsync(paths) )
			.FirstOrDefault();
	}

	private async Task<string?> QueryGetItemByHashAsync(string fileHash)
	{
		async Task<string?> LocalQueryGetItemByHashAsync(ApplicationDbContext context)
		{
			return ( await context.FileIndex.TagWith("QueryGetItemByHashAsync").FirstOrDefaultAsync(
				p => p.FileHash == fileHash
				     && p.IsDirectory != true
			) )?.FilePath;
		}

		try
		{
			return await LocalQueryGetItemByHashAsync(_context);
		}
		catch ( ObjectDisposedException )
		{
			var context = new InjectServiceScope(_scopeFactory).Context();
			return await LocalQueryGetItemByHashAsync(context);
		}
	}

	/// <summary>
	///     Get the name of Key in the cache db
	/// </summary>
	/// <param name="functionName">how is the function called</param>
	/// <param name="singleItemDbPath">the path</param>
	/// <returns>a unique key</returns>
	internal static string CachingDbName(string functionName, string? singleItemDbPath)
	{
		// when is nothing assume its the home item
		if ( string.IsNullOrWhiteSpace(singleItemDbPath) ) singleItemDbPath = "/";
		// For creating an unique name: DetailView_/2018/01/1.jpg
		var uniqueSingleDbCacheNameBuilder = new StringBuilder();
		uniqueSingleDbCacheNameBuilder.Append(functionName + "_" + singleItemDbPath);
		return uniqueSingleDbCacheNameBuilder.ToString();
	}

	/// <summary>
	///     Retry when an Exception has occured
	/// </summary>
	/// <param name="updateStatusContent"></param>
	/// <param name="e">Exception</param>
	/// <param name="source">Where from is this called, this helps to debug the code better</param>
	/// <param name="delay">retry delay in milliseconds, 1000 = 1 second</param>
	internal async Task<bool?> RetryQueryUpdateSaveChangesAsync(FileIndexItem updateStatusContent,
		Exception e, string source, int delay = 50)
	{
		if ( updateStatusContent.Id == 0 )
		{
			_logger.LogError(e, $"[RetrySaveChangesAsync] skipped due 0 id: {source}");
			return null;
		}

		_logger.LogInformation(e,
			$"[RetrySaveChangesAsync] retry catch-ed exception from {source} {updateStatusContent.FileName}");

		async Task LocalRetrySaveChangesAsyncQuery()
		{
			// InvalidOperationException: A second operation started on this context before a previous operation completed.
			// https://go.microsoft.com/fwlink/?linkid=2097913
			await Task.Delay(delay);
			var context = new InjectServiceScope(_scopeFactory).Context();
			if ( context == null! ) throw new AggregateException("Query Context is null");
			context.Attach(updateStatusContent).State = EntityState.Modified;
			await context.SaveChangesAsync();
			context.Attach(updateStatusContent).State = EntityState.Unchanged;
			await context.DisposeAsync();
		}

		try
		{
			await LocalRetrySaveChangesAsyncQuery();
		}
		catch ( MySqlException mySqlException )
		{
			_logger.LogInformation(mySqlException,
				$"[RetrySaveChangesAsync] MySqlException catch-ed and retry again, from {source}");
			await LocalRetrySaveChangesAsyncQuery();
		}
		catch ( DbUpdateConcurrencyException concurrencyException )
		{
			SolveConcurrency.SolveConcurrencyExceptionLoop(concurrencyException.Entries);
			try
			{
				_logger.LogInformation(
					"[RetrySaveChangesAsync] SolveConcurrencyExceptionLoop disposed item");
				var context = new InjectServiceScope(_scopeFactory).Context();
				await context.SaveChangesAsync();
			}
			catch ( DbUpdateConcurrencyException retry2Exception )
			{
				_logger.LogError(retry2Exception,
					"[RetrySaveChangesAsync] save failed after DbUpdateConcurrencyException");
			}
		}

		_logger.LogInformation(
			$"[RetrySaveChangesAsync] done saved from {source} {updateStatusContent.FileName}");
		return true;
	}

	/// <summary>
	///     Is Cache enabled, null object or feature toggle disabled
	/// </summary>
	/// <returns>true when enabled</returns>
	internal bool IsCacheEnabled()
	{
		if ( _cache == null || _appSettings.AddMemoryCache == false ) return false;
		return true;
	}

	/// <summary>
	///     Add child item to parent cache
	///     Private api within Query to add cached items
	///     Assumes that the parent directory already exist in the cache
	///     @see: AddCacheParentItem to add parent item
	/// </summary>
	/// <param name="updateStatusContent">the content to add</param>
	internal void AddCacheItem(FileIndexItem updateStatusContent)
	{
		// If cache is turned of
		if ( _cache == null || _appSettings.AddMemoryCache == false ) return;

		var queryCacheName = CachingDbName(nameof(FileIndexItem),
			updateStatusContent.ParentDirectory!);

		if ( !_cache.TryGetValue(queryCacheName, out var objectFileFolders) ) return;

		objectFileFolders ??= new List<FileIndexItem>();
		var displayFileFolders = ( List<FileIndexItem> )objectFileFolders;

		if ( updateStatusContent.FilePath == "/" ) return;

		displayFileFolders.Add(updateStatusContent);
		// Order by filename
		displayFileFolders = displayFileFolders.OrderBy(p => p.FileName).ToList();

		_cache.Remove(queryCacheName);
		_cache.Set(queryCacheName, displayFileFolders, new TimeSpan(1, 0, 0));
	}

	/// <summary>
	///     Cache Only! Private api within Query to remove cached items
	///     This Does remove a SINGLE item from the cache NOT from the database
	/// </summary>
	/// <param name="updateStatusContent"></param>
	public void RemoveCacheItem(FileIndexItem updateStatusContent)
	{
		// Add protection for disabled caching
		if ( _cache == null || _appSettings.AddMemoryCache == false ) return;

		var queryCacheName = CachingDbName(nameof(FileIndexItem),
			updateStatusContent.ParentDirectory!);

		if ( !_cache.TryGetValue(queryCacheName, out var objectFileFolders) ) return;

		objectFileFolders ??= new List<FileIndexItem>();
		var displayFileFolders = ( List<FileIndexItem> )objectFileFolders;

		// Order by filename
		displayFileFolders = displayFileFolders
			.Where(p => p.FilePath != updateStatusContent.FilePath)
			.OrderBy(p => p.FileName).ToList();

		_cache.Remove(queryCacheName);
		// generate list again
		_cache.Set(queryCacheName, displayFileFolders, new TimeSpan(1, 0, 0));
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
		catch ( ObjectDisposedException )
		{
			return await LocalQuery(new InjectServiceScope(_scopeFactory).Context());
		}
	}
}
