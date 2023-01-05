#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using starsky.feature.import.Helpers;
using starsky.feature.import.Interfaces;
using starsky.feature.import.Models;
using starsky.foundation.database.Helpers;
using starsky.foundation.database.Import;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.database.Query;
using starsky.foundation.database.Thumbnails;
using starsky.foundation.injection;
using starsky.foundation.thumbnailmeta.Interfaces;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Extensions;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.readmeta.Interfaces;
using starsky.foundation.readmeta.Services;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Models;
using starsky.foundation.storage.Services;
using starsky.foundation.storage.Storage;
using starsky.foundation.writemeta.Interfaces;
using starsky.foundation.writemeta.Services;

[assembly: InternalsVisibleTo("starskytest")]
namespace starsky.feature.import.Services
{
	/// <summary>
	/// Also known as ImportService - Import.cs
	/// </summary>
	[Service(typeof(IImport), InjectionLifetime = InjectionLifetime.Scoped)]
	public class Import : IImport
	{
		private readonly IImportQuery? _importQuery;
		
		// storage providers
		private readonly IStorage _filesystemStorage;
		private readonly IStorage _subPathStorage;
		private readonly IStorage _thumbnailStorage;

		private readonly AppSettings _appSettings;

		private readonly IReadMeta _readMetaHost;
		private readonly IExifTool _exifTool;
		private readonly IQuery _query;
		
		private readonly IConsole _console;
		private readonly IMetaExifThumbnailService _metaExifThumbnailService;

		private readonly IMemoryCache? _memoryCache;
		private readonly IWebLogger _logger;
		private readonly UpdateImportTransformations _updateImportTransformations;
		private readonly IThumbnailQuery _thumbnailQuery;

		/// <summary>
		/// Used when File has no exif date in description
		/// </summary>
		internal const string MessageDateTimeBasedOnFilename = "Date and Time based on filename";

		public Import(
			ISelectorStorage selectorStorage,
			AppSettings appSettings,
			IImportQuery importQuery,
			IExifTool exifTool,
			IQuery query,
			IConsole console,
			IMetaExifThumbnailService metaExifThumbnailService,
			IWebLogger logger,
			IThumbnailQuery thumbnailQuery,
			IMemoryCache? memoryCache = null)
		{
			_importQuery = importQuery;
			
			_filesystemStorage = selectorStorage.Get(SelectorStorage.StorageServices.HostFilesystem);
            _subPathStorage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
            _thumbnailStorage = selectorStorage.Get(SelectorStorage.StorageServices.Thumbnail);

            _appSettings = appSettings;
            _readMetaHost = new ReadMeta(_filesystemStorage, appSettings, null, logger);
            _exifTool = exifTool;
            _query = query;
            _console = console;
            _metaExifThumbnailService = metaExifThumbnailService;
            _memoryCache = memoryCache;
            _logger = logger;
            _updateImportTransformations = new UpdateImportTransformations(logger, _exifTool, selectorStorage, appSettings);
            _thumbnailQuery = thumbnailQuery;
		}

		/// <summary>
		/// Check if the item can be add to the database
		/// Run `importer` to perform the action
		/// </summary>
		/// <param name="fullFilePathsList">paths</param>
		/// <param name="importSettings">settings</param>
		/// <returns></returns>
		public async Task<List<ImportIndexItem>> Preflight(List<string> fullFilePathsList, 
			ImportSettingsModel importSettings)
		{
			var includedDirectoryFilePaths = AppendDirectoryFilePaths(
				fullFilePathsList, 
				importSettings).ToList();
			
			// When Directory is Empty
			if ( !includedDirectoryFilePaths.Any() ) return new List<ImportIndexItem>();
			
			var importIndexItemsList = (await includedDirectoryFilePaths
				.ForEachAsync(
					async (includedFilePath) 
						=> await PreflightPerFile(includedFilePath, importSettings),
					_appSettings.MaxDegreesOfParallelism)).ToList();
			
			var directoriesContent = ParentFoldersDictionary(importIndexItemsList);

			importIndexItemsList = CheckForDuplicateNaming(importIndexItemsList.ToList(), directoriesContent);
			CheckForReadOnlyFileSystems(importIndexItemsList, importSettings.DeleteAfter);

			return importIndexItemsList;
		}

		internal List<Tuple<string?, List<string>>> CheckForReadOnlyFileSystems( List<ImportIndexItem> importIndexItemsList, bool deleteAfter = true)
		{
			if ( !deleteAfter ) return new List<Tuple<string?, List<string>>>();
			
			var parentFolders = new List<Tuple<string?, List<string>>>();
			foreach ( var itemSourceFullFilePath in importIndexItemsList.Select(item => item.SourceFullFilePath) )
			{
				var parentFolder = Directory.GetParent(itemSourceFullFilePath)
					?.FullName;

				if ( parentFolders.All(p => p.Item1 != parentFolder) )
				{
					parentFolders.Add(new Tuple<string?, List<string>>(parentFolder, new List<string>{itemSourceFullFilePath}));
					continue;
				}

				var item2 = parentFolders.First(p => p.Item1 == parentFolder);
				parentFolders[parentFolders.IndexOf(item2)].Item2.Add(itemSourceFullFilePath);
			}

			foreach ( var parentFolder in parentFolders.Where(p => p.Item1 != null) )
			{
				var fileStorageInfo = _filesystemStorage.Info(parentFolder.Item1!);
				if ( fileStorageInfo.IsFolderOrFile !=
				     FolderOrFileModel.FolderOrFileTypeList.Folder ||
				     fileStorageInfo.IsFileSystemReadOnly != true ) continue;
				
				foreach ( var item in parentFolder.Item2.Select(parentItem => importIndexItemsList.FirstOrDefault(p =>
					         p.SourceFullFilePath == parentItem)).Where(item => item != null).Cast<ImportIndexItem>() )
				{
					importIndexItemsList[importIndexItemsList.IndexOf(item)]
						.Status = ImportStatus.ReadOnlyFileSystem;
					
					if ( _appSettings.IsVerbose() )
					{
						_console.WriteLine($"🤷🗜️ Ignored, source file system is readonly try without move to copy {item.SourceFullFilePath}");
					}
				}
			}

			return parentFolders;
		}

		/// <summary>
		/// Get a Dictionary with all the content of the parent folders
		/// Used to scan for duplicate names
		/// </summary>
		/// <param name="importIndexItemsList">files to import, use FileIndexItem.ParentDirectory and status Ok</param>
		/// <returns>All parent folders with content</returns>
		internal Dictionary<string,List<string>> ParentFoldersDictionary(List<ImportIndexItem> importIndexItemsList)
		{
			var directoriesContent = new Dictionary<string,List<string>>();
			foreach ( var importIndexItemFileIndexItemParentDirectory in importIndexItemsList.Where(p =>
				p.Status == ImportStatus.Ok).Select(p => p.FileIndexItem?.ParentDirectory) )
			{
				if ( importIndexItemFileIndexItemParentDirectory == null || directoriesContent.ContainsKey(importIndexItemFileIndexItemParentDirectory) )
					continue;
				
				var parentDirectoryList =
					_subPathStorage.GetAllFilesInDirectory(importIndexItemFileIndexItemParentDirectory).ToList();
				directoriesContent.Add(importIndexItemFileIndexItemParentDirectory!, parentDirectoryList);
			}

			return directoriesContent;
		}

		/// <summary>
		/// Preflight for duplicate fileNames
		/// </summary>
		/// <param name="importIndexItemsList">list of files to be imported</param>
		/// <param name="directoriesContent">Dictionary of all parent folders</param>
		/// <returns>updated ImportIndexItem list</returns>
		/// <exception cref="ApplicationException">when there are to many files with the same name</exception>
		internal List<ImportIndexItem> CheckForDuplicateNaming(List<ImportIndexItem> importIndexItemsList,
			Dictionary<string,List<string>> directoriesContent)
		{
			foreach ( var importIndexItem in importIndexItemsList.Where(p => 
				         p.Status == ImportStatus.Ok ) )
			{
				if ( importIndexItem.FileIndexItem == null )
				{
					_logger.LogInformation("[CheckForDuplicateNaming] FileIndexItem is missing");
					continue;
				}
				
				// Try again until the max
				var updatedFilePath = "";
				var indexer = 0;
				for ( var i = 0; i < MaxTryGetDestinationPath; i++ )
				{
					updatedFilePath = AppendIndexerToFilePath(
						importIndexItem.FileIndexItem!.ParentDirectory!, 
						importIndexItem.FileIndexItem!.FileName!, indexer);
					
					var currentDirectoryContent =
						directoriesContent[importIndexItem.FileIndexItem.ParentDirectory!];
					
					if ( currentDirectoryContent.Any(p => p == updatedFilePath)  )
					{
						indexer++;
						continue;
					}
					currentDirectoryContent.Add(updatedFilePath);
					break;
				}
		
				if ( indexer >= MaxTryGetDestinationPath || string.IsNullOrEmpty(updatedFilePath) )
				{
					throw new AggregateException($"tried after {MaxTryGetDestinationPath} times");
				}
		
				importIndexItem.FileIndexItem!.FilePath = updatedFilePath;
				importIndexItem.FileIndexItem.FileName = PathHelper.GetFileName(updatedFilePath);
				importIndexItem.FilePath = updatedFilePath;
			}
			return importIndexItemsList;
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
					includedDirectoryFilePaths.AddRange(_filesystemStorage.
						GetAllFilesInDirectoryRecursive(fullFilePath)
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
			if ( _appSettings.ImportIgnore.Any(p => inputFileFullPath.Key.Contains(p)) )
			{
				if ( _appSettings.IsVerbose() ) _console.WriteLine($"❌ skip due rules: {inputFileFullPath.Key} ");
				return new ImportIndexItem{ 
					Status = ImportStatus.Ignore, 
					FilePath = inputFileFullPath.Key,
					SourceFullFilePath = inputFileFullPath.Key,
					AddToDatabase = DateTime.UtcNow
				};
			}
			
			if ( !inputFileFullPath.Value || !_filesystemStorage.ExistFile(inputFileFullPath.Key) )
			{
				if ( _appSettings.IsVerbose() ) _console.WriteLine($"❌ not found: {inputFileFullPath.Key}");
				return new ImportIndexItem{ 
					Status = ImportStatus.NotFound, 
					FilePath = inputFileFullPath.Key,
					SourceFullFilePath = inputFileFullPath.Key,
					AddToDatabase = DateTime.UtcNow
				};
			}

			var imageFormat = ExtensionRolesHelper.GetImageFormat(
				_filesystemStorage.ReadStream(inputFileFullPath.Key, 
				160));
			
			// Check if extension is correct && Check if the file is correct
			if ( !ExtensionRolesHelper.IsExtensionSyncSupported(inputFileFullPath.Key) ||
			     !ExtensionRolesHelper.IsExtensionSyncSupported($".{imageFormat}") )
			{
				if ( _appSettings.IsVerbose() )
				{
					_console.WriteLine($"❌ extension not supported: {inputFileFullPath.Key}");
				}
				return new ImportIndexItem
				{
					Status = ImportStatus.FileError, 
					FilePath = inputFileFullPath.Key, 
					SourceFullFilePath = inputFileFullPath.Key
				};
			}
			
			var hashList = await 
				new FileHash(_filesystemStorage).GetHashCodeAsync(inputFileFullPath.Key);
			if ( !hashList.Value )
			{
				if ( _appSettings.IsVerbose() )
				{
					_console.WriteLine($"❌ FileHash error {inputFileFullPath.Key}");
				}
				return new ImportIndexItem
				{
					Status = ImportStatus.FileError, 
					FilePath = inputFileFullPath.Key,
					SourceFullFilePath = inputFileFullPath.Key
				};
			}
			
			if (importSettings.IndexMode && await _importQuery!.IsHashInImportDbAsync(hashList.Key) )
			{
				if ( _appSettings.IsVerbose() )
				{
					_console.WriteLine($"🤷 Ignored, exist already {inputFileFullPath.Key}");
				}
				return new ImportIndexItem
				{
					Status = ImportStatus.IgnoredAlreadyImported, 
					FilePath = inputFileFullPath.Key,
					FileHash = hashList.Key,
					AddToDatabase = DateTime.UtcNow,
					SourceFullFilePath = inputFileFullPath.Key
				};
			} 
			
			// Only accept files with correct meta data
			// Check if there is a xmp file that contains data
			var fileIndexItem = _readMetaHost.ReadExifAndXmpFromFile(inputFileFullPath.Key);
			
			// Parse the filename and create a new importIndexItem object
			var importIndexItem = ObjectCreateIndexItem(inputFileFullPath.Key, imageFormat, 
				hashList.Key, fileIndexItem, importSettings.ColorClass,
				_filesystemStorage.Info(inputFileFullPath.Key).Size);
			
			// Update the parent and filenames
			importIndexItem = ApplyStructure(importIndexItem, importSettings.Structure);
		
			return importIndexItem;
		}


		/// <summary>
		/// Create a new import object
		/// </summary>
		/// <param name="inputFileFullPath">full file path</param>
		/// <param name="imageFormat">is it jpeg or png or something different</param>
		/// <param name="fileHashCode">file hash base32</param>
		/// <param name="fileIndexItem">database item</param>
		/// <param name="colorClassTransformation">Force to update colorclass</param>
		/// <param name="size">Add filesize in bytes</param>
		/// <returns></returns>
		private ImportIndexItem ObjectCreateIndexItem(
				string inputFileFullPath,
				ExtensionRolesHelper.ImageFormat imageFormat,
				string fileHashCode,
				FileIndexItem fileIndexItem,
				int colorClassTransformation,
				long size)
		{
			var importIndexItem = new ImportIndexItem(_appSettings)
			{
				SourceFullFilePath = inputFileFullPath,
				DateTime = fileIndexItem.DateTime,
				FileHash = fileHashCode,
				FileIndexItem = fileIndexItem,
				Status = ImportStatus.Ok,
				FilePath = fileIndexItem.FilePath,
				ColorClass = fileIndexItem.ColorClass
			};
			
			// used for files without a Exif Date for example WhatsApp images
			if ( fileIndexItem.DateTime.Year == 1 )
			{
				importIndexItem.FileIndexItem.DateTime = importIndexItem.ParseDateTimeFromFileName();
				// used to sync exifTool and to let the user know that the transformation has been applied
				importIndexItem.FileIndexItem.Description = MessageDateTimeBasedOnFilename;
				// only set when date is parsed if not ignore update
				if ( importIndexItem.FileIndexItem.DateTime.Year != 1 )
				{
					importIndexItem.DateTimeFromFileName = true;
				}
			}

			// Also add Camera brand to list
			importIndexItem.MakeModel = importIndexItem.FileIndexItem.MakeModel; 
				
			// AddToDatabase is Used by the importer History agent
			importIndexItem.FileIndexItem.AddToDatabase = DateTime.UtcNow;
			importIndexItem.AddToDatabase = DateTime.UtcNow;
			
			importIndexItem.FileIndexItem.Size = size;
			importIndexItem.FileIndexItem.FileHash = fileHashCode;
			importIndexItem.FileIndexItem.ImageFormat = imageFormat;
			importIndexItem.FileIndexItem.Status = FileIndexItem.ExifStatus.Ok;
			if ( colorClassTransformation < 0 ) return importIndexItem;

			// only when set in ImportSettingsModel
			var colorClass = ( ColorClassParser.Color ) colorClassTransformation;
			importIndexItem.FileIndexItem.ColorClass = colorClass;
			importIndexItem.ColorClass = colorClass;
			return importIndexItem;
		}

		/// <summary>
		/// Overwrite structures when importing using a header
		/// </summary>
		/// <param name="importIndexItem"></param>
		/// <param name="overwriteStructure">to overwrite, keep empty to ignore</param>
		/// <returns>Names applied to FileIndexItem</returns>
		private ImportIndexItem ApplyStructure(ImportIndexItem importIndexItem, string overwriteStructure)
		{
			importIndexItem.Structure = _appSettings.Structure;
			
			// Feature to overwrite structures when importing using a header
			// Overwrite the structure in the ImportIndexItem
			if (!string.IsNullOrWhiteSpace(overwriteStructure))
			{
				importIndexItem.Structure = overwriteStructure;
			}
			
			var structureService = new StructureService(_subPathStorage, importIndexItem.Structure);
			
			importIndexItem.FileIndexItem!.ParentDirectory = structureService.ParseSubfolders(
				importIndexItem.FileIndexItem.DateTime, importIndexItem.FileIndexItem.FileCollectionName!,
				FilenamesHelper.GetFileExtensionWithoutDot(importIndexItem.FileIndexItem.FileName));
			
			importIndexItem.FileIndexItem.FileName = structureService.ParseFileName(
				importIndexItem.FileIndexItem.DateTime, importIndexItem.FileIndexItem.FileCollectionName!,
				FilenamesHelper.GetFileExtensionWithoutDot(importIndexItem.FileIndexItem.FileName));
			importIndexItem.FilePath = importIndexItem.FileIndexItem.FilePath;
			
			return importIndexItem;
		}
		
		/// <summary>
		/// Run import on list of files and folders (full path style)
		/// </summary>
		/// <param name="inputFullPathList">list of files and folders (full path style)</param>
		/// <param name="importSettings">settings</param>
		/// <returns>status object</returns>
		public async Task<List<ImportIndexItem>> Importer(IEnumerable<string> inputFullPathList, 
			ImportSettingsModel importSettings)
		{
			var preflightItemList = await Preflight(inputFullPathList.ToList(), importSettings);
			
			// When directory is empty 
			if ( !preflightItemList.Any() ) return new List<ImportIndexItem>();

			var directoriesContent = ParentFoldersDictionary(preflightItemList);
			if ( importSettings.IndexMode ) await CreateParentFolders(directoriesContent);

			var importIndexItemsList = (await preflightItemList.AsEnumerable()
				.ForEachAsync(
					async (preflightItem) 
						=> await Importer(preflightItem, importSettings),
					_appSettings.MaxDegreesOfParallelism)).ToList();
			
			return importIndexItemsList;
		}

		internal async Task<IEnumerable<(bool, string, string?)>> CreateMataThumbnail(IEnumerable<ImportIndexItem> 
			importIndexItemsList, ImportSettingsModel importSettings)
		{
			if ( _appSettings.MetaThumbnailOnImport == false || !importSettings.IndexMode) return new List<(bool, string, string?)>();
			var items = importIndexItemsList
				.Where(p => p.Status == ImportStatus.Ok)
				.Select(p => (p.FilePath, p.FileIndexItem!.FileHash)).Cast<(string,string)>().ToList();
			if ( !items.Any() ) return new List<(bool, string, string?)>();
			return await _metaExifThumbnailService.AddMetaThumbnail(items);
		}

		/// <summary>
		/// Run the import on the config file
		/// Does NOT add anything to the database
		/// </summary>
		/// <param name="importIndexItem">config file</param>
		/// <param name="importSettings">optional settings</param>
		/// <returns>status</returns>
		internal async Task<ImportIndexItem> Importer(ImportIndexItem importIndexItem, 
			ImportSettingsModel importSettings)
		{
			if ( importIndexItem.Status != ImportStatus.Ok ) return importIndexItem;

			// True when exist and file type is raw
			var xmpExistForThisFileType = ExistXmpSidecarForThisFileType(importIndexItem);
			
			if ( xmpExistForThisFileType || (_appSettings.ExifToolImportXmpCreate 
			                                 && ExtensionRolesHelper.IsExtensionForceXmp(importIndexItem.FilePath)))
			{
				// When a xmp file already exist (only for raws)
				// AND when this created afterwards with the ExifToolImportXmpCreate setting  (only for raws)
				importIndexItem.FileIndexItem!.AddSidecarExtension("xmp");
			}
			
			// Add item to database
			await AddToQueryAndImportDatabaseAsync(importIndexItem, importSettings);
			
			// Copy
			if ( _appSettings.IsVerbose() ) _logger.LogInformation("[Import] Next Action = Copy" +
			                        $" {importIndexItem.SourceFullFilePath} {importIndexItem.FilePath}");
			using (var sourceStream = _filesystemStorage.ReadStream(importIndexItem.SourceFullFilePath))
				await _subPathStorage.WriteStreamAsync(sourceStream, importIndexItem.FilePath!);
			
			// Copy the sidecar file
		    if ( xmpExistForThisFileType)
		    {
			    var xmpSourceFullFilePath = ExtensionRolesHelper.ReplaceExtensionWithXmp(importIndexItem.SourceFullFilePath);
			    var destinationXmpFullPath =  ExtensionRolesHelper.ReplaceExtensionWithXmp(importIndexItem.FilePath);
			    _filesystemStorage.FileCopy(xmpSourceFullFilePath, destinationXmpFullPath);
		    }
		    
		    await CreateSideCarFile(importIndexItem, xmpExistForThisFileType);

		    // Run Exiftool to Update for example colorClass
		    UpdateImportTransformations.QueryUpdateDelegate? updateItemAsync = null;
		    UpdateImportTransformations.QueryThumbnailUpdateDelegate? queryThumbnailUpdateDelegate = null;
		    
		    if ( importSettings.IndexMode )
		    {
			    updateItemAsync = new QueryFactory(
				    new SetupDatabaseTypes(_appSettings), _query,
				    _memoryCache, _appSettings, _logger).Query()!.UpdateItemAsync;
			    queryThumbnailUpdateDelegate = (size, fileHashes, setStatus) => new ThumbnailQueryFactory(
				    new SetupDatabaseTypes(_appSettings),
				    _thumbnailQuery, _logger).ThumbnailQuery()!.AddThumbnailRangeAsync(size, fileHashes, setStatus);
		    }
		    
		    await CreateMataThumbnail(new List<ImportIndexItem>{importIndexItem}, importSettings);
		    
		    // next: and save the database item
		    importIndexItem.FileIndexItem = await _updateImportTransformations
			    .UpdateTransformations(updateItemAsync, importIndexItem.FileIndexItem!, 
			    importSettings.ColorClass, importIndexItem.DateTimeFromFileName, importSettings.IndexMode);
		    
		    await UpdateCreateMetaThumbnail(queryThumbnailUpdateDelegate, importIndexItem.FileIndexItem?.FileHash, importSettings.IndexMode);

		    DeleteFileAfter(importSettings, importIndexItem);
		    
            if ( _appSettings.IsVerbose() ) _console.Write("+");
            return importIndexItem;
		}

		private async Task UpdateCreateMetaThumbnail( UpdateImportTransformations.QueryThumbnailUpdateDelegate? queryThumbnailUpdateDelegate, 
			string? fileHash, bool indexMode)
		{
			if ( fileHash == null ||  _appSettings.MetaThumbnailOnImport == false || !indexMode || queryThumbnailUpdateDelegate == null) return;
			// Check if fastest version is available to show 
			var setStatus = _thumbnailStorage.ExistFile(
				ThumbnailNameHelper.Combine(fileHash, ThumbnailSize.TinyMeta));
			await queryThumbnailUpdateDelegate(new List<ThumbnailSize>{ThumbnailSize.TinyMeta},
				new List<string>{fileHash}, setStatus);
		}

		/// <summary>
		/// To Move Files after import
		/// </summary>
		/// <param name="importSettings">to enable the 'Delete After' flag</param>
		/// <param name="importIndexItem">item</param>
		private void DeleteFileAfter(ImportSettingsModel importSettings,
			ImportIndexItem importIndexItem)
		{
			// to move files
			if ( !importSettings.DeleteAfter ) return;
			if ( _appSettings.IsVerbose() )
			{
				_console.WriteLine($"🚮 Delete file: {importIndexItem.SourceFullFilePath}");
			}
			_filesystemStorage.FileDelete(importIndexItem.SourceFullFilePath);
		}

		/// <summary>
		///	From here on the item is exit in the storage folder
		/// Creation of a sidecar xmp file
		/// </summary>
		/// <param name="importIndexItem"></param>
		/// <param name="xmpExistForThisFileType"></param>
		private async Task CreateSideCarFile(ImportIndexItem importIndexItem, bool xmpExistForThisFileType)
		{
			if ( _appSettings.ExifToolImportXmpCreate && !xmpExistForThisFileType)
			{
				var exifCopy = new ExifCopy(_subPathStorage, 
					_thumbnailStorage, _exifTool, new ReadMeta(_subPathStorage, 
					_appSettings, null, _logger));
				await exifCopy.XmpSync(importIndexItem.FileIndexItem!.FilePath);
			}
		}

		/// <summary>
		/// Support for include sidecar files - True when exist && current filetype is raw
		/// </summary>
		/// <param name="importIndexItem">to get the SourceFullFilePath</param>
		/// <returns>True when exist && current filetype is raw</returns>
		internal bool ExistXmpSidecarForThisFileType(ImportIndexItem importIndexItem)
		{
			if ( string.IsNullOrEmpty(importIndexItem.SourceFullFilePath) )
			{
				return false;
			}
			
			// Support for include sidecar files
			var xmpSourceFullFilePath =
				ExtensionRolesHelper.ReplaceExtensionWithXmp(importIndexItem
					.SourceFullFilePath);
			return ExtensionRolesHelper.IsExtensionForceXmp(importIndexItem
				       .SourceFullFilePath) &&
			       _filesystemStorage.ExistFile(xmpSourceFullFilePath);
		}
		
		/// <summary>
		/// Add item to database
		/// </summary>
		internal async Task AddToQueryAndImportDatabaseAsync(
			ImportIndexItem importIndexItem,
			ImportSettingsModel importSettings)
		{
			if ( !importSettings.IndexMode || _importQuery?.TestConnection() != true )
			{
				if ( _appSettings.IsVerbose() ) _logger.LogInformation(" AddToQueryAndImportDatabaseAsync Ignored - " +
				                                               $"IndexMode {importSettings.IndexMode} " +
				                                               $"TestConnection {_importQuery?.TestConnection()}");
				return;
			}

			// Add to Normal File Index database
			var query = new QueryFactory(new SetupDatabaseTypes(_appSettings), _query,
				_memoryCache, _appSettings,_logger).Query();
			await query!.AddItemAsync(importIndexItem.FileIndexItem!);
			
			// Add to check db, to avoid duplicate input
			var importQuery = new ImportQueryFactory(new SetupDatabaseTypes(_appSettings), _importQuery,_console, _logger).ImportQuery();
			await importQuery!.AddAsync(importIndexItem, importSettings.IsConsoleOutputModeDefault() );
			
			await query.DisposeAsync();
		}

		/// <summary>
		/// Number of checks for files with the same filePath.
		/// Change only to get Exceptions earlier
		/// </summary>
		internal int MaxTryGetDestinationPath { get; set; } = 100;

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
			return PathHelper.AddSlash(parentDirectory) + PathHelper.RemovePrefixDbSlash(fileName);
		}
		
		/// <summary>
		/// Create parent folders if the folder does not exist on disk
		/// </summary>
		/// <param name="directoriesContent">List of all ParentFolders</param>
		private async Task CreateParentFolders(Dictionary<string,List<string>> directoriesContent)
		{
			foreach ( var parentDirectoryPath in directoriesContent )
			{
				var parentDirectoriesList = parentDirectoryPath.Key.Split('/');

				var parentPath = new StringBuilder();
				await CreateNewDatabaseDirectory("/");

				foreach ( var folderName in parentDirectoriesList )
				{
					if ( string.IsNullOrEmpty(folderName) ) continue;
					parentPath.Append($"/{folderName}");

					await CreateNewDatabaseDirectory(parentPath.ToString());

					if ( _subPathStorage.ExistFolder(parentPath.ToString()))
					{
						continue;
					}
					_subPathStorage.CreateDirectory(parentPath.ToString());
				}
			}
		}
		
		/// <summary>
		/// Temp place to store parent Directories to avoid lots of Database requests
		/// </summary>
		private List<string> AddedParentDirectories { get; set; } = new List<string>();

		/// <summary>
		/// Create a directory in the database
		/// </summary>
		/// <param name="parentPath">path to create</param>
		/// <returns>async task</returns>
		private async Task CreateNewDatabaseDirectory(string parentPath)
		{
			if ( AddedParentDirectories.Contains(parentPath) || 
			     _query.SingleItem(parentPath) != null ) return;
			
			var item = new FileIndexItem(PathHelper.RemoveLatestSlash(parentPath))
			{
				AddToDatabase = DateTime.UtcNow,
				IsDirectory = true,
				ColorClass = ColorClassParser.Color.None
			};
				
			await _query.AddItemAsync(item);
			AddedParentDirectories.Add(parentPath);
		}
	}
}
