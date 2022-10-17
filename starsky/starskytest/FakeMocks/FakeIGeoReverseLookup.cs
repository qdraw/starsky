using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.feature.geolookup.Interfaces;
using starsky.foundation.database.Models;

namespace starskytest.FakeMocks
{
	public class FakeIGeoReverseLookup : IGeoReverseLookup
	{
		private readonly List<FileIndexItem> _fileIndexItems = new List<FileIndexItem>();

		public FakeIGeoReverseLookup(List<FileIndexItem> fileIndexItems = null)
		{
			if ( fileIndexItems != null ) _fileIndexItems = fileIndexItems;
		}

		public int Count { get; set; }

		public Task<List<FileIndexItem>> LoopFolderLookup(List<FileIndexItem> metaFilesInDirectory, bool overwriteLocationNames)
		{
			Count++;
			metaFilesInDirectory.AddRange(_fileIndexItems);
			return Task.FromResult(metaFilesInDirectory);
		}
	}
}
