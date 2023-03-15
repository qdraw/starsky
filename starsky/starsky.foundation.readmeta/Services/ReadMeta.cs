﻿using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Caching.Memory;
using starsky.foundation.database.Helpers;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.readmeta.Interfaces;
using starsky.foundation.readmeta.ReadMetaHelpers;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Services;

namespace starsky.foundation.readmeta.Services
{
	public sealed class ReadMeta : IReadMeta
	{
		private readonly AppSettings _appSettings;
		private readonly IMemoryCache _cache;
		private readonly IStorage _iStorage;
		private readonly ReadMetaExif _readExif;
		private readonly ReadMetaXmp _readXmp;
		private readonly ReadMetaGpx _readMetaGpx;

		/// <summary>
		/// Used to get from all locations
		/// </summary>
		/// <param name="iStorage"></param>
		/// <param name="appSettings"></param>
		/// <param name="memoryCache"></param>
		/// <param name="logger"></param>
		public ReadMeta(IStorage iStorage, AppSettings appSettings, IMemoryCache memoryCache, IWebLogger logger)
		{
			_appSettings = appSettings;
			_cache = memoryCache;
			_iStorage = iStorage;
			_readExif = new ReadMetaExif(_iStorage, appSettings,logger);
			_readXmp = new ReadMetaXmp(_iStorage, logger);
			_readMetaGpx = new ReadMetaGpx(logger);
		}

		private FileIndexItem ReadExifAndXmpFromFileDirect(string subPath)
		{
			if ( _iStorage.ExistFile(subPath) 
			     && ExtensionRolesHelper.IsExtensionForceGpx(subPath) )
			{
				// Get the item back with DateTime as Camera local datetime
				return _readMetaGpx.ReadGpxFromFileReturnAfterFirstField(
					_iStorage.ReadStream(subPath), 
					subPath); // use local
			}
	        
			var fileIndexItemWithPath = new FileIndexItem(subPath);

			// Read first the sidecar file
			var xmpFileIndexItem = _readXmp.XmpGetSidecarFile(fileIndexItemWithPath.Clone());

			// if the sidecar file is not complete, read the original file
			// when reading a .xmp file direct ignore the readExifFromFile
			if ( ExtensionRolesHelper.IsExtensionSidecar(subPath) )
				return xmpFileIndexItem;

			if ( xmpFileIndexItem.IsoSpeed != 0
			     && !string.IsNullOrEmpty(xmpFileIndexItem.Make)
			     && xmpFileIndexItem.DateTime.Year != 0
			     && !string.IsNullOrEmpty(xmpFileIndexItem.ShutterSpeed) )
			{
				return xmpFileIndexItem;
			}
			
			// so the sidecar file is not used to store the most important tags
			var fileExifItemFile = _readExif.ReadExifFromFile(subPath,fileIndexItemWithPath);
		        
			// overwrite content with incomplete sidecar file (this file can contain tags)
			FileIndexCompareHelper.Compare(fileExifItemFile, xmpFileIndexItem);
			return fileExifItemFile;
		}

		// used by the html generator
		public List<FileIndexItem> ReadExifAndXmpFromFileAddFilePathHash(List<string> subPathList, List<string> fileHashes = null)
		{
			var fileIndexList = new List<FileIndexItem>();

			for ( int i = 0; i < subPathList.Count; i++ )
			{
				var subPath = subPathList[i];
		        
				var returnItem = ReadExifAndXmpFromFile(subPath);
				var imageFormat = ExtensionRolesHelper.GetImageFormat(_iStorage.ReadStream(subPath, 50)); 

				returnItem.ImageFormat = imageFormat;
				returnItem.FileName = Path.GetFileName(subPath);
				returnItem.IsDirectory = false;
				returnItem.Status = FileIndexItem.ExifStatus.Ok;
				returnItem.ParentDirectory = FilenamesHelper.GetParentPath(subPath);

				if ( fileHashes == null || fileHashes.Count <= i )
				{
					returnItem.FileHash = new FileHash(_iStorage).GetHashCode(subPath).Key;
				}
				else
				{
					returnItem.FileHash = fileHashes[i];
				}

				fileIndexList.Add(returnItem);
			}
			return fileIndexList;
		}

		private const string CachePrefix = "info_";

		/// <summary>
		/// Different types including GPX
		/// Cached view >> IMemoryCache
		/// Short living cache Max 1. minutes
		/// </summary>
		/// <param name="subPath">path</param>
		/// <returns>metaData</returns>
		public FileIndexItem ReadExifAndXmpFromFile(string subPath)
		{
			// The CLI programs uses no cache
			if( _cache == null || _appSettings?.AddMemoryCache == false) 
				return ReadExifAndXmpFromFileDirect(subPath);
            
			// Return values from IMemoryCache
			var queryReadMetaCacheName = CachePrefix + subPath;
            
			// Return Cached object if it exist
			if (_cache.TryGetValue(queryReadMetaCacheName, out var objectExifToolModel))
				return objectExifToolModel as FileIndexItem;
            
			// Try to catch a new object
			objectExifToolModel = ReadExifAndXmpFromFileDirect(subPath);
			_cache.Set(queryReadMetaCacheName, objectExifToolModel, 
				new TimeSpan(0,1,0));
			return (FileIndexItem) objectExifToolModel;
		}

        
		/// <summary>
		/// Update Cache only for ReadMeta!
		/// To 15 minutes
		/// </summary>
		/// <param name="fullFilePath">can also be a subPath</param>
		/// <param name="objectExifToolModel">the item</param>
		public void UpdateReadMetaCache(string fullFilePath, FileIndexItem objectExifToolModel)
		{
			if (_cache == null || _appSettings?.AddMemoryCache == false) return;

			var toUpdateObject = objectExifToolModel.Clone();
			var queryReadMetaCacheName = CachePrefix + fullFilePath;
			RemoveReadMetaCache(fullFilePath);
			_cache.Set(queryReadMetaCacheName, toUpdateObject, 
				new TimeSpan(0,15,0));
		}

		/// <summary>
		/// Update cache list of items in the cache
		/// assumes that subPath style is used
		/// </summary>
		/// <param name="objectExifToolModel">list of items to update</param>
		public void UpdateReadMetaCache(IEnumerable<FileIndexItem> objectExifToolModel)
		{
			foreach ( var item in objectExifToolModel )
			{
				UpdateReadMetaCache(item.FilePath, item);
			}
		}

		/// <summary>
		/// only for ReadMeta! Cache
		/// Why removing, The Update command does not update the entire object.
		/// When you update tags, other tags will be null 
		/// </summary>
		/// <param name="fullFilePath">can also be a subPath</param>
		public void RemoveReadMetaCache(string fullFilePath)
		{
			if (_cache == null || _appSettings?.AddMemoryCache == false) return;
			var queryCacheName = CachePrefix + fullFilePath;

			if (!_cache.TryGetValue(queryCacheName, out var _)) return; 
			// continue = go to the next item in the list
			_cache.Remove(queryCacheName);
		}
	}
}
