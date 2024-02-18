using starsky.feature.desktop.Interfaces;
using starsky.feature.desktop.Models;
using starsky.foundation.database.Helpers;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.injection;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Models;
using starsky.foundation.storage.Storage;

namespace starsky.feature.desktop.Service;

[Service(typeof(IOpenEditorPreflight), InjectionLifetime = InjectionLifetime.Scoped)]
public class OpenEditorPreflight : IOpenEditorPreflight
{
	private readonly IQuery _query;
	private readonly AppSettings _appSettings;
	private readonly IStorage _iStorage;

	public OpenEditorPreflight(IQuery query, AppSettings appSettings,
		ISelectorStorage selectorStorage)
	{
		_query = query;
		_appSettings = appSettings;
		_iStorage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
	}

	public async Task<List<PathImageFormatExistsAppPathModel>> PreflightAsync(
		List<string> inputFilePaths, bool collections)
	{
		var fileIndexItemList = await GetObjectsToOpenFromDatabase(inputFilePaths, collections);
		fileIndexItemList = GroupByFileCollectionName(fileIndexItemList);

		var subPathAndImageFormatList = new List<PathImageFormatExistsAppPathModel>();

		foreach ( var fileIndexItem in fileIndexItemList )
		{
			var appSettingsDefaultEditor = _appSettings.DefaultDesktopEditor.Find(p =>
				p.ImageFormats.Contains(fileIndexItem.ImageFormat));

			subPathAndImageFormatList.Add(new PathImageFormatExistsAppPathModel
			{
				AppPath = appSettingsDefaultEditor?.ApplicationPath ?? string.Empty,
				Status = fileIndexItem.Status,
				ImageFormat = fileIndexItem.ImageFormat,
				SubPath = fileIndexItem.FilePath!,
				FullFilePath = _appSettings.DatabasePathToFilePath(fileIndexItem.FilePath!)
			});
		}

		return subPathAndImageFormatList;
	}

	private async Task<List<FileIndexItem>> GetObjectsToOpenFromDatabase(
		List<string> inputFilePaths, bool collections)
	{
		var resultFileIndexItemsList = await _query.GetObjectsByFilePathAsync(
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
			if ( new StatusCodesHelper(_appSettings).IsReadOnlyStatus(fileIndexItem)
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

		return fileIndexList;
	}

	internal List<FileIndexItem> GroupByFileCollectionName(
		IEnumerable<FileIndexItem> fileIndexInputList)
	{
		if ( _appSettings.DesktopCollectionsOpen is CollectionsOpenType.RawJpegMode.Default )
		{
			_appSettings.DesktopCollectionsOpen = CollectionsOpenType.RawJpegMode.Jpeg;
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

			switch ( _appSettings.DesktopCollectionsOpen )
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

		return toOpenResultList;
	}
}
