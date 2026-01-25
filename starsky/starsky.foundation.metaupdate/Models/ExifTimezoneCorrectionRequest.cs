namespace starsky.foundation.metaupdate.Models;

/// <summary>
///     Base interface for EXIF time correction requests
/// </summary>
public interface IExifTimeCorrectionRequest
{
}

/// <summary>
///     Request model for timezone-based EXIF correction
///     Uses IANA timezone IDs to calculate offset differences (DST-aware)
/// </summary>
public class ExifTimezoneBasedCorrectionRequest : IExifTimeCorrectionRequest
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

/// <summary>
///     Request model for custom offset EXIF correction
///     Uses custom time/date offsets (years, months, days, hours, minutes, seconds)
/// </summary>
public class ExifCustomOffsetCorrectionRequest : IExifTimeCorrectionRequest
{
	/// <summary>
	///     Custom offset: Years to add/subtract (can be negative)
	/// </summary>
	public int? CustomOffsetYears { get; set; }

	/// <summary>
	///     Custom offset: Months to add/subtract (can be negative)
	/// </summary>
	public int? CustomOffsetMonths { get; set; }

	/// <summary>
	///     Custom offset: Days to add/subtract (can be negative)
	/// </summary>
	public int? CustomOffsetDays { get; set; }

	/// <summary>
	///     Custom offset: Hours to add/subtract (can be negative)
	/// </summary>
	public int? CustomOffsetHours { get; set; }

	/// <summary>
	///     Custom offset: Minutes to add/subtract (can be negative)
	/// </summary>
	public int? CustomOffsetMinutes { get; set; }

	/// <summary>
	///     Custom offset: Seconds to add/subtract (can be negative)
	/// </summary>
	public int? CustomOffsetSeconds { get; set; }

	/// <summary>
	///     Check if at least one offset value is provided
	/// </summary>
	public bool HasAnyOffset =>
		CustomOffsetYears.HasValue ||
		CustomOffsetMonths.HasValue ||
		CustomOffsetDays.HasValue ||
		CustomOffsetHours.HasValue ||
		CustomOffsetMinutes.HasValue ||
		CustomOffsetSeconds.HasValue;
}

