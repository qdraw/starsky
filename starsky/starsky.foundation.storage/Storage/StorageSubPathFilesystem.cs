using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using starsky.foundation.injection;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Models;
using starskycore.Interfaces;
using starskycore.Models;

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
		/// Does file exist (true == exist)
		/// </summary>
		/// <param name="subPath">full file path</param>
		/// <returns>bool true = exist</returns>
		[Obsolete("do not include direct, only using ISelectorStorage")]
		public bool ExistFile(string subPath)
		{
			var isFolderOrFile = IsFolderOrFile(subPath);
			return isFolderOrFile == FolderOrFileModel.FolderOrFileTypeList.File;
		}

		[Obsolete("do not include direct, only using ISelectorStorage")]
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
		[Obsolete("do not include direct, only using ISelectorStorage")]
		public FolderOrFileModel.FolderOrFileTypeList IsFolderOrFile(string subPath)
		{
			var fullFilePath = _appSettings.DatabasePathToFilePath(subPath,false);
			return new StorageHostFullPathFilesystem().IsFolderOrFile(fullFilePath);
		}

		[Obsolete("do not include direct, only using ISelectorStorage")]
		public void FolderMove(string inputSubPath, string toSubPath)
		{
			var inputFileFullPath = _appSettings.DatabasePathToFilePath(inputSubPath, false);
			var toFileFullPath = _appSettings.DatabasePathToFilePath(toSubPath, false);
			new StorageHostFullPathFilesystem().FolderMove(inputFileFullPath,toFileFullPath);
		}

		[Obsolete("do not include direct, only using ISelectorStorage")]
		public void FileMove(string inputSubPath, string toSubPath)
		{
			var inputFileFullPath = _appSettings.DatabasePathToFilePath(inputSubPath, false);
			var toFileFullPath = _appSettings.DatabasePathToFilePath(toSubPath, false);
			new StorageHostFullPathFilesystem().FileMove(inputFileFullPath,toFileFullPath);
		}
		
		[Obsolete("do not include direct, only using ISelectorStorage")]
		public void FileCopy(string inputSubPath, string toSubPath)
		{
			var inputFileFullPath = _appSettings.DatabasePathToFilePath(inputSubPath, false);
			var toFileFullPath = _appSettings.DatabasePathToFilePath(toSubPath, false);
			new StorageHostFullPathFilesystem().FileCopy(inputFileFullPath,toFileFullPath);
		}
		
		/// <summary>
		/// Check if file exist
		/// </summary>
		/// <param name="path">subPath</param>
		/// <returns>bool</returns>
		[Obsolete("do not include direct, only using ISelectorStorage")]
		public bool FileDelete(string path)
		{
			var inputFileFullPath = _appSettings.DatabasePathToFilePath(path, false);
			return new StorageHostFullPathFilesystem().FileDelete(inputFileFullPath);
		}
		
		/// <summary>
		/// Create an Directory 
		/// </summary>
		/// <param name="subPath">location</param>
		[Obsolete("do not include direct, only using ISelectorStorage")]
		public void CreateDirectory(string subPath)
		{
			var inputFileFullPath = _appSettings.DatabasePathToFilePath(subPath, false);
			Directory.CreateDirectory(inputFileFullPath);
		}
		
		/// <summary>
		/// Delete folder and child items of that folder
		/// </summary>
		/// <param name="path">subPath</param>
		/// <returns>bool</returns>
		[Obsolete("do not include direct, only using ISelectorStorage")]
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
		/// <param name="subPath">path relative to the database</param>
		/// <returns></returns>
		[Obsolete("do not include direct, only using ISelectorStorage")]
		public IEnumerable<string> GetAllFilesInDirectory(string subPath)
		{
			var fullFilePath = _appSettings.DatabasePathToFilePath(subPath);
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
		/// <param name="subPath">path relative to the database</param>
		/// <returns></returns>
		[Obsolete("do not include direct, only using ISelectorStorage")]
		public IEnumerable<string> GetAllFilesInDirectoryRecursive(string subPath)
		{
			var fullFilePath = _appSettings.DatabasePathToFilePath(subPath);
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
		/// Returns a list of directories // Get list of child folders
		/// </summary>
		/// <param name="subPath">subpath in directory</param>
		/// <returns></returns>
		[Obsolete("do not include direct, only using ISelectorStorage")]
		public IEnumerable<string> GetDirectoryRecursive(string subPath)
		{
			var fullFilePath = _appSettings.DatabasePathToFilePath(subPath);
			if (fullFilePath == null) return Enumerable.Empty<string>();
			
			var folders = new StorageHostFullPathFilesystem().GetDirectoryRecursive(fullFilePath);

			// Used For subfolders
			// convert back to subpath style
			return _appSettings.RenameListItemsToDbStyle(folders.ToList());
		}

		/// <summary>
		/// Get the file using a stream (don't forget to dispose this)
		/// </summary>
		/// <param name="path">subPath</param>
		/// <param name="maxRead">number of bytes to read (default -1 = all)</param>
		/// <returns>FileStream</returns>
		/// <exception cref="FileNotFoundException">is file not exist, please check that first</exception>
		[Obsolete("do not include direct, only using ISelectorStorage")]
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
					FileShare.Read, maxRead, false);
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
		[Obsolete("do not include direct, only using ISelectorStorage")]
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
