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
	///     Extracts embedded JPEG previews from a RAW file.
	/// </summary>
	/// <param name="rawFilePath">Full path to the RAW file</param>
	/// <param name="outputLargePath">Output path for the large preview (or null to skip)</param>
	/// <param name="outputMediumPath">Output path for the medium preview (or null to skip)</param>
	/// <returns>true if at least one preview was successfully extracted</returns>
	public async Task<bool> TryExtractPreview(string rawFilePath, string? outputLargePath,
		string? outputMediumPath)
	{
		if ( !File.Exists(rawFilePath) )
		{
			return false;
		}

		try
		{
			var result = new EmbeddedPreviewExtractor(logger).TryExtract(rawFilePath,
				outputLargePath,
				outputMediumPath);

			// If extraction succeeded and MozJPEG is available, recompresses previews
			// to fix corrupted/malformed JPEG data and ensure compatibility with standard viewers
			if ( result && mozJpegService != null )
			{
				_ = OptimizeExtractedPreviewsAsync(outputLargePath, outputMediumPath);
			}

			return result;
		}
		catch ( Exception exception )
		{
			logger.LogError($"Failed to extract embedded preview from RAW file: {rawFilePath}. " +
			                $"Exception: {exception.Message}");
			return false;
		}
	}

	/// <summary>
	///     Recompresses extracted JPEG previews using MozJPEG to fix corruption.
	///     This ensures extracted previews are valid and compatible with standard image viewers.
	/// </summary>
	private async Task OptimizeExtractedPreviews(string? outputLargePath, string? outputMediumPath)
	{
		var targets = new List<ImageOptimisationItem>();

		if ( outputLargePath != null && File.Exists(outputLargePath) )
		{
			targets.Add(new ImageOptimisationItem
			{
				InputPath = outputLargePath,
				OutputPath = outputLargePath,
				ImageFormat = ExtensionRolesHelper.ImageFormat.jpg
			});
		}

		if ( outputMediumPath != null && File.Exists(outputMediumPath) )
		{
			targets.Add(new ImageOptimisationItem
			{
				InputPath = outputMediumPath, OutputPath = outputMediumPath
			});
		}

		if ( targets.Count == 0 )
		{
			return;
		}

		try
		{
			// Run MozJPEG optimization in fire-and-forget manner
			// Errors are logged by MozJpegService; we don't block preview extraction
			await mozJpegService.Optimize(
				targets,
				[
					new Optimizer()
					{
						Enabled = true,
						Id = "mozjpeg",
						ImageFormats =
							[ExtensionRolesHelper.ImageFormat.jpg],
						Options = new OptimizerOptions { Quality = 80 }
					}
				]
			);
		}
		catch ( Exception ex )
		{
			logger.LogDebug(
				$"[EmbeddedRawThumbnailService] MozJPEG optimization failed: {ex.Message}");
		}
	}
}
