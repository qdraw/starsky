using System;

namespace starsky.foundation.metaupdate.Models;

/// <summary>
/// Request model for EXIF timezone correction
/// </summary>
public class ExifTimezoneCorrectionRequest
{
	/// <summary>
	/// The timezone the camera thought it was in (source offset)
	/// IANA timezone ID (e.g., "Europe/Amsterdam", "America/New_York")
	/// </summary>
	public string RecordedTimezone { get; set; } = string.Empty;

	/// <summary>
	/// The actual timezone where the photo was taken (target offset)
	/// IANA timezone ID (e.g., "Europe/Amsterdam", "America/New_York")
	/// </summary>
	public string CorrectTimezone { get; set; } = string.Empty;
}

/// <summary>
/// Result model for EXIF timezone correction
/// </summary>
public class ExifTimezoneCorrectionResult
{
	/// <summary>
	/// Whether the correction was successful
	/// </summary>
	public bool Success { get; set; }

	/// <summary>
	/// Original DateTime value before correction
	/// </summary>
	public DateTime? OriginalDateTime { get; set; }

	/// <summary>
	/// Corrected DateTime value after correction
	/// </summary>
	public DateTime? CorrectedDateTime { get; set; }

	/// <summary>
	/// Time delta applied (in hours)
	/// </summary>
	public double DeltaHours { get; set; }

	/// <summary>
	/// Warning message if any
	/// </summary>
	public string? Warning { get; set; }

	/// <summary>
	/// Error message if correction failed
	/// </summary>
	public string? Error { get; set; }
}

