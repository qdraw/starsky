using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Thumbnails;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starsky.foundation.thumbnailgeneration.GenerationFactory.Generators.Interfaces;
using starsky.foundation.thumbnailgeneration.GenerationFactory.ImageSharp;
using starsky.foundation.thumbnailgeneration.GenerationFactory.Shared;
using starsky.foundation.thumbnailgeneration.Models;

namespace starsky.foundation.thumbnailgeneration.GenerationFactory.Generators;

public class ImageSharpThumbnailGenerator(
	ISelectorStorage selectorStorage,
	IWebLogger logger)
	: IThumbnailGenerator
{
	public async Task<IEnumerable<GenerationResultModel>> GenerateThumbnail(string singleSubPath,
		string fileHash,
		ThumbnailImageFormat imageFormat,
		List<ThumbnailSize> thumbnailSizes)
	{
		return await new SharedGenerate(selectorStorage, logger).GenerateThumbnail(
			ResizeThumbnailFromSourceImage,
			ExtensionRolesHelper.IsExtensionImageSharpThumbnailSupported, singleSubPath, fileHash,
			imageFormat, thumbnailSizes);
	}

	private async Task<(MemoryStream?, GenerationResultModel)> ResizeThumbnailFromSourceImage(
		ThumbnailSize biggestThumbnailSize, string singleSubPath, string fileHash,
		ThumbnailImageFormat imageFormat)
	{
		var service = new ResizeThumbnailFromSourceImageHelper(selectorStorage, logger);
		return await service.ResizeThumbnailFromSourceImage(singleSubPath,
			ThumbnailNameHelper.GetSize(biggestThumbnailSize), fileHash, false, imageFormat);
	}
}
