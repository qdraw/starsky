﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;

namespace starsky.foundation.database.Query
{
    public partial class Query // For folder displays only
    {

        /// <summary>
        /// Query all FileIndexItems with the type folder
        /// </summary>
        /// <returns>List of all folders in database, including content</returns>
        public List<FileIndexItem> GetAllFolders()
        {
	        try
	        {
		        return _context.FileIndex.Where(p => p.IsDirectory == true).ToList();
	        }
	        catch ( ObjectDisposedException )
	        {
		        var context = new InjectServiceScope( _scopeFactory).Context();
		        return context.FileIndex.Where(p => p.IsDirectory == true).ToList();
	        }
        }

            
        // Class for displaying folder content
        // This is the query part
        public IEnumerable<FileIndexItem> DisplayFileFolders(
            string subPath = "/", 
            List<ColorClassParser.Color> colorClassActiveList = null,
            bool enableCollections = true,
            bool hideDeleted = true)
        {
            subPath = SubPathSlashRemove(subPath);
            var fileIndexItems = CacheQueryDisplayFileFolders(subPath);
            
            return DisplayFileFolders(fileIndexItems, 
	            colorClassActiveList,
                enableCollections,
                hideDeleted);
        }

        // Display File folder displays content of the folder
        // without any query in this method
        public IEnumerable<FileIndexItem> DisplayFileFolders(
            List<FileIndexItem> fileIndexItems,
            List<ColorClassParser.Color> colorClassActiveList = null,
            bool enableCollections = true,
            bool hideDeleted = true)
        {
            if (colorClassActiveList == null) colorClassActiveList = new List<ColorClassParser.Color>();
            if (colorClassActiveList.Any())
            {
                fileIndexItems = fileIndexItems.Where(p => colorClassActiveList.Contains(p.ColorClass)).ToList();
            }

            if (!fileIndexItems.Any())
            {
                return new List<FileIndexItem>();
            }
            
            if (enableCollections)
            {
                // Query Collections
                fileIndexItems =  StackCollections(fileIndexItems);         
            }
            
            if(hideDeleted) return HideDeletedFileFolderList(fileIndexItems);
            return fileIndexItems;
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
            
            _cache.Set(queryCacheName, objectFileFolders, 
	            new TimeSpan(1,0,0));
            return (List<FileIndexItem>) objectFileFolders;
        }

        internal List<FileIndexItem> QueryDisplayFileFolders(string subPath = "/")
        {
	        List<FileIndexItem> QueryItems(ApplicationDbContext context)
	        {
		        var queryItems = context.FileIndex
			        .Where(p => p.ParentDirectory == subPath)
			        .OrderBy(p => p.FileName).AsEnumerable()	
			        // remove duplicates from list
			        .GroupBy(t => t.FileName).Select(g => g.First());
		        return queryItems.OrderBy(p => p.FileName, StringComparer.InvariantCulture).ToList();
	        }

	        try
	        {
		        return QueryItems(_context);
	        }
	        catch ( NotSupportedException )
	        {
		        // System.NotSupportedException:  The ReadAsync method cannot be called when another read operation is pending.
		        var context = new InjectServiceScope(_scopeFactory).Context();
		        return QueryItems(context);
	        }
	        catch ( InvalidOperationException ) // or ObjectDisposedException
	        {
		        var context = new InjectServiceScope(_scopeFactory).Context();
		        return QueryItems(context);
	        }
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

            // We use breadcrumbs to get the parent folder
            var parentFolderPath = FilenamesHelper.GetParentPath(currentFolder);
            
            // sort by alphabet
            var itemsInSubFolder = _context.FileIndex.Where(
		            p => p.ParentDirectory == parentFolderPath)
                .OrderBy(p => p.FileName).ToList();
            
            var photoIndexOfSubFolder = itemsInSubFolder.FindIndex(p => p.FilePath == currentFolder);

            var relativeObject = new RelativeObjects();
            if (photoIndexOfSubFolder != itemsInSubFolder.Count - 1 && currentFolder != "/")
            {
                // currentFolder != "/" >= on the home folder you will automaticly go to a subfolder
                relativeObject.NextFilePath = itemsInSubFolder[photoIndexOfSubFolder + 1]?.FilePath;
                relativeObject.NextHash = itemsInSubFolder[photoIndexOfSubFolder + 1]?.FileHash;
            }

            if (photoIndexOfSubFolder >= 1)
            {
                relativeObject.PrevFilePath = itemsInSubFolder[photoIndexOfSubFolder - 1]?.FilePath;
                relativeObject.PrevHash = itemsInSubFolder[photoIndexOfSubFolder - 1]?.FileHash;
            }

            return relativeObject;
        }
    }
}
