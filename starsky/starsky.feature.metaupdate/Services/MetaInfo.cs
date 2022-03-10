using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;
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
using starsky.foundation.writemeta.JsonService;

namespace starsky.feature.metaupdate.Services
{
	[Service(typeof(IMetaInfo), InjectionLifetime = InjectionLifetime.Scoped)]
	public class MetaInfo : IMetaInfo
	{
		private readonly IQuery _query;
		private readonly AppSettings _appSettings;
		private readonly ReadMeta _readMeta;
		private readonly IStorage _iStorage;
		private readonly StatusCodesHelper _statusCodeHelper;

		public MetaInfo(IQuery query, AppSettings appSettings, ISelectorStorage selectorStorage, IMemoryCache memoryCache)
		{
			_query = query;
			_appSettings = appSettings;
			_iStorage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
			_readMeta = new ReadMeta(_iStorage,_appSettings, memoryCache);
			_statusCodeHelper = new StatusCodesHelper(_appSettings);
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
					_statusCodeHelper.ReturnExifStatusError(new FileIndexItem(subPath), 
						FileIndexItem.ExifStatus.NotFoundNotInIndex,
						fileIndexResultsList);
					continue;
				}
				
				if ( !_iStorage.ExistFile(detailView.FileIndexItem.FilePath) )
				{
					_statusCodeHelper.ReturnExifStatusError(detailView.FileIndexItem, 
						FileIndexItem.ExifStatus.NotFoundSourceMissing,
						fileIndexResultsList);
					continue; 
				}
				
				// Check if extension is supported for ExtensionExifToolSupportedList
				// Not all files are able to write with exifTool
				if(!ExtensionRolesHelper.IsExtensionExifToolSupported(detailView.FileIndexItem.FileName))
				{
					_statusCodeHelper.ReturnExifStatusError(
						new FileIndexItemJsonParser(_iStorage).Read(detailView.FileIndexItem), 
						FileIndexItem.ExifStatus.ExifWriteNotSupported,
						fileIndexResultsList);
					continue;
				}
        
				var statusResults = StatusCodesHelper.IsDeletedStatus(detailView);
				// only when default status to avoid unneeded checks
				if ( statusResults == FileIndexItem.ExifStatus.Default ) statusResults = _statusCodeHelper.IsReadOnlyStatus(detailView);
				// when everything is checked, it should be good
				if ( statusResults == FileIndexItem.ExifStatus.Default ) statusResults = FileIndexItem.ExifStatus.Ok;

				var collectionSubPathList = DetailView.GetCollectionSubPathList(detailView.FileIndexItem, collections, subPath);
        
				foreach ( var collectionSubPath in collectionSubPathList )
				{
					var collectionItem = _readMeta.ReadExifAndXmpFromFile(collectionSubPath);
		            
					collectionItem.Status = statusResults;
					collectionItem.CollectionPaths = collectionSubPathList;
					collectionItem.ImageFormat =
						ExtensionRolesHelper.MapFileTypesToExtension(collectionSubPath);
					collectionItem.Size = _iStorage.Info(collectionSubPath).Size;
					fileIndexResultsList.Add(collectionItem);
				}
			}

			return fileIndexResultsList;
		}
	}
}
