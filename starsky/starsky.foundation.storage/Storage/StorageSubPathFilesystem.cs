using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using starsky.foundation.injection;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Models;

namespace starsky.foundation.storage.Storage
{
	[Service(typeof(IStorage), InjectionLifetime = InjectionLifetime.Scoped)]
	public class StorageSubPathFilesystem : IStorage
	{
		private readonly AppSettings _appSettings;

		public StorageSubPathFilesystem(AppSettings appSettings)
		{
			_appSettings = appSettings;
		}

		/// <summary>
		/// Get the storage info
		/// </summary>
		/// <param name="path">full File Path</param>
		/// <returns>StorageInfo object</returns>
		public StorageInfo Info(string path)
		{
			var subPath = _appSettings.DatabasePathToFilePath(path, false);
			
			return new StorageHostFullPathFilesystem().Info(subPath);
		}

		/// <summary>
		/// Does file exist (true == exist)
		/// </summary>
		/// <param name="path">subPath</param>
		/// <returns>bool true = exist</returns>
		public bool ExistFile(string path)
		{
			if ( string.IsNullOrEmpty(path) ) return false;
			var isFolderOrFile = IsFolderOrFile(path);
			return isFolderOrFile == FolderOrFileModel.FolderOrFileTypeList.File;
		}

		/// <summary>
		/// Check if folder exist
		/// </summary>
		/// <param name="path">subPath style</param>
		/// <returns>true if exist</returns>
		public bool ExistFolder(string path)
		{
			var isFolderOrFile = IsFolderOrFile(path);
			return isFolderOrFile == FolderOrFileModel.FolderOrFileTypeList.Folder;
		}

		/// <summary>
		/// is the subPath a folder or file, or deleted (FolderOrFileModel.FolderOrFileTypeList.Deleted)
		/// </summary>
		/// <param name="path">path of the database</param>
		/// <returns>is file, folder or deleted</returns>
		public FolderOrFileModel.FolderOrFileTypeList IsFolderOrFile(string path)
		{
			var fullFilePath = _appSettings.DatabasePathToFilePath(path,false);
			return new StorageHostFullPathFilesystem().IsFolderOrFile(fullFilePath);
		}

		/// <summary>
		/// Move a entire folder
		/// </summary>
		/// <param name="fromPath">inputSubPath</param>
		/// <param name="toPath">toSubPath</param>
		public void FolderMove(string fromPath, string toPath)
		{
			var inputFileFullPath = _appSettings.DatabasePathToFilePath(fromPath, false);
			var toFileFullPath = _appSettings.DatabasePathToFilePath(toPath, false);
			new StorageHostFullPathFilesystem().FolderMove(inputFileFullPath,toFileFullPath);
		}

		/// <summary>
		/// Move a file
		/// </summary>
		/// <param name="fromPath">inputSubPath</param>
		/// <param name="toPath">toSubPath</param>
		public void FileMove(string fromPath, string toPath)
		{
			var inputFileFullPath = _appSettings.DatabasePathToFilePath(fromPath, false);
			var toFileFullPath = _appSettings.DatabasePathToFilePath(toPath, false);
			new StorageHostFullPathFilesystem().FileMove(inputFileFullPath,toFileFullPath);
		}
		
		/// <summary>
		/// Copy a single file
		/// </summary>
		/// <param name="fromPath">inputSubPath</param>
		/// <param name="toPath">toSubPath</param>
		public void FileCopy(string fromPath, string toPath)
		{
			var inputFileFullPath = _appSettings.DatabasePathToFilePath(fromPath, false);
			var toFileFullPath = _appSettings.DatabasePathToFilePath(toPath, false);
			new StorageHostFullPathFilesystem().FileCopy(inputFileFullPath,toFileFullPath);
		}
		
		/// <summary>
		/// Delete a file
		/// </summary>
		/// <param name="path">subPath</param>
		/// <returns>bool</returns>
		public bool FileDelete(string path)
		{
			var inputFileFullPath = _appSettings.DatabasePathToFilePath(path, false);
			return new StorageHostFullPathFilesystem().FileDelete(inputFileFullPath);
		}
		
		/// <summary>
		/// Create an Directory 
		/// </summary>
		/// <param name="path">subPath location</param>
		public void CreateDirectory(string path)
		{
			var inputFileFullPath = _appSettings.DatabasePathToFilePath(path, false);
			Directory.CreateDirectory(inputFileFullPath);
		}
		
		/// <summary>
		/// Delete folder and child items of that folder
		/// </summary>
		/// <param name="path">subPath</param>
		/// <returns>bool</returns>
		public bool FolderDelete(string path)
		{
			var inputFileFullPath = _appSettings.DatabasePathToFilePath(path, false);
			return new StorageHostFullPathFilesystem().FolderDelete(inputFileFullPath);
		}
		
		/// <summary>
		/// Returns a list of Files in a directory (non-Recursive)
		/// to filter use:
		/// ..etAllFilesInDirectory(subPath)
		///	.Where(ExtensionRolesHelper.IsExtensionExifToolSupported)
		/// </summary>
		/// <param name="path">subPath path relative to the database</param>
		/// <returns></returns>
		public IEnumerable<string> GetAllFilesInDirectory(string path)
		{
			var fullFilePath = _appSettings.DatabasePathToFilePath(path);
			if (fullFilePath == null) return Enumerable.Empty<string>();

			var imageFilesList = new StorageHostFullPathFilesystem().GetAllFilesInDirectory(fullFilePath);

			// to filter use:
			// ..etAllFilesInDirectory(subPath)
			//	.Where(ExtensionRolesHelper.IsExtensionExifToolSupported)
			
			// convert back to subPath style
			return _appSettings.RenameListItemsToDbStyle(imageFilesList.ToList());
		}
		
		
		/// <summary>
		/// Returns a list of Files in a directory (Recursive)
		/// to filter use:
		/// ..etAllFilesInDirectory(subPath)
		///	.Where(ExtensionRolesHelper.IsExtensionExifToolSupported)
		/// </summary>
		/// <param name="path">subPath, path relative to the database</param>
		/// <returns>list of files</returns>
		public IEnumerable<string> GetAllFilesInDirectoryRecursive(string path)
		{
			var fullFilePath = _appSettings.DatabasePathToFilePath(path);
			if (fullFilePath == null) return Enumerable.Empty<string>();

			var imageFilesList = new StorageHostFullPathFilesystem().GetAllFilesInDirectoryRecursive(fullFilePath);

			// to filter use:
			// ..etAllFilesInDirectory(subPath)
			//	.Where(ExtensionRolesHelper.IsExtensionExifToolSupported)
			// OR:
			//  .Where(ExtensionRolesHelper.IsExtensionSyncSupported)

			// convert back to subPath style
			return _appSettings.RenameListItemsToDbStyle(imageFilesList.ToList());
		}

		/// <summary>
		/// Gets a non-Recursive list of child directories
		/// </summary>
		/// <param name="path">subPath</param>
		/// <returns>list of Directories (example: /2020_01_01/test)</returns>
		public IEnumerable<string> GetDirectories(string path)
		{
			var fullFilePath = _appSettings.DatabasePathToFilePath(path);
			if (fullFilePath == null) return Enumerable.Empty<string>();
			
			var folders = new StorageHostFullPathFilesystem().GetDirectories(fullFilePath);
			// Used For subfolders
			// convert back to subPath style
			return _appSettings.RenameListItemsToDbStyle(folders.ToList());
		}

		/// <summary>
		/// Returns a list of directories // Get list of child folders
		/// </summary>
		/// <param name="path">subPath in directory</param>
		/// <returns></returns>
		[Obsolete("do not include direct, only using ISelectorStorage")]
		public IEnumerable<string> GetDirectoryRecursive(string path)
		{
			var fullFilePath = _appSettings.DatabasePathToFilePath(path);
			if (fullFilePath == null) return Enumerable.Empty<string>();
			
			var folders = new StorageHostFullPathFilesystem().GetDirectoryRecursive(fullFilePath);

			// Used For subfolders
			// convert back to subPath style
			return _appSettings.RenameListItemsToDbStyle(folders.ToList());
		}

		/// <summary>
		/// Get the file using a stream (don't forget to dispose this)
		/// </summary>
		/// <param name="path">subPath</param>
		/// <param name="maxRead">number of bytes to read (default -1 = all)</param>
		/// <returns>FileStream</returns>
		/// <exception cref="FileNotFoundException">is file not exist, please check that first</exception>
		public Stream ReadStream(string path, int maxRead = -1)
		{
			if ( ! ExistFile(path) ) throw new FileNotFoundException(path);
			
			var fullFilePath = _appSettings.DatabasePathToFilePath(path);

			if ( _appSettings.Verbose ) Console.WriteLine(path);
				
			FileStream fileStream;
			if ( maxRead <= 1 )
			{
				fileStream = new FileStream(fullFilePath, FileMode.Open, FileAccess.Read);
			}
			else
			{
				fileStream = new FileStream(fullFilePath, FileMode.Open, FileAccess.Read,
					FileShare.Read, maxRead);
			}

			return fileStream;
		}
		
		
		/// <summary>
		/// Write fileStream to disk
		/// </summary>
		/// <param name="stream">some stream</param>
		/// <param name="path">location</param>
		/// <returns></returns>
		/// <exception cref="FileNotFoundException"></exception>
		public bool WriteStream(Stream stream, string path)
		{
			// should be able to write files that are not exist yet			
			var fullFilePath = _appSettings.DatabasePathToFilePath(path,false);

			return new StorageHostFullPathFilesystem().WriteStream(stream, fullFilePath);
		}

		public Task<bool> WriteStreamAsync(Stream stream, string path)
		{
			var fullFilePath = _appSettings.DatabasePathToFilePath(path,false);
			return new StorageHostFullPathFilesystem().WriteStreamAsync(stream, fullFilePath);
		}
	}
}
