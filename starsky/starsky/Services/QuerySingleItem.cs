using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using starsky.Models;
using starsky.ViewModels;

namespace starsky.Services
{
    public partial class Query
    {
        // For displaying single photo's
        // Display feature only?!
        // input: Name of item by db style path
        // With Caching feature :)
        
        public DetailView SingleItem(string singleItemDbPath,
            List<FileIndexItem.Color> colorClassFilterList = null,
            bool enableCollections = true,
            bool hideDeleted = true)
        {
            // reject emphy requests // --- || singleItemDbPath == "/"
            if (string.IsNullOrWhiteSpace(singleItemDbPath) ) return null;

            // A direct query to check if the file exist
            var singleItemDbPathFromDirectQuery = CacheSingleFileIndex(singleItemDbPath);
            if (singleItemDbPathFromDirectQuery == null) return null;

            var parentFolder = Breadcrumbs.BreadcrumbHelper(singleItemDbPath).LastOrDefault();

            var fileIndexItemsList = DisplayFileFolders(parentFolder,colorClassFilterList,enableCollections).ToList();
            var fileName = singleItemDbPath.Replace(parentFolder + "/", string.Empty);
            var currentFileIndexItem = fileIndexItemsList.FirstOrDefault(p => p.FileName == fileName);
            
            // When loading a raw file == next prev is using a jpeg file
            var directAccessAnNonExtensionThumbSupported = false;
            if (currentFileIndexItem == null)
            {
                directAccessAnNonExtensionThumbSupported = true;
                currentFileIndexItem = fileIndexItemsList.FirstOrDefault(
                    p => p.FileCollectionName == singleItemDbPathFromDirectQuery.FileCollectionName);
            }
            
            var itemResult = new DetailView
            {
                FileIndexItem = currentFileIndexItem,
                RelativeObjects = GetNextPrevInSubFolder(currentFileIndexItem,fileIndexItemsList),
                Breadcrumb = Breadcrumbs.BreadcrumbHelper(singleItemDbPath),
                GetAllColor = FileIndexItem.GetAllColorUserInterface(),
                ColorClassFilterList = colorClassFilterList
            };
            
            // Search for collectionItems need to add a duplicate of the cache
            // This one is always false => due need to check for raw files in db
            var collectionItemsDirectory = DisplayFileFolders(parentFolder,colorClassFilterList,false).ToList();

            if (directAccessAnNonExtensionThumbSupported)
            {
                itemResult.FileIndexItem = singleItemDbPathFromDirectQuery;
            }
            
            
            itemResult.FileIndexItem.CollectionPaths = new List<string>();
            itemResult.FileIndexItem.CollectionPaths.AddRange(collectionItemsDirectory
                .Where(p => p.FileCollectionName == singleItemDbPathFromDirectQuery.FileCollectionName)
                .Select(p => p.FilePath));
            
            return itemResult;
        }
        
        private RelativeObjects GetNextPrevInSubFolder(FileIndexItem currentFileIndexItem, 
            List<FileIndexItem> fileIndexItemsList)
        {
            // Check if this is item is not !deleted! yet;
            if (currentFileIndexItem == null) return new RelativeObjects();;
            
            var currentIndex = fileIndexItemsList.FindIndex(p => p.FilePath == currentFileIndexItem.FilePath);
            var relativeObject = new RelativeObjects();

            if (currentIndex != fileIndexItemsList.Count - 1)
            {
                relativeObject.NextFilePath = fileIndexItemsList[currentIndex + 1].FilePath;
            }
            if (currentIndex >= 1)
            {
                relativeObject.PrevFilePath = fileIndexItemsList[currentIndex - 1].FilePath;
            }
            
            return relativeObject;
        }
        
        
        private FileIndexItem CacheSingleFileIndex(string singleItemDbPath)
        {
            // The CLI programs uses no cache
            if (_cache == null) return _context.FileIndex.FirstOrDefault(
                p => p.FilePath == singleItemDbPath && !p.IsDirectory);

            // Return values from IMemoryCache
            var queryCacheName = CachingDbName(typeof(FileIndexItem).Name, 
                singleItemDbPath); // no need to specify colorClassFilterList

            object queryResult;
            if (!_cache.TryGetValue(queryCacheName, out queryResult))
            {
                queryResult = _context.FileIndex.FirstOrDefault(
                    p => p.FilePath == singleItemDbPath && !p.IsDirectory);
                
                _cache.Set(queryCacheName, queryResult, new TimeSpan(1,0,0));
            }

            return queryResult as FileIndexItem;
        }
        
        
        
//        public DetailView SingleItem(string singleItemDbPath,
//            List<FileIndexItem.Color> colorClassFilterList = null)
//        {
//            if (string.IsNullOrWhiteSpace(singleItemDbPath)) return null;
//
//            var query = CacheSingleFileIndex(singleItemDbPath);
//            
//            if (query == null) return null;
//
//            var relativeObject = CacheGetNextPrevInSubFolder(
//                query.ParentDirectory, singleItemDbPath, colorClassFilterList);
//                
//            var itemResult = new DetailView
//            {
//                FileIndexItem = query,
//                RelativeObjects = relativeObject,
//                Breadcrumb = Breadcrumbs.BreadcrumbHelper(singleItemDbPath),
//                GetAllColor = FileIndexItem.GetAllColorUserInterface(),
//                ColorClassFilterList = colorClassFilterList
//            };
//            
//            return itemResult;
//        }
//
//        private RelativeObjects CacheGetNextPrevInSubFolder(string parentDirectory, string singleItemDbPath,
//            List<FileIndexItem.Color> colorClassFilterList = null)
//        {
//            // The CLI programs uses no cache
//            if (_cache == null) return GetNextPrevInSubFolder(
//                parentDirectory, singleItemDbPath, colorClassFilterList);
//            
//            // Return values from IMemoryCache
//            var queryCacheName = CachingDbName(typeof(RelativeObjects).Name, 
//                singleItemDbPath, colorClassFilterList);
//
//            object objectRelativeObject;
//            if (!_cache.TryGetValue(queryCacheName, out objectRelativeObject))
//            {
//                objectRelativeObject = GetNextPrevInSubFolder(parentDirectory, singleItemDbPath, colorClassFilterList);
//                _cache.Set(queryCacheName, objectRelativeObject, new TimeSpan(1,0,0));
//            }
//
//            var relativeObject = (RelativeObjects) objectRelativeObject;
//
//            // It will be removed when you update this. !delete does not exist in .nextfilePath
//            // For the NextFilePath check if it is not deleted
//            if (relativeObject.NextFilePath != null)
//            {
//                if (CacheSingleFileIndex(relativeObject.NextFilePath).Tags.Contains("!delete"))
//                {
//                    _cache.Remove(queryCacheName);
//                    objectRelativeObject = GetNextPrevInSubFolder(parentDirectory,
//                        singleItemDbPath, colorClassFilterList);
//                    _cache.Set(queryCacheName, objectRelativeObject, new TimeSpan(1,0,0));
//                    relativeObject = (RelativeObjects) objectRelativeObject;
//                }
//            }
//
//            // Check if item is deleted in prev path before showing
//            if (relativeObject.PrevFilePath == null) return relativeObject;
//            if (!CacheSingleFileIndex(relativeObject.PrevFilePath).Tags.Contains("!delete")) 
//                return relativeObject;
//            
//            _cache.Remove(queryCacheName);
//            objectRelativeObject = GetNextPrevInSubFolder(parentDirectory,
//                singleItemDbPath, colorClassFilterList);
//            _cache.Set(queryCacheName, objectRelativeObject, new TimeSpan(1,0,0));
//            relativeObject = (RelativeObjects) objectRelativeObject;
//            
//            return relativeObject;
//        }
//        
//        private FileIndexItem CacheSingleFileIndex(string singleItemDbPath)
//        {
//            // The CLI programs uses no cache
//            if (_cache == null) return _context.FileIndex.FirstOrDefault(
//                p => p.FilePath == singleItemDbPath && !p.IsDirectory);
//
//            // Return values from IMemoryCache
//            var queryCacheName = CachingDbName(typeof(FileIndexItem).Name, 
//                singleItemDbPath); // no need to specify colorClassFilterList
//
//            object queryResult;
//            if (!_cache.TryGetValue(queryCacheName, out queryResult))
//            {
//                queryResult = _context.FileIndex.FirstOrDefault(
//                    p => p.FilePath == singleItemDbPath && !p.IsDirectory);
//                
//                _cache.Set(queryCacheName, queryResult, new TimeSpan(1,0,0));
//            }
//
//            return queryResult as FileIndexItem;
//        }
//
//        
//        // Show previous en next items in the singleitem view.
//        // There is equivalent class (GetNextPrevInFolder) for prev next in the folder view
//        private RelativeObjects GetNextPrevInSubFolder(
//            string parrentFolderPath, string fullImageFilePath,
//            List<FileIndexItem.Color> colorClassFilterList = null
//            )
//        {
//            if (colorClassFilterList == null) colorClassFilterList = new List<FileIndexItem.Color>();
//
//            List<FileIndexItem> itemsInSubFolder;
//            // Filter usage colorClassFilterList
//            if (!colorClassFilterList.Any())
//            {
//                itemsInSubFolder = CacheQueryDisplayFileFolders(parrentFolderPath,true).
//                    OrderBy(
//                        p => p.FileName
//                    ).ToList();
//            }
//            else
//            {
//                itemsInSubFolder = CacheQueryDisplayFileFolders(parrentFolderPath,true).Where(
//                        p => colorClassFilterList.Contains(p.ColorClass) 
//                ).OrderBy(
//                    p => p.FileName).ToList();
//            }
//            
//            // todo: if image it self is deleted and page is refreshed
//            // Use only if you are sure that there are no nulls
//            if (itemsInSubFolder.Count >= 1)
//            {
//                itemsInSubFolder =
//                    itemsInSubFolder
//                        .Where(p => !p.Tags.Contains("!delete!"))
//                        .ToList(); // Hide Deleted items from prev/next
//            }
//            
//            var photoIndexOfSubFolder = itemsInSubFolder.FindIndex(p => p.FilePath == fullImageFilePath);
//            var relativeObject = new RelativeObjects();
//            if (photoIndexOfSubFolder != itemsInSubFolder.Count - 1)
//            {
//                relativeObject.NextFilePath = itemsInSubFolder[photoIndexOfSubFolder + 1]?.FilePath;
//            }
//
//            if (photoIndexOfSubFolder >= 1)
//            {
//                relativeObject.PrevFilePath = itemsInSubFolder[photoIndexOfSubFolder - 1]?.FilePath;
//            }
//            return relativeObject;
//        }
        
        
    }
}
