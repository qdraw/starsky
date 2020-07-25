using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using starsky.feature.geolookup.Models;
using starsky.feature.geolookup.Services;
using starsky.foundation.database.Models;
using starskycore.Services;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starsky.foundation.readmeta.Interfaces;
using starsky.foundation.readmeta.Services;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Models;
using starsky.foundation.storage.Services;
using starsky.foundation.storage.Storage;
using starsky.foundation.writemeta.Interfaces;
using starsky.foundation.writemeta.Services;

namespace starsky.Controllers
{
	[Authorize]
	public class GeoController : Controller
	{
		private readonly IExifTool _exifTool;
		private readonly AppSettings _appSettings;
		private readonly IBackgroundTaskQueue _bgTaskQueue;
		private readonly IReadMeta _readMeta;
		private readonly IMemoryCache _cache;
		private readonly IStorage _thumbnailStorage;
		private readonly IStorage _iStorage;

		public GeoController(IExifTool exifTool, 
			AppSettings appSettings, IBackgroundTaskQueue queue,
			ISelectorStorage selectorStorage, 
			IMemoryCache memoryCache = null )
		{
			_appSettings = appSettings;
			_exifTool = exifTool;
			_bgTaskQueue = queue;
			_iStorage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
			_thumbnailStorage = selectorStorage.Get(SelectorStorage.StorageServices.Thumbnail);
			_readMeta = new ReadMeta(_iStorage);
			_cache = memoryCache;
		}

		
		/// <summary>
		/// Get Geo sync status
		/// </summary>
		/// <param name="f">sub path folders</param>
		/// <returns>status of geo sync</returns>
		/// <response code="200">the current status</response>
		/// <response code="404">cache service is missing</response>
		[HttpGet("/api/geo/status")]
		[ProducesResponseType(typeof(GeoCacheStatus),200)] // "cache service is missing"
		[ProducesResponseType(typeof(string),404)] // "Not found"
		[Produces("application/json")]
		public IActionResult Status(
			string f = "/")
		{
			if ( _cache == null ) return NotFound("cache service is missing");
			return Json(new GeoCacheStatusService(_cache).Status(f));
		}
		
		
		/// <summary>
		/// Reverse lookup for Geo Information and/or add Geo location based on a GPX file within the same directory
		/// </summary>
		/// <param name="f">subPath only folders</param>
		/// <param name="index">-i in cli</param>
		/// <param name="overwriteLocationNames"> -a in cli</param>
		/// <returns></returns>
		/// <response code="200">event is fired</response>
		/// <response code="404">sub path not found in the database</response>
		/// <response code="401">User unauthorized</response>
		[HttpPost("/api/geo/sync")]
		[Produces("application/json")]
		[ProducesResponseType(typeof(string),404)] // event is fired
		[ProducesResponseType(typeof(string),200)] // "Not found"
		public IActionResult SyncFolder(
			string f = "/",
			bool index = true,
			bool overwriteLocationNames = false
		)
		{
			if ( _iStorage.IsFolderOrFile(f) == FolderOrFileModel.FolderOrFileTypeList.Deleted )
			{
				return NotFound("Folder location is not found");
			}
			
			// Update >
			_bgTaskQueue.QueueBackgroundWorkItem(async token =>
			{
				if ( !_iStorage.ExistFolder(f) ) return;
				// use relative to StorageFolder
				var listOfFiles = _iStorage.GetAllFilesInDirectory(f)
					.Where(ExtensionRolesHelper.IsExtensionSyncSupported).ToList();

				var fileIndexList = _readMeta
					.ReadExifAndXmpFromFileAddFilePathHash(listOfFiles);
			
				var toMetaFilesUpdate = new List<FileIndexItem>();
				if ( index )
				{
					toMetaFilesUpdate =
						new GeoIndexGpx(_appSettings, _iStorage, _cache)
							.LoopFolder(fileIndexList);
					
					if ( _appSettings.Verbose ) Console.Write("¬");
					
					new GeoLocationWrite(_appSettings, _exifTool, _iStorage, _thumbnailStorage)
						.LoopFolder(toMetaFilesUpdate, false);
					
				}

				fileIndexList =
					new GeoReverseLookup(_appSettings,_cache)
						.LoopFolderLookup(fileIndexList, overwriteLocationNames);
				
				if ( fileIndexList.Count >= 1 )
				{
					new GeoLocationWrite(_appSettings, _exifTool, _iStorage, _thumbnailStorage).LoopFolder(
						fileIndexList, true);
				}

				// Loop though all options
				fileIndexList.AddRange(toMetaFilesUpdate);

				// update thumbs to avoid unnecessary re-generation
				foreach ( var item in fileIndexList.GroupBy(i => i.FilePath).Select(g => g.First())
					.ToList() )
				{
					var newThumb = new FileHash(_iStorage).GetHashCode(item.FilePath).Key;
					_thumbnailStorage.FileMove(item.FileHash, newThumb);
					if ( _appSettings.Verbose )
						Console.WriteLine("thumb + `" + item.FileHash + "`" + newThumb);
				}
			});
			
			return Json("event fired");
		}
	}
}
