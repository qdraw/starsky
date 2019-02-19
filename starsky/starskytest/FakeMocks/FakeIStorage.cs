using starskycore.Interfaces;
using starskycore.Models;
using starskytest.FakeCreateAn;

namespace starskytest.FakeMocks
{
	class FakeIStorage : IStorage
	{
		private readonly bool _existFile;
		private readonly bool _existFolder;
		private readonly FolderOrFileModel.FolderOrFileTypeList _isFolderOrFile;


		public FakeIStorage(bool existFile = false, bool existFolder = false, FolderOrFileModel.FolderOrFileTypeList isFolderOrFile = FolderOrFileModel.FolderOrFileTypeList.Deleted)
		{
			_existFile = existFile;
			_existFolder = existFolder;
			_isFolderOrFile = isFolderOrFile;
		}
		public bool ExistFile(string subPath)
		{
			if ( subPath == new CreateAnImage().DbPath )
			{
				return true;
			}
			return _existFile;
		}

		public bool ExistFolder(string subPath)
		{
			return _existFolder;
		}

		public FolderOrFileModel.FolderOrFileTypeList IsFolderOrFile(string subPath = "")
		{
			if ( subPath == new CreateAnImage().DbPath )
			{
				return FolderOrFileModel.FolderOrFileTypeList.File;
			}
			return _isFolderOrFile;
		}

		public void FolderMove(string inputSubPath, string toSubPath)
		{
		}

		public void FileMove(string inputSubPath, string toSubPath)
		{
		}

		public void CreateDirectory(string subPath)
		{
		}
		
		
	}
}
