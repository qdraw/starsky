using System.Threading.Tasks;

namespace starsky.foundation.thumbnailgeneration.Interfaces;

/// <summary>
///     Service for extracting embedded preview images from RAW file formats.
/// </summary>
public interface IEmbeddedRawThumbnailService
{
	/// <summary>
	///     Attempts to extract embedded preview images from a RAW file.
	/// </summary>
	/// <param name="rawFilePath">Full path to the RAW file</param>
	/// <param name="outputLargePath">Optional path to save the largest preview</param>
	/// <returns>True if preview was successfully extracted, false otherwise</returns>
	Task<bool> TryExtractPreview(string rawFilePath, string? outputLargePath);
}
