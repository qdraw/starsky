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
		IEnumerable<string> GetAllFilesInDirectory(string path);
		IEnumerable<string> GetAllFilesInDirectoryRecursive(string path);
		IEnumerable<string> GetDirectoryRecursive(string path);

		Stream ReadStream(string path, int maxRead = -1);
		
		/// <summary>
		/// Check if thumbnail exist
		/// </summary>
		/// <param name="fileHash">base32 filehash</param>
		/// <returns>if exist=true</returns>
		bool ExistThumbnail(string fileHash);
		Stream Thumbnail(string fileHash);

		
	}
}
