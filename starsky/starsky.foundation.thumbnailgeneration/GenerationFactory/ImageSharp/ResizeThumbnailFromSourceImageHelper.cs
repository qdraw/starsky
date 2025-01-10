using System;
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

	internal async Task<(MemoryStream?, GenerationResultModel)> ResizeThumbnailFromSourceImage(
		string subPath,
		int width, string? thumbnailOutputHash,
		bool removeExif,
		ThumbnailImageFormat imageFormat)
	{
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

		try
		{
			// resize the image and save it to the output stream
			using ( var inputStream = _storage.ReadStream(subPath) )
			using ( var image = await Image.LoadAsync(inputStream) )
			{
				ImageSharpImageResizeHelper.ImageSharpImageResize(image, width, removeExif);
				await SaveThumbnailImageFormatHelper.SaveThumbnailImageFormat(image, imageFormat,
					outputStream);

				// When thumbnailOutputHash is nothing return stream instead of writing down
				if ( string.IsNullOrEmpty(thumbnailOutputHash) )
				{
					result.ErrorMessage = "Ok give stream back instead of disk write";
					return ( outputStream, result );
				}

				// only when a hash exists
				var fileHashWithExtension = ThumbnailNameHelper.Combine(thumbnailOutputHash,
					result.Size, imageFormat);

				await _thumbnailStorage.WriteStreamAsync(outputStream, fileHashWithExtension);
				// Disposed in WriteStreamAsync

				new RemoveCorruptThumbnail(_thumbnailStorage).RemoveAndThrow(fileHashWithExtension);
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
			result.Success = false;
			result.ErrorMessage = message;
			return ( null, result );
		}

		result.ErrorMessage = "Ok but written to disk";
		return ( null, result );
	}
}
