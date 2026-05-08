using System;
using starsky.foundation.injection;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail.Interfaces;
using starsky.foundation.thumbnailgeneration.GenerationFactory.Generators;
using starsky.foundation.thumbnailgeneration.GenerationFactory.Generators.Interfaces;
using starsky.foundation.thumbnailgeneration.GenerationFactory.Interfaces;
using starsky.foundation.thumbnailgeneration.GenerationFactory.RawDng;
using starsky.foundation.video.Process.Interfaces;

namespace starsky.foundation.thumbnailgeneration.GenerationFactory;

/// <summary>
///     Please use IThumbnailService
/// </summary>
/// <param name="selectorStorage">storage</param>
/// <param name="logger">logger</param>
/// <param name="videoProcess">video</param>
/// <param name="nativePreviewThumbnailGenerator">native</param>
/// <param name="embeddedRawThumbnailGenerator">embedded</param>
[Service(typeof(IThumbnailGeneratorFactory), InjectionLifetime = InjectionLifetime.Scoped)]
public class ThumbnailGeneratorFactory(
	ISelectorStorage selectorStorage,
	IWebLogger logger,
	IVideoProcess videoProcess,
	INativePreviewThumbnailGenerator nativePreviewThumbnailGenerator,
	IEmbeddedRawThumbnailGenerator embeddedRawThumbnailGenerator) : IThumbnailGeneratorFactory
{
	public IThumbnailGenerator GetGenerator(string filePath)
	{
		if ( ExtensionRolesHelper.IsExtensionJpeg(filePath) )
		{
			return new CompositeThumbnailGenerator(
			[
				embeddedRawThumbnailGenerator,
				nativePreviewThumbnailGenerator,
				new ImageSharpThumbnailGenerator(selectorStorage, logger)
			], logger);
		}

		if ( ExtensionRolesHelper.IsExtensionImageHeic(filePath) )
		{
			return new CompositeThumbnailGenerator(
			[
				nativePreviewThumbnailGenerator
			], logger);
		}

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

		if ( filePath.EndsWith(".dng", StringComparison.OrdinalIgnoreCase) )
		{
			return new CompositeThumbnailGenerator(
			[
				new RawDngThumbnailGenerator(selectorStorage, logger),
				nativePreviewThumbnailGenerator
			], logger);
		}

		if ( ExtensionRolesHelper.IsExtensionRawThumbnailSupported(filePath) )
		{
			return new CompositeThumbnailGenerator(
			[
				embeddedRawThumbnailGenerator
			], logger);
		}

		return new NotSupportedFallbackThumbnailGenerator();
	}
}
