using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using MySql.Data.MySqlClient;
using starsky.Interfaces;
using starsky.Models;
using starsky.Data;

namespace starsky.Services
{
    public partial class Query : IQuery
    {
        private readonly ApplicationDbContext _context;

        private readonly IMemoryCache _cache;
        private readonly AppSettings _appSettings;

        public Query(ApplicationDbContext context, IMemoryCache memoryCache = null, AppSettings appSettings = null )
        {
            _context = context;
            _cache = memoryCache;
            _appSettings = appSettings;
        }

        // Get a list of all files inside an folder
        // But this uses a database as source
        public List<FileIndexItem> GetAllFiles(string subPath = "/")
        {
            subPath = SubPathSlashRemove(subPath);

            return _context.FileIndex.Where
                    (p => !p.IsDirectory && p.ParentDirectory == subPath)
                .OrderBy(r => r.FileName).ToList();
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

        // Return database object file or folder
        public FileIndexItem GetObjectByFilePath(string filePath)
        {
            filePath = SubPathSlashRemove(filePath);
            var query = _context.FileIndex.FirstOrDefault(p => p.FilePath == filePath);
            return query;
        }

        // Return a File Item By it Hash value
        // New added, directory hash now also hashes
        public string GetItemByHash(string fileHash)
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


        // For the API/update endpoint
        public List<FileIndexItem> UpdateItem(List<FileIndexItem> updateStatusContentList)
        {
            foreach (var item in updateStatusContentList)
            {
                _context.Attach(item).State = EntityState.Modified;
            }
            _context.SaveChanges();
            CacheUpdateItem(updateStatusContentList);
            return updateStatusContentList;
        }

        // For the API/update endpoint
        public FileIndexItem UpdateItem(FileIndexItem updateStatusContent)
        {
            _context.Attach(updateStatusContent).State = EntityState.Modified;
            _context.SaveChanges();
            
            CacheUpdateItem(new List<FileIndexItem>{updateStatusContent});

            return updateStatusContent;
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
        public void CacheUpdateItem(IEnumerable<FileIndexItem> updateStatusContent)
        {
            if( _cache == null || _appSettings?.AddMemoryCache == false) return;
            foreach (var item in updateStatusContent)
            {
                var queryCacheName = CachingDbName(typeof(List<FileIndexItem>).Name, 
                    item.ParentDirectory);

                if (!_cache.TryGetValue(queryCacheName, out var objectFileFolders)) return;
            
                var displayFileFolders = (List<FileIndexItem>) objectFileFolders;
                
                var obj = displayFileFolders.FirstOrDefault(p => p.FilePath == item.FilePath);
                if (obj == null) return;
                displayFileFolders.Remove(obj);
                // Add here item to cached index
                displayFileFolders.Add(item);
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

        public void RemoveCacheParentItem(IEnumerable<FileIndexItem> fileIndexItemList, string directoryName)
        {
            // Add protection for disabeling caching
            if( _cache == null || _appSettings?.AddMemoryCache == false) return;
            
            var queryCacheName = CachingDbName(typeof(List<FileIndexItem>).Name, 
                directoryName);
            if (!_cache.TryGetValue(queryCacheName, out var objectFileFolders)) return;
            
            _cache.Remove(queryCacheName);
        }

        // Add a new item to the database
        public FileIndexItem AddItem(FileIndexItem updateStatusContent)
        {            
            try
            {
                _context.FileIndex.Add(updateStatusContent);
                _context.SaveChanges();
            }
            catch (MySqlException e)
            {
                Console.WriteLine(updateStatusContent.FilePath);
                Console.WriteLine(e);
                throw;
            }
            
            AddCacheItem(updateStatusContent);

            return updateStatusContent;
        }
        
        // Remove a new item from the database
        public FileIndexItem RemoveItem(FileIndexItem updateStatusContent)
        {
            _context.FileIndex.Remove(updateStatusContent);
            _context.SaveChanges();

            RemoveCacheItem(updateStatusContent);
            return updateStatusContent;
        }

    }
}
