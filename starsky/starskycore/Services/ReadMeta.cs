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
    public partial class ReadMeta : IReadMeta
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

        private FileIndexItem ReadExifAndXmpFromFileDirect(
	        string subPath, ExtensionRolesHelper.ImageFormat imageFormat)
        {
	        if ( _iStorage.ExistFile(subPath) && imageFormat == ExtensionRolesHelper.ImageFormat.gpx )
	        {
				return _readGpx.ReadGpxFromFileReturnAfterFirstField(_iStorage.ReadStream(subPath));
	        }

	        var fileIndexItem = _readXmp.XmpGetSidecarFile(new FileIndexItem(subPath));

	        if ( fileIndexItem.IsoSpeed == 0 
	             || string.IsNullOrEmpty(fileIndexItem.Make) 
	             || fileIndexItem.DateTime.Year == 0)
	        {
		        var databaseItemFile = _readExif.ReadExifFromFile(subPath);
		        FileIndexCompareHelper.Compare(fileIndexItem, databaseItemFile);
	        }
	        
            return fileIndexItem;
        }

        // used by the html generator
        public List<FileIndexItem> ReadExifAndXmpFromFileAddFilePathHash(string[] subPathArray, string[] fileHashes = null)
        {
            var fileIndexList = new List<FileIndexItem>();

	        for ( int i = 0; i < subPathArray.Length; i++ )
	        {
		        var subPath = subPathArray[i];
		        var imageFormat = ExtensionRolesHelper.GetImageFormat(subPath); 
		        var returnItem = ReadExifAndXmpFromFile(subPath,imageFormat);

		        returnItem.ImageFormat = imageFormat;
		        returnItem.FileName = Path.GetFileName(subPath);
		        returnItem.IsDirectory = false;
		        returnItem.Id = -1;
		        returnItem.Status = FileIndexItem.ExifStatus.Ok;
		        returnItem.ParentDirectory = Breadcrumbs.BreadcrumbHelper(subPath).LastOrDefault();

		        if ( fileHashes == null || fileHashes.Length <= i )
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
        // Short living cache Max 10. minutes
        public FileIndexItem ReadExifAndXmpFromFile(string subPath, ExtensionRolesHelper.ImageFormat imageFormat)
        {
            // The CLI programs uses no cache
            if( _cache == null || _appSettings?.AddMemoryCache == false) 
                return ReadExifAndXmpFromFileDirect(subPath,imageFormat);
            
            // Return values from IMemoryCache
            var queryCacheName = "info_" + subPath;
            
            // Return Cached object if it exist
            if (_cache.TryGetValue(queryCacheName, out var objectExifToolModel))
                return objectExifToolModel as FileIndexItem;
            
            // Try to catch a new object
            objectExifToolModel = ReadExifAndXmpFromFileDirect(subPath,imageFormat);
            _cache.Set(queryCacheName, objectExifToolModel, new TimeSpan(0,10,0));
            return (FileIndexItem) objectExifToolModel;
        }

        
        //     Update only for ReadMeta!
        public void UpdateReadMetaCache(string fullFilePath, FileIndexItem objectExifToolModel)
        {
            if (_cache == null || _appSettings?.AddMemoryCache == false) return;

            var toUpdateObject = objectExifToolModel.Clone();
            var queryCacheName = "info_" + fullFilePath;
            RemoveReadMetaCache(fullFilePath);
            _cache.Set(queryCacheName, toUpdateObject, new TimeSpan(0,10,0));
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