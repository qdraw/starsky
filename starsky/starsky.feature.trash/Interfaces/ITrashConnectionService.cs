using starsky.foundation.database.Models;

namespace starsky.feature.trash.Interfaces;

public interface ITrashConnectionService
{
	Task<List<FileIndexItem>> ConnectionServiceAsync(List<FileIndexItem> moveToTrash,
		bool isSystemTrash);
}
