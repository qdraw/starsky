using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.injection;
using starsky.foundation.native.PreviewImageNative.Interfaces;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Thumbnails;
using starsky.foundation.readmeta.Interfaces;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Models;
using starsky.foundation.storage.Storage;
using starsky.foundation.thumbnailgeneration.GenerationFactory.ImageSharp;
using starsky.foundation.thumbnailgeneration.GenerationFactory.Interfaces;
using starsky.foundation.thumbnailgeneration.GenerationFactory.Models;
using starsky.foundation.thumbnailgeneration.GenerationFactory.NativePreview;
using starsky.foundation.thumbnailgeneration.GenerationFactory.Shared;
using starsky.foundation.thumbnailgeneration.Models;

namespace starsky.foundation.thumbnailgeneration.GenerationFactory.Generators;

[Service(typeof(INativePreviewThumbnailGenerator), InjectionLifetime = InjectionLifetime.Scoped)]
public class NativePreviewThumbnailGenerator(
	ISelectorStorage selectorStorage,
	IPreviewImageNativeService imageNativeService,
	IWebLogger logger,
	AppSettings appSettings,
	IReadMetaSubPathStorage readMeta,
	IFullFilePathExistsService existsService)
	: INativePreviewThumbnailGenerator
{
	private readonly IStorage _storage =
		selectorStorage.Get(SelectorStorage.StorageServices.SubPath);

	private readonly IStorage _tempStorage =
		selectorStorage.Get(SelectorStorage.StorageServices.Temporary);

	public async Task<IEnumerable<GenerationResultModel>> GenerateThumbnail(string singleSubPath,
		string fileHash, ThumbnailImageFormat imageFormat,
		List<ThumbnailSize> thumbnailSizes)
	{
		return await new SharedGenerate(selectorStorage, logger).GenerateThumbnail(
			ResizeThumbnailFromSourceImage,
			ExtensionRolesHelper.IsExtensionImageSharpThumbnailSupported,
			singleSubPath, fileHash,
			imageFormat, thumbnailSizes);
	}

	private async Task<GenerationResultModel> ResizeThumbnailFromSourceImage(
		ThumbnailSize biggestThumbnailSize, string singleSubPath, string fileHash,
		ThumbnailImageFormat imageFormat)
	{
		var nativePreviewHelper = new NativePreviewHelper(imageNativeService, _storage,
			_tempStorage, appSettings, existsService, readMeta, logger);

		var result =
			await nativePreviewHelper.NativePreviewImage(biggestThumbnailSize, singleSubPath,
				fileHash);

		if ( !result.IsSuccess || result.ResultPath == null )
		{
			return ErrorGenerationResultModel.FailedResult(biggestThumbnailSize,
				singleSubPath, fileHash, true, imageFormat, result.ErrorLog, result.ErrorMessage);
		}

		var service = new ResizeThumbnailFromSourceImageHelper(selectorStorage, logger);
		var generationResult = await service.ResizeThumbnailFromSourceImage(result.ResultPath,
			result.ResultPathType,
			ThumbnailNameHelper.GetSize(biggestThumbnailSize), fileHash,
			false, imageFormat);

		nativePreviewHelper.CleanTemporaryFile(result.ResultPath,
			result.ResultPathType);
		return generationResult;
	}
}
