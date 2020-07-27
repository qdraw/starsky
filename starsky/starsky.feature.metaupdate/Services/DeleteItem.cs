using System.Collections.Generic;
using starsky.feature.metaupdate.Interfaces;
using starsky.foundation.database.Helpers;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Models;
using starsky.foundation.storage.Storage;
using starsky.foundation.writemeta.JsonService;

namespace starsky.feature.metaupdate.Services
{
	public class DeleteItem : IDeleteItem
	{
		private readonly IQuery _query;
		private readonly AppSettings _appSettings;
		private readonly IStorage _iStorage;
		private readonly IStorage _thumbnailStorage;
		private readonly StatusCodesHelper _statusCodeHelper;

		public DeleteItem(IQuery query, AppSettings appSettings, ISelectorStorage selectorStorage)
		{
			_query = query;
			_appSettings = appSettings;
			_iStorage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
			_thumbnailStorage = selectorStorage.Get(SelectorStorage.StorageServices.Thumbnail);
			_statusCodeHelper = new StatusCodesHelper(_appSettings);
		}
		
		public List<FileIndexItem> Delete(string f, bool collections)
		{
			 var inputFilePaths = PathHelper.SplitInputFilePaths(f);
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

                if ( _iStorage.IsFolderOrFile(detailView.FileIndexItem.FilePath) == 
                     FolderOrFileModel.FolderOrFileTypeList.Deleted)
                {
	                _statusCodeHelper.ReturnExifStatusError(detailView.FileIndexItem, 
		                FileIndexItem.ExifStatus.NotFoundSourceMissing,
		                fileIndexResultsList);
	                continue; 
                }

                // Dir is readonly / don't delete
                if ( _statusCodeHelper.IsReadOnlyStatus(detailView) 
                     == FileIndexItem.ExifStatus.ReadOnly)
                {
	                _statusCodeHelper.ReturnExifStatusError(detailView.FileIndexItem, 
		                FileIndexItem.ExifStatus.ReadOnly,
		                fileIndexResultsList);
	                continue; 
                }
				
                // Status should be deleted before you can delete the item
                if ( _statusCodeHelper.IsDeletedStatus(detailView) 
                     != FileIndexItem.ExifStatus.Deleted)
                {
	                _statusCodeHelper.ReturnExifStatusError(detailView.FileIndexItem, 
		                FileIndexItem.ExifStatus.OperationNotSupported,
		                fileIndexResultsList);
	                continue;
                }

                var collectionSubPathList = detailView.GetCollectionSubPathList(detailView, collections, subPath);
     
                // display the to delete items
                for (int i = 0; i < collectionSubPathList.Count; i++)
                {
                    var collectionSubPath = collectionSubPathList[i];
                    var detailViewItem = _query.SingleItem(collectionSubPath, 
	                    null, collections, false);
     
                    // Allow only files that contains the delete tag
                    if (!detailViewItem.FileIndexItem.Tags.Contains("!delete!"))
                    {
                        detailViewItem.FileIndexItem.Status = FileIndexItem.ExifStatus.Unauthorized;
                        fileIndexResultsList.Add(detailViewItem.FileIndexItem.Clone());
                        continue;
                    }
	                // return a Ok, which means the file is deleted
	                detailViewItem.FileIndexItem.Status = FileIndexItem.ExifStatus.Ok;
     
					// remove thumbnail from disk
					_thumbnailStorage.FileDelete(detailViewItem.FileIndexItem.FileHash);
     
                    fileIndexResultsList.Add(detailViewItem.FileIndexItem.Clone());
	                
                    // remove item from db
                    _query.RemoveItem(detailViewItem.FileIndexItem);
     
	                // remove the sidecar file (if exist)
	                if ( ExtensionRolesHelper.IsExtensionForceXmp(detailViewItem.FileIndexItem
		                .FileName) )
	                {
		                _iStorage.FileDelete(
			                ExtensionRolesHelper.ReplaceExtensionWithXmp(detailViewItem
				                .FileIndexItem.FilePath));
	                }

	                // remove the json sidecar file (if exist)
	                var jsonSubPath = new FileIndexItemJsonParser(_iStorage).JsonLocation(detailViewItem
		                .FileIndexItem.ParentDirectory, detailViewItem
		                .FileIndexItem.FileName);
	                _iStorage.FileDelete(jsonSubPath);
	                
	                // and remove the actual file
	                _iStorage.FileDelete(detailViewItem.FileIndexItem.FilePath);
                }
            }

            return fileIndexResultsList;
		}
	}
}
