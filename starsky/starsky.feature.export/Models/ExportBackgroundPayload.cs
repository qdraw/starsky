using System.Collections.Generic;
using starsky.foundation.database.Models;

namespace starsky.feature.export.Models;

public sealed class ExportBackgroundPayload
{
	public List<FileIndexItem> FileIndexResultsList { get; set; } = [];
	public bool Thumbnail { get; set; }
	public string ZipOutputName { get; set; } = string.Empty;
}
