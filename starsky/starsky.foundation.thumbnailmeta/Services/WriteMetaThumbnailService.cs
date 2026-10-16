using System;
using System.IO;
using System.Threading.Tasks;
using starsky.foundation.database.Models;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Thumbnails;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starsky.foundation.thumbnailmeta.Interfaces;
using starsky.foundation.thumbnailmeta.Models;

namespace starsky.foundation.thumbnailmeta.Services;

[Service(typeof(IWriteMetaThumbnailService), InjectionLifetime = InjectionLifetime.Scoped)]
public sealed class WriteMetaThumbnailService : IWriteMetaThumbnailService
{
	private readonly AppSettings _appSettings;
	private readonly IWebLogger _logger;
	private readonly IMetaThumbnailImageProcessor _metaThumbnailImageProcessor =
		new MetaThumbnailImageProcessorFactory().Create();

	private readonly IStorage _thumbnailStorage;

	public WriteMetaThumbnailService(ISelectorStorage selectorStorage, IWebLogger logger,
		AppSettings appSettings)
	{
		_thumbnailStorage = selectorStorage.Get(SelectorStorage.StorageServices.Thumbnail);
		_logger = logger;
		_appSettings = appSettings;
	}

	public async Task<bool> WriteAndCropFile(string fileHash,
		OffsetModel offsetData, int sourceWidth,
		int sourceHeight, ImageRotation.Rotation rotation,
		string? reference = null)
	{
		if ( offsetData.Data == null )
		{
			return false;
		}

		try
		{
			using ( var thumbnailStream =
			       new MemoryStream(offsetData.Data, offsetData.Index, offsetData.Count) )
			using ( var outputStream = new MemoryStream() )
			{
				_metaThumbnailImageProcessor.CropResizeRotate(thumbnailStream, sourceWidth,
					sourceHeight, rotation, _appSettings.ThumbnailImageFormat, outputStream);

				await _thumbnailStorage.WriteStreamAsync(outputStream,
					ThumbnailNameHelper.Combine(fileHash, ThumbnailSize.TinyMeta,
						_appSettings.ThumbnailImageFormat));
				if ( _appSettings.ApplicationType == AppSettings.StarskyAppType.WebController )
				{
					_logger.LogInformation(
						$"[WriteAndCropFile] fileHash: {fileHash} is written");
				}
			}

			return true;
		}
		catch ( Exception exception )
		{
			const string imageCannotBeLoadedErrorMessage = "Image cannot be loaded";

			var message = exception.Message;
			if ( message.StartsWith(imageCannotBeLoadedErrorMessage) )
			{
				message = imageCannotBeLoadedErrorMessage;
			}

			_logger.LogInformation(
				$"[WriteFile@meta] Meta data read - Exception {reference} {message} - can continue without",
				exception);
			return false;
		}
	}
}
