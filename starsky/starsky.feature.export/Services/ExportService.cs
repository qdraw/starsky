using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using starsky.feature.export.Interfaces;
using starsky.foundation.database.Helpers;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.injection;
using starsky.foundation.platform.Extensions;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Thumbnails;
using starsky.foundation.storage.ArchiveFormats;
using starsky.foundation.storage.Helpers;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Models;
using starsky.foundation.storage.Storage;
using starsky.foundation.thumbnailgeneration.GenerationFactory.Interfaces;

[assembly: InternalsVisibleTo("starskytest")]

namespace starsky.feature.export.Services;

/// <summary>
///     Also known as Download
/// </summary>
[Service(typeof(IExport), InjectionLifetime = InjectionLifetime.Scoped)]
public class ExportService : IExport
{
	private readonly AppSettings _appSettings;
	private readonly IStorage _hostFileSystemStorage;
	private readonly IStorage _iStorage;
	private readonly IWebLogger _logger;
	private readonly IQuery _query;
	private readonly IThumbnailService _thumbnailService;

	public ExportService(IQuery query, AppSettings appSettings,
		ISelectorStorage selectorStorage, IWebLogger logger, IThumbnailService thumbnailService)
	{
		_appSettings = appSettings;
		_query = query;
		_iStorage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
		_hostFileSystemStorage =
			selectorStorage.Get(SelectorStorage.StorageServices.HostFilesystem);
		_thumbnailService = thumbnailService;
		_logger = logger;
	}

	/// <summary>
	///     Export preflight
	/// </summary>
	/// <param name="inputFilePaths">list of subPaths</param>
	/// <param name="collections">is stack collections enabled</param>
	/// <param name="thumbnail">should export thumbnail or not</param>
	/// <returns>zipHash, fileIndexResultsList</returns>
	public async Task<Tuple<string, List<FileIndexItem>>> PreflightAsync(
		string[] inputFilePaths,
		bool collections = true,
		bool thumbnail = false)
	{
		// the result list
		var fileIndexResultsList = new List<FileIndexItem>();

		foreach ( var subPath in inputFilePaths )
		{
			var detailView = _query.SingleItem(subPath, null, collections, false);
			if ( string.IsNullOrEmpty(detailView?.FileIndexItem?.FilePath) )
			{
				StatusCodesHelper.ReturnExifStatusError(new FileIndexItem(subPath),
					FileIndexItem.ExifStatus.NotFoundNotInIndex,
					fileIndexResultsList);
				continue;
			}

			if ( _iStorage.IsFolderOrFile(detailView.FileIndexItem.FilePath) ==
			     FolderOrFileModel.FolderOrFileTypeList.Deleted )
			{
				StatusCodesHelper.ReturnExifStatusError(detailView.FileIndexItem,
					FileIndexItem.ExifStatus.NotFoundSourceMissing,
					fileIndexResultsList);
				continue;
			}

			if ( detailView.FileIndexItem.IsDirectory == true )
			{
				await AddFileIndexResultsListForDirectory(detailView, fileIndexResultsList);
				continue;
			}

			// Now Add Collection based images
			AddCollectionBasedImages(detailView, fileIndexResultsList, collections, subPath);
		}

		var isThumbnail = thumbnail ? "TN" : "SR"; // has:notHas
		var zipHash = isThumbnail + GetName(fileIndexResultsList);

		return new Tuple<string, List<FileIndexItem>>(zipHash, fileIndexResultsList);
	}

	/// <summary>
	///     Based on the preflight create a Zip Export
	/// </summary>
	/// <param name="fileIndexResultsList">Result of Preflight</param>
	/// <param name="thumbnail">isThumbnail?</param>
	/// <param name="zipOutputFileName">filename of zip type file (no extension)</param>
	/// <returns>nothing</returns>
	public async Task CreateZip(List<FileIndexItem> fileIndexResultsList, bool thumbnail,
		string zipOutputFileName)
	{
		var fullFilePaths = await CreateListToExport(fileIndexResultsList, thumbnail);
		var fileNames = await FilePathToFileNameAsync(fullFilePaths, thumbnail);

		new Zipper(_logger).CreateZip(_appSettings.TempFolder, fullFilePaths, fileNames,
			zipOutputFileName);

		// Write a single file to be sure that writing is ready
		var doneFileFullPath = Path.Combine(_appSettings.TempFolder, zipOutputFileName) + ".done";
		await _hostFileSystemStorage.WriteStreamAsync(StringToStreamHelper.StringToStream("OK"),
			doneFileFullPath);
		if ( _appSettings.IsVerbose() )
		{
			_logger.LogInformation("[CreateZip] Zip done: " + doneFileFullPath);
		}
	}

	/// <summary>
	///     Is Zip Ready?
	/// </summary>
	/// <param name="zipOutputFileName">fileName without extension</param>
	/// <returns>null if status file is not found, true if done file exist</returns>
	public Tuple<bool?, string?> StatusIsReady(string zipOutputFileName)
	{
		var sourceFullPath = Path.Combine(_appSettings.TempFolder, zipOutputFileName) + ".zip";
		var doneFileFullPath = Path.Combine(_appSettings.TempFolder, zipOutputFileName) + ".done";

		if ( !_hostFileSystemStorage.ExistFile(sourceFullPath) )
		{
			return new Tuple<bool?, string?>(null, null);
		}

		// Read a single file to be sure that writing is ready
		return new Tuple<bool?, string?>(_hostFileSystemStorage.ExistFile(doneFileFullPath),
			sourceFullPath);
	}

	private async Task AddFileIndexResultsListForDirectory(DetailView detailView,
		List<FileIndexItem> fileIndexResultsList)
	{
		var allFilesInFolder =
			await _query.GetAllRecursiveAsync(detailView
				.FileIndexItem?.FilePath!);
		foreach ( var item in
		         allFilesInFolder.Where(item =>
			         item.FilePath != null && _iStorage.ExistFile(item.FilePath)) )
		{
			item.Status = FileIndexItem.ExifStatus.Ok;
			fileIndexResultsList.Add(item);
		}
	}

	private void AddCollectionBasedImages(DetailView detailView,
		List<FileIndexItem> fileIndexResultsList, bool collections, string subPath)
	{
		var collectionSubPathList =
			DetailView.GetCollectionSubPathList(detailView.FileIndexItem!, collections, subPath);
		foreach ( var item in collectionSubPathList )
		{
			var itemFileIndexItem = _query.SingleItem(item, null,
				false, false)?.FileIndexItem;
			if ( itemFileIndexItem == null )
			{
				continue;
			}

			itemFileIndexItem.Status = FileIndexItem.ExifStatus.Ok;
			fileIndexResultsList.Add(itemFileIndexItem);
		}
	}

	/// <summary>
	///     This list will be included in the zip - Export is called Download in the UI
	/// </summary>
	/// <param name="fileIndexResultsList">the items</param>
	/// <param name="thumbnail">add the thumbnail or the source image</param>
	/// <returns>list of file paths</returns>
	public async Task<List<string>> CreateListToExport(List<FileIndexItem> fileIndexResultsList,
		bool thumbnail)
	{
		var filePaths = new List<string>();

		foreach ( var item in fileIndexResultsList.Where(p =>
			         p is { Status: FileIndexItem.ExifStatus.Ok, FileHash: not null }).ToList() )
		{
			if ( thumbnail )
			{
				var sourceThumb = Path.Combine(_appSettings.ThumbnailTempFolder,
					ThumbnailNameHelper.Combine(item.FileHash!,
						ThumbnailSize.Large, _appSettings.ThumbnailImageFormat));

				await _thumbnailService
					.GenerateThumbnail(item.FilePath!, item.FileHash!,
						ThumbnailGenerationType.SkipExtraLarge);

				filePaths.Add(sourceThumb);
				continue;
			}

			var sourceFile = _appSettings.DatabasePathToFilePath(item.FilePath!);

			if ( !_hostFileSystemStorage.ExistFile(sourceFile) )
			{
				continue;
			}

			// the jpeg file for example
			filePaths.Add(sourceFile);

			// when there is .xmp sidecar file
			// (but only when file is a RAW file, ignored when for example jpeg)
			if ( !ExtensionRolesHelper.IsExtensionForceXmp(item.FilePath) ||
			     !_iStorage.ExistFile(
				     ExtensionRolesHelper.ReplaceExtensionWithXmp(
					     item.FilePath)) )
			{
				continue;
			}

			var xmpFileFullPath = _appSettings.DatabasePathToFilePath(
				ExtensionRolesHelper.ReplaceExtensionWithXmp(
					item.FilePath));

			if ( !_hostFileSystemStorage.ExistFile(xmpFileFullPath) )
			{
				continue;
			}

			filePaths.Add(xmpFileFullPath);
		}

		return filePaths;
	}

	/// <summary>
	///     Get the filename (in case of thumbnail the source image name)
	///     Preserves directory structure by returning relative paths when subfolders exist
	/// </summary>
	/// <param name="fullFilePaths">the full file paths </param>
	/// <param name="thumbnail">copy the thumbnail (true) or the source image (false)</param>
	/// <returns>List of filenames or relative paths in Unix style</returns>
	internal async Task<List<string>> FilePathToFileNameAsync(IEnumerable<string> fullFilePaths,
		bool thumbnail)
	{
		var filePathsList = fullFilePaths.ToList();
		var fileNames = new List<string>();

		if ( filePathsList.Count == 0 )
		{
			return fileNames;
		}

		// Determine if we have subfolders - check if any file has more than just a filename
		var hasSubFolders = DetectHasSubFolders(filePathsList, thumbnail);

		foreach ( var filePath in filePathsList )
		{
			if ( thumbnail )
			{
				// We use base32 fileHashes but export 
				// the file with the original name

				var thumbFilename = Path.GetFileNameWithoutExtension(filePath);
				var subPath = ( await _query.GetSubPathsByHashAsync(thumbFilename) )
					.FirstOrDefaultWithFallback(filePath);

				if ( !string.IsNullOrEmpty(subPath) )
				{
					var filename = FilenamesHelper.GetFileName(subPath);
					fileNames.Add(filename);
				}

				continue;
			}

			// For non-thumbnail files, preserve directory structure if subfolders exist
			if ( hasSubFolders )
			{
				// Get the relative path from storage folder in Unix style
				var relativePath = GetRelativePathFromStorage(filePath);
				fileNames.Add(relativePath);
			}
			else
			{
				// No subfolders - just use the filename
				fileNames.Add(Path.GetFileName(filePath));
			}
		}

		fileNames = FindCommonAncestor(hasSubFolders, fileNames);
		return fileNames;
	}

	/// <summary>
	///     If subfolders exist, find common ancestor and strip it from all paths
	/// </summary>
	/// <param name="hasSubFolders">return direct if no subfolders</param>
	/// <param name="fileNames">list of filenames</param>
	/// <returns></returns>
	[SuppressMessage("ReSharper", "ConvertIfStatementToReturnStatement")]
	private static List<string> FindCommonAncestor(bool hasSubFolders, List<string> fileNames)
	{
		if ( !hasSubFolders || !fileNames.Any(f => f.Contains('/') || f.Contains('\\')) )
		{
			return fileNames;
		}

		var commonAncestor = FindCommonAncestorPath(fileNames);
		if ( !string.IsNullOrEmpty(commonAncestor) )
		{
			fileNames = fileNames.Select(f =>
			{
				if ( f.StartsWith(commonAncestor + "/", StringComparison.OrdinalIgnoreCase) )
				{
					return f[( commonAncestor.Length + 1 )..];
				}

				return f;
			}).ToList();
		}

		return fileNames;
	}

	/// <summary>
	///     Detect if there are any subdirectories in the file list
	/// </summary>
	private bool DetectHasSubFolders(List<string> fullFilePaths, bool thumbnail)
	{
		if ( thumbnail )
		{
			// For thumbnails, we don't currently preserve folder structure
			return false;
		}

		var storageFolder = PathHelper.AddBackslash(_appSettings.StorageFolder);

		foreach ( var filePath in fullFilePaths )
		{
			if ( string.IsNullOrEmpty(filePath) )
			{
				continue;
			}

			// Get the relative part after the storage folder
			if ( !filePath.StartsWith(storageFolder, StringComparison.OrdinalIgnoreCase) )
			{
				continue;
			}

			var relativePart = filePath[storageFolder.Length..];

			// If there's a directory separator in the relative part, we have subfolders
			if ( relativePart.Contains(Path.DirectorySeparatorChar) ||
			     relativePart.Contains(Path.AltDirectorySeparatorChar) )
			{
				return true;
			}
		}

		return false;
	}

	/// <summary>
	///     Get the relative path from the storage folder in Unix style (forward slashes)
	/// </summary>
	internal string GetRelativePathFromStorage(string fullFilePath)
	{
		if ( string.IsNullOrEmpty(fullFilePath) )
		{
			return string.Empty;
		}

		var storageFolder = PathHelper.AddBackslash(_appSettings.StorageFolder);

		// If the file path starts with storage folder, get the relative part
		if ( !fullFilePath.StartsWith(storageFolder, StringComparison.OrdinalIgnoreCase) )
		{
			// Fallback to just the filename if we can't determine relative path
			return PathHelper.GetFileName(fullFilePath);
		}

		// Get relative path and convert to Unix style (forward slashes)
		var relativePath = fullFilePath[storageFolder.Length..];
		return relativePath.Replace(Path.DirectorySeparatorChar, '/')
			.Replace(Path.AltDirectorySeparatorChar, '/');
	}

	/// <summary>
	///     Find the common ancestor directory path for all files in Unix style paths
	///     For example.
	///     - ["2025/06/2025_06_18/image.jpg", "2025/06/2025_06_14/image.jpg"] -> "2025/06"
	///     - ["2025/06/file1.jpg", "2025/07/file2.jpg"] -> "2025"
	///     - ["2026/06/file1.jpg", "2025/06/file2.jpg"] -> ""
	/// </summary>
	internal static string FindCommonAncestorPath(List<string> unixStylePaths)
	{
		switch ( unixStylePaths.Count )
		{
			case 0:
				return string.Empty;
			case 1:
			{
				var lastSlash = unixStylePaths[0].LastIndexOf('/');
				return lastSlash > 0 ? unixStylePaths[0][..lastSlash] : string.Empty;
			}
		}

		// Split all paths into parts
		var allParts = unixStylePaths.Select(p => p.Split('/')).ToList();

		// Find the depth of the shallowest path (minimum number of directory levels)
		var minDepth = allParts.Min(parts => parts.Length - 1); // -1 for filename

		// Find common ancestor by comparing parts a level by level
		var commonParts = new List<string>();
		for ( var i = 0; i < minDepth; i++ )
		{
			var part = allParts[0][i];

			// Check if this part is the same across all paths at this level
			if ( allParts.All(parts => parts.Length > i &&
			                           parts[i].Equals(part, StringComparison.OrdinalIgnoreCase)) )
			{
				commonParts.Add(part);
			}
			else
			{
				// Stop at first mismatch
				break;
			}
		}

		return string.Join("/", commonParts);
	}

	/// <summary>
	///     to create a unique name of the zip using c# get hashcode
	/// </summary>
	/// <param name="fileIndexResultsList">list of objects with fileHashes</param>
	/// <returns>unique 'get hashcode' string</returns>
	private static string GetName(IEnumerable<FileIndexItem> fileIndexResultsList)
	{
		var tempFileNameStringBuilder = new StringBuilder();
		foreach ( var item in fileIndexResultsList )
		{
			tempFileNameStringBuilder.Append(item.FileHash);
		}

		// to be sure that the max string limit
		var shortName = tempFileNameStringBuilder.ToString().GetHashCode()
			.ToString(CultureInfo.InvariantCulture).ToLower().Replace("-", "A");

		return shortName;
	}
}
