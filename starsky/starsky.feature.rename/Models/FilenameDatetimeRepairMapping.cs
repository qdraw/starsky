using System;
using System.Collections.Generic;
using starsky.feature.rename.Helpers;
using starsky.foundation.database.Models;

namespace starsky.feature.rename.Models;

/// <summary>
///     Result model for filename datetime repair preview
/// </summary>
public class FilenameDatetimeRepairMapping : IFileItemQuery
{
	/// <summary>
	///     Source file path
	/// </summary>
	public string SourceFilePath { get; set; } = string.Empty;

	/// <summary>
	///     Detected datetime pattern in filename (e.g., YYYYMMDD_HHMMSS)
	/// </summary>
	public string DetectedPatternDescription { get; set; } = string.Empty;

	/// <summary>
	///     Original datetime extracted from filename
	/// </summary>
	public DateTime? OriginalDateTime { get; set; }

	/// <summary>
	///     Corrected datetime after applying offset
	/// </summary>
	public DateTime? CorrectedDateTime { get; set; }

	/// <summary>
	///     Target file path with corrected datetime in filename
	/// </summary>
	public string TargetFilePath { get; set; } = string.Empty;

	/// <summary>
	///     Related file paths (sidecars)
	/// </summary>
	public List<(string source, string target)> RelatedFilePaths { get; set; } = [];

	/// <summary>
	///     Time offset applied (in hours)
	/// </summary>
	public double OffsetHours { get; set; }

	/// <summary>
	///     Indicates if there was an error
	/// </summary>
	public bool HasError { get; set; }

	/// <summary>
	///     Error message if HasError is true
	/// </summary>
	public string? ErrorMessage { get; set; }

	/// <summary>
	///     Warning message (e.g., day rollover)
	/// </summary>
	public string? Warning { get; set; }

	public FileIndexItem? FileIndexItem { get; set; }
}
