using System.Collections.Generic;

namespace starsky.feature.rename.Models;

public class BatchRenameRequest
{
	public List<string> FilePaths { get; set; } = [];
	public string Pattern { get; set; } = string.Empty;
	public bool Collections { get; set; }
}
