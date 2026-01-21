using starsky.foundation.database.Models;

namespace starsky.foundation.metaupdate.Models;

/// <summary>
///     Result model for EXIF timezone correction
/// </summary>
public class ExifTimezoneCorrectionResult
{
	/// <summary>
	///     Whether the correction was successful
	/// </summary>
	public bool Success { get; set; }

	/// <summary>
	///     Original DateTime value before correction
	/// </summary>
	public DateTime OriginalDateTime { get; set; }

	/// <summary>
	///     Corrected DateTime value after correction
	/// </summary>
	public DateTime CorrectedDateTime { get; set; }

	/// <summary>
	///     Time delta applied
	/// </summary>
	public TimeSpan Delta { get; set; }

	/// <summary>
	///     Warning message if any
	/// </summary>
	public string Warning { get; set; } = string.Empty;

	/// <summary>
	///     Error message if correction failed
	/// </summary>
	public string Error { get; set; } = string.Empty;

	public FileIndexItem? FileIndexItem { get; set; }
}
