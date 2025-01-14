using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using starsky.feature.import.Helpers;
using starsky.feature.import.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Thumbnails;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;

namespace starsky.feature.import.Services;

[Service(typeof(IImportThumbnailService), InjectionLifetime = InjectionLifetime.Scoped)]
public class ImportThumbnailService : IImportThumbnailService
{
	private readonly AppSettings _appSettings;
	private readonly IStorage _hostFileSystemStorage;
	private readonly IWebLogger _logger;
	private readonly RemoveTempAndParentStreamFolderHelper _removeTempAndParentStreamFolderHelper;
	private readonly IStorage _thumbnailStorage;

	public ImportThumbnailService(ISelectorStorage selectorStorage, IWebLogger logger,
		AppSettings appSettings)
	{
		_logger = logger;
		_appSettings = appSettings;
		_hostFileSystemStorage =
			selectorStorage.Get(SelectorStorage.StorageServices.HostFilesystem);
		_thumbnailStorage = selectorStorage.Get(SelectorStorage.StorageServices.Thumbnail);
		_removeTempAndParentStreamFolderHelper =
			new RemoveTempAndParentStreamFolderHelper(_hostFileSystemStorage, _appSettings);
	}

	public IEnumerable<ThumbnailResultDataTransferModel> MapToTransferObject(
		List<string> thumbnailNames)
	{
		var items = new List<ThumbnailResultDataTransferModel>();
		foreach ( var thumbnailNameWithSuffix in thumbnailNames )
		{
			var thumb = ThumbnailNameHelper.GetSize(thumbnailNameWithSuffix,
				_appSettings.ThumbnailImageFormat);
			var name = ThumbnailNameHelper.RemoveSuffix(thumbnailNameWithSuffix);
			var item = new ThumbnailResultDataTransferModel(name);
			item.Change(thumb, true);
			items.Add(item);
		}

		return items;
	}

	public List<string> GetThumbnailNamesWithSuffix(List<string> tempImportPaths)
	{
		var thumbnailNamesWithSuffix = new List<string>();
		// ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
		foreach ( var tempImportSinglePath in tempImportPaths )
		{
			var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(tempImportSinglePath);

			var thumbToUpperCase = fileNameWithoutExtension.ToUpperInvariant();

			_logger.LogInformation($"[Import/Thumbnail] - {thumbToUpperCase}");

			if ( ThumbnailNameHelper.GetSize(thumbToUpperCase, _appSettings.ThumbnailImageFormat) ==
			     ThumbnailSize.Unknown )
			{
				continue;
			}

			// remove existing thumbnail if exist
			if ( _thumbnailStorage.ExistFile(thumbToUpperCase) )
			{
				_logger.LogInformation(
					$"[Import/Thumbnail] remove already exists - {thumbToUpperCase}");
				_thumbnailStorage.FileDelete(thumbToUpperCase);
			}

			thumbnailNamesWithSuffix.Add(thumbToUpperCase);
		}

		return thumbnailNamesWithSuffix;
	}

	public async Task<bool> WriteThumbnails(List<string> tempImportPaths,
		List<string> thumbnailNames)
	{
		if ( tempImportPaths.Count != thumbnailNames.Count )
		{
			_removeTempAndParentStreamFolderHelper.RemoveTempAndParentStreamFolder(tempImportPaths);
			return false;
		}

		for ( var i = 0; i < tempImportPaths.Count; i++ )
		{
			if ( !_hostFileSystemStorage.ExistFile(tempImportPaths[i]) )
			{
				_logger.LogInformation(
					$"[Import/Thumbnail] ERROR {tempImportPaths[i]} does not exist");
				continue;
			}

			await _thumbnailStorage.WriteStreamAsync(
				_hostFileSystemStorage.ReadStream(tempImportPaths[i]), thumbnailNames[i]);

			// Remove from temp folder to avoid long list of files
			_removeTempAndParentStreamFolderHelper.RemoveTempAndParentStreamFolder(
				tempImportPaths[i]);
		}

		return true;
	}
}
