using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using starsky.feature.geolookup.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.injection;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.readmeta.Interfaces;
using starsky.foundation.readmeta.Services;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Services;
using starsky.foundation.storage.Storage;
using starsky.foundation.writemeta.Interfaces;

namespace starsky.feature.geolookup.Services
{
	[Service(typeof(IGeoBackgroundTask), InjectionLifetime = InjectionLifetime.Scoped)]
	public class GeoBackgroundTask : IGeoBackgroundTask
	{
		private readonly AppSettings _appSettings;
		private readonly IReadMeta _readMeta;
		private readonly IStorage _thumbnailStorage;
		private readonly IStorage _iStorage;
		private readonly IGeoLocationWrite _geoLocationWrite;
		private readonly IWebLogger _logger;
		private readonly GeoIndexGpx _geoIndexGpx;
		private readonly IGeoReverseLookup _geoReverseLookup;

		public GeoBackgroundTask(AppSettings appSettings, ISelectorStorage selectorStorage, 
			IGeoLocationWrite geoLocationWrite, IMemoryCache memoryCache, 
			IWebLogger logger, IGeoReverseLookup geoReverseLookup)
		{
			_appSettings = appSettings;
			_iStorage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
			_thumbnailStorage = selectorStorage.Get(SelectorStorage.StorageServices.Thumbnail);
			_readMeta = new ReadMeta(_iStorage);
			_geoLocationWrite = geoLocationWrite;
			_logger = logger;
			_geoIndexGpx = new GeoIndexGpx(_appSettings, _iStorage, memoryCache);
			_geoReverseLookup = geoReverseLookup;
		}
		
		public async Task<List<FileIndexItem>> GeoBackgroundTaskAsync(
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
					_geoIndexGpx
						.LoopFolder(fileIndexList);
					
				if ( _appSettings.IsVerbose() ) Console.Write("¬");
					
				await _geoLocationWrite
					.LoopFolderAsync(toMetaFilesUpdate, false);
			}

			fileIndexList =
				_geoReverseLookup
					.LoopFolderLookup(fileIndexList, overwriteLocationNames);
				
			if ( fileIndexList.Count >= 1 )
			{
				await _geoLocationWrite.LoopFolderAsync(
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
				if ( _appSettings.IsVerbose() )
					_logger.LogInformation("[/api/geo/sync] thumb rename + `" + item.FileHash + "`" + newThumb);
			}

			return fileIndexList;
		}
	}
}
