using starsky.foundation.platform.Models;
using starsky.foundation.thumbnailgeneration.GenerationFactory.Generators.Interfaces;

namespace starsky.foundation.thumbnailgeneration.GenerationFactory.Interfaces;

/// <summary>
///     The one place that decides which image library does the resizing, rotating and encoding
/// </summary>
public interface IImageProviderFactory
{
	IThumbnailGenerator CreateThumbnailGenerator();
	IResizeThumbnailFromSourceImageHelper CreateResizeThumbnailFromSourceImageHelper();
	IResizeThumbnailFromThumbnailImageHelper CreateResizeThumbnailFromThumbnailImageHelper();
	IRotateThumbnailHelper CreateRotateThumbnailHelper(AppSettings appSettings);
}
