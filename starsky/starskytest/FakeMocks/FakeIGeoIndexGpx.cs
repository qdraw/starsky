using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.feature.geolookup.Interfaces;
using starsky.foundation.database.Models;

namespace starskytest.FakeMocks
{
	public class FakeIGeoIndexGpx : IGeoIndexGpx
	{
		public int Count { get; set; }
		
		public Task<List<FileIndexItem>> LoopFolderAsync(List<FileIndexItem> metaFilesInDirectory)
		{
			Count++;
			return Task.FromResult(new List<FileIndexItem>());
		}
	}
}
