using System.Collections.Generic;
using starsky.foundation.metaupdate.Models;

namespace starsky.feature.rename.Models;

/// <summary>
///     Request model for filename datetime repair operations
/// </summary>
public class FilenameDatetimeRepairRequest<T> where T : IExifTimeCorrectionRequest
{
	/// <summary>
	///     List of file paths to process
	/// </summary>
	public required List<string> FilePaths { get; set; } = [];

	/// <summary>
	///     Include related sidecar files (XMP, etc.)
	/// </summary>
	public bool Collections { get; set; } = true;

	/// <summary>
	///     Timezone or custom offset correction request
	/// </summary>
	public required T CorrectionRequest { get; set; }
}
