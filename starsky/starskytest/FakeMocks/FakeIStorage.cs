using starskycore.Interfaces;
using starskycore.Models;

namespace starskytest.FakeMocks
{
	class FakeIStorage : IStorage
	{
		public bool ExistFile(string subPath)
		{
			throw new System.NotImplementedException();
		}

		public bool ExistFolder(string subPath)
		{
			throw new System.NotImplementedException();
		}

		public FolderOrFileModel.FolderOrFileTypeList IsFolderOrFile(string fullFilePath = "")
		{
			throw new System.NotImplementedException();
		}

		public void DirectoryMove(string inputSubPath, string toSubPath)
		{
			throw new System.NotImplementedException();
		}

		public void FileMove(string inputSubPath, string toSubPath)
		{
			throw new System.NotImplementedException();
		}
	}
}
