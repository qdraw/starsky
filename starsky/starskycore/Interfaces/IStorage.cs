using System.Collections.Generic;
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
			
	}
}
