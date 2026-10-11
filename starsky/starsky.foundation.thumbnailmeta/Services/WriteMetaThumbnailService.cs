using System;
using System.IO;
using System.Threading.Tasks;
using SkiaSharp;
using starsky.foundation.database.Models;
using starsky.foundation.injection;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Thumbnails;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starsky.foundation.thumbnailgeneration.GenerationFactory.Skia;
using starsky.foundation.thumbnailmeta.Helpers;
using starsky.foundation.thumbnailmeta.Interfaces;
using starsky.foundation.thumbnailmeta.Models;

namespace starsky.foundation.thumbnailmeta.Services;

[Service(typeof(IWriteMetaThumbnailService), InjectionLifetime = InjectionLifetime.Scoped)]
public sealed class WriteMetaThumbnailService : IWriteMetaThumbnailService
{
	private readonly AppSettings _appSettings;
	private readonly IWebLogger _logger;
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
			using ( var smallImage = SkiaImageHelper.Decode(thumbnailStream) )
			using ( var outputStream = new MemoryStream() )
			{
				var result = NewImageSize.NewImageSizeCalc(smallImage.Width,
					smallImage.Height, sourceWidth, sourceHeight);

				using var cropped = SkiaImageHelper.Crop(smallImage, result.DestX,
					result.DestY, result.DestWidth, result.DestHeight);

				var larger = ( int ) Math.Round(result.DestWidth * 1.2, 0);
				using var resized = SkiaImageHelper.Resize(cropped, larger, 0);
				using var rotated = SkiaImageHelper.Rotate(resized, rotation.ToDegrees());

				SaveAsImage(rotated, outputStream);

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

	private void SaveAsImage(SKBitmap image, MemoryStream outputStream)
	{
		if ( _appSettings.ThumbnailImageFormat == ThumbnailImageFormat.webp )
		{
			SkiaImageHelper.Encode(image, SKEncodedImageFormat.Webp, 75, outputStream);
		}
		else
		{
			SkiaImageHelper.Encode(image, SKEncodedImageFormat.Jpeg, 75, outputStream);
		}
	}
}
