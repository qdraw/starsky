using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starsky.foundation.thumbnailgeneration.GenerationFactory.Shared;
using starsky.foundation.thumbnailgeneration.Models;

[assembly:
	InternalsVisibleTo(nameof(starsky.foundation.thumbnailgeneration.GenerationFactory.Generators))]

namespace starsky.foundation.thumbnailgeneration.GenerationFactory.ImageSharp;

internal class ResizeThumbnailFromSourceImageHelper(
	ISelectorStorage selectorStorage,
	IWebLogger logger)
{
	private readonly IStorage _storage =
		selectorStorage.Get(SelectorStorage.StorageServices.SubPath);

	private readonly IStorage _thumbnailStorage =
		selectorStorage.Get(SelectorStorage.StorageServices.Thumbnail);

	internal async Task<GenerationResultModel> ResizeThumbnailFromSourceImage(
		string subPath,
		int width, string? thumbnailOutputHash,
		bool removeExif,
		ThumbnailImageFormat imageFormat)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(thumbnailOutputHash);
		if ( imageFormat == ThumbnailImageFormat.unknown )
		{
			throw new InvalidEnumArgumentException("imageFormat should not be unknown");
		}

		var outputStream = new MemoryStream();
		var result = new GenerationResultModel
		{
			FileHash = ThumbnailNameHelper.RemoveSuffix(thumbnailOutputHash),
			IsNotFound = false,
			SizeInPixels = width,
			Success = true,
			SubPath = subPath,
			ImageFormat = imageFormat,
			Size = ThumbnailNameHelper.GetSize(width)
		};

		var fileHashWithExtension = ThumbnailNameHelper.Combine(thumbnailOutputHash,
			ThumbnailNameHelper.GetSize(width), imageFormat);

		try
		{
			// resize the image and save it to the output stream
			using ( var inputStream = _storage.ReadStream(subPath) )
			using ( var image = await Image.LoadAsync(inputStream) )
			{
				ImageSharpImageResizeHelper.ImageSharpImageResize(image, width, removeExif);
				await SaveThumbnailImageFormatHelper.SaveThumbnailImageFormat(image, imageFormat,
					outputStream);

				await _thumbnailStorage.WriteStreamAsync(outputStream, fileHashWithExtension);
				// Disposed in WriteStreamAsync

				new RemoveCorruptThumbnail(selectorStorage).RemoveIfCorrupt(fileHashWithExtension);
			}
		}
		catch ( Exception ex )
		{
			const string imageCannotBeLoadedErrorMessage = "Image cannot be loaded";
			var message = ex.Message;
			if ( message.StartsWith(imageCannotBeLoadedErrorMessage) )
			{
				message = imageCannotBeLoadedErrorMessage;
			}

			logger.LogError($"[ResizeThumbnailFromSourceImage] Exception {subPath} {message}", ex);
			logger.LogError(ex.StackTrace);

			result.Success = false;
			result.ErrorMessage = message;

			new RemoveCorruptThumbnail(selectorStorage).RemoveIfCorrupt(fileHashWithExtension);

			return result;
		}

		result.ErrorMessage = "Ok and written to disk";
		return result;
	}
}
