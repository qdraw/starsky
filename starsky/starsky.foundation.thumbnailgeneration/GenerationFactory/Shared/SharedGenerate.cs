using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Thumbnails;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
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
		ResizeThumbnailFromSourceImage resizeThumbnailFromSourceImage,
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
		var largeImageResult = await resizeThumbnailFromSourceImage(toGenerateSize,
			singleSubPath, fileHash, imageFormat);

		if ( !largeImageResult.Success )
		{
			logger.LogError(
				$"[SharedGenerate] ResizeThumbnailFromSourceImage failed for " +
				$"S: {singleSubPath} - H: {fileHash} SI: {toGenerateSize}");

			var failedResults = UpdateAllSizesToFailure(
				thumbnailSizes, fileHash, singleSubPath, imageFormat,
				largeImageResult.ErrorMessage);
			return preflightResult
				.AddOrUpdateRange(failedResults)
				.AddOrUpdateRange([largeImageResult]);
		}

		var results = await _resizeThumbnail.ResizeThumbnailFromThumbnailImageLoop(singleSubPath,
			fileHash, imageFormat, thumbnailSizes, toGenerateSize);

		return preflightResult
			.AddOrUpdateRange(results)
			.AddOrUpdateRange([largeImageResult]);
	}

	/// <summary>
	///     Update the results with a failure for all sizes.
	/// </summary>
	/// <param name="thumbnailSizes">all sizes which to update</param>
	/// <param name="thumbnailOutputHash">which item</param>
	/// <param name="subPathReference">the path in subpath style</param>
	/// <param name="imageFormat">jpg,png</param>
	/// <param name="errorMessage">why it failed</param>
	/// <returns></returns>
	private static IEnumerable<GenerationResultModel> UpdateAllSizesToFailure(
		List<ThumbnailSize> thumbnailSizes,
		string thumbnailOutputHash,
		string subPathReference,
		ThumbnailImageFormat imageFormat,
		string? errorMessage)
	{
		return thumbnailSizes.Select(size => new GenerationResultModel
		{
			FileHash = ThumbnailNameHelper.RemoveSuffix(thumbnailOutputHash),
			IsNotFound = false,
			SizeInPixels = ThumbnailNameHelper.GetSize(size),
			Success = false,
			SubPath = subPathReference,
			ImageFormat = imageFormat,
			Size = size,
			ErrorMessage = errorMessage
		});
	}
}
