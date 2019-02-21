using System.Collections.Generic;
using starskycore.Models;

namespace starskycore.Interfaces
{
	public interface IStorage
	{
		bool ExistFile(string subPath);
		bool ExistFolder(string subPath);
		FolderOrFileModel.FolderOrFileTypeList IsFolderOrFile(string subPath = "");
		void FolderMove(string inputSubPath, string toSubPath);
		void FileMove(string inputSubPath, string toSubPath);
		void CreateDirectory(string subPath);
		IEnumerable<string> GetAllFilesInDirectory(string subPath);

	}
}
