using System;
using System.Collections.Generic;
using System.IO;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail;
using starsky.foundation.thumbnailgeneration.GenerationFactory.Generators;
using starsky.foundation.thumbnailgeneration.GenerationFactory.Generators.Interfaces;
using starsky.foundation.thumbnailgeneration.GenerationFactory.Interfaces;
using starsky.foundation.video.Process.Interfaces;

namespace starsky.foundation.thumbnailgeneration.GenerationFactory;

internal class ThumbnailGeneratorFactory(
	ISelectorStorage selectorStorage,
	IWebLogger logger,
	IVideoProcess videoProcess,
	INativePreviewThumbnailGenerator nativePreviewThumbnailGenerator)
{

	internal IThumbnailGenerator GetGenerator(string filePath)
	{
		if ( ExtensionRolesHelper.IsExtensionImageSharpThumbnailSupported(filePath) )
		{
			return new CompositeThumbnailGenerator(
			[
				nativePreviewThumbnailGenerator,
				new ImageSharpThumbnailGenerator(selectorStorage, logger)
			], logger);
		}

		if ( ExtensionRolesHelper.IsExtensionVideoSupported(filePath) )
		{
			return new CompositeThumbnailGenerator(
				[new FfmpegVideoThumbnailGenerator(selectorStorage, videoProcess, logger)], logger);
		}

		if ( ExtensionRolesHelper.IsExtensionRawThumbnailSupported(filePath) )
		{
			return new CompositeThumbnailGenerator(
			[
				new EmbeddedRawThumbnailGenerator(selectorStorage,
					new EmbeddedRawThumbnailService(logger, selectorStorage), logger),
//				nativePreviewThumbnailGenerator
			], logger);
		}

		return new NotSupportedFallbackThumbnailGenerator();
	}

}
