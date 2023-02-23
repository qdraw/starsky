using starsky.foundation.database.Models;

namespace starsky.feature.trash.Interfaces;

public interface IMoveToTrashService
{
	Task<List<FileIndexItem>> MoveToTrashAsync(string[] inputFilePaths,
		bool collections);
}
