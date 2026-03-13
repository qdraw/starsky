using starsky.feature.trash.Services;
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
	Task<List<FileIndexItem>> CreateEvent(List<string> inputFilePaths,
		bool collections);

	/// <summary>
	/// Is supported and enabled in the feature toggle
	/// </summary>
	/// <returns>Should you use it?</returns>
	bool IsEnabled();

	/// <summary>
	/// Is it supported to use the system Trash
	/// It does NOT check if the feature toggle is enabled
	/// Used by End2End test to enable system trash if supported on the platform
	/// </summary>
	/// <returns>true if supported</returns>
	public bool DetectToUseSystemTrash();

	/// <summary>
	/// Moves the specified list of files to the trash asynchronously.
	/// </summary>
	/// <param name="payload">Payload</param>
	/// <returns>A task representing the asynchronous operation.</returns>
	Task MoveToTrashAsync(MoveToTrashPayload payload);
}
