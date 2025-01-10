using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Thumbnails;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.thumbnailgeneration.GenerationFactory.ImageSharp;
using starsky.foundation.thumbnailgeneration.GenerationFactory.Testers;
using starsky.foundation.thumbnailgeneration.Models;

namespace starsky.foundation.thumbnailgeneration.GenerationFactory.Shared;

public class SharedGenerate(ISelectorStorage selectorStorage, IWebLogger logger)
{
	private readonly ResizeThumbnailFromThumbnailImageHelper _resizeThumbnail =
		new(selectorStorage, logger);
	
	public delegate Task<(MemoryStream?, GenerationResultModel)> ResizeThumbnailFromSourceImage(
		ThumbnailSize biggestThumbnailSize, string singleSubPath, string fileHash,
		ThumbnailImageFormat imageFormat);
	
	public async Task<IEnumerable<GenerationResultModel>> GenerateThumbnail(ResizeThumbnailFromSourceImage resizeDelegate, string singleSubPath,
		string fileHash, ThumbnailImageFormat imageFormat,
		List<ThumbnailSize> thumbnailSizes)
	{
		var preflightResult =
			new PreflightThumbnailGeneration(selectorStorage).Preflight(thumbnailSizes,
				singleSubPath, fileHash,
				imageFormat);
		thumbnailSizes = PreflightThumbnailGeneration.MapThumbnailSizes(preflightResult);
		if ( preflightResult.Any(p => !p.ToGenerate) || thumbnailSizes.Count == 0 )
		{
			return preflightResult;
		}

		var toGenerateSize = thumbnailSizes[0];
		var (_, largeImageResult) =
			await resizeDelegate(toGenerateSize, singleSubPath, fileHash,
				imageFormat);

		var results = await _resizeThumbnail.ResizeThumbnailFromThumbnailImageLoop(singleSubPath,
			fileHash, imageFormat, thumbnailSizes, toGenerateSize);

		return preflightResult.AddOrUpdateRange(results?.Select(p => p.Item2))
			.AddOrUpdateRange([largeImageResult]);
	}
}
