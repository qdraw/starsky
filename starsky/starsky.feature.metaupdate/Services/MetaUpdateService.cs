using System;
using System.Collections.Generic;
using System.Linq;
using starsky.feature.update.Interfaces;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.injection;
using starsky.foundation.platform.Helpers;
using starsky.foundation.readmeta.Interfaces;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Services;
using starsky.foundation.thumbnailgeneration.Services;
using starsky.foundation.writemeta.Interfaces;
using starsky.foundation.writemeta.JsonService;
using ExifToolCmdHelper = starsky.foundation.writemeta.Helpers.ExifToolCmdHelper;

namespace starsky.feature.update.Services
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

		public MetaUpdateService(
			IQuery query,
			IExifTool exifTool, 
			IReadMeta readMeta,
			IStorage iStorage,
			IStorage thumbnailStorage,
			IMetaPreflight metaPreflight)
		{
			_query = query;
			_exifTool = exifTool;
			_readMeta = readMeta;
			_iStorage = iStorage;
			_thumbnailStorage = thumbnailStorage;
			_metaPreflight = metaPreflight;
		}

		/// <summary>
		/// Run Update
		/// </summary>
		/// <param name="changedFileIndexItemName">Per file stored  string{fileHash},
		/// List*string*{FileIndexItem.name (e.g. Tags) that are changed}</param>
		/// <param name="fileIndexResultsList">items stored in the database</param>
		/// <param name="inputModel">This model is overwritten in the database and ExifTool</param>
		/// <param name="collections">enable or disable this feature</param>
		/// <param name="append">only for disabled cache or changedFileIndexItemName=null</param>
		/// <param name="rotateClock">rotation value 1 left, -1 right, 0 nothing</param>
		public void Update(Dictionary<string, List<string>> changedFileIndexItemName, 
			List<FileIndexItem> fileIndexResultsList,
			FileIndexItem inputModel, 
			bool collections, bool append, int rotateClock)
		{
			var collectionsDetailViewList = fileIndexResultsList.Where(p => p.Status == FileIndexItem.ExifStatus.Ok).ToList();
			foreach ( var item in collectionsDetailViewList )
			{
				// need to recheck because this process is async, so in the meanwhile there are changes possible
				var detailView = _query.SingleItem(item.FilePath,null,collections,false);

				// to get a value when null	
				if ( changedFileIndexItemName == null ) changedFileIndexItemName = new Dictionary<string, List<string>>();
				
				if ( !changedFileIndexItemName.ContainsKey(detailView.FileIndexItem.FilePath) || !_query.IsCacheEnabled() )
				{
					// the inputModel is always DoNotChange, so checking from the field is useless
					inputModel.Orientation = detailView.FileIndexItem.Orientation;
					
					// when you disable cache the field is not filled with the data
					// Compare Rotation and All other tags
					_metaPreflight.CompareAllLabelsAndRotation(changedFileIndexItemName, detailView, 
						inputModel, append, rotateClock);
				}
				
				// used for tracking differences, in the database/ExifTool compare
				var comparedNamesList = changedFileIndexItemName[detailView.FileIndexItem.FilePath];
				
				// Then update it on exifTool,database and rotation
				UpdateWriteDiskDatabase(detailView, comparedNamesList, rotateClock);
			}
		}
		
		/// <summary>
		/// Update ExifTool, Thumbnail, Database and if needed rotateClock
		/// </summary>
		/// <param name="detailView">output database object</param>
		/// <param name="comparedNamesList">name of fields updated by exifTool</param>
		/// <param name="rotateClock">rotation value (if needed)</param>
		public void UpdateWriteDiskDatabase(DetailView detailView, List<string> comparedNamesList, int rotateClock = 0)
		{
			var exifTool = new ExifToolCmdHelper(_exifTool,_iStorage,_thumbnailStorage,_readMeta);
					
			// feature to exif update
			var exifUpdateFilePaths = new List<string>
			{
				detailView.FileIndexItem.FilePath           
			};

			// do rotation on thumbs
			RotationThumbnailExecute(rotateClock, detailView.FileIndexItem);

							
			if ( ExtensionRolesHelper.IsExtensionExifToolSupported(detailView.FileIndexItem.FileName) )
			{
				// Do an Exif Sync for all files, including thumbnails
				var exifResult = exifTool.Update(detailView.FileIndexItem, exifUpdateFilePaths, comparedNamesList);
				Console.WriteLine($"exifResult: {exifResult}");
			}
			else
			{
				new FileIndexItemJsonWriter(_iStorage).Write(detailView.FileIndexItem);
			}
                        
			// change thumbnail names after the original is changed
			var newFileHash = new FileHash(_iStorage).GetHashCode(detailView.FileIndexItem.FilePath).Key;
			_thumbnailStorage.FileMove(detailView.FileIndexItem.FileHash, newFileHash);
					
			// Update the hash in the database
			detailView.FileIndexItem.FileHash = newFileHash;
			
			// Do a database sync + cache sync
			_query.UpdateItem(detailView.FileIndexItem);
                        
			// > async > force you to read the file again
			// do not include thumbs in MetaCache
			// only the full path url of the source image
			_readMeta.RemoveReadMetaCache(detailView.FileIndexItem.FilePath);		
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
