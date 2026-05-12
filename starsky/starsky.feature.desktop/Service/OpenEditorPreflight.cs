using starsky.feature.desktop.Interfaces;
using starsky.feature.desktop.Models;
using starsky.foundation.database.Helpers;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.injection;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Models;
using starsky.foundation.storage.Storage;

namespace starsky.feature.desktop.Service;

[Service(typeof(IOpenEditorPreflight), InjectionLifetime = InjectionLifetime.Scoped)]
public class OpenEditorPreflight(
	IQuery query,
	AppSettings appSettings,
	ISelectorStorage selectorStorage,
	IWebLogger logger)
	: IOpenEditorPreflight
{
	private readonly IStorage _hostFileSystem =
		selectorStorage.Get(SelectorStorage.StorageServices.HostFilesystem);

	private readonly IStorage _iStorage =
		selectorStorage.Get(SelectorStorage.StorageServices.SubPath);

	public async Task<List<PathImageFormatExistsAppPathModel>> PreflightAsync(
		List<string> inputFilePaths, bool collections)
	{
		var fileIndexItemList = await GetObjectsToOpenFromDatabase(inputFilePaths, collections);
		fileIndexItemList = GroupByFileCollectionName(fileIndexItemList, collections);

		return fileIndexItemList.Select(fileIndexItem => new PathImageFormatExistsAppPathModel
			{
				AppPath = GetDesktopEditorPath(fileIndexItem.ImageFormat),
				Status = fileIndexItem.Status,
				ImageFormat = fileIndexItem.ImageFormat,
				SubPath = fileIndexItem.FilePath!,
				FullFilePath = appSettings.DatabasePathToFilePath(fileIndexItem.FilePath!)
			})
			.ToList();
	}

	private string GetDesktopEditorPath(ExtensionRolesHelper.ImageFormat imageFormat)
	{
		var appSettingsDefaultEditor = appSettings.DefaultDesktopEditor.Find(p =>
			p.ImageFormats.Contains(imageFormat));

		var appPath = appSettingsDefaultEditor?.ApplicationPath ?? string.Empty;

		if ( string.IsNullOrEmpty(appPath) )
		{
			return string.Empty;
		}

		// Under macOS the ApplicationPath is a .app folder
		// Under Windows the ApplicationPath is a .exe file
		if ( _hostFileSystem.IsFolderOrFile(appPath) !=
		     FolderOrFileModel.FolderOrFileTypeList.Deleted )
		{
			return appPath;
		}

		logger.LogError("[OpenEditorPreflight] AppPath not found: " + appPath);
		return string.Empty;
	}

	internal async Task<List<FileIndexItem>> GetObjectsToOpenFromDatabase(
		List<string> inputFilePaths, bool collections)
	{
		var resultFileIndexItemsList = await query.GetObjectsByFilePathAsync(
			inputFilePaths, collections);
		var fileIndexList = new List<FileIndexItem>();

		foreach ( var fileIndexItem in resultFileIndexItemsList )
		{
			// Files that are not on disk
			if ( _iStorage.IsFolderOrFile(fileIndexItem.FilePath!) ==
			     FolderOrFileModel.FolderOrFileTypeList.Deleted )
			{
				StatusCodesHelper.ReturnExifStatusError(fileIndexItem,
					FileIndexItem.ExifStatus.NotFoundSourceMissing,
					fileIndexList);
				continue;
			}

			// Dir is readonly / don't edit
			if ( new StatusCodesHelper(appSettings).IsReadOnlyStatus(fileIndexItem)
			     == FileIndexItem.ExifStatus.ReadOnly )
			{
				StatusCodesHelper.ReturnExifStatusError(fileIndexItem,
					FileIndexItem.ExifStatus.ReadOnly,
					fileIndexList);
				continue;
			}

			if ( fileIndexItem.ImageFormat is ExtensionRolesHelper.ImageFormat.xmp
			    or ExtensionRolesHelper.ImageFormat.meta_json )
			{
				continue;
			}

			if ( fileIndexItem.Status is FileIndexItem.ExifStatus.Default
			    or FileIndexItem.ExifStatus.OkAndSame )
			{
				fileIndexItem.Status = FileIndexItem.ExifStatus.Ok;
			}

			fileIndexList.Add(fileIndexItem);
		}

		return fileIndexList.DistinctBy(p => p.FilePath).ToList();
	}

	internal List<FileIndexItem> GroupByFileCollectionName(
		IEnumerable<FileIndexItem> fileIndexInputList, bool collections = true)
	{
		// Skip if no collections, no need to filter on the right file
		if ( !collections )
		{
			return fileIndexInputList.ToList();
		}

		if ( appSettings.DesktopCollectionsOpen is CollectionsOpenType.RawJpegMode.Default )
		{
			appSettings.DesktopCollectionsOpen = CollectionsOpenType.RawJpegMode.Jpeg;
		}

		var toOpenResultList = new List<FileIndexItem>();

		var groupedByName = fileIndexInputList.GroupBy(item => item.FileCollectionName);
		foreach ( var group in groupedByName )
		{
			if ( group.Count() == 1 )
			{
				toOpenResultList.AddRange(group);
				continue;
			}

			var byOrderResultList = new List<FileIndexItem>();

			switch ( appSettings.DesktopCollectionsOpen )
			{
				case CollectionsOpenType.RawJpegMode.Jpeg:
					byOrderResultList.AddRange(group.Where(p =>
						p.ImageFormat is ExtensionRolesHelper.ImageFormat.jpg
							or ExtensionRolesHelper.ImageFormat.bmp
							or ExtensionRolesHelper.ImageFormat.png
							or ExtensionRolesHelper.ImageFormat.gif
					));
					break;
				case CollectionsOpenType.RawJpegMode.Raw:
					byOrderResultList.AddRange(group.Where(p =>
						p.ImageFormat == ExtensionRolesHelper.ImageFormat.tiff));
					break;
			}

			// When files are not found in the list, take the first one
			if ( byOrderResultList.Count == 0 && group.FirstOrDefault() != null )
			{
				byOrderResultList.Add(group.First());
			}

			var fileIndexItem = byOrderResultList.OrderBy(p => p.ImageFormat).First();
			toOpenResultList.Add(fileIndexItem);
		}

		// could be that the same file is in multiple collections
		return toOpenResultList.DistinctBy(p => p.FilePath).ToList();
	}
}
