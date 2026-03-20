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
	///     Extracts embedded JPEG previews from a RAW file asynchronously.
	/// </summary>
	/// <param name="rawFilePath">Full path to the RAW file</param>
	/// <param name="outputLargePath">Output path for the large preview (or null to skip)</param>
	/// <param name="outputMediumPath">Output path for the medium preview (or null to skip)</param>
	/// <returns>true if at least one preview was successfully extracted</returns>
	public Task<bool> TryExtractPreviewAsync(string rawFilePath, string? outputLargePath,
		string? outputMediumPath)
	{
		return Task.FromResult(TryExtractPreview(rawFilePath, outputLargePath,
			outputMediumPath));
	}

	/// <summary>
	///     Synchronously extracts embedded JPEG previews from a RAW file.
	/// </summary>
	/// <param name="rawFilePath">Full path to the RAW file</param>
	/// <param name="outputLargePath">Output path for the large preview (or null to skip)</param>
	/// <param name="outputMediumPath">Output path for the medium preview (or null to skip)</param>
	/// <returns>true if at least one preview was successfully extracted</returns>
	public bool TryExtractPreview(string rawFilePath, string? outputLargePath,
		string? outputMediumPath)
	{
		if ( !File.Exists(rawFilePath) )
		{
			return false;
		}

		try
		{
			return new EmbeddedPreviewExtractor(logger).TryExtract(rawFilePath, outputLargePath,
				outputMediumPath);
		}
		catch ( Exception )
		{
			return false;
		}
	}
}
