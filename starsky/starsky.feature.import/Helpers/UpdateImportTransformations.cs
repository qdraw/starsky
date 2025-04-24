using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.readmeta.Services;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Services;
using starsky.foundation.storage.Storage;
using starsky.foundation.writemeta.Helpers;
using starsky.foundation.writemeta.Interfaces;

namespace starsky.feature.import.Helpers;

public class UpdateImportTransformations
{
	public delegate Task<List<ThumbnailItem>?> QueryThumbnailUpdateDelegate(
		List<ThumbnailResultDataTransferModel> thumbnailItems);


	public delegate Task<FileIndexItem> QueryUpdateDelegate(FileIndexItem fileIndexItem);

	private readonly AppSettings _appSettings;
	private readonly IExifTool _exifTool;
	private readonly IWebLogger _logger;
	private readonly IStorage _subPathStorage;
	private readonly IThumbnailQuery _thumbnailQuery;
	private readonly IStorage _thumbnailStorage;

	public UpdateImportTransformations(IWebLogger logger,
		IExifTool exifTool, ISelectorStorage selectorStorage, AppSettings appSettings,
		IThumbnailQuery thumbnailQuery)
	{
		_logger = logger;
		_exifTool = exifTool;
		_subPathStorage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
		_thumbnailStorage = selectorStorage.Get(SelectorStorage.StorageServices.Thumbnail);
		_appSettings = appSettings;
		_thumbnailQuery = thumbnailQuery;
	}

	/// <summary>
	///     Run Transformation on Import to the files in the database and Update fileHash in database
	/// </summary>
	/// <param name="queryUpdateDelegate"></param>
	/// <param name="fileIndexItem">information</param>
	/// <param name="colorClassTransformation">change colorClass</param>
	/// <param name="dateTimeParsedFromFileName">is date time parsed from fileName</param>
	/// <param name="indexMode">should update database</param>
	/// <param name="reverseGeoCode">should reverse geocode</param>
	internal async Task<FileIndexItem> UpdateTransformations(
		QueryUpdateDelegate? queryUpdateDelegate,
		FileIndexItem fileIndexItem,
		int colorClassTransformation, bool dateTimeParsedFromFileName,
		bool indexMode, bool reverseGeoCode)
	{
		if ( !ExtensionRolesHelper.IsExtensionExifToolSupported(fileIndexItem.FileName) )
		{
			return fileIndexItem;
		}

		var comparedNamesList = new List<string>();
		if ( dateTimeParsedFromFileName )
		{
			_logger.LogInformation($"[Import] DateTimeParsedFromFileName " +
			                       $"ExifTool Sync {fileIndexItem.FilePath}");
			comparedNamesList = DateTimeParsedComparedNamesList();
		}

		if ( colorClassTransformation >= 0 )
		{
			_logger.LogInformation($"[Import] ColorClassComparedNamesList " +
			                       $"ExifTool Sync {fileIndexItem.FilePath}");
			comparedNamesList = ColorClassComparedNamesList(comparedNamesList);
		}

		if ( reverseGeoCode )
		{
			comparedNamesList = ReverseGeoCodeComparedNamesList(comparedNamesList);
		}

		if ( comparedNamesList.Count == 0 )
		{
			return fileIndexItem;
		}

		// Update ColorClass and DateTime if requested
		var exifToolCmdHelper = new ExifToolCmdHelper(_exifTool,
			_subPathStorage, _thumbnailStorage,
			new ReadMeta(_subPathStorage, _appSettings, null!, _logger),
			_thumbnailQuery, _logger);
		await exifToolCmdHelper.UpdateAsync(fileIndexItem, comparedNamesList);

		// Only update database when indexMode is true
		if ( !indexMode || queryUpdateDelegate == null )
		{
			return fileIndexItem;
		}

		// Hash is changed after transformation
		fileIndexItem.FileHash =
			( await new FileHash(_subPathStorage).GetHashCodeAsync(fileIndexItem.FilePath!) )
			.Key;

		await queryUpdateDelegate(fileIndexItem);

		return fileIndexItem.Clone();
	}

	internal static List<string> DateTimeParsedComparedNamesList()
	{
		return new List<string>
		{
			nameof(FileIndexItem.Description).ToLowerInvariant(),
			nameof(FileIndexItem.DateTime).ToLowerInvariant()
		};
	}

	internal static List<string> ColorClassComparedNamesList(List<string> list)
	{
		list.Add(nameof(FileIndexItem.ColorClass).ToLowerInvariant());
		return list;
	}

	internal static List<string> ReverseGeoCodeComparedNamesList(List<string> list)
	{
		list.Add(nameof(FileIndexItem.LocationCity).ToLowerInvariant());
		list.Add(nameof(FileIndexItem.LocationState).ToLowerInvariant());
		list.Add(nameof(FileIndexItem.LocationCountry).ToLowerInvariant());
		list.Add(nameof(FileIndexItem.LocationCountryCode).ToLowerInvariant());
		return list;
	}
}
