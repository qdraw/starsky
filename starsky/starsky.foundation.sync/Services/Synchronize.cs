using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Models;
using starsky.foundation.storage.Storage;

namespace starsky.foundation.sync.Services
{
	public class Synchronize
	{
		private readonly IQuery _query;
		private readonly IStorage _iStorage;

		public Synchronize(IQuery query, ISelectorStorage selectorStorage)
		{
			_query = query;
			_iStorage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
		}
		
		public async Task<List<FileIndexItem>> Sync(string subPath, bool recursive = true)
		{
			// Prefix / for database
			subPath = PathHelper.PrefixDbSlash(subPath);
			subPath = PathHelper.RemoveLatestSlash(subPath);


			switch ( _iStorage.IsFolderOrFile(subPath) )
			{
				case FolderOrFileModel.FolderOrFileTypeList.Folder:
					return await Folder(subPath);
					break;
				case FolderOrFileModel.FolderOrFileTypeList.File:
					return await NewSingleFile(subPath);
					break;
				case FolderOrFileModel.FolderOrFileTypeList.Deleted:
					break;
			}
			return new List<FileIndexItem>();
		}

		internal async Task<List<FileIndexItem>> NewSingleFile(string subPath)
		{
			var fileIndexItem = new FileIndexItem(subPath);

			// File extension is not supported
			if ( !ExtensionRolesHelper.IsExtensionSyncSupported(subPath) )
			{
				fileIndexItem.Status = FileIndexItem.ExifStatus.OperationNotSupported;
				return new List<FileIndexItem>{fileIndexItem};
			}

			// File check if jpg #not corrupt
			var imageFormat = ExtensionRolesHelper.GetImageFormat(_iStorage.ReadStream(subPath,160));
			if ( !ExtensionRolesHelper.ExtensionSyncSupportedList.Contains($"{imageFormat}") )
			{
				fileIndexItem.Status = FileIndexItem.ExifStatus.OperationNotSupported;
				return new List<FileIndexItem>{fileIndexItem};
			}

			fileIndexItem =  await _query.GetObjectByFilePathAsync(subPath);
			if ( fileIndexItem != null )
			{
				
			}
			
			// await _query.AddParentItemsAsync(subPath);
			return new List<FileIndexItem>();
		}

		private async Task<List<FileIndexItem>> Folder(string subPath)
		{
			return new List<FileIndexItem>();
		}
		
		
	}
}
