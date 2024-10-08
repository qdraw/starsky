using System;
using System.IO;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using starsky.foundation.database.Models;
using starsky.foundation.injection;
using starsky.foundation.thumbnailmeta.Helpers;
using starsky.foundation.thumbnailmeta.Interfaces;
using starsky.foundation.thumbnailmeta.Models;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;

namespace starsky.foundation.thumbnailmeta.Services;

[Service(typeof(IWriteMetaThumbnailService), InjectionLifetime = InjectionLifetime.Scoped)]
public sealed class WriteMetaThumbnailService : IWriteMetaThumbnailService
{
	private readonly IWebLogger _logger;
	private readonly IStorage _thumbnailStorage;
	private readonly AppSettings _appSettings;

	public WriteMetaThumbnailService(ISelectorStorage selectorStorage, IWebLogger logger,
		AppSettings appSettings)
	{
		_thumbnailStorage = selectorStorage.Get(SelectorStorage.StorageServices.Thumbnail);
		_logger = logger;
		_appSettings = appSettings;
	}

	public async Task<bool> WriteAndCropFile(string fileHash,
		OffsetModel offsetData, int sourceWidth,
		int sourceHeight, FileIndexItem.Rotation rotation,
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

				var rotate = RotateEnumToDegrees(rotation);
				smallImage.Mutate(
					i => i.Rotate(rotate));

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

	internal static float RotateEnumToDegrees(FileIndexItem.Rotation rotation)
	{
		// ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
		switch ( rotation )
		{
			case FileIndexItem.Rotation.Rotate180:
				return 180;
			case FileIndexItem.Rotation.Rotate90Cw:
				return 90;
			case FileIndexItem.Rotation.Rotate270Cw:
				return 270;
			default:
				return 0;
		}
	}
}
