using System.Collections.Generic;
using System.Linq;
using starsky.feature.metaupdate.Interfaces;
using starsky.foundation.database.Helpers;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.injection;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Models;
using starsky.foundation.storage.Storage;

namespace starsky.feature.metaupdate.Services
{
	[Service(typeof(IDeleteItem), InjectionLifetime = InjectionLifetime.Scoped)]
	public class DeleteItem : IDeleteItem
	{
		private readonly IQuery _query;
		private readonly IStorage _iStorage;
		private readonly IStorage _thumbnailStorage;
		private readonly StatusCodesHelper _statusCodeHelper;

		public DeleteItem(IQuery query, AppSettings appSettings, ISelectorStorage selectorStorage)
		{
			_query = query;
			_iStorage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
			_thumbnailStorage = selectorStorage.Get(SelectorStorage.StorageServices.Thumbnail);
			_statusCodeHelper = new StatusCodesHelper(appSettings);
		}
		
		public List<FileIndexItem> Delete(string f, bool collections)
		{
			var inputFilePaths = PathHelper.SplitInputFilePaths(f);
            
			// the result list
			var fileIndexResultsList = new List<FileIndexItem>();
			var collectionAndInsideDirectoryList = new List<string>();
			
			foreach ( var subPath in inputFilePaths )
			{
				var detailView =
					_query.SingleItem(subPath, null, collections, false);

				if ( detailView?.FileIndexItem == null )
				{
					_statusCodeHelper.ReturnExifStatusError(
						new FileIndexItem(subPath),
						FileIndexItem.ExifStatus.NotFoundNotInIndex,
						fileIndexResultsList);
					continue;
				}

				if ( _iStorage.IsFolderOrFile(detailView.FileIndexItem
					     .FilePath) ==
				     FolderOrFileModel.FolderOrFileTypeList.Deleted )
				{
					_statusCodeHelper.ReturnExifStatusError(
						detailView.FileIndexItem,
						FileIndexItem.ExifStatus.NotFoundSourceMissing,
						fileIndexResultsList);
					continue;
				}

				// Dir is readonly / don't delete
				if ( _statusCodeHelper.IsReadOnlyStatus(detailView)
				     == FileIndexItem.ExifStatus.ReadOnly )
				{
					_statusCodeHelper.ReturnExifStatusError(
						detailView.FileIndexItem,
						FileIndexItem.ExifStatus.ReadOnly,
						fileIndexResultsList);
					continue;
				}

				// Status should be deleted before you can delete the item
				if ( _statusCodeHelper.IsDeletedStatus(detailView)
				     != FileIndexItem.ExifStatus.Deleted )
				{
					_statusCodeHelper.ReturnExifStatusError(
						detailView.FileIndexItem,
						FileIndexItem.ExifStatus.OperationNotSupported,
						fileIndexResultsList);
					continue;
				}
				
				collectionAndInsideDirectoryList.AddRange(detailView.GetCollectionSubPathList(detailView.FileIndexItem, collections, subPath));
				
				// For deleting content of an entire directory
				if ( detailView.FileIndexItem.IsDirectory != true ) continue;

				// when deleting a folder the collections setting does nothing
				collectionAndInsideDirectoryList.AddRange(
					_query.GetAllFiles(detailView.FileIndexItem.FilePath).Select(itemInDirectory => itemInDirectory.FilePath)
				);
			}

			// collectionAndInsideDirectoryList should not have duplicate items
			foreach ( var collectionSubPath in new HashSet<string>(collectionAndInsideDirectoryList) )
			{
				var detailViewItem = _query.SingleItem(collectionSubPath, 
					null, false, false);
				
				// null only happens when some other process also delete this item
				if ( detailViewItem == null ) continue;
				
				// return a Ok, which means the file is deleted
				detailViewItem.FileIndexItem.Status = FileIndexItem.ExifStatus.Ok;
     
				// remove thumbnail from disk
				_thumbnailStorage.FileDelete(detailViewItem.FileIndexItem.FileHash);
     
				fileIndexResultsList.Add(detailViewItem.FileIndexItem.Clone());
	                
				// remove item from db
				_query.RemoveItem(detailViewItem.FileIndexItem);

				RemoveXmpSideCarFile(detailViewItem);
				RemoveJsonSideCarFile(detailViewItem);
				RemoveFileOrFolderFromDisk(detailViewItem);
				
				// the child directories are still stored in the database
				if ( detailViewItem.FileIndexItem.IsDirectory != true )
					continue;
				
				foreach ( var item in _query.GetAllRecursive(collectionSubPath).Where(p => p.IsDirectory == true) )
				{
					item.Status = FileIndexItem.ExifStatus.Deleted;
					fileIndexResultsList.Add(item.Clone());
					_query.RemoveItem(item);
				}
			}
			return fileIndexResultsList;
		}

		private void RemoveXmpSideCarFile(DetailView detailViewItem)
		{
			// remove the sidecar file (if exist)
			if ( ExtensionRolesHelper.IsExtensionForceXmp(detailViewItem.FileIndexItem
				.FileName) )
			{
				_iStorage.FileDelete(
					ExtensionRolesHelper.ReplaceExtensionWithXmp(detailViewItem
						.FileIndexItem.FilePath));
			}
		}

		private void RemoveJsonSideCarFile(DetailView detailViewItem)
		{
			// remove the json sidecar file (if exist)
			var jsonSubPath = JsonSidecarLocation.JsonLocation(detailViewItem
				.FileIndexItem.ParentDirectory, detailViewItem
				.FileIndexItem.FileName);
			_iStorage.FileDelete(jsonSubPath);
		}

		private void RemoveFileOrFolderFromDisk(DetailView detailViewItem)
		{
			if ( detailViewItem.FileIndexItem.IsDirectory == true )
			{
				_iStorage.FolderDelete(detailViewItem.FileIndexItem.FilePath);
				return;
			}
	                
			// and remove the actual file
			_iStorage.FileDelete(detailViewItem.FileIndexItem.FilePath);
		}
	}
}
