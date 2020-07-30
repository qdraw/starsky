using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.database.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Services;

namespace starsky.foundation.sync.Helpers
{
	public class HashcodeCompare
	{
		private readonly IStorage _subPathStorage;

		public HashcodeCompare(IStorage subPathStorage)
		{
			_subPathStorage = subPathStorage;
		}

		public async Task SingleItem(FileIndexItem fileIndexItems)
		{
			var (key,success) = await new FileHash(_subPathStorage).GetHashCodeAsync(fileIndexItems.FilePath);
			if ( !success ) return;
			

		}

		
		
		public async Task Compare(IEnumerable<FileIndexItem> fileIndexItems)
		{
			foreach ( var item in fileIndexItems )
			{
				var (key,success) = await new FileHash(_subPathStorage).GetHashCodeAsync(item.FilePath);
				if ( !success ) continue;

			}

		}
	}
}
