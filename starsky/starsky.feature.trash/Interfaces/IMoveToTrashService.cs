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
	Task<List<FileIndexItem>> MoveToTrashAsync(string[] inputFilePaths,
		bool collections);
	
	/// <summary>
	/// Is it supported to use the system trash
	/// But it does NOT check if the feature toggle is enabled
	/// </summary>
	/// <returns>true if supported</returns>
	bool DetectToUseSystemTrash();
}
