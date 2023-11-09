using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
		
		public async Task<List<FileIndexItem>> DeleteAsync(string filePath, bool includeCollections)
		{
			var inputFilePaths = PathHelper.SplitInputFilePaths(filePath);
			var fileIndexResultsList = new List<FileIndexItem>();
			var collectionAndInsideDirectoryList = new List<string>();

			foreach (var subPath in inputFilePaths)
			{
				var detailView = _query.SingleItem(subPath, null, includeCollections, false);

				if (detailView?.FileIndexItem?.FilePath == null)
				{
					HandleNotFoundStatus(subPath, fileIndexResultsList);
					continue;
				}

				// Status should be deleted before you can delete the item
				if (_iStorage.IsFolderOrFile(detailView.FileIndexItem.FilePath) == FolderOrFileModel.FolderOrFileTypeList.Deleted)
				{
					HandleNotFoundSourceMissingStatus(detailView.FileIndexItem, fileIndexResultsList);
					continue;
				}

				// Dir is readonly / don't delete
				if (_statusCodeHelper.IsReadOnlyStatus(detailView) == FileIndexItem.ExifStatus.ReadOnly)
				{
					HandleReadOnlyStatus(detailView.FileIndexItem, fileIndexResultsList);
					continue;
				}

				if (StatusCodesHelper.IsDeletedStatus(detailView) != FileIndexItem.ExifStatus.Deleted)
				{
					HandleOperationNotSupportedStatus(detailView.FileIndexItem, fileIndexResultsList);
					continue;
				}

				collectionAndInsideDirectoryList.AddRange(DetailView.GetCollectionSubPathList(detailView.FileIndexItem, includeCollections, subPath));

				// For deleting content of an entire directory
				if ( detailView.FileIndexItem.IsDirectory != true ) continue;

				// when deleting a folder the collections setting does nothing
				collectionAndInsideDirectoryList.AddRange((await _query.GetAllFilesAsync(detailView.FileIndexItem.FilePath)).Select(itemInDirectory => itemInDirectory.FilePath));
			}

			await HandleCollectionDeletion(collectionAndInsideDirectoryList, fileIndexResultsList);

			return fileIndexResultsList;
		}

		private async Task HandleCollectionDeletion(List<string> collectionAndInsideDirectoryList, List<FileIndexItem> fileIndexResultsList)
		{
			// collectionAndInsideDirectoryList should not have duplicate items
			foreach (var collectionSubPath in new HashSet<string>(collectionAndInsideDirectoryList))
			{
				var detailViewItem = _query.SingleItem(collectionSubPath, null, false, false);

				// null only happens when some other process also delete this item
				if (detailViewItem == null) continue;

				// return a Ok, which means the file is deleted
				detailViewItem.FileIndexItem.Status = FileIndexItem.ExifStatus.Ok;
				
				// remove thumbnail from disk
				_thumbnailStorage.FileDelete(detailViewItem.FileIndexItem.FileHash);
				fileIndexResultsList.Add(detailViewItem.FileIndexItem.Clone());

				// remove item from db
				await _query.RemoveItemAsync(detailViewItem.FileIndexItem);
				RemoveXmpSideCarFile(detailViewItem);
				RemoveJsonSideCarFile(detailViewItem);
				RemoveFileOrFolderFromDisk(detailViewItem);

				// the child directories are still stored in the database
				if (detailViewItem.FileIndexItem.IsDirectory != true) continue;

				foreach (var item in (await _query.GetAllRecursiveAsync(collectionSubPath)).Where(p => p.IsDirectory == true))
				{
					item.Status = FileIndexItem.ExifStatus.Deleted;
					fileIndexResultsList.Add(item.Clone());
					await _query.RemoveItemAsync(item);
				}
			}
		}

		private static void HandleNotFoundStatus(string subPath, List<FileIndexItem> fileIndexResultsList)
		{
			StatusCodesHelper.ReturnExifStatusError(new FileIndexItem(subPath), FileIndexItem.ExifStatus.NotFoundNotInIndex, fileIndexResultsList);
		}

		private static void HandleNotFoundSourceMissingStatus(FileIndexItem fileIndexItem, List<FileIndexItem> fileIndexResultsList)
		{
			StatusCodesHelper.ReturnExifStatusError(fileIndexItem, FileIndexItem.ExifStatus.NotFoundSourceMissing, fileIndexResultsList);
		}

		private static void HandleReadOnlyStatus(FileIndexItem fileIndexItem, List<FileIndexItem> fileIndexResultsList)
		{
			StatusCodesHelper.ReturnExifStatusError(fileIndexItem, FileIndexItem.ExifStatus.ReadOnly, fileIndexResultsList);
		}

		private static void HandleOperationNotSupportedStatus(FileIndexItem fileIndexItem, List<FileIndexItem> fileIndexResultsList)
		{
			StatusCodesHelper.ReturnExifStatusError(fileIndexItem, FileIndexItem.ExifStatus.OperationNotSupported, fileIndexResultsList);
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
