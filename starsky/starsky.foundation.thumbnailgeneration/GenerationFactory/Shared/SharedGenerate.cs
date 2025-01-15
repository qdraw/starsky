using System.Collections.Generic;
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
	/// <summary>
	///     eg. ExtensionRolesHelper.IsExtensionImageSharpThumbnailSupported,
	///     ExtensionRolesHelper.IsExtensionVideoSupported
	/// </summary>
	public delegate Task<GenerationResultModel> ResizeThumbnailFromSourceImage(
		ThumbnailSize biggestThumbnailSize, string singleSubPath, string fileHash,
		ThumbnailImageFormat imageFormat);

	private readonly ResizeThumbnailFromThumbnailImageHelper _resizeThumbnail =
		new(selectorStorage, logger);

	public async Task<IEnumerable<GenerationResultModel>> GenerateThumbnail(
		ResizeThumbnailFromSourceImage resizeDelegate,
		PreflightThumbnailGeneration.IsExtensionSupportedDelegate isExtensionSupportedDelegate,
		string singleSubPath,
		string fileHash, ThumbnailImageFormat imageFormat,
		List<ThumbnailSize> thumbnailSizes)
	{
		var preflightResult =
			new PreflightThumbnailGeneration(selectorStorage).Preflight(
				isExtensionSupportedDelegate,
				thumbnailSizes,
				singleSubPath, fileHash,
				imageFormat);

		thumbnailSizes = PreflightThumbnailGeneration.MapThumbnailSizes(preflightResult);
		if ( preflightResult.Any(p => !p.ToGenerate) || thumbnailSizes.Count == 0 )
		{
			return preflightResult;
		}

		var toGenerateSize = thumbnailSizes[0];
		var largeImageResult =
			await resizeDelegate(toGenerateSize, singleSubPath, fileHash,
				imageFormat);

		var results = await _resizeThumbnail.ResizeThumbnailFromThumbnailImageLoop(singleSubPath,
			fileHash, imageFormat, thumbnailSizes, toGenerateSize);

		return preflightResult.AddOrUpdateRange(results)
			.AddOrUpdateRange([largeImageResult]);
	}
}
