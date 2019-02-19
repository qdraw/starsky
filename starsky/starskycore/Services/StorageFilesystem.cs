using System.IO;
using starskycore.Interfaces;
using starskycore.Models;

namespace starskycore.Services
{
	public class StorageFilesystem : IStorage
	{
		private AppSettings _appSettings;

		public StorageFilesystem(AppSettings appSettings)
		{
			_appSettings = appSettings;
		}
		
		/// <summary>
		/// Does file exist (true == exist)
		/// </summary>
		/// <param name="subPath">full file path</param>
		/// <returns>bool true = exist</returns>
		public bool ExistFile(string subPath)
		{
			var isFolderOrFile = IsFolderOrFile(subPath);
			return isFolderOrFile == FolderOrFileModel.FolderOrFileTypeList.File;
		}

		public bool ExistFolder(string subPath)
		{
			var isFolderOrFile = IsFolderOrFile(subPath);
			return isFolderOrFile == FolderOrFileModel.FolderOrFileTypeList.Folder;
		}

		/// <summary>
		/// is the subpath a folder or file, or deleted (FolderOrFileModel.FolderOrFileTypeList.Deleted)
		/// </summary>
		/// <param name="subPath">path of the database</param>
		/// <returns>is file, folder or deleted</returns>
		public FolderOrFileModel.FolderOrFileTypeList IsFolderOrFile(string subPath)
		{
			var fullFilePath = _appSettings.DatabasePathToFilePath(subPath,false);
			
			if (!Directory.Exists(fullFilePath) && File.Exists(fullFilePath))
			{
				// file
				return FolderOrFileModel.FolderOrFileTypeList.File;
			}

			if (!File.Exists(fullFilePath) && Directory.Exists(fullFilePath))
			{
				// Directory
				return FolderOrFileModel.FolderOrFileTypeList.Folder;
			}

			return FolderOrFileModel.FolderOrFileTypeList.Deleted;
		}

		public void FolderMove(string inputSubPath, string toSubPath)
		{
			var inputFileFullPath = _appSettings.DatabasePathToFilePath(inputSubPath, false);
			var toFileFullPath = _appSettings.DatabasePathToFilePath(toSubPath, false);
			Directory.Move(inputFileFullPath,toFileFullPath);
		}

		public void FileMove(string inputSubPath, string toSubPath)
		{
			var inputFileFullPath = _appSettings.DatabasePathToFilePath(inputSubPath, false);
			var toFileFullPath = _appSettings.DatabasePathToFilePath(toSubPath, false);
			File.Move(inputFileFullPath,toFileFullPath);
		}

		public void CreateDirectory(string subPath)
		{
			var inputFileFullPath = _appSettings.DatabasePathToFilePath(subPath, false);
			Directory.CreateDirectory(inputFileFullPath);
		}
		
	}
}
