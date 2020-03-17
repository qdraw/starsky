using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using starsky.foundation.database.Helpers;
using starsky.foundation.database.Models;
using starsky.foundation.injection;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Services;
using starskycore.Helpers;
using starskycore.Interfaces;
using starskycore.Models;

namespace starskycore.Services
{
	[Service(typeof(IReadMeta), InjectionLifetime = InjectionLifetime.Scoped)]
    public class ReadMeta : IReadMeta
    {
        private readonly AppSettings _appSettings;
        private readonly IMemoryCache _cache;
	    private readonly IStorage _iStorage;
	    private readonly ReadMetaExif _readExif;
	    private readonly ReadMetaXmp _readXmp;
	    private readonly ReadMetaGpx _readGpx;

	    /// <summary>
	    /// Used to get from all locations
	    /// </summary>
	    /// <param name="iStorage"></param>
	    /// <param name="appSettings"></param>
	    /// <param name="memoryCache"></param>
	    public ReadMeta(IStorage iStorage, AppSettings appSettings = null, IMemoryCache memoryCache = null)
        {
            _appSettings = appSettings;
            _cache = memoryCache;
            _iStorage = iStorage;
	        _readExif = new ReadMetaExif(_iStorage, appSettings);
	        _readXmp = new ReadMetaXmp(_iStorage, memoryCache);
	        _readGpx = new ReadMetaGpx();
        }

        private FileIndexItem ReadExifAndXmpFromFileDirect(string subPath)
        {
	        if ( _iStorage.ExistFile(subPath) 
	             && ExtensionRolesHelper.IsExtensionForceGpx(subPath) )
	        {
				return _readGpx.ReadGpxFromFileReturnAfterFirstField(_iStorage.ReadStream(subPath));
	        }
	        
			var fileIndexItemWithPath = new FileIndexItem(subPath);

	        // Read first the sidecar file
	        var xmpFileIndexItem = _readXmp.XmpGetSidecarFile(fileIndexItemWithPath.Clone());

	        if ( xmpFileIndexItem.IsoSpeed == 0 
	             || string.IsNullOrEmpty(xmpFileIndexItem.Make) 
	             || xmpFileIndexItem.DateTime.Year == 0)
	        {
		        // so the sidecar file is not used
		        var fileExifItemFile = _readExif.ReadExifFromFile(subPath,fileIndexItemWithPath);
		        
		        // overwrite content with incomplete sidecar file (this file can contain tags)
		        FileIndexCompareHelper.Compare(fileExifItemFile, xmpFileIndexItem);
		        return fileExifItemFile;
	        }
	        
            return xmpFileIndexItem;
        }

        // used by the html generator
        public List<FileIndexItem> ReadExifAndXmpFromFileAddFilePathHash(List<string> subPathList, List<string> fileHashes = null)
        {
            var fileIndexList = new List<FileIndexItem>();

	        for ( int i = 0; i < subPathList.Count; i++ )
	        {
		        var subPath = subPathList[i];
		        
		        var returnItem = ReadExifAndXmpFromFile(subPath);
		        var imageFormat = ExtensionRolesHelper.GetImageFormat(_iStorage.ReadStream(subPath, 512)); 

		        returnItem.ImageFormat = imageFormat;
		        returnItem.FileName = Path.GetFileName(subPath);
		        returnItem.IsDirectory = false;
		        returnItem.Status = FileIndexItem.ExifStatus.Ok;
		        returnItem.ParentDirectory = FilenamesHelper.GetParentPath(subPath);

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
        public FileIndexItem ReadExifAndXmpFromFile(string subPath)
        {
            // The CLI programs uses no cache
            if( _cache == null || _appSettings?.AddMemoryCache == false) 
                return ReadExifAndXmpFromFileDirect(subPath);
            
            // Return values from IMemoryCache
            var queryCacheName = "info_" + subPath;
            
            // Return Cached object if it exist
            if (_cache.TryGetValue(queryCacheName, out var objectExifToolModel))
                return objectExifToolModel as FileIndexItem;
            
            // Try to catch a new object
            objectExifToolModel = ReadExifAndXmpFromFileDirect(subPath);
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
