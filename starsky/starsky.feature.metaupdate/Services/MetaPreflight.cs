using System;
using System.Collections.Generic;
using starsky.feature.metaupdate.Interfaces;
using starsky.foundation.database.Helpers;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.injection;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;

namespace starsky.feature.metaupdate.Services
{
	[Service(typeof(IMetaPreflight), InjectionLifetime = InjectionLifetime.Scoped)]
	public class MetaPreflight : IMetaPreflight
	{
		private readonly IQuery _query;
		private readonly AppSettings _appSettings;
		private readonly IStorage _iStorage;

		public MetaPreflight(IQuery query, AppSettings appSettings, ISelectorStorage selectorStorage)
		{
			_query = query;
			_appSettings = appSettings;
			if ( selectorStorage != null ) _iStorage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
		}

		public (List<FileIndexItem> fileIndexResultsList, 
			Dictionary<string, List<string>>
			changedFileIndexItemName) Preflight(FileIndexItem inputModel, string[] inputFilePaths,
				bool append, bool collections, int rotateClock)
		{
			// the result list
			var fileIndexResultsList = new List<FileIndexItem>();
			
			// Per file stored key = string[fileHash] item => List <string> FileIndexItem.name (e.g. Tags) that are changed
			var changedFileIndexItemName = new Dictionary<string, List<string>>();
			
			foreach (var subPath in inputFilePaths)
			{
				var detailView = _query.SingleItem(subPath,null,collections,false);
				
				// todo : check if file/directory exist 
				
				// Dir is readonly / don't edit
				if ( new StatusCodesHelper(_appSettings).IsReadOnlyStatus(detailView) 
				     == FileIndexItem.ExifStatus.ReadOnly)
				{
					new StatusCodesHelper().ReturnExifStatusError(detailView.FileIndexItem, 
						FileIndexItem.ExifStatus.ReadOnly,
						fileIndexResultsList);
					continue; 
				}
				
				// Deleted is allowed but the status need be updated
				if ( new StatusCodesHelper(_appSettings).IsDeletedStatus(detailView) 
				     == FileIndexItem.ExifStatus.Deleted)
				{
					new StatusCodesHelper().ReturnExifStatusError(detailView.FileIndexItem, 
						FileIndexItem.ExifStatus.ReadOnly,
						fileIndexResultsList);
				}
				
				var collectionSubPathList = detailView.GetCollectionSubPathList(detailView, collections, subPath);
				foreach ( var collectionSubPath in collectionSubPathList )
				{
					var collectionsDetailView = _query.SingleItem(collectionSubPath, 
						null, collections, false);
					
					CompareAllLabelsAndRotation(changedFileIndexItemName,
								collectionsDetailView, inputModel, append, 0); // todo fix rotate
					
					// this one is good :)
					collectionsDetailView.FileIndexItem.Status = FileIndexItem.ExifStatus.Ok;
					
					// update database cache
					_query.CacheUpdateItem(new List<FileIndexItem>{collectionsDetailView.FileIndexItem});
					
					// The hash in FileIndexItem is not correct
					fileIndexResultsList.Add(collectionsDetailView.FileIndexItem);
				}
			}
			return (fileIndexResultsList, changedFileIndexItemName);
		}

		/// <summary>
		/// Compare Rotation and All other tags
		/// </summary>
		/// <param name="changedFileIndexItemName">Per file stored  string{FilePath},
		/// List*string*{FileIndexItem.name (e.g. Tags) that are changed}</param>
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
		/// Add to comparedNames list and add to detail view
		/// </summary>
		/// <param name="rotateClock">-1 or 1</param>
		/// <param name="fileIndexItem">main db object</param>
		/// <param name="comparedNamesList">list of types that are changes</param>
		/// <returns>updated image</returns>
		public FileIndexItem RotationCompare(int rotateClock, FileIndexItem fileIndexItem, 
			ICollection<string> comparedNamesList)
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
	}
}
