using starskycore.Models;

namespace starskycore.Interfaces
{
	public interface IStorage
	{
		bool ExistFile(string subPath);
		bool ExistFolder(string subPath);
		FolderOrFileModel.FolderOrFileTypeList IsFolderOrFile(string fullFilePath = "");
		void DirectoryMove(string inputSubPath, string toSubPath);
		void FileMove(string inputSubPath, string toSubPath);

	}
}
