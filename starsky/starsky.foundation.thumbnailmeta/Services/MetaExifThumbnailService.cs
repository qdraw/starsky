using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using starsky.foundation.injection;
using starsky.foundation.platform.Extensions;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Thumbnails;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Models;
using starsky.foundation.storage.Services;
using starsky.foundation.storage.Storage;
using starsky.foundation.thumbnailmeta.Interfaces;

namespace starsky.foundation.thumbnailmeta.Services;

[Service(typeof(IMetaExifThumbnailService), InjectionLifetime = InjectionLifetime.Scoped)]
public sealed class MetaExifThumbnailService : IMetaExifThumbnailService
{
	private readonly AppSettings _appSettings;
	private readonly IStorage _iStorage;
	private readonly IWebLogger _logger;
	private readonly IOffsetDataMetaExifThumbnail _offsetDataMetaExifThumbnail;
	private readonly IStorage _thumbnailStorage;
	private readonly IWriteMetaThumbnailService _writeMetaThumbnailService;

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
		var isFolderOrFile = _iStorage.IsFolderOrFile(subPath);
		// ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
		switch ( isFolderOrFile )
		{
			case FolderOrFileModel.FolderOrFileTypeList.Deleted:
				_logger.LogError($"[AddMetaThumbnail] folder or file not found {subPath}");
				return new List<(bool, bool, string, string?)>
				{
					( false, false, subPath, "folder or file not found" )
				};
			case FolderOrFileModel.FolderOrFileTypeList.Folder:
			{
				var contentOfDir = _iStorage.GetAllFilesInDirectoryRecursive(subPath)
					.Where(ExtensionRolesHelper.IsExtensionExifToolSupported).ToList();

				var results = await contentOfDir
					.ForEachAsync(async singleSubPath =>
							await AddMetaThumbnail(singleSubPath, null!),
						_appSettings.MaxDegreesOfParallelism);

				return results!.ToList();
			}
			default:
			{
				var result = await new FileHash(_iStorage).GetHashCodeAsync(subPath);
				return !result.Value
					? new List<(bool, bool, string, string?)>
					{
						( false, false, subPath, "hash not found" )
					}
					: new List<(bool, bool, string, string?)>
					{
						await AddMetaThumbnail(subPath, result.Key)
					};
			}
		}
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
		if ( !_iStorage.ExistFile(subPath) )
		{
			return ( false, false, subPath, "not found" );
		}

		var first50BytesStream = _iStorage.ReadStream(subPath, 50);
		var imageFormat = ExtensionRolesHelper.GetImageFormat(first50BytesStream);

		if ( imageFormat != ExtensionRolesHelper.ImageFormat.jpg &&
		     imageFormat != ExtensionRolesHelper.ImageFormat.tiff )
		{
			_logger.LogDebug($"[AddMetaThumbnail] {subPath} is not a jpg or tiff file");
			return ( false, false, subPath, $"{subPath} is not a jpg or tiff file" );
		}

		if ( string.IsNullOrEmpty(fileHash) )
		{
			var result = await new FileHash(_iStorage).GetHashCodeAsync(subPath);
			if ( !result.Value )
			{
				_logger.LogError("[MetaExifThumbnail] hash failed");
				return ( false, true, subPath, "hash failed" );
			}

			fileHash = result.Key;
		}

		if ( _thumbnailStorage.ExistFile(
			    ThumbnailNameHelper.Combine(fileHash, ThumbnailSize.TinyMeta,
				    _appSettings.ThumbnailImageFormat)) )
		{
			return ( true, true, subPath, "already exist" );
		}

		var (exifThumbnailDir, sourceWidth, sourceHeight, rotation) =
			_offsetDataMetaExifThumbnail.GetExifMetaDirectories(subPath);
		var offsetData = _offsetDataMetaExifThumbnail.ParseOffsetData(exifThumbnailDir, subPath);
		if ( !offsetData.Success )
		{
			return ( false, true, subPath, offsetData.Reason );
		}

		return ( await _writeMetaThumbnailService.WriteAndCropFile(fileHash, offsetData,
			sourceWidth,
			sourceHeight, rotation, subPath), true, subPath, null );
	}
}
