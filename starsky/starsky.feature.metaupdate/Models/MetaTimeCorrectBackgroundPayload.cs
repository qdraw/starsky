using System.Collections.Generic;
using starsky.foundation.metaupdate.Models;

namespace starsky.feature.metaupdate.Models;

public sealed class MetaTimeCorrectBackgroundPayload
{
	public List<ExifTimezoneCorrectionResult> ValidateResults { get; set; } = [];
	public string RequestType { get; set; } = string.Empty;
	public string RequestJson { get; set; } = string.Empty;
	public string CorrectionType { get; set; } = string.Empty;
}
