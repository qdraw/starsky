using System;
using System.Collections.Generic;
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
	public class ImportService : IImport
	{
		private readonly IImportQuery _importQuery;
		
		// storage providers
		private readonly ISelectorStorage _selectorStorage;
		private readonly IStorage _filesystemStorage;
		private readonly IStorage _subPathStorage;
		private readonly IStorage _thumbnailStorage;

		private readonly AppSettings _appSettings;

		private readonly IReadMeta _readMetaHost;

		public ImportService(
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
			var listOfTasks = new List<Task<ImportIndexItem>>();
			AppendDirectoryFilePaths(fullFilePathsList, importSettings);
			
			foreach ( var fullFilePath in fullFilePathsList )
			{
				listOfTasks.Add(PreflightPerFile(fullFilePath, importSettings));
			}
			var items = await Task.WhenAll(listOfTasks);
			return items.ToList();
		}

		/// <summary>
		/// To Add files form directory to list
		/// </summary>
		/// <param name="fullFilePathsList">full file Path</param>
		/// <param name="importSettings">settings to add recursive</param>
		private void AppendDirectoryFilePaths(List<string> fullFilePathsList, ImportSettingsModel importSettings)
		{
			foreach ( var fullFilePath in fullFilePathsList )
			{
				if ( _filesystemStorage.ExistFolder(fullFilePath) && importSettings.RecursiveDirectory)
				{
					// recursive
					fullFilePathsList.AddRange( 
						_filesystemStorage.GetAllFilesInDirectoryRecursive(fullFilePath)
							.Where(ExtensionRolesHelper.IsExtensionSyncSupported)
					);
				}
				else if ( _filesystemStorage.ExistFolder(fullFilePath) && !importSettings.RecursiveDirectory)
				{
					// non-recursive
					fullFilePathsList.AddRange(  
						_filesystemStorage.GetAllFilesInDirectory(fullFilePath)
							.Where(ExtensionRolesHelper.IsExtensionSyncSupported)
					);
				}
			}
		}

		internal async Task<ImportIndexItem> PreflightPerFile(string inputFileFullPath, ImportSettingsModel importSettings)
		{
			if ( _filesystemStorage.ExistFile(inputFileFullPath) ) return new ImportIndexItem{ Status = ImportStatus.FileError, FilePath = inputFileFullPath};

			var imageFormat = ExtensionRolesHelper.GetImageFormat(
				_filesystemStorage.ReadStream(inputFileFullPath, 
				160));
			
			// Check if extension is correct && Check if the file is correct
			if ( !ExtensionRolesHelper.IsExtensionSyncSupported(inputFileFullPath) ||
			     !ExtensionRolesHelper.IsExtensionSyncSupported($".{imageFormat}") )
			{
				return new ImportIndexItem{ Status = ImportStatus.FileError, FilePath = inputFileFullPath};
			}
			
			var hashList = await new FileHash(_filesystemStorage).GetHashCodeAsync(inputFileFullPath);
			if ( !hashList.Value )
			{
				Console.WriteLine(">> FileHash error");
				return new ImportIndexItem{ Status = ImportStatus.FileError, FilePath = inputFileFullPath};
			}
			
			if (importSettings.IndexMode && await _importQuery.IsHashInImportDb(hashList.Key) )
			{
				return new ImportIndexItem
				{
					Status = ImportStatus.IgnoredAlreadyImported, 
					FilePath = inputFileFullPath,
					FileHash = hashList.Key
				};
			}
			
			// Only accept files with correct meta data
			// Check if there is a xmp file that contains data
			var fileIndexItem = _readMetaHost.ReadExifAndXmpFromFile(inputFileFullPath);
			
			// Parse the filename and create a new importIndexItem object
			return ObjectCreateIndexItem(inputFileFullPath, imageFormat, hashList.Key, fileIndexItem, importSettings.Structure);
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
				FileHash = fileHashCode
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
		
		public List<string> Import(IEnumerable<string> inputFullPathList, ImportSettingsModel importSettings)
		{
			throw new System.NotImplementedException();
		}

		public List<string> Import(string inputFullPathList, ImportSettingsModel importSettings)
		{
			throw new System.NotImplementedException();
		}
	}
}
