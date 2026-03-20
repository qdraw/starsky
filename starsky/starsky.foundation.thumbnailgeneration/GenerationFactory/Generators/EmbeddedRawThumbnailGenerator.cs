using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.injection;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Thumbnails;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Models;
using starsky.foundation.storage.Storage;
using starsky.foundation.thumbnailgeneration.GenerationFactory.ImageSharp;
using starsky.foundation.thumbnailgeneration.GenerationFactory.Interfaces;
using starsky.foundation.thumbnailgeneration.GenerationFactory.Models;
using starsky.foundation.thumbnailgeneration.GenerationFactory.Shared;
using starsky.foundation.thumbnailgeneration.Interfaces;
using starsky.foundation.thumbnailgeneration.Models;

namespace starsky.foundation.thumbnailgeneration.GenerationFactory.Generators;

[Service(typeof(IEmbeddedRawThumbnailGenerator), InjectionLifetime = InjectionLifetime.Scoped)]
public class EmbeddedRawThumbnailGenerator(
	ISelectorStorage selectorStorage,
	IWebLogger logger,
	AppSettings appSettings,
	IEmbeddedRawThumbnailService embeddedRawThumbnailService,
	IFullFilePathExistsService fullFilePathExistsService)
	: IEmbeddedRawThumbnailGenerator
{
	private readonly IStorage _tempStorage =
		selectorStorage.Get(SelectorStorage.StorageServices.Temporary);

	public async Task<IEnumerable<GenerationResultModel>> GenerateThumbnail(string singleSubPath,
		string fileHash, ThumbnailImageFormat imageFormat,
		List<ThumbnailSize> thumbnailSizes)
	{
		return await new SharedGenerate(selectorStorage, logger).GenerateThumbnail(
			ResizeThumbnailFromEmbeddedRawPreview,
			ExtensionRolesHelper.IsExtensionEmbeddedRawThumbnailSupported,
			singleSubPath, fileHash,
			imageFormat, thumbnailSizes);
	}

	private async Task<GenerationResultModel> ResizeThumbnailFromEmbeddedRawPreview(
		ThumbnailSize biggestThumbnailSize, string singleSubPath, string fileHash,
		ThumbnailImageFormat imageFormat)
	{
		var (isSuccess, fullFilePath, isTempInput, tempInputName) =
			await fullFilePathExistsService.GetFullFilePath(singleSubPath, fileHash);
		if ( !isSuccess )
		{
			return ErrorGenerationResultModel.FailedResult(biggestThumbnailSize,
				singleSubPath, fileHash, true, imageFormat, true,
				"File does not exist");
		}

		var previewTempName = $"{fileHash}.embedded-raw-preview.jpg";
		var previewFullPath = appSettings.DatabasePathToTempFolderFilePath(previewTempName);
		var extracted = await embeddedRawThumbnailService.TryExtractPreviewAsync(fullFilePath,
			previewFullPath, null);

		fullFilePathExistsService.CleanTemporaryFile(tempInputName, isTempInput);
		if ( !extracted )
		{
			return ErrorGenerationResultModel.FailedResult(biggestThumbnailSize,
				singleSubPath, fileHash, true, imageFormat, false,
				"Embedded RAW preview not found");
		}

		var resizeHelper = new ResizeThumbnailFromSourceImageHelper(selectorStorage, logger);
		var generationResult = await resizeHelper.ResizeThumbnailFromSourceImage(previewTempName,
			SelectorStorage.StorageServices.Temporary,
			ThumbnailNameHelper.GetSize(biggestThumbnailSize),
			fileHash,
			false,
			imageFormat);

		_tempStorage.FileDelete(previewTempName);
		return generationResult;
	}
}


