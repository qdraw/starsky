using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Thumbnails;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starsky.foundation.thumbnailgeneration.GenerationFactory.Generators.Interfaces;
using starsky.foundation.thumbnailgeneration.GenerationFactory.ImageSharp;
using starsky.foundation.thumbnailgeneration.GenerationFactory.Testers;
using starsky.foundation.thumbnailgeneration.Models;
using starsky.foundation.video.Process;
using starsky.foundation.video.Process.Interfaces;

namespace starsky.foundation.thumbnailgeneration.GenerationFactory.Generators;

public class FfmpegVideoThumbnailGenerator(
	ISelectorStorage selectorStorage,
	IVideoProcess videoProcess,
	IWebLogger logger)
	: IThumbnailGenerator
{
	private readonly ResizeThumbnailFromThumbnailImageHelper _resizeThumbnail =
		new(selectorStorage, logger);

	public async Task<IEnumerable<GenerationResultModel>> GenerateThumbnail(string singleSubPath,
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
			await ResizeThumbnailFromSourceImage(toGenerateSize, singleSubPath, fileHash,
				imageFormat);

		var results = await _resizeThumbnail.ResizeThumbnailFromThumbnailImageLoop(singleSubPath,
			fileHash, imageFormat, thumbnailSizes, toGenerateSize);

		return preflightResult.AddOrUpdateRange(results?.Select(p => p.Item2))
			.AddOrUpdateRange([largeImageResult]);
	}

	private async Task<(MemoryStream?, GenerationResultModel)> ResizeThumbnailFromSourceImage(
		ThumbnailSize biggestThumbnailSize, string singleSubPath, string fileHash,
		ThumbnailImageFormat imageFormat)
	{
		var result =
			await videoProcess.RunVideo(singleSubPath, fileHash, VideoProcessTypes.Thumbnail);
		if ( !result.IsSuccess || result.ResultPath == null )
		{
			return ( null, ErrorGenerationResultModel.FailedResult(biggestThumbnailSize,
				singleSubPath, fileHash, true, result.ErrorMessage!) );
		}

		var service = new ResizeThumbnailFromSourceImageHelper(selectorStorage, logger);
		return await service.ResizeThumbnailFromSourceImage(result.ResultPath,
			ThumbnailNameHelper.GetSize(biggestThumbnailSize), fileHash, false, imageFormat);
	}
}
