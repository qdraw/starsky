using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dropbox.Api.Files;
using starskytest.starsky.feature.cloudimport;

namespace starskytest.FakeMocks;

public class FakeFilesUserRoutes
{
	public List<string> ListFolderCalledWith { get; } = new();
	public List<FakeCloudFileEntry> Entries { get; } = new();
	public bool HasMore { get; set; }
	public string Cursor { get; set; } = string.Empty;

	public Task<ListFolderResult> ListFolderAsync(string path)
	{
		ListFolderCalledWith.Add(path);
		var entries = Entries.Select(e =>
			new FileMetadata(
				id: e.AsFile.Id,
				name: e.AsFile.Name,
				clientModified: e.AsFile.ServerModified.UtcDateTime,
				serverModified: e.AsFile.ServerModified.UtcDateTime,
				rev: "123456789",
				size: ( ulong ) e.AsFile.Size,
				pathLower: e.AsFile.PathLower,
				pathDisplay: e.AsFile.PathDisplay,
				sharingInfo: null,
				isDownloadable: true,
				contentHash: new string('a', 64)
			)
		).Cast<Metadata>().ToList();
		var result = new ListFolderResult(entries, "1", HasMore);
		return Task.FromResult(result);
	}
}
