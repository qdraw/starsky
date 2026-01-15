namespace starsky.foundation.metaupdate.Models;

/// <summary>
///     Request model for EXIF timezone correction
/// </summary>
public class ExifTimezoneCorrectionRequest
{
	/// <summary>
	///     The timezone the camera thought it was in (source offset)
	///     IANA timezone ID (e.g., "Europe/Amsterdam", "America/New_York")
	/// </summary>
	public required string RecordedTimezone { get; set; } = string.Empty;

	/// <summary>
	///     The actual timezone where the photo was taken (target offset)
	///     IANA timezone ID (e.g., "Europe/Amsterdam", "America/New_York")
	/// </summary>
	public required string CorrectTimezone { get; set; } = string.Empty;
}
