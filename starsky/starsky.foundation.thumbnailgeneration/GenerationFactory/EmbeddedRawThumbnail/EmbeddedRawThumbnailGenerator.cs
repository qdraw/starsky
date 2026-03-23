using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.injection;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Thumbnails;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starsky.foundation.thumbnailgeneration.GenerationFactory.Generators.Interfaces;
using starsky.foundation.thumbnailgeneration.GenerationFactory.ImageSharp;
using starsky.foundation.thumbnailgeneration.GenerationFactory.Models;
using starsky.foundation.thumbnailgeneration.GenerationFactory.Shared;
using starsky.foundation.thumbnailgeneration.Interfaces;
using starsky.foundation.thumbnailgeneration.Models;

namespace starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail;

/// <summary>
///     Generator for extracting embedded preview thumbnails from RAW image files.
///     Supports: CR2, CR3, NEF, ARW, DNG, RAF, FFF, X3F formats.
/// </summary>
[Service(typeof(IThumbnailGenerator),
	InjectionLifetime = InjectionLifetime.Transient)]
public class EmbeddedRawThumbnailGenerator(
	ISelectorStorage selectorStorage,
	IEmbeddedRawThumbnailService embeddedRawThumbnailService,
	IWebLogger logger)
	: IThumbnailGenerator
{
	public async Task<IEnumerable<GenerationResultModel>> GenerateThumbnail(string singleSubPath,
		string fileHash, ThumbnailImageFormat imageFormat,
		List<ThumbnailSize> thumbnailSizes)
	{
		if ( !ExtensionRolesHelper.IsExtensionRawThumbnailSupported(singleSubPath) )
		{
			return [];
		}

		return await new SharedGenerate(selectorStorage, logger).GenerateThumbnail(
			ResizeThumbnailFromEmbeddedPreview,
			ExtensionRolesHelper.IsExtensionRawThumbnailSupported,
			singleSubPath, fileHash,
			imageFormat, thumbnailSizes);
	}

	private async Task<GenerationResultModel> ResizeThumbnailFromEmbeddedPreview(
		ThumbnailSize biggestThumbnailSize, string singleSubPath, string fileHash,
		ThumbnailImageFormat imageFormat)
	{
		// Use temporary storage abstraction for extracted preview payload.
		var previewPath = $"/preview_large_{fileHash}.jpg";
		var tempStorage = selectorStorage.Get(SelectorStorage.StorageServices.Temporary);

		try
		{
			var extracted = await embeddedRawThumbnailService.TryExtractPreview(singleSubPath,
				previewPath);

			if ( !extracted )
			{
				return ErrorGenerationResultModel.FailedResult(biggestThumbnailSize,
					singleSubPath, fileHash, true, imageFormat, true,
					"No embedded preview found in RAW file");
			}

			// Use the largest available preview

			if ( !tempStorage.ExistFile(previewPath) )
			{
				return ErrorGenerationResultModel.FailedResult(biggestThumbnailSize,
					singleSubPath, fileHash, true, imageFormat, true,
					"Failed to extract preview files");
			}

			try
			{
				// Resize the extracted preview to requested thumbnail sizes
				var resizeHelper =
					new ResizeThumbnailFromSourceImageHelper(selectorStorage, logger);
				var generationResult = await resizeHelper.ResizeThumbnailFromSourceImage(
					previewPath,
					SelectorStorage.StorageServices.Temporary,
					ThumbnailNameHelper.GetSize(biggestThumbnailSize), fileHash,
					false, imageFormat);

				return generationResult;
			}
			finally
			{
				// Clean up temporary preview files
				if ( tempStorage.ExistFile(previewPath) )
				{
					try
					{
						tempStorage.FileDelete(previewPath);
					}
					catch
					{
						// Ignore cleanup errors
					}
				}
			}
		}
		catch ( Exception exception )
		{
			logger.LogError(
				$"[EmbeddedRawThumbnailGenerator] Failed to extract preview from {singleSubPath}: {exception.Message}");
			return ErrorGenerationResultModel.FailedResult(biggestThumbnailSize,
				singleSubPath, fileHash, true, imageFormat, true, exception.Message);
		}
	}
}
