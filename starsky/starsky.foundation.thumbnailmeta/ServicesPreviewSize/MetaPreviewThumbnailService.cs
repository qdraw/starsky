using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.injection;
using starsky.foundation.platform.Extensions;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.readmeta.ReadMetaHelpers;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starsky.foundation.thumbnailmeta.Helpers;
using starsky.foundation.thumbnailmeta.ServicesPreviewSize.Interfaces;

namespace starsky.foundation.thumbnailmeta.ServicesPreviewSize;

[Service(typeof(IMetaPreviewThumbnailService), InjectionLifetime = InjectionLifetime.Scoped)]
public class MetaPreviewThumbnailService : IMetaPreviewThumbnailService
{
	private readonly AppSettings _appSettings;
	private readonly IStorage _iStorage;
	private readonly IWebLogger _logger;
	private readonly IOffsetDataMetaExifPreviewThumbnail _offsetDataMetaExifPreviewThumbnail;
	private readonly IWritePreviewThumbnailService _writePreviewThumbnailService;

	/// <summary>
	///     Get MetaData from Thumbnail
	/// </summary>
	/// <param name="appSettings">Settings</param>
	/// <param name="selectorStorage">Where to get</param>
	/// <param name="offsetDataMetaExifPreviewThumbnail">Get Meta Data for offset</param>
	/// <param name="writePreviewThumbnailService">Writing service</param>
	/// <param name="logger">log all the things</param>
	public MetaPreviewThumbnailService(AppSettings appSettings, ISelectorStorage selectorStorage,
		IOffsetDataMetaExifPreviewThumbnail offsetDataMetaExifPreviewThumbnail,
		IWritePreviewThumbnailService writePreviewThumbnailService, IWebLogger logger)
	{
		_appSettings = appSettings;
		_iStorage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
		_offsetDataMetaExifPreviewThumbnail = offsetDataMetaExifPreviewThumbnail;
		_writePreviewThumbnailService = writePreviewThumbnailService;
		_logger = logger;
	}

	/// <summary>
	///     This feature is used to crawl over directories and add this to the thumbnail-folder
	///     Or File
	/// </summary>
	/// <param name="subPath">folder subPath style</param>
	/// <returns>fail/pass, right type, string=subPath, string?2= error reason</returns>
	/// <exception cref="System.IO.FileNotFoundException">if folder/file not exist</exception>
	public async Task<List<(bool, bool, string, string?)>> AddPreviewThumbnail(string subPath)
	{
		return await new AddMetaPreviewDirectoryHelper(_iStorage, _logger, _appSettings)
			.AddMetaPreviewDirectory(AddPreviewThumbnail, subPath);
	}

	/// <summary>
	///     Run for list that contains subPath and FileHash at once create Meta Thumbnail
	/// </summary>
	/// <param name="subPathsAndHash">(subPath, FileHash)</param>
	/// <returns>fail/pass, string=subPath, string?2= error reason</returns>
	public async Task<IEnumerable<(bool, bool, string, string?)>> AddPreviewThumbnail(
		IEnumerable<(string, string)> subPathsAndHash)
	{
		return ( await subPathsAndHash
			.ForEachAsync(async item =>
					await AddPreviewThumbnail(item.Item1, item.Item2),
				_appSettings.MaxDegreesOfParallelism) )!;
	}

	public async Task<(bool, bool, string, string?)> AddPreviewThumbnail(string subPath,
		string fileHash)
	{
		var (statusOkay, rightType, _, reason) =
			new PreflightCheck(_iStorage, _logger).Check(subPath);
		if ( !statusOkay )
		{
			return ( statusOkay, rightType, subPath, reason );
		}

		fileHash = await new PreflightCheck(_iStorage, _logger).GetFileHash(subPath, fileHash);

		var allExifItems = _offsetDataMetaExifPreviewThumbnail.ReadExifMetaDirectory(subPath);

		var offsetData =
			_offsetDataMetaExifPreviewThumbnail.ParseOffsetData(allExifItems, subPath);

		if ( !offsetData.Success )
		{
			return ( false, true, subPath, offsetData.Reason );
		}

		var rotation = ReadMetaExif.GetOrientationFromExifItem(allExifItems);

		var isWriteSuccess = await _writePreviewThumbnailService.WriteFile(
			fileHash,
			offsetData,
			rotation,
			subPath);

		return ( isWriteSuccess, true, subPath, null );
	}
}
