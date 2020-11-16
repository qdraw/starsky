using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;

namespace starsky.foundation.sync.SyncServices
{
	public class SyncFolder
	{
		private readonly IStorage _subPathStorage;

		public SyncFolder(IQuery query, ISelectorStorage selectorStorage)
		{
			_subPathStorage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
		}

		public Task<List<FileIndexItem>> Folder(string subPath)
		{
			return Task.FromResult(new List<FileIndexItem>());
		}
	}
}
