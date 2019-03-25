using System.Collections.Generic;
using System.IO;
using starskycore.Models;

namespace starskycore.Interfaces
{
	public interface IStorage
	{
		bool ExistFile(string path);
		bool ExistFolder(string path);
		FolderOrFileModel.FolderOrFileTypeList IsFolderOrFile(string path);
		void FolderMove(string fromPath, string toPath);
		void FileMove(string fromPath, string toPath);
		void FileCopy(string fromPath, string toPath);
		bool FileDelete(string path);

		void CreateDirectory(string path);
		
		/// <summary>
		/// Returns a list of Files in a directory (non-Recursive)
		/// to filter use:
		/// ..etAllFilesInDirectory(subPath)
		///	.Where(ExtensionRolesHelper.IsExtensionExifToolSupported)
		/// </summary>
		/// <param name="path">path relative to the database</param>
		/// <returns></returns>
		IEnumerable<string> GetAllFilesInDirectory(string path);
		IEnumerable<string> GetAllFilesInDirectoryRecursive(string path);
		
		/// <summary>
		/// Returns a list of directories // Get list of child folders
		/// old name: GetFilesRecursive
		/// </summary>
		/// <param name="path">directory</param>
		/// <returns>list</returns>
		IEnumerable<string> GetDirectoryRecursive(string path);

		Stream ReadStream(string path, int maxRead = -1);
		
		bool WriteStream(Stream stream, string path);
		
		/// <summary>
		/// Check if thumbnail exist
		/// </summary>
		/// <param name="fileHash">base32 filehash</param>
		/// <returns>if exist=true</returns>
		bool ExistThumbnail(string fileHash);
		Stream ReadThumbnail(string fileHash);
		bool WriteThumbnailStream(Stream stream, string fileHash);
		
	}
}
