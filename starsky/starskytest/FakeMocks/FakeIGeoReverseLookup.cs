using System.Collections.Generic;
using starsky.feature.geolookup.Interfaces;
using starsky.foundation.database.Models;

namespace starskytest.FakeMocks
{
	public class FakeIGeoReverseLookup : IGeoReverseLookup
	{
		public int Count { get; set; }

		public List<FileIndexItem> LoopFolderLookup(List<FileIndexItem> metaFilesInDirectory, bool overwriteLocationNames)
		{
			Count++;
			return metaFilesInDirectory;
		}
	}
}
