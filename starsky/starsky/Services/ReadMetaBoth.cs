using System;
using Microsoft.Extensions.Caching.Memory;
using starsky.Interfaces;
using starsky.Models;

namespace starsky.Services
{
    public partial class ReadMeta : IReadMeta
    {
        private readonly AppSettings _appSettings;
        private readonly IMemoryCache _cache;

        public ReadMeta(AppSettings appSettings = null, IMemoryCache memoryCache = null)
        {
            _appSettings = appSettings;
            _cache = memoryCache;
        }

        private FileIndexItem ReadExifAndXmpFromFileDirect(string singleFilePath)
        {
            var databaseItem = ReadExifFromFile(singleFilePath);
            databaseItem = XmpGetSidecarFile(databaseItem, singleFilePath);
            return databaseItem;
        }
        
        // Cached view >> IMemoryCache
        // Short living cache Max 10. minutes
        public FileIndexItem ReadExifAndXmpFromFile(string fullFilePath)
        {
            // The CLI programs uses no cache
            if( _cache == null || _appSettings?.AddMemoryCache == false) 
                return ReadExifAndXmpFromFileDirect(fullFilePath);
            
            // Return values from IMemoryCache
            var queryCacheName = "info_" + fullFilePath;
            
            // Return Cached object if it exist
            if (_cache.TryGetValue(queryCacheName, out var objectExifToolModel))
                return objectExifToolModel as FileIndexItem;
            
            // Try to catch a new object
            objectExifToolModel = ReadExifAndXmpFromFileDirect(fullFilePath);
            _cache.Set(queryCacheName, objectExifToolModel, new TimeSpan(0,10,0));
            return (FileIndexItem) objectExifToolModel;
        }

        //     only for ReadMeta!
        //     Why removing, The Update command does not update the entire object.
        //     When you update tags, other tags will be null 
        public void RemoveReadMetaCache(string fullFilePath)
        {
            if (_cache == null || _appSettings?.AddMemoryCache == false) return;
            var queryCacheName = "info_" + fullFilePath;

            if (!_cache.TryGetValue(queryCacheName, out var _)) return;
            _cache.Remove(queryCacheName);
        }
    }
}