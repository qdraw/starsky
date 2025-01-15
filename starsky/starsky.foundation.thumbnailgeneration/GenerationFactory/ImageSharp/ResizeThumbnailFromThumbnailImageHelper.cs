using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Extensions;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Thumbnails;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starsky.foundation.thumbnailgeneration.GenerationFactory.Shared;
using starsky.foundation.thumbnailgeneration.Models;

namespace starsky.foundation.thumbnailgeneration.GenerationFactory.ImageSharp;

internal class ResizeThumbnailFromThumbnailImageHelper(
	ISelectorStorage selectorStorage,
	IWebLogger logger)
{
	private readonly IStorage _thumbnailStorage =
		selectorStorage.Get(SelectorStorage.StorageServices.Thumbnail);

	internal async Task<IEnumerable<GenerationResultModel>?>
		ResizeThumbnailFromThumbnailImageLoop(
			string singleSubPath, string fileHash, ThumbnailImageFormat imageFormat,
			List<ThumbnailSize> thumbnailSizes,
			ThumbnailSize toGenerateSize)
	{
		var results = await thumbnailSizes.Skip(1).ForEachAsync(
			async size
				=> await ResizeThumbnailFromThumbnailImage(
					fileHash, // source location
					toGenerateSize,
					ThumbnailNameHelper.GetSize(size),
					singleSubPath, // used for reference only
					fileHash, false, imageFormat),
			thumbnailSizes.Count);
		return results;
	}


	/// <summary>
	///     Resize image from other thumbnail
	/// </summary>
	/// <param name="fileHash">source location</param>
	/// <param name="width">width in pixels</param>
	/// <param name="inputThumbnailSize">thumbnail size</param>
	/// <param name="thumbnailOutputHash">name of output file (DO NOT include size)</param>
	/// <param name="removeExif">remove meta data</param>
	/// <param name="imageFormat">jpg, or png</param>
	/// <param name="subPathReference">for reference only</param>
	/// <returns>Result model</returns>
	internal async Task<GenerationResultModel> ResizeThumbnailFromThumbnailImage(
		string fileHash, // source location
		ThumbnailSize inputThumbnailSize,
		int width, string? subPathReference, string thumbnailOutputHash,
		bool removeExif,
		ThumbnailImageFormat imageFormat
	)
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
			SubPath = subPathReference!,
			ImageFormat = imageFormat,
			Size = ThumbnailNameHelper.GetSize(width)
		};

		try
		{
			// resize the image and save it to the output stream
			var inputFileHashWithExtension =
				ThumbnailNameHelper.Combine(fileHash, inputThumbnailSize, imageFormat);
			using ( var inputStream = _thumbnailStorage.ReadStream(inputFileHashWithExtension) )
			using ( var image = await Image.LoadAsync(inputStream) )
			{
				ImageSharpImageResizeHelper.ImageSharpImageResize(image, width, removeExif);
				await SaveThumbnailImageFormatHelper.SaveThumbnailImageFormat(image, imageFormat,
					outputStream);

				// only when a hash exists
				var outputFileHashWithExtension =
					ThumbnailNameHelper.Combine(thumbnailOutputHash, result.Size, imageFormat);
				await _thumbnailStorage.WriteStreamAsync(outputStream, outputFileHashWithExtension);
				// Disposed in WriteStreamAsync

				new RemoveCorruptThumbnail(selectorStorage).RemoveIfCorrupt(
					outputFileHashWithExtension);
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
			return result;
		}

		return result;
	}
}
