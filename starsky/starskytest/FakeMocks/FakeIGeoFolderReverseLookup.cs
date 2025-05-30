using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using starsky.feature.geolookup.Interfaces;
using starsky.foundation.database.Models;

namespace starskytest.FakeMocks;

public class FakeIGeoFolderReverseLookup : IGeoFolderReverseLookup
{
	private readonly List<FileIndexItem> _fileIndexItems = new();

	public FakeIGeoFolderReverseLookup(List<FileIndexItem>? fileIndexItems = null)
	{
		if ( fileIndexItems != null )
		{
			_fileIndexItems = fileIndexItems;
		}
	}

	public int Count { get; set; }

	public Task<List<FileIndexItem>> LoopFolderLookup(List<FileIndexItem> metaFilesInDirectory,
		bool overwriteLocationNames)
	{
		Count++;
		metaFilesInDirectory.AddRange(_fileIndexItems);
		metaFilesInDirectory = _fileIndexItems.GroupBy(i => i.FilePath)
			.Select(g => g.Last()) // get last item
			.ToList();
		return Task.FromResult(metaFilesInDirectory);
	}
}
