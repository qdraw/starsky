using System;
using System.IO;
using System.Threading.Tasks;
using starsky.foundation.injection;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail.Interfaces;
using starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail.TiffEmbeded;

namespace starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail;

/// <summary>
///     Service for extracting embedded JPEG previews from RAW image files.
///     Supports: CR2 (Canon), NEF (Nikon), ARW (Sony), DNG (Adobe), RAF (Fujifilm), FFF (Hasselblad),
///     X3F (Sigma)
/// </summary>
[Service(typeof(IEmbeddedRawThumbnailService),
	InjectionLifetime = InjectionLifetime.Scoped)]
public class EmbeddedRawThumbnailService(IWebLogger logger, ISelectorStorage selectorStorage)
	: IEmbeddedRawThumbnailService
{
	private IStorage SubPathStorage => selectorStorage.Get(SelectorStorage.StorageServices.SubPath);

	public async Task<bool> TryExtractPreview(string rawFilePath, string? outputLargePath)
	{
		try
		{
			var extension = Path.GetExtension(rawFilePath).ToLowerInvariant();
			var imageFormat =
				new ExtensionRolesHelper(logger).GetImageFormat(
					SubPathStorage.ReadStream(rawFilePath, 160));

			var tiffExtractor = new TiffEmbeddedPreviewExtractor(logger, selectorStorage);
			var rafExtractor = new RafPreviewExtractor(logger, selectorStorage);
			var containerExtractor = new ContainerFormatPreviewExtractor(logger, selectorStorage);
			var lightweightContainerExtractor =
				new LightweightContainerPreviewExtractor(logger, selectorStorage);

			// Use TIFF-based extractor for DNG, CR2, NEF, ARW
			var result = imageFormat switch
			{
				ExtensionRolesHelper.ImageFormat.arw
					or ExtensionRolesHelper.ImageFormat.cr2
					or ExtensionRolesHelper.ImageFormat.nef
					or ExtensionRolesHelper.ImageFormat.dng
					or ExtensionRolesHelper.ImageFormat.tiff =>
					await tiffExtractor.TryExtract(rawFilePath,
						outputLargePath),
				ExtensionRolesHelper.ImageFormat.raf =>
					await rafExtractor.TryExtract(rawFilePath,
						outputLargePath),
				ExtensionRolesHelper.ImageFormat.cr3 =>
					await containerExtractor.TryExtract(rawFilePath,
						outputLargePath),
				_ when extension is ".fff" or ".x3f" =>
					await lightweightContainerExtractor.TryExtract(rawFilePath,
						outputLargePath),
				_ => false
			};

			if ( result )
			{
				return true;
			}

			logger.LogInformation(
				$"[EmbeddedRawThumbnailService] No preview found for {rawFilePath}");
			return false;
		}
		catch ( Exception exception )
		{
			logger.LogError($"Failed to extract embedded preview from RAW file: {rawFilePath}. " +
			                $"Exception: {exception.Message}");
			return false;
		}
	}
}
