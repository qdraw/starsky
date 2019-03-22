using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using starskycore.Helpers;
using starskycore.Interfaces;
using starskycore.Models;

namespace starskycore.Services
{
    public class ReadMeta : IReadMeta
    {
        private readonly AppSettings _appSettings;
        private readonly IMemoryCache _cache;
	    private readonly IStorage _iStorage;
	    private readonly ReadMetaExif _readExif;
	    private readonly ReadMetaXmp _readXmp;
	    private readonly ReadMetaGpx _readGpx;

	    public ReadMeta(IStorage iStorage, AppSettings appSettings = null, IMemoryCache memoryCache = null)
        {
            _appSettings = appSettings;
            _cache = memoryCache;
	        _iStorage = iStorage;
	        _readExif = new ReadMetaExif(iStorage);
	        _readXmp = new ReadMetaXmp(iStorage,memoryCache);
	        _readGpx = new ReadMetaGpx();

        }

        private FileIndexItem ReadExifAndXmpFromFileDirect(FileIndexItem fileIndexItemWithPath)
        {
	        if ( _iStorage.ExistFile(fileIndexItemWithPath.FilePath) 
	             && ExtensionRolesHelper.IsExtensionForceGpx(fileIndexItemWithPath.FileName) )
	        {
				return _readGpx.ReadGpxFromFileReturnAfterFirstField(_iStorage.ReadStream(fileIndexItemWithPath.FilePath));
	        }

	        var fileIndexItem = _readXmp.XmpGetSidecarFile(fileIndexItemWithPath);

	        if ( fileIndexItem.IsoSpeed == 0 
	             || string.IsNullOrEmpty(fileIndexItem.Make) 
	             || fileIndexItem.DateTime.Year == 0)
	        {
		        var databaseItemFile = _readExif.ReadExifFromFile(fileIndexItemWithPath.FilePath);
		        FileIndexCompareHelper.Compare(fileIndexItem, databaseItemFile);
	        }
	        
            return fileIndexItem;
        }

        // used by the html generator
        public List<FileIndexItem> ReadExifAndXmpFromFileAddFilePathHash(List<string> subPathList, List<string> fileHashes = null)
        {
            var fileIndexList = new List<FileIndexItem>();

	        for ( int i = 0; i < subPathList.Count; i++ )
	        {
		        var subPath = subPathList[i];
		        
		        // todo: fix dependency on filesystem
		        var imageFormat = ExtensionRolesHelper.GetImageFormat(subPath); 
		        
		        var returnItem = new FileIndexItem(subPath);
		        
		        returnItem = ReadExifAndXmpFromFile(returnItem);

		        returnItem.ImageFormat = imageFormat;
		        returnItem.FileName = Path.GetFileName(subPath);
		        returnItem.IsDirectory = false;
		        returnItem.Status = FileIndexItem.ExifStatus.Ok;
		        returnItem.ParentDirectory = Breadcrumbs.BreadcrumbHelper(subPath).LastOrDefault();

		        if ( fileHashes == null || fileHashes.Count <= i )
		        {
			        returnItem.FileHash = new FileHash(_iStorage).GetHashCode(subPath);
		        }
		        else
		        {
			        returnItem.FileHash = fileHashes[i];
		        }

		        fileIndexList.Add(returnItem);
	        }
            return fileIndexList;
        }

        // Cached view >> IMemoryCache
        // Short living cache Max 15. minutes
        public FileIndexItem ReadExifAndXmpFromFile(FileIndexItem fileIndexItemWithLocation)
        {
            // The CLI programs uses no cache
            if( _cache == null || _appSettings?.AddMemoryCache == false) 
                return ReadExifAndXmpFromFileDirect(fileIndexItemWithLocation);
            
            // Return values from IMemoryCache
            var queryCacheName = "info_" + fileIndexItemWithLocation.FilePath;
            
            // Return Cached object if it exist
            if (_cache.TryGetValue(queryCacheName, out var objectExifToolModel))
                return objectExifToolModel as FileIndexItem;
            
            // Try to catch a new object
            objectExifToolModel = ReadExifAndXmpFromFileDirect(fileIndexItemWithLocation);
            _cache.Set(queryCacheName, objectExifToolModel, new TimeSpan(0,15,0));
            return (FileIndexItem) objectExifToolModel;
        }

        
        //     Update only for ReadMeta!
        public void UpdateReadMetaCache(string fullFilePath, FileIndexItem objectExifToolModel)
        {
            if (_cache == null || _appSettings?.AddMemoryCache == false) return;

            var toUpdateObject = objectExifToolModel.Clone();
            var queryCacheName = "info_" + fullFilePath;
            RemoveReadMetaCache(fullFilePath);
            _cache.Set(queryCacheName, toUpdateObject, new TimeSpan(0,15,0));
        }
        

        //     only for ReadMeta!
        //     Why removing, The Update command does not update the entire object.
        //     When you update tags, other tags will be null 
        public void RemoveReadMetaCache(string fullFilePath)
        {
            if (_cache == null || _appSettings?.AddMemoryCache == false) return;
            var queryCacheName = "info_" + fullFilePath;

            if (!_cache.TryGetValue(queryCacheName, out var _)) return; 
            // continue = go to the next item in the list
            _cache.Remove(queryCacheName);
        }
    }
}