using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using starsky.feature.trash.Interfaces;
using starsky.foundation.database.Models;

namespace starskytest.FakeMocks;

public class FakeIMoveToTrashService : IMoveToTrashService
{
	public FakeIMoveToTrashService(List<FileIndexItem> fileIndexItems, bool detectToUseSystemTrash = true)
	{
		FileIndexItems = fileIndexItems;
		DetectToUseSystemTrashToggle = detectToUseSystemTrash;
	}

	public bool DetectToUseSystemTrashToggle { get; set; }

	public List<FileIndexItem> FileIndexItems { get; set; }

	public Task<List<FileIndexItem>> MoveToTrashAsync(List<string> inputFilePaths, bool collections)
	{
		var result = new List<FileIndexItem>();
		foreach (var inputFilePath in inputFilePaths)
		{
			var exifStatus = FileIndexItems
				        .Find(p => p.FilePath == inputFilePath)
				        ?.Status ??
			        FileIndexItem.ExifStatus.NotFoundSourceMissing;
			result.Add(new FileIndexItem{FilePath = inputFilePath, Status = exifStatus});
		}
		return Task.FromResult(result);
	}

	public bool DetectToUseSystemTrash()
	{
		return DetectToUseSystemTrashToggle;
	}

	public bool IsEnabled()
	{
		return DetectToUseSystemTrashToggle;
	}
}
