using System.Collections.Generic;
using starsky.feature.update.Interfaces;
using starsky.foundation.database.Helpers;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.injection;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;

namespace starsky.feature.update.Services
{
	[Service(typeof(IPreflightUpdate), InjectionLifetime = InjectionLifetime.Scoped)]
	public class PreflightUpdate : IPreflightUpdate
	{
		private readonly IQuery _query;
		private readonly AppSettings _appSettings;
		private readonly IStorage _iStorage;

		public PreflightUpdate(IQuery query, AppSettings appSettings, ISelectorStorage selectorStorage)
		{
			_query = query;
			_appSettings = appSettings;
			_iStorage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
		}
		
		public List<FileIndexItem> Preflight(FileIndexItem inputModel, string[] inputFilePaths, bool collections)
		{
			// the result list
			var fileIndexResultsList = new List<FileIndexItem>();
			
			foreach (var subPath in inputFilePaths)
			{
				var detailView = _query.SingleItem(subPath,null,collections,false);
				
				var statusResults = new StatusCodesHelper(_appSettings).FileCollectionsCheck(detailView);
				
				//
				// var statusModel = inputModel.Clone();
				// statusModel.IsDirectory = false;
				// statusModel.SetFilePath(subPath);
				//
				//
				// // Readonly is not allowed
				// if(new StatusCodesHelper().ReadonlyDenied(statusModel, statusResults, fileIndexResultsList)) continue;
				//
				// // if one item fails, the status will added
				// if(new StatusCodesHelper().ReturnExifStatusError(statusModel, statusResults, fileIndexResultsList)) continue;
				//
				// if ( detailView == null ) throw new InvalidDataException("DetailView is null " + nameof(detailView));
				//
				//
				// var collectionSubPathList = detailView.GetCollectionSubPathList(detailView, collections, subPath);
    //             
				// // loop to update
				// foreach ( var collectionSubPath in collectionSubPathList )
				// {
				// 	var collectionsDetailView = _query.SingleItem(collectionSubPath, null, collections, false);
				//
				// 	// Compare Rotation and All other tags
				// 	new UpdateService(_query, _exifTool, _readMeta,_iStorage, _thumbnailStorage)
				// 		.CompareAllLabelsAndRotation(changedFileIndexItemName,
				// 			collectionsDetailView, statusModel, append, rotateClock);
				// 	
				// 	// this one is good :)
				// 	collectionsDetailView.FileIndexItem.Status = FileIndexItem.ExifStatus.Ok;
				// 	
				// 	// When it done this will be removed,
				// 	// to avoid conflicts
				// 	_readMeta.UpdateReadMetaCache(collectionSubPath,collectionsDetailView.FileIndexItem);
				// 	
				// 	// update database cache
				// 	_query.CacheUpdateItem(new List<FileIndexItem>{collectionsDetailView.FileIndexItem});
				// 	
				// 	// The hash in FileIndexItem is not correct
				// 	fileIndexResultsList.Add(collectionsDetailView.FileIndexItem);
				// }
            }

			return fileIndexResultsList;
		}
	}
}
