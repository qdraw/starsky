using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.database.Models;
using starsky.foundation.metaupdate.Interfaces;

namespace starskytest.FakeMocks;

public class FakeMetaPreflight2 : IMetaPreflight
{
	public List<FileIndexItem> FileIndexResultsList { get; set; } = [];
	public Dictionary<string, List<string>> ChangedFileIndexItemName { get; set; } = new();

	public Task<(List<FileIndexItem> fileIndexResultsList, Dictionary<string,
		List<string>> changedFileIndexItemName)> PreflightAsync(FileIndexItem? inputModel,
		List<string> inputFilePaths, bool append,
		bool collections, int rotateClock)
	{
		return Task.FromResult(( FileIndexResultsList, ChangedFileIndexItemName ));
	}
}
