using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.thumbnailgeneration.GenerationFactory.Generators;
using starsky.foundation.thumbnailgeneration.GenerationFactory.Generators.Interfaces;

namespace starsky.foundation.thumbnailgeneration.GenerationFactory;

internal class ThumbnailGeneratorFactory(
	ISelectorStorage selectorStorage,
	IWebLogger logger,
	AppSettings appSettings)
{
	internal IThumbnailGenerator GetGenerator(string filePath)
	{
		if ( ExtensionRolesHelper.IsExtensionImageSharpThumbnailSupported(filePath) )
		{
			return new CompositeThumbnailGenerator(
				[new ImageSharpThumbnailGenerator(selectorStorage, logger)], logger);
		}

		if ( ExtensionRolesHelper.IsExtensionVideoSupported(filePath) )
		{
			return new CompositeThumbnailGenerator(
				[new FfmpegVideoThumbnailGenerator(selectorStorage)], logger);
		}

		return new NotSupportedFallbackThumbnailGenerator();
	}
}
