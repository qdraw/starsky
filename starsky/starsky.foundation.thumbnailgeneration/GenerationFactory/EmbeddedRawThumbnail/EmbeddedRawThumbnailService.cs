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
	/// <summary>
	///     Synchronously extracts embedded JPEG previews from a RAW file.
	/// </summary>
	public async Task<bool> TryExtractPreview(string rawFilePath, string? outputLargePath,
		string? outputMediumPath)
	{
		if ( !File.Exists(rawFilePath) )
		{
			return false;
		}

		try
		{
			return await
				new EmbeddedPreviewExtractor(logger).TryExtract(rawFilePath, outputLargePath,
					outputMediumPath);
		}
		catch ( Exception exception )
		{
			logger.LogError($"Failed to extract embedded preview from RAW file: {rawFilePath}. " +
			                $"Exception: {exception.Message}");
			return false;
		}
	}
}
