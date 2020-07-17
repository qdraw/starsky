using System.Collections.Generic;
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
		private readonly IStorage _iStorage;

		public MetaInfo(IQuery query, AppSettings appSettings, ISelectorStorage selectorStorage)
		{
			_query = query;
			_appSettings = appSettings;
			_iStorage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
			_readMeta = new ReadMeta(_iStorage);
		}
		
		public List<FileIndexItem> GetInfo(List<string> inputFilePaths, bool collections)
		{
			// the result list
			var fileIndexResultsList = new List<FileIndexItem>();
        
			foreach (var subPath in inputFilePaths)
			{
				var detailView = _query.SingleItem(subPath, null, collections, false);
				
				if ( detailView?.FileIndexItem == null )
				{
					new StatusCodesHelper().ReturnExifStatusError(new FileIndexItem(subPath), 
						FileIndexItem.ExifStatus.NotFoundNotInIndex,
						fileIndexResultsList);
					continue;
				}
				
				if ( !_iStorage.ExistFile(detailView.FileIndexItem.FilePath) )
				{
					new StatusCodesHelper().ReturnExifStatusError(detailView.FileIndexItem, 
						FileIndexItem.ExifStatus.NotFoundSourceMissing,
						fileIndexResultsList);
					continue; 
				}
				
				// Check if extension is supported for ExtensionExifToolSupportedList
				// Not all files are able to write with exifTool
				if(!ExtensionRolesHelper.IsExtensionExifToolSupported(detailView.FileIndexItem.FileName))
				{
					new StatusCodesHelper().ReturnExifStatusError(detailView.FileIndexItem, 
						FileIndexItem.ExifStatus.OperationNotSupported,
						fileIndexResultsList);
					continue;
				}
        
				var statusResults = new StatusCodesHelper(_appSettings).IsDeletedStatus(detailView);
				if ( statusResults == FileIndexItem.ExifStatus.Default ) statusResults = new StatusCodesHelper().IsReadOnlyStatus(detailView);
        
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
