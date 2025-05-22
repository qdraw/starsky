using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.database.Models;
using starsky.foundation.metaupdate.Interfaces;

namespace starskytest.FakeMocks;

public class FakeIMetaUpdateService : IMetaUpdateService
{
	public List<Dictionary<string, List<string>>>
		ChangedFileIndexItemNameContent { get; set; } =
		new();

	public Task<List<FileIndexItem>> UpdateAsync(
		Dictionary<string, List<string>> changedFileIndexItemName,
		List<FileIndexItem> fileIndexResultsList, FileIndexItem? inputModel,
		bool collections, bool append, int rotateClock)
	{
		ChangedFileIndexItemNameContent.Add(changedFileIndexItemName);
		return Task.FromResult(fileIndexResultsList);
	}

	public void UpdateReadMetaCache(IEnumerable<FileIndexItem> returnNewResultList)
	{
	}
}
