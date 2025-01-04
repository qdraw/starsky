using System;
using System.IO;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using starsky.foundation.platform.Helpers;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starsky.foundation.thumbnailgeneration.GenerationFactory.ImageSharp;

namespace starsky.foundation.thumbnailgeneration.GenerationFactory;

public class RotateThumbnailHelper(ISelectorStorage selectorStorage)
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
		var rotateMode = RotateMode.Rotate90;
		if ( orientation == -1 )
		{
			rotateMode = RotateMode.Rotate270;
		}

		try
		{
			using ( var inputStream = _thumbnailStorage.ReadStream(fileHash) )
			using ( var image = await Image.LoadAsync(inputStream) )
			using ( var stream = new MemoryStream() )
			{
				image.Mutate(x => x
					.Resize(width, height, KnownResamplers.Lanczos3)
				);
				image.Mutate(x => x
					.Rotate(rotateMode));

				// Image<Rgba32> image, ExtensionRolesHelper.ImageFormat imageFormat, MemoryStream outputStream
				await SaveThumbnailImageFormatHelper.SaveThumbnailImageFormat(image,
					ExtensionRolesHelper.ImageFormat.jpg, stream);
				await _thumbnailStorage.WriteStreamAsync(stream, fileHash);
			}
		}
		catch ( Exception ex )
		{
			Console.WriteLine(ex);
			return false;
		}

		return true;
	}
}
