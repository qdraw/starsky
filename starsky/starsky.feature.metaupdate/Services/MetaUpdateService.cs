using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using starsky.feature.metaupdate.Interfaces;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.injection;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.readmeta.Interfaces;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Services;
using starsky.foundation.storage.Storage;
using starsky.foundation.thumbnailgeneration.Helpers;
using starsky.foundation.webtelemetry.Interfaces;
using starsky.foundation.writemeta.Interfaces;
using starsky.foundation.writemeta.JsonService;
using ExifToolCmdHelper = starsky.foundation.writemeta.Helpers.ExifToolCmdHelper;

[assembly: InternalsVisibleTo("starskytest")]
namespace starsky.feature.metaupdate.Services
{
	[Service(typeof(IMetaUpdateService), InjectionLifetime = InjectionLifetime.Scoped)]
	public class MetaUpdateService : IMetaUpdateService
	{ 
		private readonly IQuery _query;
		private readonly IExifTool _exifTool;
		private readonly IReadMeta _readMeta;
		private readonly IStorage _iStorage;
		private readonly IStorage _thumbnailStorage;
		private readonly IMetaPreflight _metaPreflight;
		private readonly IWebLogger _logger;
		private readonly ITelemetryService _telemetryService;

		public MetaUpdateService(
			IQuery query,
			IExifTool exifTool, 
			IReadMeta readMeta,
			ISelectorStorage selectorStorage,
			IMetaPreflight metaPreflight,
			IWebLogger logger, ITelemetryService telemetryService = null)
		{
			_query = query;
			_exifTool = exifTool;
			_readMeta = readMeta;
			_iStorage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
			_thumbnailStorage = selectorStorage.Get(SelectorStorage.StorageServices.Thumbnail);
			_metaPreflight = metaPreflight;
			_logger = logger;
			_telemetryService = telemetryService;
		}

		/// <summary>
		/// Run Update
		/// </summary>
		/// <param name="changedFileIndexItemName">Per file stored  string{fileHash},
		///     List*string*{FileIndexItem.name (e.g. Tags) that are changed}</param>
		/// <param name="fileIndexResultsList">items stored in the database</param>
		/// <param name="inputModel">(only used when cache is disabled)
		///     This model is overwritten in the database and ExifTool</param>
		/// <param name="collections">enable or disable this feature</param>
		/// <param name="append">only for disabled cache or changedFileIndexItemName=null</param>
		/// <param name="rotateClock">rotation value 1 left, -1 right, 0 nothing</param>
		public async Task<List<FileIndexItem>> Update(
			List<FileIndexItem> fileIndexResultsList,
			FileIndexItem inputModel,
			bool collections, bool append, int rotateClock)
		{
			// need to get again and check to know if its changed on the road
			var (databaseFileIndexItems , changedFileIndexItemName )= 
				(await _metaPreflight.Preflight(inputModel,
					fileIndexResultsList.Select(p => p.FilePath).ToArray(), append, collections,
					rotateClock));
				
			var updatedItems = new List<FileIndexItem>();
			var preflightResultFilePathList = fileIndexResultsList
				.Where(p => p.Status == FileIndexItem.ExifStatus.Ok 
				            || p.Status == FileIndexItem.ExifStatus.Deleted)
				.Select(p => p.FilePath).ToList();
				
			foreach ( var filePath in preflightResultFilePathList )
			{
				var fileIndexItem = databaseFileIndexItems.FirstOrDefault(p => p.FilePath == filePath);

				if ( fileIndexItem != null && changedFileIndexItemName.ContainsKey(filePath) )
				{
					// used for tracking differences, in the database/ExifTool compare
					var comparedNamesList = changedFileIndexItemName[filePath];

					await UpdateWriteDiskDatabase(fileIndexItem, comparedNamesList, rotateClock);
					updatedItems.Add(fileIndexItem);
					continue;
				}

				if ( fileIndexItem == null && changedFileIndexItemName.ContainsKey(filePath) )
				{
					_telemetryService?.TrackException(
						new InvalidDataException("detailView is missing for and NOT Saved: " +
						                         filePath));
					continue;
				}

				throw new ArgumentException($"Missing in key: {filePath}",
					nameof(inputModel));

			}

			return updatedItems;
		}

		public void UpdateReadMetaCache(IEnumerable<FileIndexItem> returnNewResultList)
		{
			_readMeta.UpdateReadMetaCache(returnNewResultList);
		}

		/// <summary>
		/// Update ExifTool, Thumbnail, Database and if needed rotateClock
		/// </summary>
		/// <param name="fileIndexItem">output database object</param>
		/// <param name="comparedNamesList">name of fields updated by exifTool</param>
		/// <param name="rotateClock">rotation value (if needed)</param>
		private async Task UpdateWriteDiskDatabase(FileIndexItem fileIndexItem, List<string> comparedNamesList, int rotateClock = 0)
		{
			// do rotation on thumbs
			RotationThumbnailExecute(rotateClock, fileIndexItem);

			if ( fileIndexItem.IsDirectory != true 
			     && ExtensionRolesHelper.IsExtensionExifToolSupported(fileIndexItem.FileName) )
			{
				// feature to exif update
				var exifUpdateFilePaths = new List<string>
				{
					fileIndexItem.FilePath           
				};
				var exifTool = new ExifToolCmdHelper(_exifTool,_iStorage,_thumbnailStorage,_readMeta);
				
				// Do an Exif Sync for all files, including thumbnails
				var (exifResult,newFileHashes) = await exifTool.UpdateAsync(fileIndexItem, 
					exifUpdateFilePaths, comparedNamesList,true, true);

				await ApplyOrGenerateUpdatedFileHash(newFileHashes, fileIndexItem);
				_logger.LogInformation($"[UpdateWriteDiskDatabase] exifResult: {exifResult}");
			}
			else
			{
				await new FileIndexItemJsonParser(_iStorage).WriteAsync(fileIndexItem);
			}

			// Do a database sync + cache sync
			await _query.UpdateItemAsync(fileIndexItem);
			
			// > async > force you to read the file again
			// do not include thumbs in MetaCache
			// only the full path url of the source image
			_readMeta.RemoveReadMetaCache(fileIndexItem.FilePath);		
		}

		internal async Task ApplyOrGenerateUpdatedFileHash(List<string> newFileHashes, FileIndexItem fileIndexItem)
		{
			if ( !string.IsNullOrWhiteSpace(newFileHashes.FirstOrDefault()))
			{
				fileIndexItem.FileHash = newFileHashes.FirstOrDefault();
				_logger.LogInformation($"use fileHash from exiftool {fileIndexItem.FileHash}");
				return;
			}
			// when newFileHashes is null or string.empty
			var newFileHash = (await new FileHash(_iStorage).GetHashCodeAsync(fileIndexItem.FilePath)).Key;
			_thumbnailStorage.FileMove(fileIndexItem.FileHash, newFileHash);
			fileIndexItem.FileHash = newFileHash;
		}

		/// <summary>
		/// Run the Orientation changes on the thumbnail (only relative)
		/// </summary>
		/// <param name="rotateClock">-1 or 1</param>
		/// <param name="fileIndexItem">object contains fileHash</param>
		/// <returns>updated image</returns>
		private void RotationThumbnailExecute(int rotateClock, FileIndexItem fileIndexItem)
		{
			// Do orientation
			if(FileIndexItem.IsRelativeOrientation(rotateClock)) 
				new Thumbnail(_iStorage,_thumbnailStorage).RotateThumbnail(fileIndexItem.FileHash,rotateClock);
		}
	}
}
