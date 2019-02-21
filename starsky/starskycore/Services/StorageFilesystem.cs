using System.Collections.Generic;
using System.IO;
using System.Linq;
using starskycore.Helpers;
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
		
		/// <summary>
		/// Returns a list of Files in a directory (non-recruisive)
		/// </summary>
		/// <param name="subPath">path relative to the database</param>
		/// <returns></returns>
		public IEnumerable<string> GetAllFilesInDirectory(string subPath)
		{
			var fullFilePath = _appSettings.DatabasePathToFilePath(subPath);
			if (fullFilePath == null) return Enumerable.Empty<string>();

			string[] allFiles = Directory.GetFiles(fullFilePath);

			var imageFilesList = new List<string>();
			foreach (var file in allFiles)
			{
				// Path.GetExtension uses (.ext)
				// the same check in SingleFile
				// Recruisive >= same check
				// ignore Files with ._ names, this is Mac OS specific
				var isAppleDouble = Path.GetFileName(file).StartsWith("._");
				if (!isAppleDouble)
				{
					imageFilesList.Add(file);
				}
			}

			// to filter use:
			// ..etAllFilesInDirectory(subPath)
			//	.Where(ExtensionRolesHelper.IsExtensionExifToolSupported)
			
			return imageFilesList;
		}
		
		/// <summary>
		/// Returns a list of directories // Get list of child folders
		/// </summary>
		/// <param name="subPath">subpath in directory</param>
		/// <returns></returns>
		public IEnumerable<string> GetDirectoryRecursive(string subPath)
		{
			var fullFilePath = _appSettings.DatabasePathToFilePath(subPath);
			if (fullFilePath == null) return Enumerable.Empty<string>();
			
			string[] folders = Directory.GetDirectories(fullFilePath, "*", SearchOption.AllDirectories);
			// Used For subfolders

			return folders;
		}
		
		
		

	}
}
