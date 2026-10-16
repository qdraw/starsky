using System.Threading.Tasks;
using starsky.foundation.platform.Enums;
using starsky.foundation.storage.Storage;
using starsky.foundation.thumbnailgeneration.Models;

namespace starsky.foundation.thumbnailgeneration.GenerationFactory.Interfaces;

/// <summary>
///     Create a thumbnail from a (source) image, get an instance with IImageProviderFactory
/// </summary>
public interface IResizeThumbnailFromSourceImageHelper
{
	Task<GenerationResultModel> ResizeThumbnailFromSourceImage(string subPath,
		SelectorStorage.StorageServices? subPathType,
		int width, string? thumbnailOutputHash,
		ThumbnailImageFormat imageFormat);
}
