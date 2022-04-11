using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.feature.metaupdate.Interfaces;
using starsky.foundation.database.Models;

namespace starskytest.FakeMocks
{
	public class FakeIMetaUpdateService : IMetaUpdateService
	{
		public List<Dictionary<string, List<string>>>
			ChangedFileIndexItemNameContent { get; set; } =
			new List<Dictionary<string, List<string>>>();
		
		public Task<List<FileIndexItem>> UpdateAsync(
			Dictionary<string, List<string>> changedFileIndexItemName,
			List<FileIndexItem> fileIndexResultsList, FileIndexItem inputModel,
			bool collections, bool append, int rotateClock)
		{
			ChangedFileIndexItemNameContent.Add(changedFileIndexItemName);
			return Task.FromResult(fileIndexResultsList);
		}

		public void UpdateReadMetaCache(IEnumerable<FileIndexItem> returnNewResultList)
		{
		}
	}
}
