using System;
using System.IO;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starsky.foundation.thumbnailgeneration.Models;

namespace starsky.foundation.thumbnailgeneration.GenerationFactory.ImageSharp;

public class ResizeThumbnailFromSourceImageHelper(
	ISelectorStorage selectorStorage,
	IWebLogger logger)
{
	private readonly IStorage _storage =
		selectorStorage.Get(SelectorStorage.StorageServices.SubPath);

	private readonly IStorage _thumbnailStorage =
		selectorStorage.Get(SelectorStorage.StorageServices.Thumbnail);

	public async Task<(MemoryStream?, GenerationResultModel)> ResizeThumbnailFromSourceImage(
		string subPath,
		int width, string? thumbnailOutputHash = null,
		bool removeExif = false,
		ExtensionRolesHelper.ImageFormat imageFormat = ExtensionRolesHelper.ImageFormat.jpg)
	{
		var outputStream = new MemoryStream();
		var result = new GenerationResultModel
		{
			FileHash = ThumbnailNameHelper.RemoveSuffix(thumbnailOutputHash),
			IsNotFound = false,
			SizeInPixels = width,
			Success = true,
			SubPath = subPath
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
				await _thumbnailStorage.WriteStreamAsync(outputStream, thumbnailOutputHash);
				// Disposed in WriteStreamAsync
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
