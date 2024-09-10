using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using starsky.foundation.storage.Models;

namespace starsky.foundation.storage.Interfaces;

public interface IStorage
{
	bool ExistFile(string path);
	bool ExistFolder(string path);
	FolderOrFileModel.FolderOrFileTypeList IsFolderOrFile(string path);
	void FolderMove(string fromPath, string toPath);
	bool FileMove(string fromPath, string toPath);
	void FileCopy(string fromPath, string toPath);
	bool FileDelete(string path);

	void CreateDirectory(string path);
	bool FolderDelete(string path);

	/// <summary>
	/// Returns a list of Files in a directory (non-Recursive)
	/// to filter use:
	/// ..etAllFilesInDirectory(subPath)
	///	.Where(ExtensionRolesHelper.IsExtensionExifToolSupported)
	/// </summary>
	/// <param name="path">filePath</param>
	/// <returns></returns>
	IEnumerable<string> GetAllFilesInDirectory(string path);

	/// <summary>
	/// Returns a list of Files in a directory (Recursive)
	/// to filter use:
	/// ..etAllFilesInDirectory(subPath)
	///	.Where(ExtensionRolesHelper.IsExtensionExifToolSupported)
	/// </summary>
	/// <param name="path">subPath, path relative to the database</param>
	/// <returns>list of files</returns>
	IEnumerable<string> GetAllFilesInDirectoryRecursive(string path);

	/// <summary>
	/// Returns a NON-Recursive list of child directories
	/// </summary>
	/// <param name="path">filePath</param>
	/// <returns>list of NON-Recursive child directories</returns>
	IEnumerable<string> GetDirectories(string path);

	/// <summary>
	/// Returns a list of directories // Get list of child folders
	/// old name: GetFilesRecursive
	/// </summary>
	/// <param name="path">directory</param>
	/// <returns>list</returns>
	IEnumerable<KeyValuePair<string, DateTime>> GetDirectoryRecursive(string path);

	/// <summary>
	/// Read Stream (and keep open)
	/// </summary>
	/// <param name="path">location</param>
	/// <param name="maxRead">how many bytes are read (default all or -1)</param>
	/// <returns>Stream with data (non-disposed)</returns>
	Stream ReadStream(string path, int maxRead = -1);

	/// <summary>
	/// Write and Dispose / Flush afterwards
	/// </summary>
	/// <param name="stream">stream</param>
	/// <param name="path">where to write to</param>
	/// <returns>is Success</returns>
	bool WriteStream(Stream stream, string path);

	/// <summary>
	/// Append To Open Stream
	/// </summary>
	/// <param name="stream">what to append</param>
	/// <param name="path">location</param>
	/// <returns></returns>
	bool WriteStreamOpenOrCreate(Stream stream, string path);

	/// <summary>
	/// Write and Dispose / Flush afterwards
	/// </summary>
	/// <param name="stream">stream</param>
	/// <param name="path">where to write to</param>
	/// <returns>is Success</returns>
	Task<bool> WriteStreamAsync(Stream stream, string path);

	StorageInfo Info(string path);

	bool IsFileReady(string path);
}
