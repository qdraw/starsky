using System.Collections.Generic;
using System.IO;
using starsky.feature.metaupdate.Interfaces;
using starsky.foundation.database.Helpers;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.injection;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starsky.foundation.readmeta.Services;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;

namespace starsky.feature.metaupdate.Services
{
	[Service(typeof(IMetaInfo), InjectionLifetime = InjectionLifetime.Scoped)]
	public class MetaInfo : IMetaInfo
	{
		private readonly IQuery _query;
		private readonly AppSettings _appSettings;
		private readonly ReadMeta _readMeta;

		public MetaInfo(IQuery query, AppSettings appSettings, ISelectorStorage selectorStorage)
		{
			_query = query;
			_appSettings = appSettings;
			var iStorage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
			_readMeta = new ReadMeta(iStorage);
		}
		
		public List<FileIndexItem> GetInfo(List<string> inputFilePaths, bool collections)
		{
			// the result list
			var fileIndexResultsList = new List<FileIndexItem>();
        
			foreach (var subPath in inputFilePaths)
			{
				var detailView = _query.SingleItem(subPath, null, collections, false);
               
				// Check if extension is supported for ExtensionExifToolSupportedList
				// Not all files are able to write with exifTool
				if(detailView != null && !ExtensionRolesHelper.IsExtensionExifToolSupported(detailView.FileIndexItem.FileName))
				{
					detailView.FileIndexItem.Status = FileIndexItem.ExifStatus.ReadOnly;
					fileIndexResultsList.Add(detailView.FileIndexItem);
					continue;
				}
        
				// todo check if exist
				// todo readonly status
				var statusResults = new StatusCodesHelper(_appSettings).IsDeletedStatus(detailView);
				if ( statusResults == FileIndexItem.ExifStatus.Default ) statusResults = new StatusCodesHelper().IsReadOnlyStatus(detailView);

				// var statusModel = new FileIndexItem(subPath);
				// if(new StatusCodesHelper().ReturnExifStatusError(statusModel, statusResults, fileIndexResultsList)) continue;
	            
				if ( detailView == null ) throw new InvalidDataException("DetailView is null " + nameof(detailView));
        
				var collectionSubPathList = detailView.GetCollectionSubPathList(detailView, collections, subPath);
        
				foreach ( var collectionSubPath in collectionSubPathList )
				{
					var collectionItem = _readMeta.ReadExifAndXmpFromFile(collectionSubPath);
		            
					collectionItem.Status = statusResults;
					collectionItem.CollectionPaths = collectionSubPathList;
					collectionItem.ImageFormat =
						ExtensionRolesHelper.MapFileTypesToExtension(collectionSubPath);
        
					fileIndexResultsList.Add(collectionItem);
				}
			}

			return fileIndexResultsList;
		}
	}
}
