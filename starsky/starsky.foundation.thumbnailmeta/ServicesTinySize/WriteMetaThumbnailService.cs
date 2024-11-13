using System;
using System.IO;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using starsky.foundation.database.Models;
using starsky.foundation.injection;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starsky.foundation.thumbnailmeta.Helpers;
using starsky.foundation.thumbnailmeta.Models;
using starsky.foundation.thumbnailmeta.ServicesTinySize.Interfaces;

namespace starsky.foundation.thumbnailmeta.ServicesTinySize;

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
		int sourceHeight, RotationModel.Rotation rotation,
		string? reference = null)
	{
		if ( offsetData.Data == null )
		{
			return false;
		}

		try
		{
			// TODO check if not Index offsetData.Index
			using ( var thumbnailStream =
			       new MemoryStream(offsetData.Data, 0, offsetData.Count) )
			using ( var smallImage = await Image.LoadAsync(thumbnailStream) )
			using ( var outputStream = new MemoryStream() )
			{
				var smallImageWidth = smallImage.Width;
				var smallImageHeight = smallImage.Height;

				var result = NewImageSize.NewImageSizeCalc(smallImageWidth,
					smallImageHeight, sourceWidth, sourceHeight);

				smallImage.Mutate(
					i => i.Resize(smallImageWidth, smallImageHeight, KnownResamplers.Lanczos3)
						.Crop(new Rectangle(result.DestX, result.DestY, result.DestWidth,
							result.DestHeight)));

				var larger = ( int ) Math.Round(result.DestWidth * 1.2, 0);

				smallImage.Mutate(
					i => i.Resize(larger, 0, KnownResamplers.Lanczos3));

				smallImage.Mutate(
					i => i.Rotate(rotation.ToDegrees()));

				await smallImage.SaveAsJpegAsync(outputStream);

				await _thumbnailStorage.WriteStreamAsync(outputStream,
					ThumbnailNameHelper.Combine(fileHash, ThumbnailSize.TinyMeta));
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
			_logger.LogInformation(
				$"[WriteFile@meta] Meta data read - Exception {reference} {ImageErrorMessage.Error(exception)} - can continue without",
				exception);
			return false;
		}
	}
}
