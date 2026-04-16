using System.Collections.Generic;

namespace starsky.foundation.import.Models;

public sealed class ImportBackgroundPayload
{
	public List<string> TempImportPaths { get; set; } = [];
	public ImportSettingsModel ImportSettings { get; set; } = new();
	public bool IsVerbose { get; set; }
}
