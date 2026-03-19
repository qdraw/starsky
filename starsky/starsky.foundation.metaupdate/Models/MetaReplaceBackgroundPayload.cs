namespace starsky.foundation.metaupdate.Models;

public sealed class MetaReplaceBackgroundPayload
{
	public List<string> SubPaths { get; set; } = [];
	public bool Collections { get; set; }
}
