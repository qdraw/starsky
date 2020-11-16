using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;

namespace starsky.foundation.database.Interfaces
{
    public interface IQuery
    {
        List<FileIndexItem> GetAllFiles(string subPath);
        
        List<FileIndexItem> GetAllRecursive(string subPath = "");

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
            bool hideDeleted = true);

        // To make an object without any query
        DetailView SingleItem(
            List<FileIndexItem> fileIndexItemsList, 
            string singleItemDbPath, 
            List<ColorClassParser.Color> colorClassActiveList = null,
            bool enableCollections = true,
            bool hideDeleted = true);
        
        FileIndexItem GetObjectByFilePath(string filePath);
        Task<FileIndexItem> GetObjectByFilePathAsync(string filePath);
        
        FileIndexItem RemoveItem(FileIndexItem updateStatusContent);
        Task<FileIndexItem> RemoveItemAsync(FileIndexItem updateStatusContent);

        /// <summary>
        /// Clear the directory name from the cache
        /// </summary>
        /// <param name="directoryName">the path of the directory (there is no parent generation)</param>
        bool RemoveCacheParentItem(string directoryName);

        string GetSubPathByHash(string fileHash);
	    void ResetItemByHash(string fileHash);

        List<FileIndexItem> GetAllFolders();

        FileIndexItem AddItem(FileIndexItem updateStatusContent);
        Task<FileIndexItem> AddItemAsync(FileIndexItem fileIndexItem);

        Task<List<FileIndexItem>> AddRangeAsync(List<FileIndexItem> fileIndexItemList);
        
        FileIndexItem UpdateItem(FileIndexItem updateStatusContent);
        List<FileIndexItem> UpdateItem(List<FileIndexItem> updateStatusContentList);

        [Obsolete("use PathHelper.RemoveLatestSlash()")]
        string SubPathSlashRemove(string subPath = "/");

        RelativeObjects GetNextPrevInFolder(string currentFolder);

        List<FileIndexItem> StackCollections(List<FileIndexItem> databaseSubFolderList);
        void CacheUpdateItem(List<FileIndexItem> updateStatusContent);

        Task AddParentItemsAsync(string subPath);
        Task<FileIndexItem> UpdateItemAsync(FileIndexItem updateStatusContent);
    }
}
