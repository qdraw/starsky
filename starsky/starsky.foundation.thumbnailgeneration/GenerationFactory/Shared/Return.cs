using System.IO;
using System.Threading.Tasks;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Thumbnails;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starsky.foundation.thumbnailgeneration.Models;

namespace starsky.foundation.thumbnailgeneration.GenerationFactory.Shared;

public class Return(ISelectorStorage selectorStorage, IWebLogger logger)
{
	private readonly IStorage
		_thumbnailStorage = selectorStorage.Get(SelectorStorage.StorageServices.Thumbnail);

	public async Task<(Stream?, GenerationResultModel)> Test(string fileHash,
		string singleSubPath,
		ThumbnailSize thumbnailSize, ThumbnailImageFormat imageFormat)
	{
		if ( _thumbnailStorage.ExistFile(ThumbnailNameHelper.Combine(
			    fileHash, thumbnailSize, imageFormat)) )
		{
			var stream = _thumbnailStorage.ReadStream(ThumbnailNameHelper.Combine(
				fileHash, thumbnailSize, imageFormat));

			return ( stream,
				new GenerationResultModel
				{
					FileHash = fileHash,
					IsNotFound = false,
					SizeInPixels = ThumbnailNameHelper.GetSize(thumbnailSize),
					Success = true,
					SubPath = singleSubPath
				} );
		}

		return ( null, new GenerationResultModel() );
	}
}
