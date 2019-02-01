using System.Collections.Generic;
using starsky.ViewModels;
using starskycore.Models;
using starskycore.ViewModels;

namespace starskycore.Interfaces
{
    public interface IQuery
    {

        List<FileIndexItem> GetAllFiles(string subPath);
        
        List<FileIndexItem> GetAllRecursive(string subPath = "");

        // to do the query and return object
        IEnumerable<FileIndexItem> DisplayFileFolders(
            string subPath = "/", 
            List<FileIndexItem.Color> colorClassFilterList = null,
            bool enableCollections = true,
            bool hideDeleted = true);

        // To make an object without any query
        IEnumerable<FileIndexItem> DisplayFileFolders(
            List<FileIndexItem> fileIndexItems,
            List<FileIndexItem.Color> colorClassFilterList = null,
            bool enableCollections = true,
            bool hideDeleted = true);

        // to do the query and return object
        DetailView SingleItem(
            string singleItemDbPath, 
            List<FileIndexItem.Color> colorClassFilterList = null,
            bool enableCollections = true,
            bool hideDeleted = true);

        // To make an object without any query
        DetailView SingleItem(
            List<FileIndexItem> fileIndexItemsList, 
            string singleItemDbPath, 
            List<FileIndexItem.Color> colorClassFilterList = null,
            bool enableCollections = true,
            bool hideDeleted = true);
        
        FileIndexItem GetObjectByFilePath(string filePath);

        FileIndexItem RemoveItem(FileIndexItem updateStatusContent);
        void RemoveCacheParentItem(string directoryName);

        string GetSubPathByHash(string fileHash);
	    void ResetItemByHash(string fileHash);

        List<FileIndexItem> GetAllFolders();

        FileIndexItem AddItem(FileIndexItem updateStatusContent);

        FileIndexItem UpdateItem(FileIndexItem updateStatusContent);
        List<FileIndexItem> UpdateItem(List<FileIndexItem> updateStatusContentList);

        string SubPathSlashRemove(string subPath = "/");

        RelativeObjects GetNextPrevInFolder(string currentFolder);

        List<FileIndexItem> StackCollections(List<FileIndexItem> databaseSubFolderList);
        void CacheUpdateItem(List<FileIndexItem> updateStatusContent);

	    bool IsCacheEnabled();
    }
}
