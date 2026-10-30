using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using starsky.foundation.database.Models;
using starsky.foundation.metaupdate.Interfaces;

namespace starskytest.FakeMocks;

public class FakeIMetaUpdateRunner : IMetaUpdateRunner
{
	public FakeIMetaUpdateRunner(List<FileIndexItem>? existing = null)
	{
		Existing = existing ?? [];
	}

	private List<FileIndexItem> Existing { get; }

	public List<(FileIndexItem Model, string[] Paths, bool Append, bool Collections)> Calls
	{
		get;
	} = [];

	public Task<MetaUpdateRunResult> UpdateAsync(FileIndexItem inputModel,
		string[] inputFilePaths, bool append, bool collections, int rotateClock)
	{
		Calls.Add((inputModel, inputFilePaths, append, collections));
		var results = inputFilePaths.Select(path =>
			Existing.Find(p => p.FilePath == path) ?? new FileIndexItem(path)
			{
				Status = FileIndexItem.ExifStatus.NotFoundNotInIndex
			}).ToList();

		var anyFound = results.Exists(p => p.Status == FileIndexItem.ExifStatus.Ok);
		return Task.FromResult(new MetaUpdateRunResult(results,
			anyFound ? results.Select(p => p.Clone()).ToList() : null));
	}
}
