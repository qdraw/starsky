using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using starsky.feature.metaupdate.Interfaces;
using starsky.foundation.database.Interfaces;
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
using starsky.foundation.thumbnailgeneration.Helpers;
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
		private readonly IReadMetaSubPathStorage _readMeta;
		private readonly IStorage _iStorage;
		private readonly IStorage _thumbnailStorage;
		private readonly IMetaPreflight _metaPreflight;
		private readonly IWebLogger _logger;

		public MetaUpdateService(
			IQuery query,
			IExifTool exifTool, 
			ISelectorStorage selectorStorage,
			IMetaPreflight metaPreflight,
			IWebLogger logger,
			IReadMetaSubPathStorage readMetaSubPathStorage)
		{
			_query = query;
			_exifTool = exifTool;
			_iStorage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
			_thumbnailStorage = selectorStorage.Get(SelectorStorage.StorageServices.Thumbnail);
			_readMeta = readMetaSubPathStorage;
			_metaPreflight = metaPreflight;
			_logger = logger;
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
		public async Task<List<FileIndexItem>> UpdateAsync(
			Dictionary<string,List<string>> changedFileIndexItemName,
			List<FileIndexItem> fileIndexResultsList,
			FileIndexItem inputModel, // only when changedFileIndexItemName = null
			bool collections, bool append, // only when changedFileIndexItemName = null
			int rotateClock) // <- this one is needed
		{
			if ( changedFileIndexItemName == null )
			{
				changedFileIndexItemName = (await _metaPreflight.Preflight(inputModel,
					fileIndexResultsList.Select(p => p.FilePath).ToArray(), append, collections,
					rotateClock)).changedFileIndexItemName;
			}
			
			var updatedItems = new List<FileIndexItem>();
			var fileIndexItemList = fileIndexResultsList
				.Where(p => p.Status == FileIndexItem.ExifStatus.Ok 
				            || p.Status == FileIndexItem.ExifStatus.Deleted).ToList();
				
			foreach ( var fileIndexItem in fileIndexItemList )
			{
				if (changedFileIndexItemName.ContainsKey(fileIndexItem.FilePath) )
				{
					// used for tracking differences, in the database/ExifTool compare
					var comparedNamesList = changedFileIndexItemName[fileIndexItem.FilePath];

					await UpdateWriteDiskDatabase(fileIndexItem, comparedNamesList, rotateClock);
					updatedItems.Add(fileIndexItem);
					continue;
				}
				
				_logger.LogError($"Missing in key: {fileIndexItem.FilePath}",
					new InvalidDataException($"changedFileIndexItemName: " +
					                         $"{string.Join(",",changedFileIndexItemName)}"));
				throw new ArgumentException($"Missing in key: {fileIndexItem.FilePath}",
					nameof(changedFileIndexItemName));
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
			await RotationThumbnailExecute(rotateClock, fileIndexItem);

			if ( fileIndexItem.IsDirectory != true 
			     && ExtensionRolesHelper.IsExtensionExifToolSupported(fileIndexItem.FileName) )
			{
				// feature to exif update
				var exifUpdateFilePaths = new List<string>
				{
					fileIndexItem.FilePath           
				};
				var exifTool = new ExifToolCmdHelper(_exifTool,_iStorage,_thumbnailStorage,_readMeta);

				// to avoid diskWatcher catch up
				_query.SetGetObjectByFilePathCache(fileIndexItem.FilePath, fileIndexItem, TimeSpan.FromSeconds(10));
					
				// Do an Exif Sync for all files, including thumbnails
				var (exifResult,newFileHashes) = await exifTool.UpdateAsync(fileIndexItem, 
					exifUpdateFilePaths, comparedNamesList,true, true);

				await ApplyOrGenerateUpdatedFileHash(newFileHashes, fileIndexItem);
				_logger.LogInformation(string.IsNullOrEmpty(exifResult)
					? $"[UpdateWriteDiskDatabase] ExifTool result is Nothing or " +
					  $"Null for: path:{fileIndexItem.FilePath} {DateTime.UtcNow.ToShortTimeString()}"
					: $"[UpdateWriteDiskDatabase] ExifTool result: {exifResult} path:{fileIndexItem.FilePath}");
			}
			else
			{
				await new FileIndexItemJsonParser(_iStorage).WriteAsync(fileIndexItem);
			}

			// Do a database sync + cache sync
			// Clone to avoid reference when cache exist
			await _query.UpdateItemAsync(fileIndexItem.Clone());
			
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
		private async Task RotationThumbnailExecute(int rotateClock, FileIndexItem fileIndexItem)
		{
			// Do orientation
			if(FileIndexItem.IsRelativeOrientation(rotateClock)) 
				await new Thumbnail(_iStorage,_thumbnailStorage,
					_logger).RotateThumbnail(fileIndexItem.FileHash,rotateClock);
		}
	}
}
