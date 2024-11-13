using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using starsky.foundation.injection;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Extensions;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starsky.foundation.thumbnailmeta.Helpers;
using starsky.foundation.thumbnailmeta.ServicesTinySize.Interfaces;

namespace starsky.foundation.thumbnailmeta.ServicesTinySize;

[Service(typeof(IMetaExifThumbnailService), InjectionLifetime = InjectionLifetime.Scoped)]
public sealed class MetaExifThumbnailService : IMetaExifThumbnailService
{
	private readonly AppSettings _appSettings;
	private readonly IStorage _iStorage;
	private readonly IWebLogger _logger;
	private readonly IOffsetDataMetaExifThumbnail _offsetDataMetaExifThumbnail;
	private readonly IStorage _thumbnailStorage;
	private readonly IWriteMetaThumbnailService _writeMetaThumbnailService;

	/// <summary>
	///     Get MetaData from Thumbnail
	/// </summary>
	/// <param name="appSettings">Settings</param>
	/// <param name="selectorStorage">Where to get</param>
	/// <param name="offsetDataMetaExifThumbnail">Get Meta Data for offset</param>
	/// <param name="writeMetaThumbnailService">Writing service</param>
	/// <param name="logger">log all the things</param>
	public MetaExifThumbnailService(AppSettings appSettings, ISelectorStorage selectorStorage,
		IOffsetDataMetaExifThumbnail offsetDataMetaExifThumbnail,
		IWriteMetaThumbnailService writeMetaThumbnailService, IWebLogger logger)
	{
		_appSettings = appSettings;
		_iStorage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
		_thumbnailStorage = selectorStorage.Get(SelectorStorage.StorageServices.Thumbnail);
		_offsetDataMetaExifThumbnail = offsetDataMetaExifThumbnail;
		_writeMetaThumbnailService = writeMetaThumbnailService;
		_logger = logger;
	}

	/// <summary>
	///     Run for list that contains subPath and FileHash at once create Meta Thumbnail
	/// </summary>
	/// <param name="subPathsAndHash">(subPath, FileHash)</param>
	/// <returns>fail/pass, string=subPath, string?2= error reason</returns>
	public async Task<IEnumerable<(bool, bool, string, string?)>> AddMetaThumbnail(
		IEnumerable<(string, string)> subPathsAndHash)
	{
		return ( await subPathsAndHash
			.ForEachAsync(async item =>
					await AddMetaThumbnail(item.Item1, item.Item2),
				_appSettings.MaxDegreesOfParallelism) )!;
	}

	/// <summary>
	///     This feature is used to crawl over directories and add this to the thumbnail-folder
	///     Or File
	/// </summary>
	/// <param name="subPath">folder subPath style</param>
	/// <returns>fail/pass, right type, string=subPath, string?2= error reason</returns>
	/// <exception cref="FileNotFoundException">if folder/file not exist</exception>
	public async Task<List<(bool, bool, string, string?)>> AddMetaThumbnail(string subPath)
	{
		return await new AddMetaPreviewDirectoryHelper(_iStorage, _logger, _appSettings)
			.AddMetaPreviewDirectory(AddMetaThumbnail, subPath);
	}

	/// <summary>
	///     Create Meta Thumbnail
	/// </summary>
	/// <param name="subPath">location on disk</param>
	/// <param name="fileHash">hash</param>
	/// <returns>fail/pass, right type, subPath</returns>
	public async Task<(bool, bool, string, string?)> AddMetaThumbnail(string subPath,
		string fileHash)
	{
		var (statusOkay, rightType, _, reason) =
			new PreflightCheck(_iStorage, _logger).Check(subPath);
		if ( !statusOkay )
		{
			return ( statusOkay, rightType, subPath, reason );
		}

		fileHash = await new PreflightCheck(_iStorage, _logger).GetFileHash(subPath, fileHash);

		if ( _thumbnailStorage.ExistFile(
			    ThumbnailNameHelper.Combine(fileHash, ThumbnailSize.TinyMeta)) )
		{
			return ( true, true, subPath, "already exist" );
		}

		var (exifThumbnailDir, sourceWidth, sourceHeight, rotation) =
			_offsetDataMetaExifThumbnail.GetExifMetaDirectories(subPath);
		var offsetData = _offsetDataMetaExifThumbnail.ParseOffsetData(exifThumbnailDir,
			subPath);
		if ( !offsetData.Success )
		{
			return ( false, true, subPath, offsetData.Reason );
		}

		var isWriteSuccess = await _writeMetaThumbnailService.WriteAndCropFile(
			fileHash,
			offsetData,
			sourceWidth,
			sourceHeight,
			rotation,
			subPath);

		return ( isWriteSuccess, true, subPath, null );
	}
}
