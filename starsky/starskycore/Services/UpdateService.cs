using System;
using System.Collections.Generic;
using System.Linq;
using starskycore.Helpers;
using starskycore.Interfaces;
using starskycore.Models;
using starskycore.ViewModels;

namespace starskycore.Services
{
	public class UpdateService
	{ 
		private readonly IQuery _query;
		private readonly IExifTool _exifTool;
		private readonly IReadMeta _readMeta;
		private readonly IStorage _iStorage;
		private readonly IStorage _thumbnailStorage;

		public UpdateService(
			IQuery query,
			IExifTool exifTool, 
			IReadMeta readMeta,
			IStorage iStorage,
			IStorage thumbnailStorage)
		{
			_query = query;
			_exifTool = exifTool;
			_readMeta = readMeta;
			_iStorage = iStorage;
			_thumbnailStorage = thumbnailStorage;
		}

		/// <summary>
		/// Compare Rotation and All other tags
		/// </summary>
		/// <param name="changedFileIndexItemName">Per file stored  string{FilePath}, List*string*{FileIndexItem.name (e.g. Tags) that are changed}</param>
		/// <param name="collectionsDetailView">DetailView input, only to display changes</param>
		/// <param name="statusModel">object that include the changes</param>
		/// <param name="append">true= for tags to add</param>
		/// <param name="rotateClock">rotation value 1 left, -1 right, 0 nothing</param>
		public void CompareAllLabelsAndRotation( Dictionary<string, List<string>> changedFileIndexItemName, 
			DetailView collectionsDetailView, FileIndexItem statusModel, bool append, int rotateClock)
		{
			if ( changedFileIndexItemName == null )
				throw new MissingFieldException(nameof(changedFileIndexItemName));
			
			// compare and add changes to collectionsDetailView
			var comparedNamesList = FileIndexCompareHelper
				.Compare(collectionsDetailView.FileIndexItem, statusModel, append);
					
			// if requested, add changes to rotation
			collectionsDetailView.FileIndexItem = 
				RotationCompare(rotateClock, collectionsDetailView.FileIndexItem, comparedNamesList);

			if ( ! changedFileIndexItemName.ContainsKey(collectionsDetailView.FileIndexItem.FilePath) )
			{
				// add to list
				changedFileIndexItemName.Add(collectionsDetailView.FileIndexItem.FilePath,comparedNamesList);
				return;
			}
			
			// overwrite list if already exist
			changedFileIndexItemName[collectionsDetailView.FileIndexItem.FilePath] = comparedNamesList;
			
		}

		/// <summary>
		/// Run Update
		/// </summary>
		/// <param name="changedFileIndexItemName">Per file stored  string{fileHash}, List*string*{FileIndexItem.name (e.g. Tags) that are changed}</param>
		/// <param name="fileIndexResultsList"></param>
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
					CompareAllLabelsAndRotation(changedFileIndexItemName, detailView, inputModel, append, rotateClock);
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

			// Do an Exif Sync for all files, including thumbnails
			var exifResult = exifTool.Update(detailView.FileIndexItem, exifUpdateFilePaths, comparedNamesList);
			
			Console.WriteLine($"exifResult: {exifResult}");
                        
			// change thumbnail names after the original is changed
			var newFileHash = new FileHash(_iStorage).GetHashCode(detailView.FileIndexItem.FilePath);
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
		/// Add to comparedNames list and add to detail view
		/// </summary>
		/// <param name="rotateClock">-1 or 1</param>
		/// <param name="fileIndexItem">main db object</param>
		/// <param name="comparedNamesList">list of types that are changes</param>
		/// <returns>updated image</returns>
		public FileIndexItem RotationCompare(int rotateClock, FileIndexItem fileIndexItem, ICollection<string> comparedNamesList)
		{
			// Do orientation / Rotate if needed (after compare)
			if (!FileIndexItem.IsRelativeOrientation(rotateClock)) return fileIndexItem;
			// run this on detail view => statusModel is always default
			fileIndexItem.SetRelativeOrientation(rotateClock);
			
			// list of exifTool to update this field
			if ( !comparedNamesList.Contains(nameof(fileIndexItem.Orientation)) )
			{
				comparedNamesList.Add(nameof(fileIndexItem.Orientation));
			}
			return fileIndexItem;
		}
		
		/// <summary>
		/// Run the Orientation changes on the thumbnail (only relative)
		/// </summary>
		/// <param name="rotateClock">-1 or 1</param>
		/// <param name="fileIndexItem">object contains filehash</param>
		/// <returns>updated image</returns>
		private void RotationThumbnailExecute(int rotateClock, FileIndexItem fileIndexItem)
		{
			// Do orientation
			if(FileIndexItem.IsRelativeOrientation(rotateClock)) 
				new Thumbnail(_iStorage,_thumbnailStorage).RotateThumbnail(fileIndexItem.FileHash,rotateClock);
		}
	}
}
