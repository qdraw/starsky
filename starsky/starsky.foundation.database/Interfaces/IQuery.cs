using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;

namespace starsky.foundation.database.Interfaces
{
    public interface IQuery
    {
        List<FileIndexItem> GetAllFiles(string subPath);
        
        /// <summary>
        /// Get a list of all files inside an folder (NOT recursive)
        /// But this uses a database as source
        /// </summary>
        /// <param name="filePaths">relative database path</param>
        /// <returns>list of FileIndex-objects</returns>
        Task<List<FileIndexItem>> GetAllFilesAsync(List<string> filePaths);
        
        /// <summary>
        /// Get a list of all files inside an folder (NOT recursive)
        /// But this uses a database as source
        /// </summary>
        /// <param name="subPath">relative database path</param>
        /// <returns>list of FileIndex-objects</returns>
        Task<List<FileIndexItem>> GetAllFilesAsync(string subPath);
        
        List<FileIndexItem> GetAllRecursive(string subPath = "/");
        Task<List<FileIndexItem>> GetAllRecursiveAsync(string subPath = "/");

        Task<List<FileIndexItem>> GetAllRecursiveAsync(List<string> filePathList);
        
        /// <summary>
        /// to do the query and return object
        /// </summary>
        /// <param name="subPath">subPath style</param>
        /// <param name="colorClassActiveList">filter the colorClass</param>
        /// <param name="enableCollections">enable to show only one file with a base name</param>
        /// <param name="hideDeleted">files that are marked as trash</param>
        /// <returns></returns>
        IEnumerable<FileIndexItem> DisplayFileFolders(
            string subPath = "/", 
            List<ColorClassParser.Color> colorClassActiveList = null,
            bool enableCollections = true,
            bool hideDeleted = true);

        // To make an object without any query
        IEnumerable<FileIndexItem> DisplayFileFolders(
            List<FileIndexItem> fileIndexItems,
            List<ColorClassParser.Color> colorClassActiveList = null,
            bool enableCollections = true,
            bool hideDeleted = true);

        // to do the query and return object
        DetailView SingleItem(
            string singleItemDbPath, 
            List<ColorClassParser.Color> colorClassActiveList = null,
            bool enableCollections = true,
            bool hideDeleted = true, 
            SortType sort = SortType.FileName);

        // To make an object without any query
        DetailView SingleItem(
            List<FileIndexItem> fileIndexItemsList, 
            string singleItemDbPath, 
            List<ColorClassParser.Color> colorClassActiveList = null,
            bool enableCollections = true,
            bool hideDeleted = true, 
            SortType sort = SortType.FileName);
        
        /// <summary>
        /// Get FirstOrDefault for that filePath
        /// </summary>
        /// <param name="filePath">subPath style</param>
        /// <returns>item</returns>
        FileIndexItem GetObjectByFilePath(string filePath);
        
        /// <summary>
        /// Get FirstOrDefault for that filePath
        /// </summary>
        /// <param name="filePath">subPath style</param>
        /// <param name="cacheTime">time of cache </param>
        /// <returns>item</returns>
        Task<FileIndexItem> GetObjectByFilePathAsync(string filePath, TimeSpan? cacheTime = null);

        /// <summary>
        /// Cached result that contain values
        /// </summary>
        /// <param name="inputFilePaths">List of filePaths</param>
        /// <param name="collections">enable implicit raw files with the same base name</param>
        /// <returns></returns>
        Task<List<FileIndexItem>> GetObjectsByFilePathAsync(List<string> inputFilePaths,
	        bool collections);

        /// <summary>
        /// Query direct by filePaths (without cache)
        /// </summary>
        /// <param name="filePathList">List of filePaths</param>
        /// <returns></returns>
        Task<List<FileIndexItem>> GetObjectsByFilePathQueryAsync(
	        List<string> filePathList);
        
        FileIndexItem RemoveItem(FileIndexItem updateStatusContent);
        Task<FileIndexItem> RemoveItemAsync(FileIndexItem updateStatusContent);

        /// <summary>
        /// Clear the directory name from the cache
        /// </summary>
        /// <param name="directoryName">the path of the directory (there is no parent generation)</param>
        bool RemoveCacheParentItem(string directoryName);

        string GetSubPathByHash(string fileHash);
        Task<string> GetSubPathByHashAsync(string fileHash);

        Task<List<FileIndexItem>> GetObjectsByFileHashAsync(
	        List<string> fileHashesList);

	    void ResetItemByHash(string fileHash);

	    /// <summary>
	    /// Only global search for all folder
	    /// </summary>
	    /// <returns></returns>
        List<FileIndexItem> GetAllFolders();

	    Task<List<FileIndexItem>> GetFoldersAsync(string subPath);

	    Task<List<FileIndexItem>> GetAllObjectsAsync(string subPath);
	    Task<List<FileIndexItem>> GetAllObjectsAsync(List<string> filePaths,
		    int fallbackDelay = 5000);
	    
        FileIndexItem AddItem(FileIndexItem updateStatusContent);
        Task<FileIndexItem> AddItemAsync(FileIndexItem fileIndexItem);

        Task<List<FileIndexItem>> AddRangeAsync(List<FileIndexItem> fileIndexItemList);
        
        FileIndexItem UpdateItem(FileIndexItem updateStatusContent);
        List<FileIndexItem> UpdateItem(List<FileIndexItem> updateStatusContentList);

        Task<FileIndexItem> UpdateItemAsync(FileIndexItem updateStatusContent);
        Task<List<FileIndexItem>> UpdateItemAsync(List<FileIndexItem> updateStatusContentList);

        RelativeObjects GetNextPrevInFolder(string currentFolder);


        /// <summary>
        /// Update parent item with all data from child items
        /// </summary>
        /// <param name="directoryName">parent directory</param>
        /// <param name="items">all items that are in this folder</param>
        /// <returns>success or not</returns>
        bool AddCacheParentItem(string directoryName,
	        List<FileIndexItem> items);
        
        /// <summary>
        /// Cache API within Query to update cached items
        /// For DisplayFileFolders and SingleItem
        /// </summary>
        /// <param name="updateStatusContent">items to update</param>
        void CacheUpdateItem(List<FileIndexItem> updateStatusContent);
        
        /// <summary>
        /// And remove content from cache
        /// </summary>
        /// <param name="updateStatusContent">list of items</param>
        void RemoveCacheItem(List<FileIndexItem> updateStatusContent);

        /// <summary>
        /// Single remove content item from cache
        /// </summary>
        /// <param name="updateStatusContent">item</param>
        void RemoveCacheItem(FileIndexItem updateStatusContent);

        Tuple<bool, List<FileIndexItem>> CacheGetParentFolder(string subPath);
        
        
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
        Task AddParentItemsAsync(string subPath);
        IQuery Clone( ApplicationDbContext applicationDbContext);
        void Invoke(ApplicationDbContext applicationDbContext);

        void SetGetObjectByFilePathCache(string filePath, 
	        FileIndexItem result,
	        TimeSpan? cacheTime);

        Task DisposeAsync();
        Task<int> CountAsync(Expression<Func<FileIndexItem, bool>> expression = null);
    }
}

