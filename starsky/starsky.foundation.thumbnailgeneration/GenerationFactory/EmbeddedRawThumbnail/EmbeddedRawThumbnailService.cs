using System;
using System.IO;
using System.Threading.Tasks;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.thumbnailgeneration.Interfaces;

namespace starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail;

/// <summary>
///     Service for extracting embedded JPEG previews from RAW image files.
///     Supports: CR2 (Canon), NEF (Nikon), ARW (Sony), DNG (Adobe)
/// </summary>
[Service(typeof(IEmbeddedRawThumbnailService),
	InjectionLifetime = InjectionLifetime.Scoped)]
public class EmbeddedRawThumbnailService(IWebLogger logger) : IEmbeddedRawThumbnailService
{
	public async Task<bool> TryExtractPreview(string rawFilePath, string? outputLargePath,
		string? outputMediumPath)
	{
		if ( !File.Exists(rawFilePath) )
		{
			return false;
		}

		try
		{
			var extension = Path.GetExtension(rawFilePath).ToLowerInvariant();
			var result = extension switch
			{
				".cr3" => await new Cr3BmffPreviewExtractor(logger).TryExtract(rawFilePath,
					outputLargePath, outputMediumPath),
				".raf" => await new RafPreviewExtractor(logger).TryExtract(rawFilePath,
					outputLargePath, outputMediumPath),
				".fff" or ".x3f" =>
					await new LightweightContainerPreviewExtractor(logger).TryExtract(rawFilePath,
						outputLargePath, outputMediumPath),
				_ => await new EmbeddedPreviewExtractor(logger).TryExtract(rawFilePath,
					outputLargePath,
					outputMediumPath)
			};

			if ( result )
			{
				return true;
			}

			// Fallback for files with non-standard/unsupported preview variants.
			var fullFileLength = new FileInfo(rawFilePath).Length;
			return new JpegSegmentScanner(logger).TryExtract(rawFilePath,
				[(0, fullFileLength)], outputLargePath, outputMediumPath);
		}
		catch ( Exception exception )
		{
			logger.LogError($"Failed to extract embedded preview from RAW file: {rawFilePath}. " +
			                $"Exception: {exception.Message}");
			return false;
		}
	}
}
