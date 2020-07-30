using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starsky.foundation.readmeta.Services;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Models;
using starsky.foundation.storage.Storage;
using starsky.foundation.sync.Helpers;

namespace starsky.foundation.sync.Services
{
	public class Synchronize
	{
		private readonly IQuery _query;
		private readonly IStorage _subPathStorage;
		private readonly AppSettings _appSettings;
		private readonly NewItem _newItem;

		public Synchronize(AppSettings appSettings, IQuery query, ISelectorStorage selectorStorage)
		{
			_appSettings = appSettings;
			_query = query;
			_subPathStorage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
			_newItem = new NewItem(_subPathStorage, new ReadMeta(_subPathStorage, _appSettings));
		}
		
		public async Task<List<FileIndexItem>> Sync(string subPath, bool recursive = true)
		{
			// Prefix / for database
			subPath = PathHelper.PrefixDbSlash(subPath);
			subPath = PathHelper.RemoveLatestSlash(subPath);

			switch ( _subPathStorage.IsFolderOrFile(subPath) )
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
			var statusItem = new FileIndexItem(subPath);

			// File extension is not supported
			if ( !ExtensionRolesHelper.IsExtensionSyncSupported(subPath) )
			{
				statusItem.Status = FileIndexItem.ExifStatus.OperationNotSupported;
				return new List<FileIndexItem>{statusItem};
			}

			// File check if jpg #not corrupt
			var imageFormat = ExtensionRolesHelper.GetImageFormat(_subPathStorage.ReadStream(subPath,160));
			if ( !ExtensionRolesHelper.ExtensionSyncSupportedList.Contains($"{imageFormat}") )
			{
				statusItem.Status = FileIndexItem.ExifStatus.OperationNotSupported;
				return new List<FileIndexItem>{statusItem};
			}

			var dbItem =  await _query.GetObjectByFilePathAsync(subPath);
			// when item does not exist in Database
			if ( dbItem == null )
			{
				dbItem = await _newItem.Item(statusItem);
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
