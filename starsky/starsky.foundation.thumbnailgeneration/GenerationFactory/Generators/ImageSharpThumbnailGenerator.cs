using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using starsky.foundation.platform.Extensions;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Thumbnails;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starsky.foundation.thumbnailgeneration.GenerationFactory.Generators.Interfaces;
using starsky.foundation.thumbnailgeneration.GenerationFactory.ImageSharp;
using starsky.foundation.thumbnailgeneration.GenerationFactory.Testers;
using starsky.foundation.thumbnailgeneration.Models;

namespace starsky.foundation.thumbnailgeneration.GenerationFactory.Generators;

public class ImageSharpThumbnailGenerator(ISelectorStorage selectorStorage, IWebLogger logger)
	: IThumbnailGenerator
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

		var (_, largeImageResult) =
			await ResizeThumbnailFromSourceImage(thumbnailSizes[0], singleSubPath, fileHash);

		var results = await thumbnailSizes.Skip(1).ForEachAsync(
			async size
				=> await ResizeThumbnailFromThumbnailImage(
					fileHash, // source location
					ThumbnailNameHelper.GetSize(size),
					singleSubPath, // used for reference only
					ThumbnailNameHelper.Combine(fileHash, size)),
			thumbnailSizes.Count);

		return results!.Select(p => p.Item2).Append(largeImageResult);
	}

	private async Task<(MemoryStream?, GenerationResultModel)> ResizeThumbnailFromThumbnailImage(
		string fileHash, // source location
		int width, string? subPathReference = null, string? thumbnailOutputHash = null)
	{
		var service = new ResizeThumbnailFromThumbnailImageHelper(selectorStorage, logger);
		return await service.ResizeThumbnailFromThumbnailImage(fileHash, width, subPathReference,
			thumbnailOutputHash);
	}

	private async Task<(MemoryStream?, GenerationResultModel)> ResizeThumbnailFromSourceImage(
		ThumbnailSize biggestThumbnailSize, string singleSubPath, string fileHash)
	{
		var service = new ResizeThumbnailFromSourceImageHelper(selectorStorage, logger);
		return await service.ResizeThumbnailFromSourceImage(singleSubPath,
			ThumbnailNameHelper.GetSize(biggestThumbnailSize), fileHash);
	}
}
