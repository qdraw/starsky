using System.Threading.Tasks;

namespace starsky.foundation.thumbnailgeneration.Interfaces;

/// <summary>
///     Service for extracting embedded JPEG previews from RAW image files
/// </summary>
public interface IEmbeddedRawThumbnailService
{
	/// <summary>
	///     Extracts embedded JPEG previews from a RAW file.
	///     Attempts to find the largest available preview.
	/// </summary>
	/// <param name="rawFilePath">Full path to the RAW file (e.g., CR2, NEF, ARW, DNG)</param>
	/// <param name="outputLargePath">Output path for the large preview (or null to skip)</param>
	/// <param name="outputMediumPath">Output path for the medium preview (or null to skip)</param>
	/// <returns>true if at least one preview was successfully extracted</returns>
	Task<bool> TryExtractPreviewAsync(string rawFilePath, string? outputLargePath,
		string? outputMediumPath);

	/// <summary>
	///     Synchronously extracts embedded JPEG previews from a RAW file.
	/// </summary>
	/// <param name="rawFilePath">Full path to the RAW file</param>
	/// <param name="outputLargePath">Output path for the large preview (or null to skip)</param>
	/// <param name="outputMediumPath">Output path for the medium preview (or null to skip)</param>
	/// <returns>true if at least one preview was successfully extracted</returns>
	bool TryExtractPreview(string rawFilePath, string? outputLargePath,
		string? outputMediumPath);
}

