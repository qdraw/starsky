using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using starsky.foundation.database.Models;
using starskycore.Helpers;
using starskycore.Interfaces;
using starskycore.Models;
using starskycore.Services;
using starsky.foundation.geo.Models;
using starsky.foundation.geo.Services;

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
		private readonly ISelectorStorage _selectorStorage;

		public GeoController(IExifTool exifTool, 
			AppSettings appSettings, IBackgroundTaskQueue queue,
			IReadMeta readMeta,
			ISelectorStorage selectorStorage, 
			IMemoryCache memoryCache = null )
		{
			_appSettings = appSettings;
			_exifTool = exifTool;
			_bgTaskQueue = queue;
			_readMeta = readMeta;
			_iStorage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
			_thumbnailStorage = selectorStorage.Get(SelectorStorage.StorageServices.Thumbnail);
			_selectorStorage = selectorStorage;
			_cache = memoryCache;
		}

		
		/// <summary>
		/// Get Geo sync status (WIP)
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
		/// WIP Alpha API -- Reverse lookup for Geo Information and/or add Geo location based on a GPX file within the same directory
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
					
					Console.Write("¬");
					
					new GeoLocationWrite(_appSettings, _exifTool, _selectorStorage)
						.LoopFolder(toMetaFilesUpdate, false);
					Console.Write("(gps added)");
				}

				fileIndexList =
					new GeoReverseLookup(_appSettings,_cache)
						.LoopFolderLookup(fileIndexList, overwriteLocationNames);
				
				if ( fileIndexList.Count >= 1 )
				{
					new GeoLocationWrite(_appSettings, _exifTool, _selectorStorage).LoopFolder(
						fileIndexList, true);
				}

				// Loop though all options
				fileIndexList.AddRange(toMetaFilesUpdate);

				// update thumbs to avoid unnecessary re-generation
				foreach ( var item in fileIndexList.GroupBy(i => i.FilePath).Select(g => g.First())
					.ToList() )
				{
					var newThumb = new FileHash(_iStorage).GetHashCode(item.FilePath);
					_thumbnailStorage.FileMove(item.FileHash, newThumb);
					if ( _appSettings.Verbose )
						Console.WriteLine("thumb + `" + item.FileHash + "`" + newThumb);
				}
			});
			
			return Json("event fired");
		}
	}
}
