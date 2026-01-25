using starsky.foundation.database.Models;
using starsky.foundation.metaupdate.Models;

namespace starsky.foundation.metaupdate.Interfaces;

/// <summary>
///     Service to correct EXIF timestamps for images recorded in the wrong timezone or with custom offsets
/// </summary>
public interface IExifTimezoneCorrectionService
{
	/// <summary>
	///     Validate timezone or custom offset correction request
	/// </summary>
	/// <param name="subPaths">File paths to validate</param>
	/// <param name="collections">Include collections</param>
	/// <param name="request">Timezone or custom offset correction parameters</param>
	/// <returns>Results for each image</returns>
	Task<List<ExifTimezoneCorrectionResult>> Validate(
		string[] subPaths,
		bool collections,
		IExifTimeCorrectionRequest request);

	/// <summary>
	///     Correct EXIF timestamps for multiple images
	/// </summary>
	/// <param name="fileIndexItems">The images to correct</param>
	/// <param name="request">Timezone or custom offset correction parameters</param>
	/// <returns>Results for each image</returns>
	Task<List<ExifTimezoneCorrectionResult>> CorrectTimezoneAsync(
		List<FileIndexItem> fileIndexItems,
		IExifTimeCorrectionRequest request);
}
