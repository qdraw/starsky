using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using starsky.feature.geolookup.Interfaces;
using starsky.feature.geolookup.Models;
using starsky.feature.geolookup.Services;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.readmeta.Interfaces;
using starsky.foundation.readmeta.Services;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Models;
using starsky.foundation.storage.Services;
using starsky.foundation.storage.Storage;
using starsky.foundation.worker.Services;
using starsky.foundation.writemeta.Interfaces;

namespace starsky.Controllers
{
	[Authorize]
	public class GeoController : Controller
	{
		private readonly AppSettings _appSettings;
		private readonly IBackgroundTaskQueue _bgTaskQueue;
		private readonly IReadMeta _readMeta;
		private readonly IMemoryCache _cache;
		private readonly IStorage _thumbnailStorage;
		private readonly IStorage _iStorage;
		private readonly IGeoLocationWrite _geoLocationWrite;
		private readonly IWebLogger _logger;

		public GeoController(AppSettings appSettings, IBackgroundTaskQueue queue,
			ISelectorStorage selectorStorage, 
			IGeoLocationWrite geoLocationWrite,
			IMemoryCache memoryCache, IWebLogger logger)
		{
			_appSettings = appSettings;
			_bgTaskQueue = queue;
			_iStorage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
			_thumbnailStorage = selectorStorage.Get(SelectorStorage.StorageServices.Thumbnail);
			_readMeta = new ReadMeta(_iStorage);
			_geoLocationWrite = geoLocationWrite;
			_cache = memoryCache;
			_logger = logger;
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
				GeoBackgroundTask(
					new GeoIndexGpx(_appSettings, _iStorage, _cache),
					new GeoReverseLookup(_appSettings, new GeoFileDownload(_appSettings), _cache), 
					_geoLocationWrite,
					f, index,
					overwriteLocationNames);
			});
			
			return Json("event fired");
		}

		internal List<FileIndexItem> GeoBackgroundTask(
			IGeoIndexGpx geoIndexGpx,
			IGeoReverseLookup geoReverseLookup,
			IGeoLocationWrite geoLocationWrite,
			string f = "/",
			bool index = true,
			bool overwriteLocationNames = false)
		{
			if ( !_iStorage.ExistFolder(f) ) return new List<FileIndexItem>();
			// use relative to StorageFolder
			var listOfFiles = _iStorage.GetAllFilesInDirectory(f)
				.Where(ExtensionRolesHelper.IsExtensionSyncSupported).ToList();

			var fileIndexList = _readMeta
				.ReadExifAndXmpFromFileAddFilePathHash(listOfFiles);
			
			var toMetaFilesUpdate = new List<FileIndexItem>();
			if ( index )
			{
				toMetaFilesUpdate =
					geoIndexGpx
						.LoopFolder(fileIndexList);
					
				if ( _appSettings.Verbose ) Console.Write("¬");
					
				geoLocationWrite
					.LoopFolder(toMetaFilesUpdate, false);
			}

			fileIndexList =
				geoReverseLookup
					.LoopFolderLookup(fileIndexList, overwriteLocationNames);
				
			if ( fileIndexList.Count >= 1 )
			{
				geoLocationWrite.LoopFolder(
					fileIndexList, true);
			}

			// Loop though all options
			fileIndexList.AddRange(toMetaFilesUpdate);

			// update thumbs to avoid unnecessary re-generation
			foreach ( var item in fileIndexList.GroupBy(i => i.FilePath).Select(g => g.First())
				.ToList() )
			{
				var newThumb = new FileHash(_iStorage).GetHashCode(item.FilePath).Key;
				if ( item.FileHash == newThumb) continue;
				new ThumbnailFileMoveAllSizes(_thumbnailStorage).FileMove(item.FileHash, newThumb);
				if ( _appSettings.Verbose )
					_logger.LogInformation("[/api/geo/sync] thumb rename + `" + item.FileHash + "`" + newThumb);
			}

			return fileIndexList;
		}
	}
}
