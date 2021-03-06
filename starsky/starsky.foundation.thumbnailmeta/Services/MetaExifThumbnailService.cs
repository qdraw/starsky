using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using starsky.foundation.injection;
using starsky.foundation.metathumbnail.Interfaces;
using starsky.foundation.platform.Extensions;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Models;
using starsky.foundation.storage.Services;
using starsky.foundation.storage.Storage;

namespace starsky.foundation.metathumbnail.Services
{
	[Service(typeof(IMetaExifThumbnailService), InjectionLifetime = InjectionLifetime.Scoped)]
	public class MetaExifThumbnailService : IMetaExifThumbnailService
	{
		private readonly IStorage _iStorage;
		private readonly IStorage _thumbnailStorage;
		private readonly IWebLogger _logger;
		private readonly IOffsetDataMetaExifThumbnail _offsetDataMetaExifThumbnail;
		private readonly IWriteMetaThumbnailService _writeMetaThumbnailService;
		private readonly AppSettings _appSettings;

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
		/// Run for list that contains subPath and FileHash at once
		/// </summary>
		/// <param name="subPathsAndHash">(subPath, FileHash)</param>
		/// <returns></returns>
		public async Task<bool> AddMetaThumbnail(IEnumerable<(string, string)> subPathsAndHash)
		{
			await subPathsAndHash
				.ForEachAsync(async item => 
						await AddMetaThumbnail(item.Item1, item.Item2),
					_appSettings.MaxDegreesOfParallelism);

			return true;
		}

		/// <summary>
		///  This feature is used to crawl over directories and add this to the thumbnail-folder
		///  Or File
		/// </summary>
		/// <param name="subPath">folder subPath style</param>
		/// <returns>fail/pass</returns>
		/// <exception cref="FileNotFoundException">if folder/file not exist</exception>
		public async Task<bool> AddMetaThumbnail(string subPath)
		{
			var isFolderOrFile = _iStorage.IsFolderOrFile(subPath);
			// ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
			switch ( isFolderOrFile )
			{
				case FolderOrFileModel.FolderOrFileTypeList.Deleted:
					_logger.LogError($"[AddMetaThumbnail] folder or file not found {subPath}");
					return false;
				case FolderOrFileModel.FolderOrFileTypeList.Folder:
				{
					var contentOfDir = _iStorage.GetAllFilesInDirectoryRecursive(subPath)
						.Where(ExtensionRolesHelper.IsExtensionExifToolSupported).ToList();
					
					await contentOfDir
						.ForEachAsync(async singleSubPath => 
							await AddMetaThumbnail(singleSubPath, null),
							_appSettings.MaxDegreesOfParallelism);

					return true;
				}
				default:
				{
					var result = (await  new FileHash(_iStorage).GetHashCodeAsync(subPath));
					if ( !result.Value ) return false;
					return await AddMetaThumbnail(subPath, result.Key);
				}
			}
		}

		public async Task<bool> AddMetaThumbnail(string subPath, string fileHash)
		{
			var first50BytesStream = _iStorage.ReadStream(subPath,50);
			var imageFormat = ExtensionRolesHelper.GetImageFormat(first50BytesStream);
			if ( imageFormat != ExtensionRolesHelper.ImageFormat.jpg )
			{
				return false;
			}

			if ( string.IsNullOrEmpty(fileHash) )
			{
				var result = (await  new FileHash(_iStorage).GetHashCodeAsync(subPath));
				if ( !result.Value )
				{
					_logger.LogError("[MetaExifThumbnail] hash failed");
					return false;
				}
				fileHash = result.Key;
			}

			if ( !_iStorage.ExistFile(subPath) || _thumbnailStorage.ExistFile(ThumbnailNameHelper.Combine(fileHash,ThumbnailSize.TinyMeta)) )
			{
				return false;
			}
				
			var (exifThumbnailDir, sourceWidth, sourceHeight, rotation) = 
				_offsetDataMetaExifThumbnail.GetExifMetaDirectories(subPath);
			var offsetData = _offsetDataMetaExifThumbnail.
				ParseOffsetData(exifThumbnailDir,subPath);
			if ( !offsetData.Success ) return false;

			return await _writeMetaThumbnailService.WriteAndCropFile(fileHash, offsetData, sourceWidth,
				sourceHeight, rotation, subPath);
		}
	}
}
