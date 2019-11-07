using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using starskycore.Helpers;
using starskycore.Interfaces;
using starskycore.Models;
using starskycore.Services;
using starskygeocore.Services;

namespace starsky.Controllers
{
	[Authorize]
	public class GeoController : Controller
	{
		private readonly IExifTool _exifTool;
		private readonly AppSettings _appSettings;
		private readonly IBackgroundTaskQueue _bgTaskQueue;
		private readonly IReadMeta _readMeta;
		private readonly IStorage _iStorage;

		public GeoController(IExifTool exifTool, 
			AppSettings appSettings, IBackgroundTaskQueue queue,
			IReadMeta readMeta,
			IStorage iStorage, 
			IMemoryCache memoryCache = null )
		{
			_appSettings = appSettings;
			_exifTool = exifTool;
			_bgTaskQueue = queue;
			_readMeta = readMeta;
			_iStorage = iStorage;
		}
		
		/// <summary>
		/// WIP Alpha API -- Reverse lookup for Geo Information and/or add Geo location based on a GPX file within the same directory
		/// </summary>
		/// <param name="f">subPath only folders</param>
		/// <param name="index">-i in cli</param>
		/// <param name="overwriteLocationNames"> -a in cli</param>
		/// <returns></returns>
		/// <response code="200">event is fired</response>
		/// <response code="404">subpath not found in the database</response>
		/// <response code="401">User unauthorized</response>
		[HttpGet("/api/geo/sync")]
		[Produces("application/json")]
		[ProducesResponseType(404)]
		public IActionResult SyncFolder(
			string f = "/",
			bool index = true,
			bool overwriteLocationNames = false
		)
		{
			if ( _iStorage.IsFolderOrFile("/") == FolderOrFileModel.FolderOrFileTypeList.Deleted )
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
						new GeoIndexGpx(_appSettings, _iStorage).LoopFolder(
							fileIndexList);
					Console.Write("¬");
					new GeoLocationWrite(_appSettings, _exifTool).LoopFolder(
						toMetaFilesUpdate, false);
					Console.Write("(gps added)");
				}

				fileIndexList =
					new GeoReverseLookup(_appSettings).LoopFolderLookup(fileIndexList,
						overwriteLocationNames);
				if ( fileIndexList.Count >= 1 )
				{
					new GeoLocationWrite(_appSettings, _exifTool).LoopFolder(
						fileIndexList, true);
				}

				// Loop though all options
				fileIndexList.AddRange(toMetaFilesUpdate);

				// update thumbs to avoid unnecessary re-generation
				foreach ( var item in fileIndexList.GroupBy(i => i.FilePath).Select(g => g.First())
					.ToList() )
				{
					var newThumb = new FileHash(_iStorage).GetHashCode(item.FilePath);
					_iStorage.ThumbnailMove(item.FileHash, newThumb);
					if ( _appSettings.Verbose )
						Console.WriteLine("thumb+ `" + item.FileHash + "`" + newThumb);
				}
			});
			
			return Json("event fired");
		}
	}
}
