using System;
using System.IO;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Processing;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starsky.foundation.thumbnailmeta.Helpers;
using starsky.foundation.thumbnailmeta.Models;
using starsky.foundation.thumbnailmeta.ServicesPreviewSize.Interfaces;

namespace starsky.foundation.thumbnailmeta.ServicesPreviewSize;

public class WritePreviewThumbnailService : IWritePreviewThumbnailService
{
	private readonly AppSettings _appSettings;
	private readonly IWebLogger _logger;
	private readonly IStorage _thumbnailStorage;

	public WritePreviewThumbnailService(ISelectorStorage selectorStorage, IWebLogger logger,
		AppSettings appSettings)
	{
		_thumbnailStorage = selectorStorage.Get(SelectorStorage.StorageServices.Thumbnail);
		_logger = logger;
		_appSettings = appSettings;
	}

	public async Task<bool> WriteFile(string fileHash, OffsetModel offsetData,
		RotationModel.Rotation rotation,
		string? reference = null)
	{
		if ( offsetData.Data == null )
		{
			return false;
		}

		var previewIdentify =
			await Image.IdentifyAsync(new DecoderOptions(), new MemoryStream(offsetData.Data));

		var thumbnailFromThumbnailUpdateList =
			new ThumbnailSizesChecker(_thumbnailStorage).ListThumbnailToBeCreated(
				[.. ThumbnailNameHelper.GeneratedThumbnailSizes],
				fileHash);

		foreach ( var size in thumbnailFromThumbnailUpdateList )
		{
			if ( previewIdentify.Width >= size.Width() )
			{
				await ResizeAsync(offsetData, rotation, size, fileHash, reference);
			}
		}

		return true;
	}

	private async Task<bool> ResizeAsync(OffsetModel offsetData, RotationModel.Rotation rotation,
		ThumbnailSize size, string fileHash, string? reference)
	{
		if ( offsetData.Data == null )
		{
			return false;
		}

		try
		{
			using ( var thumbnailStream =
			       new MemoryStream(offsetData.Data, 0, offsetData.Count) )
			using ( var previewImage = await Image.LoadAsync(thumbnailStream) )
			using ( var outputStream = new MemoryStream() )
			{
				previewImage.Mutate(
					i => i.Resize(size.Width(), 0, KnownResamplers.Lanczos3));
				previewImage.Mutate(
					i => i.Rotate(rotation.ToDegrees()));

				await previewImage.SaveAsJpegAsync(outputStream);

				await _thumbnailStorage.WriteStreamAsync(outputStream,
					ThumbnailNameHelper.Combine(fileHash, size));

				if ( _appSettings.ApplicationType == AppSettings.StarskyAppType.WebController )
				{
					_logger.LogInformation(
						$"[WriteAndCropFile] fileHash: {fileHash} is written");
				}
			}
		}
		catch ( Exception exception )
		{
			_logger.LogInformation(
				$"[WriteFile@preview] Preview read - Exception {reference} " +
				$"{ImageErrorMessage.Error(exception)} - can continue without",
				exception);
			return false;
		}

		return true;
	}
}
