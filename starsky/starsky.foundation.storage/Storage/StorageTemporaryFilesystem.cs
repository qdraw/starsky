using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Models;

namespace starsky.foundation.storage.Storage;

[Service(typeof(IStorage), InjectionLifetime = InjectionLifetime.Scoped)]
public sealed class StorageTemporaryFilesystem : IStorage
{
	private readonly AppSettings _appSettings;
	private readonly IWebLogger _logger;

	public StorageTemporaryFilesystem(AppSettings appSettings, IWebLogger logger)
	{
		_appSettings = appSettings;
		_logger = logger;
	}

	/// <summary>
	///     Checks if a file is ready
	/// </summary>
	/// <param name="path"></param>
	/// <returns></returns>
	public bool IsFileReady(string path)
	{
		var fullPath = _appSettings.DatabasePathToTempFolderFilePath(path);
		return new StorageHostFullPathFilesystem(_logger).IsFileReady(fullPath);
	}

	/// <summary>
	///     Get the storage info
	/// </summary>
	/// <param name="path">full File Path</param>
	/// <returns>StorageInfo object</returns>
	public StorageInfo Info(string path)
	{
		var fullPath = _appSettings.DatabasePathToTempFolderFilePath(path);
		return new StorageHostFullPathFilesystem(_logger).Info(fullPath);
	}

	/// <summary>
	///     Does file exist (true == exist)
	/// </summary>
	/// <param name="path">subPath</param>
	/// <returns>bool true = exist</returns>
	public bool ExistFile(string path)
	{
		if ( string.IsNullOrEmpty(path) )
		{
			return false;
		}

		var isFolderOrFile = IsFolderOrFile(path);
		return isFolderOrFile == FolderOrFileModel.FolderOrFileTypeList.File;
	}

	/// <summary>
	///     Check if folder exist
	/// </summary>
	/// <param name="path">subPath style</param>
	/// <returns>true if exist</returns>
	public bool ExistFolder(string path)
	{
		var isFolderOrFile = IsFolderOrFile(path);
		return isFolderOrFile == FolderOrFileModel.FolderOrFileTypeList.Folder;
	}

	/// <summary>
	///     is the subPath a folder or file, or deleted (FolderOrFileModel.FolderOrFileTypeList.Deleted)
	/// </summary>
	/// <param name="path">path of the database</param>
	/// <returns>is file, folder or deleted</returns>
	public FolderOrFileModel.FolderOrFileTypeList IsFolderOrFile(string path)
	{
		var fullFilePath = _appSettings.DatabasePathToTempFolderFilePath(path);
		return new StorageHostFullPathFilesystem(_logger).IsFolderOrFile(fullFilePath);
	}

	/// <summary>
	///     Move an entire folder
	/// </summary>
	/// <param name="fromPath">inputSubPath</param>
	/// <param name="toPath">toSubPath</param>
	public void FolderMove(string fromPath, string toPath)
	{
		var inputFileFullPath = _appSettings.DatabasePathToTempFolderFilePath(fromPath);
		var toFileFullPath = _appSettings.DatabasePathToTempFolderFilePath(toPath);
		new StorageHostFullPathFilesystem(_logger).FolderMove(inputFileFullPath,
			toFileFullPath);
	}

	/// <summary>
	///     Move a file
	/// </summary>
	/// <param name="fromPath">inputSubPath</param>
	/// <param name="toPath">toSubPath</param>
	public bool FileMove(string fromPath, string toPath)
	{
		var inputFileFullPath = _appSettings.DatabasePathToTempFolderFilePath(fromPath);
		var toFileFullPath = _appSettings.DatabasePathToTempFolderFilePath(toPath);
		
		var hostFilesystem = new StorageHostFullPathFilesystem(_logger);
		var existOldFile = hostFilesystem.ExistFile(inputFileFullPath);
		var existNewFile = hostFilesystem.ExistFile(toFileFullPath);

		if ( !existOldFile || existNewFile )
		{
			return false;
		}
		
		return new StorageHostFullPathFilesystem(_logger).FileMove(inputFileFullPath,
			toFileFullPath);
	}

	/// <summary>
	///     Copy a single file
	/// </summary>
	/// <param name="fromPath">inputSubPath</param>
	/// <param name="toPath">toSubPath</param>
	public void FileCopy(string fromPath, string toPath)
	{
		var hostFilesystem = new StorageHostFullPathFilesystem(_logger);
		
		var inputFileFullPath = _appSettings.DatabasePathToTempFolderFilePath(fromPath);
		var toFileFullPath = _appSettings.DatabasePathToTempFolderFilePath(toPath);
		
		var existOldFile = hostFilesystem.ExistFile(inputFileFullPath);
		var existNewFile = hostFilesystem.ExistFile(toFileFullPath);

		if ( !existOldFile || existNewFile )
		{
			return;
		}
		
		new StorageHostFullPathFilesystem(_logger).FileCopy(inputFileFullPath, toFileFullPath);
	}

	/// <summary>
	///     Delete a file
	/// </summary>
	/// <param name="path">subPath</param>
	/// <returns>bool</returns>
	public bool FileDelete(string path)
	{
		var inputFileFullPath = _appSettings.DatabasePathToTempFolderFilePath(path);
		return new StorageHostFullPathFilesystem(_logger).FileDelete(inputFileFullPath);
	}

	/// <summary>
	///     Create a Directory
	/// </summary>
	/// <param name="path">subPath location</param>
	public void CreateDirectory(string path)
	{
		var inputFileFullPath = _appSettings.DatabasePathToTempFolderFilePath(path);
		Directory.CreateDirectory(inputFileFullPath);
	}

	/// <summary>
	///     Delete folder and child items of that folder
	/// </summary>
	/// <param name="path">subPath</param>
	/// <returns>bool</returns>
	public bool FolderDelete(string path)
	{
		var inputFileFullPath = _appSettings.DatabasePathToTempFolderFilePath(path);
		return new StorageHostFullPathFilesystem(_logger).FolderDelete(inputFileFullPath);
	}

	/// <summary>
	///     Returns a list of Files in a directory (non-Recursive)
	///     to filter use:
	///     ..etAllFilesInDirectory(subPath)
	///     .Where(ExtensionRolesHelper.IsExtensionExifToolSupported)
	/// </summary>
	/// <param name="path">subPath path relative to the database</param>
	/// <returns></returns>
	public IEnumerable<string> GetAllFilesInDirectory(string path)
	{
		var storage = new StorageHostFullPathFilesystem(_logger);
		var fullFilePath = _appSettings.DatabasePathToTempFolderFilePath(path);

		if ( !storage.ExistFolder(fullFilePath) )
		{
			return [];
		}

		var imageFilesList = storage.GetAllFilesInDirectory(fullFilePath);

		// to filter use:
		// ..etAllFilesInDirectory(subPath)
		//	.Where(ExtensionRolesHelper.IsExtensionExifToolSupported)

		// convert back to subPath style
		return _appSettings.RenameListItemsToDbStyle(imageFilesList.ToList());
	}


	/// <summary>
	///     Returns a list of Files in a directory (Recursive)
	///     to filter use:
	///     ..etAllFilesInDirectory(subPath)
	///     .Where(ExtensionRolesHelper.IsExtensionExifToolSupported)
	/// </summary>
	/// <param name="path">subPath, path relative to the database</param>
	/// <returns>list of files</returns>
	public IEnumerable<string> GetAllFilesInDirectoryRecursive(string path)
	{
		var fullFilePath = _appSettings.DatabasePathToTempFolderFilePath(path);
		var storage = new StorageHostFullPathFilesystem(_logger);

		if ( !storage.ExistFolder(fullFilePath) )
		{
			return [];
		}

		var imageFilesList = storage.GetAllFilesInDirectoryRecursive(fullFilePath);

		// to filter use:
		// ...etAllFilesInDirectory(subPath)
		//	.Where(ExtensionRolesHelper.IsExtensionExifToolSupported)
		// OR:
		//  .Where(ExtensionRolesHelper.IsExtensionSyncSupported)

		// convert back to subPath style
		return _appSettings.RenameTempListItemsToDbStyle(imageFilesList.ToList());
	}

	/// <summary>
	///     Gets a non-Recursive list of child directories
	/// </summary>
	/// <param name="path">subPath</param>
	/// <returns>list of Directories (example: /2020_01_01/test)</returns>
	public IEnumerable<string> GetDirectories(string path)
	{
		var fullFilePath = _appSettings.DatabasePathToTempFolderFilePath(path);
		var storage = new StorageHostFullPathFilesystem(_logger);

		if ( !storage.ExistFolder(fullFilePath) )
		{
			return [];
		}

		var folders = storage.GetDirectories(fullFilePath);

		// Used For subfolders
		// convert back to subPath style
		return _appSettings.RenameListItemsToDbStyle(folders.ToList());
	}

	/// <summary>
	///     Returns a list of directories // Get list of child folders
	/// </summary>
	/// <param name="path">subPath in dir</param>
	/// <returns>list of paths</returns>
	public IEnumerable<KeyValuePair<string, DateTime>> GetDirectoryRecursive(string path)
	{
		var storage = new StorageHostFullPathFilesystem(_logger);

		var fullFilePath = _appSettings.DatabasePathToTempFolderFilePath(path);
		if ( !storage.ExistFolder(fullFilePath) )
		{
			return [];
		}

		var folders = storage.GetDirectoryRecursive(fullFilePath);

		// Used For subfolders
		// convert back to subPath style
		return _appSettings.RenameListItemsToDbStyle(folders.ToList());
	}

	/// <summary>
	///     Get the file using a stream (don't forget to dispose this)
	/// </summary>
	/// <param name="path">subPath</param>
	/// <param name="maxRead">number of bytes to read (default -1 = all)</param>
	/// <returns>FileStream or Stream.Null when file dont exist</returns>
	public Stream ReadStream(string path, int maxRead = -1)
	{
		if ( !ExistFile(path) )
		{
			return Stream.Null;
		}

		if ( _appSettings.IsVerbose() )
		{
			Console.WriteLine(path);
		}

		return new RetryStream().Retry(LocalGet);

		Stream LocalGet()
		{
			var fullFilePath = _appSettings.DatabasePathToTempFolderFilePath(path);
			var fileStream = new FileStream(fullFilePath, FileMode.Open, FileAccess.Read);
			if ( maxRead <= 1 )
			{
				// read all
				return fileStream;
			}

			// read only the first number of bytes
			var buffer = new byte[maxRead];
			var actualRead = fileStream.Read(buffer, 0, maxRead);
			if ( actualRead != maxRead )
			{
				_logger.LogDebug("[TempSubPathFileSystem] ReadStream: actualRead != maxRead");
			}

			fileStream.Flush();
			fileStream.Close();
			fileStream.Dispose(); // also flush
			return new MemoryStream(buffer);
		}
	}

	/// <summary>
	///     Write fileStream to disk
	/// </summary>
	/// <param name="stream">some stream</param>
	/// <param name="path">location</param>
	/// <returns></returns>
	/// <exception cref="FileNotFoundException"></exception>
	public bool WriteStream(Stream stream, string path)
	{
		// should be able to write files that are not exist yet			
		var fullFilePath = _appSettings.DatabasePathToTempFolderFilePath(path);

		return new StorageHostFullPathFilesystem(_logger).WriteStream(stream, fullFilePath);
	}

	public bool WriteStreamOpenOrCreate(Stream stream, string path)
	{
		throw new NotSupportedException();
	}

	public Task<bool> WriteStreamAsync(Stream stream, string path)
	{
		var fullFilePath = _appSettings.DatabasePathToTempFolderFilePath(path);
		var service = new StorageHostFullPathFilesystem(_logger);
		return service.WriteStreamAsync(stream, fullFilePath);
	}
}
