using starsky.foundation.database.Models;

namespace starsky.feature.trash.Interfaces;

public interface IMoveToTrashService
{
	/// <summary>
	/// Move a file to the internal trash or system trash
	/// Depends on the feature toggle
	/// </summary>
	/// <param name="inputFilePaths">list of paths</param>
	/// <param name="collections">is stack collections enabled</param>
	/// <returns>list of files</returns>
	Task<List<FileIndexItem>> MoveToTrashAsync(List<string> inputFilePaths,
		bool collections);

	/// <summary>
	/// Is supported and enabled in the feature toggle
	/// </summary>
	/// <returns>Should you use it?</returns>
	bool IsEnabled();
}
