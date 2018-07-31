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
using starsky.Helpers;


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


        private string CachingDbName(string functionName, string singleItemDbPath,
            IReadOnlyCollection<FileIndexItem.Color> colorClassFilterList = null)
        {
            // For creating an unique name: DetailView_/2018/01/1.jpg_Superior
            
            var uniqueSingleDbCacheNameBuilder = new StringBuilder();
            uniqueSingleDbCacheNameBuilder.Append(functionName + "_" + singleItemDbPath);
            if (colorClassFilterList != null)
            {
                uniqueSingleDbCacheNameBuilder.Append("_");
                foreach (var oneColor in colorClassFilterList)
                {
                    uniqueSingleDbCacheNameBuilder.Append(oneColor);
                }
            }
            return uniqueSingleDbCacheNameBuilder.ToString();
        }

     
      

        // For the API/update endpoint
        public FileIndexItem UpdateItem(FileIndexItem updateStatusContent)
        {
            _context.Attach(updateStatusContent).State = EntityState.Modified;
            _context.SaveChanges();
            CacheUpdateItem(updateStatusContent);

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
            _cache.Set(queryCacheName, displayFileFolders);
        }

        // Private api within Query to update cached items
        public void CacheUpdateItem(FileIndexItem updateStatusContent)
        {
            if( _cache == null || _appSettings?.AddMemoryCache == false) return;
            
            var queryCacheName = CachingDbName(typeof(List<FileIndexItem>).Name, 
                updateStatusContent.ParentDirectory);

            if (!_cache.TryGetValue(queryCacheName, out var objectFileFolders)) return;
            
            var displayFileFolders = (List<FileIndexItem>) objectFileFolders;
                
            var obj = displayFileFolders.FirstOrDefault(p => p.FilePath == updateStatusContent.FilePath);
            if (obj == null) return;
            displayFileFolders.Remove(obj);
            displayFileFolders.Add(updateStatusContent);
            // Order by filename
            displayFileFolders = displayFileFolders.OrderBy(p => p.FileName).ToList();
            
            _cache.Remove(queryCacheName);
            _cache.Set(queryCacheName, displayFileFolders);

        }


        // Add a new item to the database
        public FileIndexItem AddItem(FileIndexItem updateStatusContent)
        {
//            if (!SqliteHelper.IsReady()) throw new ArgumentException("database error");
            
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
//            if (!SqliteHelper.IsReady()) throw new ArgumentException("database error");

            _context.FileIndex.Remove(updateStatusContent);
            _context.SaveChanges();
            return updateStatusContent;
        }

    }
}
