using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using starsky.feature.import.Interfaces;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.database.Query;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starsky.foundation.readmeta.Interfaces;
using starsky.foundation.readmeta.Services;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Services;
using starsky.foundation.storage.Storage;
using starsky.foundation.writemeta.Helpers;
using starsky.foundation.writemeta.Interfaces;
using starsky.foundation.writemeta.Services;
using starskycore.Models;

[assembly: InternalsVisibleTo("starskytest")]
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
		private readonly IExifTool _exifTool;
		private IQuery _query;

		public Import(
			ISelectorStorage selectorStorage,
			AppSettings appSettings,
			IImportQuery importQuery,
			IExifTool exifTool,
			IQuery query)
		{
			_selectorStorage = selectorStorage;
			_importQuery = importQuery;
			
			_filesystemStorage = selectorStorage.Get(SelectorStorage.StorageServices.HostFilesystem);
            _subPathStorage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
            _thumbnailStorage = selectorStorage.Get(SelectorStorage.StorageServices.Thumbnail);

            _appSettings = appSettings;
            _readMetaHost = new ReadMeta(_filesystemStorage);
            _exifTool = exifTool;
            _query = query;
		}

		/// <summary>
		/// Check if the item can be add to the database
		/// Run `importer` to perform the action
		/// </summary>
		/// <param name="fullFilePathsList">paths</param>
		/// <param name="importSettings">settings</param>
		/// <returns></returns>
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
		private List<KeyValuePair<string,bool>> AppendDirectoryFilePaths(List<string> fullFilePathsList, 
			ImportSettingsModel importSettings)
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

		internal async Task<ImportIndexItem> PreflightPerFile(KeyValuePair<string,bool> inputFileFullPath, 
			ImportSettingsModel importSettings)
		{
			if ( !inputFileFullPath.Value || !_filesystemStorage.ExistFile(inputFileFullPath.Key) ) 
				return new ImportIndexItem{ 
					Status = ImportStatus.NotFound, 
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
			
			if (importSettings.IndexMode && await _importQuery.IsHashInImportDbAsync(hashList.Key) )
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
		/// Used when File has no exif date in description
		/// </summary>
		internal string MessageDateTimeBasedOnFilename = "Date and Time based on filename";

		/// <summary>
		/// Create a new import object
		/// </summary>
		/// <param name="inputFileFullPath">full file path</param>
		/// <param name="imageFormat">is it jpeg or png or something different</param>
		/// <param name="fileHashCode">file hash base32</param>
		/// <param name="fileIndexItem">database item</param>
		/// <param name="overwriteStructure">structure to overwite</param>
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
			
			// used for files without a Exif Date for example WhatsApp images
			if ( fileIndexItem.DateTime.Year == 1 )
			{
				importIndexItem.FileIndexItem.DateTime = importIndexItem.ParseDateTimeFromFileName();
				// used to sync exifTool and to let the user know that the transformation has been applied
				importIndexItem.FileIndexItem.Description = MessageDateTimeBasedOnFilename;
			}

			// Feature to overwrite structures when importing using a header
			// Overwrite the structure in the ImportIndexItem
			if (!string.IsNullOrWhiteSpace(overwriteStructure))
			{
				importIndexItem.Structure = overwriteStructure;
			}
			
			fileIndexItem.FileName = importIndexItem.ParseFileName(imageFormat);
			
			return importIndexItem;
		}
		
		/// <summary>
		/// Run import on list of files and folders (full path style)
		/// </summary>
		/// <param name="inputFullPathList">list of files and folders (full path style)</param>
		/// <param name="importSettings">settings</param>
		/// <returns>status object</returns>
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

		/// <summary>
		/// Run the import on the config file
		/// </summary>
		/// <param name="importIndexItem">config file</param>
		/// <param name="importSettings">optional settings</param>
		/// <returns>status</returns>
		private async Task<ImportIndexItem> Importer(ImportIndexItem importIndexItem, ImportSettingsModel importSettings)
		{
			if ( importIndexItem.Status != ImportStatus.Ok ) return importIndexItem;

			importIndexItem.FilePath  = GetDestinationPath(importIndexItem.FileIndexItem);
			
			_filesystemStorage.FileCopy(importIndexItem.SourceFullFilePath, 
				_appSettings.DatabasePathToFilePath(importIndexItem.FilePath));

			 // Support for include sidecar files
		    var xmpFullFilePath = ExtensionRolesHelper.ReplaceExtensionWithXmp(importIndexItem.FilePath);
		    if ( ExtensionRolesHelper.IsExtensionForceXmp(importIndexItem.FilePath)  &&
		         _filesystemStorage.ExistFile(xmpFullFilePath))
		    {
			    var destinationXmpFullPath =  ExtensionRolesHelper.ReplaceExtensionWithXmp(importIndexItem.FilePath);
			    _filesystemStorage.FileCopy(xmpFullFilePath, destinationXmpFullPath);
		    }
		    
		    // From here on the item is exit in the storage folder
		    // Creation of a sidecar xmp file --> NET CORE <--
		    if ( _appSettings.ExifToolImportXmpCreate && !_appSettings.AddLegacyOverwrite )
		    {
			    var exifCopy = new ExifCopy(_subPathStorage, _thumbnailStorage, 
				    new ExifTool(_selectorStorage,_appSettings), new ReadMeta(_subPathStorage));
			    exifCopy.XmpSync(importIndexItem.FileIndexItem.FilePath);
		    }

			UpdateImportTransformations(importIndexItem.FileIndexItem, importSettings.ColorClass);

	        // Ignore the sync part if the connection is missing
	        // or option enabled
	        if ( importIndexItem.Status == ImportStatus.Ok && importSettings.IndexMode && _importQuery.TestConnection() )
	        {
		        await _query.AddItemAsync(importIndexItem.FileIndexItem);
		        // To the list of imported folders
		        await _importQuery.AddAsync(importIndexItem);
	        }

	        if ( _appSettings.Verbose ) Console.Write("+");
	        
			// to move files
            if (importSettings.DeleteAfter)
            {
	            _filesystemStorage.FileDelete(importIndexItem.FilePath);
            }

	        return importIndexItem;
		}

		/// <summary>
		/// Run Transformation on Import to the files in the database
		/// </summary>
		/// <param name="fileIndexItem">information</param>
		/// <param name="colorClassTransformation">change colorClass</param>
		private void UpdateImportTransformations(FileIndexItem fileIndexItem, int colorClassTransformation)
		{
			if ( !ExtensionRolesHelper.IsExtensionExifToolSupported(fileIndexItem.FileName) ) return;

			// Update the contents to the file the imported item
			if ( fileIndexItem.Description != MessageDateTimeBasedOnFilename &&
			     colorClassTransformation == 0 ) return;
			
			if ( _appSettings.Verbose ) Console.WriteLine("Do a exifToolSync");

			var comparedNamesList = new List<string>
			{
				nameof(FileIndexItem.DateTime).ToLowerInvariant(),
				nameof(FileIndexItem.ColorClass).ToLowerInvariant(),
				nameof(FileIndexItem.Description).ToLowerInvariant(),
			};

			fileIndexItem.ColorClass = ( ColorClassParser.Color ) colorClassTransformation;

			new ExifToolCmdHelper(_exifTool,_subPathStorage, _thumbnailStorage, 
				new ReadMeta(_subPathStorage)).Update(fileIndexItem, comparedNamesList);
		}
		
		/// <summary>
		/// Number of checks for files with the same filePath.
		/// Change only to get Exceptions earlier
		/// </summary>
		internal int MaxTryGetDestinationPath { get; set; } = 100;

		/// <summary>
		/// Get a path with checking the fileName
		/// </summary>
		/// <param name="fileIndexItem">input file path</param>
		/// <returns>subPath</returns>
		/// <exception cref="ApplicationException">When there are to many files with the same name</exception>
		private string GetDestinationPath(FileIndexItem fileIndexItem)
		{
			if (!_subPathStorage.ExistFile(fileIndexItem.FilePath) ) return fileIndexItem.FilePath;
			for ( var i = 1; i < MaxTryGetDestinationPath; i++ )
			{
				var tryAgainSubPath = AppendIndexerToFilePath(
					fileIndexItem.ParentDirectory, 
					fileIndexItem.FileName, i);
				
				if ( !_subPathStorage.ExistFile(tryAgainSubPath) )
				{
					return tryAgainSubPath;
				}
			}
			throw new ApplicationException($"tried after {MaxTryGetDestinationPath} times");
		}

		/// <summary>
		/// Append test_1.jpg to filepath (subPath style)
		/// </summary>
		/// <param name="fileName">the fileName</param>
		/// <param name="index">number</param>
		/// <param name="parentDirectory">subPath style</param>
		/// <returns>test_1.jpg with complete filePath</returns>
		internal static string AppendIndexerToFilePath(string parentDirectory, string fileName , int index)
		{
			if ( index >= 1 )
			{
				fileName = string.Concat(
					FilenamesHelper.GetFileNameWithoutExtension(fileName),
					$"_{index}.",
					FilenamesHelper.GetFileExtensionWithoutDot(fileName)
				);
			}
			return PathHelper.AddSlash(parentDirectory) + fileName;;
		}
	}
}
