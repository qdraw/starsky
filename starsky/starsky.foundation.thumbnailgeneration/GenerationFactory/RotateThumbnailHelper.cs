using System;
using System.IO;
using System.Threading.Tasks;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starsky.foundation.thumbnailgeneration.GenerationFactory.Skia;

namespace starsky.foundation.thumbnailgeneration.GenerationFactory;

public class RotateThumbnailHelper(
	ISelectorStorage selectorStorage,
	AppSettings appSettings,
	IWebLogger logger)
{
	private readonly IStorage
		_thumbnailStorage = selectorStorage.Get(SelectorStorage.StorageServices.Thumbnail);

	/// <summary>
	///     Rotate an image, by rotating the pixels and resize the thumbnail.Please do not apply any
	///     orientation exif-tag on this file
	/// </summary>
	/// <param name="fileHash"></param>
	/// <param name="orientation">-1 > Rotate -90degrees, anything else 90 degrees</param>
	/// <param name="width">to resize, default 1000</param>
	/// <param name="height">to resize, default keep ratio (0)</param>
	/// <returns>Is successful? // private feature</returns>
	internal async Task<bool> RotateThumbnail(string fileHash, int orientation, int width = 1000,
		int height = 0)
	{
		if ( !_thumbnailStorage.ExistFile(fileHash) )
		{
			return false;
		}

		// the orientation is -1 or 1
		var degrees = orientation == -1 ? 270 : 90;

		try
		{
			using ( var inputStream = _thumbnailStorage.ReadStream(fileHash) )
			using ( var image = SkiaImageHelper.Decode(inputStream) )
			using ( var resized = SkiaImageHelper.Resize(image, width, height) )
			using ( var rotated = SkiaImageHelper.Rotate(resized, degrees) )
			using ( var stream = new MemoryStream() )
			{
				SkiaImageHelper.Encode(rotated, appSettings.ThumbnailImageFormat, stream);
				await _thumbnailStorage.WriteStreamAsync(stream, fileHash);
			}
		}
		catch ( Exception ex )
		{
			logger.LogError(ex, "[RotateThumbnailHelper] " + ex.Message);
			return false;
		}

		return true;
	}
}
