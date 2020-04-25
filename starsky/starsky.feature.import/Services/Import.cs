using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using starsky.feature.import.Interfaces;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starsky.foundation.readmeta.Interfaces;
using starsky.foundation.readmeta.Services;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Services;
using starsky.foundation.storage.Storage;
using starsky.foundation.writemeta.Interfaces;
using starskycore.Models;

namespace starsky.feature.import.Services
{
	public class Import : IImport
	{
		private readonly IImportQuery _importQuery;
		
		// storage providers
		private readonly ISelectorStorage _selectorStorage;
		private readonly IStorage _filesystemStorage;
		private readonly IStorage _subPathStorage;
		private readonly IStorage _thumbnailStorage;

		private readonly AppSettings _appSettings;

		private readonly IReadMeta _readMetaHost;

		public Import(
			ISelectorStorage selectorStorage,
			AppSettings appSettings,
			IImportQuery importQuery,
			IExifTool exifTool,
			IServiceScopeFactory scopeFactory)
		{
			_selectorStorage = selectorStorage;
			_importQuery = importQuery;
			
			_filesystemStorage = selectorStorage.Get(SelectorStorage.StorageServices.HostFilesystem);
            _subPathStorage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
            _thumbnailStorage = selectorStorage.Get(SelectorStorage.StorageServices.Thumbnail);

            _appSettings = appSettings;
            _readMetaHost = new ReadMeta(_filesystemStorage);
		}

		public async Task<List<ImportIndexItem>> Preflight(List<string> fullFilePathsList, ImportSettingsModel importSettings)
		{
			var includedDirectoryFilePaths = AppendDirectoryFilePaths(fullFilePathsList, importSettings);

			var listOfTasks = new List<Task<ImportIndexItem>>();
			foreach ( var includedFilePath in includedDirectoryFilePaths )
			{
				listOfTasks.Add(PreflightPerFile(includedFilePath, importSettings));
			}
			var items = await Task.WhenAll(listOfTasks);
			return items.ToList();
		}

		/// <summary>
		/// To Add files form directory to list
		/// </summary>
		/// <param name="fullFilePathsList">full file Path</param>
		/// <param name="importSettings">settings to add recursive</param>
		private List<KeyValuePair<string,bool>> AppendDirectoryFilePaths(List<string> fullFilePathsList, ImportSettingsModel importSettings)
		{
			var includedDirectoryFilePaths = new List<KeyValuePair<string,bool>>();
			foreach ( var fullFilePath in fullFilePathsList )
			{
				if ( _filesystemStorage.ExistFolder(fullFilePath) && importSettings.RecursiveDirectory)
				{
					// recursive
					includedDirectoryFilePaths.AddRange(_filesystemStorage.GetAllFilesInDirectoryRecursive(fullFilePath)
						.Where(ExtensionRolesHelper.IsExtensionSyncSupported)
						.Select(syncedFiles => new KeyValuePair<string, bool>(syncedFiles, true)));
					continue;
				}
				if ( _filesystemStorage.ExistFolder(fullFilePath) && !importSettings.RecursiveDirectory)
				{
					// non-recursive
					includedDirectoryFilePaths.AddRange(_filesystemStorage.GetAllFilesInDirectory(fullFilePath)
						.Where(ExtensionRolesHelper.IsExtensionSyncSupported)
						.Select(syncedFiles => new KeyValuePair<string, bool>(syncedFiles, true)));
					continue;
				}
				
				includedDirectoryFilePaths.Add(
					new KeyValuePair<string, bool>(fullFilePath,_filesystemStorage.ExistFile(fullFilePath))
				);
			}

			return includedDirectoryFilePaths;
		}

		internal async Task<ImportIndexItem> PreflightPerFile(KeyValuePair<string,bool> inputFileFullPath, ImportSettingsModel importSettings)
		{
			if ( !inputFileFullPath.Value || !_filesystemStorage.ExistFile(inputFileFullPath.Key) ) 
				return new ImportIndexItem{ 
					Status = ImportStatus.FileError, 
					FilePath = inputFileFullPath.Key
				};

			var imageFormat = ExtensionRolesHelper.GetImageFormat(
				_filesystemStorage.ReadStream(inputFileFullPath.Key, 
				160));
			
			// Check if extension is correct && Check if the file is correct
			if ( !ExtensionRolesHelper.IsExtensionSyncSupported(inputFileFullPath.Key) ||
			     !ExtensionRolesHelper.IsExtensionSyncSupported($".{imageFormat}") )
			{
				return new ImportIndexItem{ Status = ImportStatus.FileError, FilePath = inputFileFullPath.Key};
			}
			
			var hashList = await new FileHash(_filesystemStorage).GetHashCodeAsync(inputFileFullPath.Key);
			if ( !hashList.Value )
			{
				Console.WriteLine(">> FileHash error");
				return new ImportIndexItem{ Status = ImportStatus.FileError, FilePath = inputFileFullPath.Key};
			}
			
			if (importSettings.IndexMode && await _importQuery.IsHashInImportDb(hashList.Key) )
			{
				return new ImportIndexItem
				{
					Status = ImportStatus.IgnoredAlreadyImported, 
					FilePath = inputFileFullPath.Key,
					FileHash = hashList.Key
				};
			}
			
			// Only accept files with correct meta data
			// Check if there is a xmp file that contains data
			var fileIndexItem = _readMetaHost.ReadExifAndXmpFromFile(inputFileFullPath.Key);


			// Parse the filename and create a new importIndexItem object
			var importIndexItem = ObjectCreateIndexItem(inputFileFullPath.Key, imageFormat, hashList.Key, 
				fileIndexItem, importSettings.Structure);
			
			// get information to move to (in the future)
			importIndexItem.FileIndexItem.FileName = importIndexItem.ParseFileName(imageFormat);
			importIndexItem.FileIndexItem.ParentDirectory = importIndexItem.ParseSubfolders();
			importIndexItem.FileIndexItem.FileHash = hashList.Key;
			return importIndexItem;
		}

		/// <summary>
		/// Create a new import object
		/// </summary>
		/// <param name="inputFileFullPath"></param>
		/// <param name="imageFormat"></param>
		/// <param name="fileHashCode"></param>
		/// <param name="fileIndexItem"></param>
		/// <param name="overwriteStructure"></param>
		/// <returns></returns>
		internal ImportIndexItem ObjectCreateIndexItem(
				string inputFileFullPath,
				ExtensionRolesHelper.ImageFormat imageFormat,
				string fileHashCode,
				FileIndexItem fileIndexItem,
				string overwriteStructure)
		{
			var importIndexItem = new ImportIndexItem(_appSettings)
			{
				SourceFullFilePath = inputFileFullPath,
				DateTime = fileIndexItem.DateTime,
				FileHash = fileHashCode,
				FileIndexItem = fileIndexItem,
				Status = ImportStatus.Ok,
				FilePath = fileIndexItem.FilePath,
			};

			// Feature to overwrite structures when importing using a header
			// Overwrite the structure in the ImportIndexItem
			if (!string.IsNullOrWhiteSpace(overwriteStructure))
			{
				importIndexItem.Structure = overwriteStructure;
			}
			
			fileIndexItem.FileName = importIndexItem.ParseFileName(imageFormat);
			
			return importIndexItem;
		}
		
		public async Task<List<ImportIndexItem>> Importer(IEnumerable<string> inputFullPathList, ImportSettingsModel importSettings)
		{
			var preflightItemList = await Preflight(inputFullPathList.ToList(), importSettings);
			
			var listOfTasks = new List<Task<ImportIndexItem>>();
			foreach ( var preflightItem in preflightItemList )
			{
				listOfTasks.Add(Importer(preflightItem, importSettings));
			}
			var items = await Task.WhenAll(listOfTasks);
			return items.ToList();
			
		}

		private async Task<ImportIndexItem> Importer(ImportIndexItem preflightItem, ImportSettingsModel importSettings)
		{
			if ( preflightItem.Status != ImportStatus.Ok ) return preflightItem;

			var path = GetDestinationPath(preflightItem.FileIndexItem.FilePath);
			
			return new ImportIndexItem();
		}

		private string GetDestinationPath(string destinationFullPath)
		{
			if (!_filesystemStorage.ExistFile(destinationFullPath) ) return destinationFullPath;
			for ( int i = 1; i < 100; i++ )
			{
				var tryAgainFileName =
					FilenamesHelper.GetFileNameWithoutExtension(destinationFullPath) + $"_{i}." +
					FilenamesHelper.GetFileExtensionWithoutDot(destinationFullPath);
				if ( !_filesystemStorage.ExistFile(tryAgainFileName) )
				{
					return tryAgainFileName;
				}
			}
			throw new ApplicationException("tried after 100 times");
		}


	}
}
