namespace starsky.foundation.storage.Models;

public sealed class FolderOrFileModel
{
	/// <summary>
	///     Enum FolderOrFileTypeList
	/// </summary>
	public enum FolderOrFileTypeList
	{
		Folder = 1,
		File = 2,
		Deleted = 0
	}

	/// <summary>
	///     To Store output if file exist, folder or deleted
	/// </summary>
	public FolderOrFileTypeList IsFolderOrFile { get; set; }
}
