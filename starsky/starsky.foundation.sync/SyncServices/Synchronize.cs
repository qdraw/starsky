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
				case FolderOrFileModel.FolderOrFileTypeList.File:
					return await SyncSingleFile(subPath);
				case FolderOrFileModel.FolderOrFileTypeList.Deleted:
					break;
			}
			return new List<FileIndexItem>();
		}

		

		private async Task<List<FileIndexItem>> Folder(string subPath)
		{
			return new List<FileIndexItem>();
		}
		
		
	}
}
