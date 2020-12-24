using System.Collections.Generic;
using starsky.feature.geolookup.Interfaces;
using starsky.foundation.database.Models;

namespace starskytest.FakeMocks
{
	public class FakeIGeoIndexGpx : IGeoIndexGpx
	{
		public int Count { get; set; }
		
		public List<FileIndexItem> LoopFolder(List<FileIndexItem> metaFilesInDirectory)
		{
			Count++;
			return metaFilesInDirectory;
		}
	}
}
