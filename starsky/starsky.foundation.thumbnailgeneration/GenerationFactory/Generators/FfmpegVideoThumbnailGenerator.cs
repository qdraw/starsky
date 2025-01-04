using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.platform.Thumbnails;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starsky.foundation.thumbnailgeneration.GenerationFactory.Generators.Interfaces;
using starsky.foundation.thumbnailgeneration.GenerationFactory.Testers;
using starsky.foundation.thumbnailgeneration.Models;

namespace starsky.foundation.thumbnailgeneration.GenerationFactory.Generators;

public class FfmpegVideoThumbnailGenerator(ISelectorStorage selectorStorage) : IThumbnailGenerator
{
	private readonly IStorage
		_storage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);

	public async Task<IEnumerable<GenerationResultModel>> GenerateThumbnail(string singleSubPath,
		string fileHash,
		List<ThumbnailSize> thumbnailSizes)
	{
		var preflightResult = new Preflight(_storage).Test(thumbnailSizes, singleSubPath, fileHash);
		if ( preflightResult != null )
		{
			return preflightResult;
		}

		throw new NotImplementedException();
	}
}
