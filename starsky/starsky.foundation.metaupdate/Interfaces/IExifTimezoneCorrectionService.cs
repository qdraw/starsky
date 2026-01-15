using starsky.foundation.database.Models;
using starsky.foundation.metaupdate.Models;

namespace starsky.foundation.metaupdate.Interfaces;

/// <summary>
///     Service to correct EXIF timestamps for images recorded in the wrong timezone
/// </summary>
public interface IExifTimezoneCorrectionService
{
	/// <summary>
	///     Validate timezone correction request
	/// </summary>
	/// <param name="request">Timezone correction parameters</param>
	/// <returns>Results for each image</returns>
	Task<List<ExifTimezoneCorrectionResult>> Validate(
		string[] subPaths,
		bool collections,
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
}
