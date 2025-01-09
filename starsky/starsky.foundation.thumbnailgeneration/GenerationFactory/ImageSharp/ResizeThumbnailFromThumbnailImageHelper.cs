using System;
using System.IO;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starsky.foundation.thumbnailgeneration.Models;

namespace starsky.foundation.thumbnailgeneration.GenerationFactory.ImageSharp;

internal class ResizeThumbnailFromThumbnailImageHelper(
	ISelectorStorage selectorStorage,
	IWebLogger logger)
{
	private readonly IStorage _thumbnailStorage =
		selectorStorage.Get(SelectorStorage.StorageServices.Thumbnail);

	/// <summary>
	///     Resize image from other thumbnail
	/// </summary>
	/// <param name="fileHash">source location</param>
	/// <param name="width">width in pixels</param>
	/// <param name="thumbnailOutputHash">name of output file</param>
	/// <param name="removeExif">remove meta data</param>
	/// <param name="imageFormat">jpg, or png</param>
	/// <param name="subPathReference">for reference only</param>
	/// <returns>(stream, fileHash, and is ok)</returns>
	internal async Task<(MemoryStream?, GenerationResultModel)> ResizeThumbnailFromThumbnailImage(
		string fileHash, // source location
		int width, string? subPathReference, string? thumbnailOutputHash,
		bool removeExif,
		ThumbnailImageFormat imageFormat
	)
	{
		var outputStream = new MemoryStream();
		var result = new GenerationResultModel
		{
			FileHash = ThumbnailNameHelper.RemoveSuffix(thumbnailOutputHash),
			IsNotFound = false,
			SizeInPixels = width,
			Success = true,
			SubPath = subPathReference!
		};

		try
		{
			// resize the image and save it to the output stream
			using ( var inputStream = _thumbnailStorage.ReadStream(fileHash) )
			using ( var image = await Image.LoadAsync(inputStream) )
			{
				ImageSharpImageResizeHelper.ImageSharpImageResize(image, width, removeExif);
				await SaveThumbnailImageFormatHelper.SaveThumbnailImageFormat(image, imageFormat,
					outputStream);

				// When thumbnailOutputHash is nothing return stream instead of writing down
				if ( string.IsNullOrEmpty(thumbnailOutputHash) )
				{
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

			logger.LogError($"[ResizeThumbnailFromThumbnailImage] Exception {fileHash} {message}",
				ex);
			result.Success = false;
			result.ErrorMessage = message;
			return ( null, result );
		}

		return ( outputStream, result );
	}
}
