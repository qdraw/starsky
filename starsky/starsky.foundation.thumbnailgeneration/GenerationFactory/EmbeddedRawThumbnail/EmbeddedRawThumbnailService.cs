using System;
using System.IO;
using System.Threading.Tasks;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.thumbnailgeneration.Interfaces;

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
	public async Task<bool> TryExtractPreview(string rawFilePath, string? outputLargePath)
	{
		try
		{
			var extension = Path.GetExtension(rawFilePath).ToLowerInvariant();
			var extractor = new EmbeddedPreviewExtractor(logger, selectorStorage);

			// Use TIFF-based extractor for DNG, CR2, NEF, ARW
			var result = extension switch
			{
				".dng" or ".cr2" or ".nef" or ".arw" =>
					await extractor.TryExtract(rawFilePath, outputLargePath),
				// TODO: Add format-specific extractors
				// ".cr3" => await new Cr3BmffPreviewExtractor(logger).TryExtract(...),
				// ".raf" => await new RafPreviewExtractor(logger).TryExtract(...),
				// ".fff" or ".x3f" => await new LightweightContainerPreviewExtractor(logger).TryExtract(...),
				_ => false
			};

			if ( result )
			{
				return true;
			}

			// Fallback: scan for JPEG segments in file
			// TODO: Implement JpegSegmentScanner for fallback
			logger.LogDebug($"[EmbeddedRawThumbnailService] No preview found for {rawFilePath}");
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
