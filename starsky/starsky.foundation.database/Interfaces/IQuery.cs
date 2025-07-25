using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;

namespace starsky.foundation.database.Interfaces;

public interface IQuery
{
	/// <summary>
	///     Get a list of all files inside a folder (NOT recursive)
	///     But this uses a database as source
	/// </summary>
	/// <param name="filePaths">relative database path</param>
	/// <param name="timeout"></param>
	/// <returns>list of FileIndex-objects</returns>
	Task<List<FileIndexItem>> GetAllFilesAsync(List<string> filePaths, int timeout = 1000);

	/// <summary>
	///     Get a list of all files inside a folder (NOT recursive)
	///     But this uses a database as source
	/// </summary>
	/// <param name="subPath">relative database path</param>
	/// <returns>list of FileIndex-objects</returns>
	Task<List<FileIndexItem>> GetAllFilesAsync(string subPath);

	Task<List<FileIndexItem>> GetAllRecursiveAsync(string subPath = "/");

	Task<List<FileIndexItem>> GetAllRecursiveAsync(List<string> filePathList);

	/// <summary>
	///     to do the query and return object
	/// </summary>
	/// <param name="subPath">subPath style</param>
	/// <param name="colorClassActiveList">filter the colorClass</param>
	/// <param name="enableCollections">enable to show only one file with a base name</param>
	/// <param name="hideDeleted">files that are marked as trash</param>
	/// <returns></returns>
	IEnumerable<FileIndexItem> DisplayFileFolders(
		string subPath = "/",
		List<ColorClassParser.Color>? colorClassActiveList = null,
		bool enableCollections = true,
		bool hideDeleted = true);

	// To make an object without any query
	IEnumerable<FileIndexItem> DisplayFileFolders(
		List<FileIndexItem> fileIndexItems,
		List<ColorClassParser.Color>? colorClassActiveList = null,
		bool enableCollections = true,
		bool hideDeleted = true);

	/// <summary>
	///     to do the query and return object
	/// </summary>
	/// <param name="singleItemDbPath"></param>
	/// <param name="colorClassActiveList"></param>
	/// <param name="enableCollections"></param>
	/// <param name="hideDeleted"></param>
	/// <param name="sort"></param>
	/// <returns></returns>
	DetailView? SingleItem(
		string singleItemDbPath,
		List<ColorClassParser.Color>? colorClassActiveList = null,
		bool enableCollections = true,
		bool hideDeleted = true,
		SortType? sort = SortType.FileName);

	/// <summary>
	///     To make an object without any query
	/// </summary>
	/// <param name="fileIndexItemsList"></param>
	/// <param name="singleItemDbPath"></param>
	/// <param name="colorClassActiveList"></param>
	/// <param name="enableCollections"></param>
	/// <param name="hideDeleted"></param>
	/// <param name="sort"></param>
	/// <returns></returns>
	DetailView? SingleItem(
		List<FileIndexItem> fileIndexItemsList,
		string singleItemDbPath,
		List<ColorClassParser.Color>? colorClassActiveList = null,
		bool enableCollections = true,
		bool hideDeleted = true,
		SortType? sort = SortType.FileName);

	/// <summary>
	///     Get FirstOrDefault for that filePath
	/// </summary>
	/// <param name="filePath">subPath style</param>
	/// <returns>item</returns>
	FileIndexItem? GetObjectByFilePath(string filePath);

	/// <summary>
	///     Get FirstOrDefault for that filePath
	/// </summary>
	/// <param name="filePath">subPath style</param>
	/// <param name="cacheTime">time of cache </param>
	/// <returns>item</returns>
	Task<FileIndexItem?> GetObjectByFilePathAsync(string filePath, TimeSpan? cacheTime = null);

	/// <summary>
	/// </summary>
	/// <param name="inputFilePath"></param>
	/// <param name="collections"></param>
	/// <returns></returns>
	Task<List<FileIndexItem>> GetObjectsByFilePathAsync(string inputFilePath,
		bool collections);

	/// <summary>
	///     Cached result that contain values
	/// </summary>
	/// <param name="inputFilePaths">List of filePaths</param>
	/// <param name="collections">enable implicit raw files with the same base name</param>
	/// <returns></returns>
	Task<List<FileIndexItem>> GetObjectsByFilePathAsync(List<string> inputFilePaths,
		bool collections);

	/// <summary>
	///     Query direct by filePaths (without cache)
	/// </summary>
	/// <param name="filePathList">List of filePaths</param>
	/// <returns></returns>
	Task<List<FileIndexItem>> GetObjectsByFilePathQueryAsync(
		List<string> filePathList);

	Task<FileIndexItem> RemoveItemAsync(FileIndexItem updateStatusContent);

	Task<List<FileIndexItem>> RemoveItemAsync(
		List<FileIndexItem> updateStatusContentList);

	/// <summary>
	///     Clear the directory name from the cache
	/// </summary>
	/// <param name="directoryName">the path of the directory (there is no parent generation)</param>
	bool RemoveCacheParentItem(string directoryName);

	Task<string?> GetSubPathByHashAsync(string fileHash);

	Task<List<FileIndexItem>> GetObjectsByFileHashAsync(
		List<string> fileHashesList, int retryCount = 2);

	/// <summary>
	///     Reset Cache for the item that is renamed
	/// </summary>
	/// <param name="fileHash">where to look for</param>
	void ResetItemByHash(string fileHash);

	Task<List<FileIndexItem>> GetFoldersAsync(string subPath);

	Task<List<FileIndexItem>> GetAllObjectsAsync(string subPath);

	Task<List<FileIndexItem>> GetAllObjectsAsync(List<string> filePaths,
		int fallbackDelay = 5000);

	Task<FileIndexItem> AddItemAsync(FileIndexItem fileIndexItem);

	Task<bool> ExistsAsync(string filePath);

	Task<List<FileIndexItem>> AddRangeAsync(List<FileIndexItem> fileIndexItemList);

	Task<FileIndexItem> UpdateItemAsync(FileIndexItem updateStatusContent);
	Task<List<FileIndexItem>> UpdateItemAsync(List<FileIndexItem> updateStatusContentList);

	RelativeObjects GetNextPrevInFolder(string currentFolder);


	/// <summary>
	///     Update parent item with all data from child items
	/// </summary>
	/// <param name="directoryName">parent directory</param>
	/// <param name="items">all items that are in this folder</param>
	/// <returns>success or not</returns>
	bool AddCacheParentItem(string directoryName,
		List<FileIndexItem> items);

	/// <summary>
	///     Cache API within Query to update cached items
	///     For DisplayFileFolders and SingleItem
	/// </summary>
	/// <param name="updateStatusContent">items to update</param>
	void CacheUpdateItem(List<FileIndexItem> updateStatusContent);

	/// <summary>
	///     And remove content from cache
	/// </summary>
	/// <param name="updateStatusContent">list of items</param>
	void RemoveCacheItem(List<FileIndexItem> updateStatusContent);

	Tuple<bool, List<FileIndexItem>> CacheGetParentFolder(string subPath);


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
	/// <returns>void</returns>
	Task<List<FileIndexItem>> AddParentItemsAsync(string subPath);

	void Invoke(ApplicationDbContext applicationDbContext);

	void SetGetObjectByFilePathCache(string filePath,
		FileIndexItem result,
		TimeSpan? cacheTime);

	Task DisposeAsync();

	Task<int> CountAsync(Expression<Func<FileIndexItem, bool>>? expression = null);
}
