using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Thumbnails;
using starsky.foundation.thumbnailgeneration.Models;

namespace starsky.foundation.thumbnailgeneration.GenerationFactory.Interfaces;

/// <summary>
///     Create smaller thumbnails from an existing (larger) thumbnail,
///     get an instance with IImageProviderFactory
/// </summary>
public interface IResizeThumbnailFromThumbnailImageHelper
{
	Task<IEnumerable<GenerationResultModel>?> ResizeThumbnailFromThumbnailImageLoop(
		string singleSubPath, string fileHash, ThumbnailImageFormat imageFormat,
		List<ThumbnailSize> thumbnailSizes, ThumbnailSize toGenerateSize);

	Task<GenerationResultModel> ResizeThumbnailFromThumbnailImage(
		string fileHash, ThumbnailSize inputThumbnailSize,
		int width, string? subPathReference, string thumbnailOutputHash,
		ThumbnailImageFormat imageFormat);
}
