using starsky.foundation.database.Helpers;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Models;
using starsky.foundation.storage.Storage;

namespace starsky.feature.trash.Helpers;

public class TrashPreflight
{
	private readonly IQuery _query;
	private readonly IStorage _iStorage;
	private readonly AppSettings _appSettings;

	public TrashPreflight(IQuery query, AppSettings appSettings, ISelectorStorage selectorStorage)
	{
		_query = query;
		_appSettings = appSettings;
		_iStorage = selectorStorage.Get(
			SelectorStorage.StorageServices.SubPath);
	}

	public async Task<List<FileIndexItem>>
		PreflightAsync(List<string> inputFilePaths,
			bool collections)
	{
		var fileIndexUpdateList = new List<FileIndexItem>();
		var resultFileIndexItemsList = await _query.GetObjectsByFilePathAsync(
			inputFilePaths, collections);

		foreach ( var fileIndexItem in resultFileIndexItemsList )
		{
			// Files that are not on disk
			if ( _iStorage!.IsFolderOrFile(fileIndexItem.FilePath!) ==
			     FolderOrFileModel.FolderOrFileTypeList.Deleted )
			{
				StatusCodesHelper.ReturnExifStatusError(fileIndexItem,
					FileIndexItem.ExifStatus.NotFoundSourceMissing,
					fileIndexUpdateList);
				continue;
			}

			// Dir is readonly / don't edit
			if ( new StatusCodesHelper(_appSettings).IsReadOnlyStatus(fileIndexItem)
			     == FileIndexItem.ExifStatus.ReadOnly )
			{
				StatusCodesHelper.ReturnExifStatusError(fileIndexItem,
					FileIndexItem.ExifStatus.ReadOnly,
					fileIndexUpdateList);
				continue;
			}

			// this one is good :)
			if ( fileIndexItem.Status is FileIndexItem.ExifStatus.Default
			    or FileIndexItem.ExifStatus.OkAndSame )
			{
				fileIndexItem.Status = FileIndexItem.ExifStatus.Ok;
			}

			// Deleted is allowed but the status need be updated
			if ( ( StatusCodesHelper.IsDeletedStatus(fileIndexItem)
			       == FileIndexItem.ExifStatus.Deleted ) )
			{
				fileIndexItem.Status = FileIndexItem.ExifStatus.Deleted;
			}
		}

		return resultFileIndexItemsList;
	}

}
