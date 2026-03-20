using System.Threading.Tasks;

namespace starsky.foundation.thumbnailgeneration.Interfaces;

/// <summary>
///     Service for extracting embedded JPEG previews from RAW image files
/// </summary>
public interface IEmbeddedRawThumbnailService
{
	/// <summary>
	///     Synchronously extracts embedded JPEG previews from a RAW file.
	/// </summary>
	/// <param name="rawFilePath">Full path to the RAW file</param>
	/// <param name="outputLargePath">Output path for the large preview (or null to skip)</param>
	/// <param name="outputMediumPath">Output path for the medium preview (or null to skip)</param>
	/// <returns>true if at least one preview was successfully extracted</returns>
	Task<bool> TryExtractPreview(string rawFilePath, string? outputLargePath,
		string? outputMediumPath);
}
