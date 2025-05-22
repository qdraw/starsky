using starsky.foundation.database.Models;

namespace starsky.foundation.metaupdate.Interfaces
{
	public interface IMetaPreflight
	{
		Task<(List<FileIndexItem> fileIndexResultsList, Dictionary<string, List<string>> changedFileIndexItemName)>
			PreflightAsync(FileIndexItem? inputModel, List<string> inputFilePaths,
				bool append,
				bool collections, int rotateClock);
	}
}
