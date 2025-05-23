using System;
using System.Collections.Generic;
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
	/// <param name="zipOutputFileName">filename of zip file (no extension)</param>
	/// <returns>nothing</returns>
	public async Task CreateZip(List<FileIndexItem> fileIndexResultsList, bool thumbnail,
		string zipOutputFileName)
	{
		var filePaths = await CreateListToExport(fileIndexResultsList, thumbnail);
		var fileNames = await FilePathToFileNameAsync(filePaths, thumbnail);

		new Zipper(_logger).CreateZip(_appSettings.TempFolder, filePaths, fileNames,
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
			         p.Status == FileIndexItem.ExifStatus.Ok && p.FileHash != null).ToList() )
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

			// when there is .xmp sidecar file (but only when file is a RAW file, ignored when for example jpeg)
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
	/// </summary>
	/// <param name="filePaths">the full file paths </param>
	/// <param name="thumbnail">copy the thumbnail (true) or the source image (false)</param>
	/// <returns></returns>
	internal async Task<List<string>> FilePathToFileNameAsync(IEnumerable<string> filePaths,
		bool thumbnail)
	{
		var fileNames = new List<string>();
		foreach ( var filePath in filePaths )
		{
			if ( thumbnail )
			{
				// We use base32 fileHashes but export 
				// the file with the original name

				var thumbFilename = Path.GetFileNameWithoutExtension(filePath);
				var subPath = await _query.GetSubPathByHashAsync(thumbFilename);
				var filename = subPath?.Split('/').LastOrDefault()!; // first a string
				fileNames.Add(filename);
				continue;
			}

			fileNames.Add(Path.GetFileName(filePath));
		}

		return fileNames;
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
