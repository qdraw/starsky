using starsky.feature.desktop.Interfaces;
using starsky.feature.desktop.Models;
using starsky.foundation.database.Helpers;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.injection;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Models;

namespace starsky.feature.desktop.Service;

[Service(typeof(IOpenEditorPreflight), InjectionLifetime = InjectionLifetime.Scoped)]
public class OpenEditorPreflight : IOpenEditorPreflight
{
	private readonly IQuery _query;
	private readonly AppSettings _appSettings;
	private readonly IStorage _iStorage;

	public OpenEditorPreflight(IQuery query, AppSettings appSettings, IStorage iStorage)
	{
		_query = query;
		_appSettings = appSettings;
		_iStorage = iStorage;
	}

	public async Task<List<PathImageFormatExistsAppPathModel>> PreflightAsync(
		List<string> inputFilePaths, bool collections)
	{
		var fileIndexItemList = await GetObjectsToOpenFromDatabase(inputFilePaths, collections);


		var subPathAndImageFormatList = new List<PathImageFormatExistsAppPathModel>();

		foreach ( var fileIndexItem in fileIndexItemList )
		{
			var appSettingsDefaultEditor =
				_appSettings.DefaultDesktopEditor.Find(p =>
					p.ImageFormats.Contains(fileIndexItem.ImageFormat));

			subPathAndImageFormatList.Add(new PathImageFormatExistsAppPathModel
			{
				AppPath = appSettingsDefaultEditor?.ApplicationPath ?? string.Empty,
				Exists = true,
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

			fileIndexList.Add(fileIndexItem);
		}

		return fileIndexList;
	}
}
