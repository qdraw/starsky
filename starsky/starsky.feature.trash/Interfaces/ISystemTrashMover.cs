using starsky.foundation.database.Models;

namespace starsky.feature.trash.Interfaces;

/// <summary>
///     The system trash (Recycle Bin, Finder trash) of the files of a tenant
/// </summary>
public interface ISystemTrashMover
{
	/// <summary>
	///     Is supported and enabled in the feature toggle
	/// </summary>
	bool IsEnabled();

	/// <summary>
	///     Is it supported on this platform, does NOT check the feature toggle
	/// </summary>
	bool DetectToUseSystemTrash();

	/// <summary>
	///     Move the files to the system trash
	/// </summary>
	/// <param name="moveToTrash">items with a database style FilePath, in the storage of the tenant</param>
	void Trash(IEnumerable<FileIndexItem> moveToTrash);
}
