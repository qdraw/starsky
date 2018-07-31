using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using starsky.Models;
using starsky.ViewModels;

namespace starsky.Services
{
    public partial class Query // For folder displays only
    {
        // Class for displaying folder content
        
        // Display File folder displays content of the folder
        public IEnumerable<FileIndexItem> DisplayFileFolders(string subPath = "/", 
            List<FileIndexItem.Color> colorClassFilterList = null)
        {
            if (colorClassFilterList == null) colorClassFilterList = new List<FileIndexItem.Color>();
            
            subPath = SubPathSlashRemove(subPath);

            var queryItems = CacheQueryDisplayFileFolders(subPath);

            if (colorClassFilterList.Any())
            {
                queryItems = queryItems.Where(p => colorClassFilterList.Contains(p.ColorClass)).ToList();
            }

            if (!queryItems.Any())
            {
                return new List<FileIndexItem>();
            }
            return HideDeletedFileFolderList(queryItems);
        }

        private List<FileIndexItem> QueryDisplayFileFolders(string subPath = "/")
        {
            var queryItems = _context.FileIndex
                .Where(p => p.ParentDirectory == subPath)
                .OrderBy(p => p.FileName).ToList();     
            return queryItems;
        }
        
        private List<FileIndexItem> CacheQueryDisplayFileFolders(string subPath)
        {
            // The CLI programs uses no cache
            if( _cache == null || _appSettings?.AddMemoryCache == false) return QueryDisplayFileFolders(subPath);
            
            // Return values from IMemoryCache
            var queryCacheName = CachingDbName(typeof(List<FileIndexItem>).Name, 
                subPath);

            if (_cache.TryGetValue(queryCacheName, out var objectFileFolders))
                return objectFileFolders as List<FileIndexItem>;
            
            objectFileFolders = QueryDisplayFileFolders(subPath);
            _cache.Set(queryCacheName, objectFileFolders, new TimeSpan(1,0,0));
            return (List<FileIndexItem>) objectFileFolders;
        }

        // Hide Deleted items in folder
        private IEnumerable<FileIndexItem> HideDeletedFileFolderList(List<FileIndexItem> queryItems){
            // temp feature to hide deleted items
            var displayItems = new List<FileIndexItem>();
                foreach (var item in queryItems)
            {
                if (!item.Tags.Contains("!delete!"))
                {
                    displayItems.Add(item);
                }
            }
            return displayItems;
            // temp feature to hide deleted items
        }
        
        // Show previous en next items in the folder view.
        // There is equivalent class for prev next in the display view
        public RelativeObjects GetNextPrevInFolder(string currentFolder)
        {
            currentFolder = SubPathSlashRemove(currentFolder);

            // We use breadcrums to get the parent folder
            var parrentFolderPath = Breadcrumbs.BreadcrumbHelper(currentFolder).LastOrDefault();
            
            var itemsInSubFolder = _context.FileIndex
                .Where(p => p.ParentDirectory == parrentFolderPath)
                .OrderBy(p => p.FileName).ToList();
            
            var photoIndexOfSubFolder = itemsInSubFolder.FindIndex(p => p.FilePath == currentFolder);

            var relativeObject = new RelativeObjects();
            if (photoIndexOfSubFolder != itemsInSubFolder.Count - 1 && currentFolder != "/")
            {
                // currentFolder != "/" >= on the home folder you will automaticly go to a subfolder
                relativeObject.NextFilePath = itemsInSubFolder[photoIndexOfSubFolder + 1]?.FilePath;
            }

            if (photoIndexOfSubFolder >= 1)
            {
                relativeObject.PrevFilePath = itemsInSubFolder[photoIndexOfSubFolder - 1]?.FilePath;
            }

            return relativeObject;
        }
    }
}