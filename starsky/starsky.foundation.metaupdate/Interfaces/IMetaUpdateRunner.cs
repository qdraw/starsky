using starsky.foundation.database.Models;

namespace starsky.foundation.metaupdate.Interfaces;

public interface IMetaUpdateRunner
{
	/// <summary>
	///     Check the input, queue the update of the files and push the change to the clients
	/// </summary>
	/// <param name="inputModel">tags, colorClass, description, title</param>
	/// <param name="inputFilePaths">subPaths of the files</param>
	/// <param name="append">only for strings, add update to existing items</param>
	/// <param name="collections">update files with the same name before the extension</param>
	/// <param name="rotateClock">relative orientation -1 or 1</param>
	Task<MetaUpdateRunResult> UpdateAsync(FileIndexItem inputModel,
		string[] inputFilePaths, bool append, bool collections, int rotateClock);
}

/// <summary>
///     Result of <see cref="IMetaUpdateRunner.UpdateAsync" />
/// </summary>
/// <param name="Results">status of every input file, as checked before the update</param>
/// <param name="Updated">the updated items to show, null when none of the files was found</param>
public sealed record MetaUpdateRunResult(
	List<FileIndexItem> Results,
	List<FileIndexItem>? Updated)
{
	public bool AllNotFound => Updated == null;
}
