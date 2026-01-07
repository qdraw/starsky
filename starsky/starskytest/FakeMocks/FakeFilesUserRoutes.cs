using System.Collections.Generic;
using System.Threading.Tasks;
using Dropbox.Api.Files;
using starskytest.starsky.feature.cloudimport;

namespace starskytest.FakeMocks;

public class FakeFilesUserRoutes
{
	public List<string> ListFolderCalledWith { get; } = new();
	public List<FakeCloudFileEntry> Entries { get; } = new();

	public Task<ListFolderResult> ListFolderAsync(string path)
	{
		ListFolderCalledWith.Add(path);
		var result = new ListFolderResult(new List<Metadata>(), "1", false);
		return Task.FromResult(result);
	}
}
