using System.IO;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Thumbnails;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;

namespace starsky.foundation.thumbnailgeneration.GenerationFactory.Shared;

public class GetThumbnailStream(ISelectorStorage selectorStorage)
{
	private Stream GetThumbnail(string fileHash)
	{
		var storage = selectorStorage.Get(SelectorStorage.StorageServices.Thumbnail);
		var stream = storage.ReadStream(fileHash);
		return stream;
	}

	public Stream GetThumbnail(string fileHash, ThumbnailSize size,
		ThumbnailImageFormat imageFormat)
	{
		var hashWithSizeAndImageFormat = ThumbnailNameHelper.Combine(fileHash, size, imageFormat);
		return GetThumbnail(hashWithSizeAndImageFormat);
	}
}
