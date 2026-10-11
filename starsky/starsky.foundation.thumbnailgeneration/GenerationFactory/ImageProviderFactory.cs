using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.thumbnailgeneration.GenerationFactory.Generators;
using starsky.foundation.thumbnailgeneration.GenerationFactory.Generators.Interfaces;
using starsky.foundation.thumbnailgeneration.GenerationFactory.Interfaces;
using starsky.foundation.thumbnailgeneration.GenerationFactory.Skia;

namespace starsky.foundation.thumbnailgeneration.GenerationFactory;

/// <summary>
///     SkiaSharp is the image library, this is the only place in the thumbnail generation
///     that creates the implementations. Switch library here
/// </summary>
[Service(typeof(IImageProviderFactory), InjectionLifetime = InjectionLifetime.Scoped)]
public class ImageProviderFactory(ISelectorStorage selectorStorage, IWebLogger logger)
	: IImageProviderFactory
{
	public IThumbnailGenerator CreateThumbnailGenerator()
	{
		return new SkiaThumbnailGenerator(selectorStorage, logger);
	}

	public IResizeThumbnailFromSourceImageHelper CreateResizeThumbnailFromSourceImageHelper()
	{
		return new ResizeThumbnailFromSourceImageHelper(selectorStorage, logger);
	}

	public IResizeThumbnailFromThumbnailImageHelper CreateResizeThumbnailFromThumbnailImageHelper()
	{
		return new ResizeThumbnailFromThumbnailImageHelper(selectorStorage, logger);
	}

	public IRotateThumbnailHelper CreateRotateThumbnailHelper(AppSettings appSettings)
	{
		return new RotateThumbnailHelper(selectorStorage, appSettings, logger);
	}
}
