using starsky.foundation.database.Models;
using starsky.foundation.metaupdate.Models;

namespace starsky.foundation.metaupdate.Interfaces;

/// <summary>
///     Service to correct EXIF timestamps for images recorded in the wrong timezone
/// </summary>
public interface IExifTimezoneCorrectionService
{
	/// <summary>
	///     Correct EXIF timestamps for a single image
	/// </summary>
	/// <param name="fileIndexItem">The image to correct</param>
	/// <param name="request">Timezone correction parameters</param>
	/// <returns>Result of the correction operation</returns>
	Task<ExifTimezoneCorrectionResult> CorrectTimezoneAsync(
		FileIndexItem fileIndexItem,
		ExifTimezoneCorrectionRequest request);

	/// <summary>
	///     Correct EXIF timestamps for multiple images
	/// </summary>
	/// <param name="fileIndexItems">The images to correct</param>
	/// <param name="request">Timezone correction parameters</param>
	/// <returns>Results for each image</returns>
	Task<List<ExifTimezoneCorrectionResult>> CorrectTimezoneAsync(
		List<FileIndexItem> fileIndexItems,
		ExifTimezoneCorrectionRequest request);

	/// <summary>
	///     Validate timezone correction request
	/// </summary>
	/// <param name="fileIndexItem">The image to validate</param>
	/// <param name="request">Timezone correction parameters</param>
	/// <returns>Validation result with warnings</returns>
	ExifTimezoneCorrectionResult ValidateCorrection(
		FileIndexItem fileIndexItem,
		ExifTimezoneCorrectionRequest request);
}
