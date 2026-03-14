using starsky.foundation.database.Models;

namespace starsky.foundation.metaupdate.Models;

public sealed class MetaReplaceBackgroundPayload
{
	public Dictionary<string, List<string>> ChangedFileIndexItemName { get; set; } = new();
	public List<FileIndexItem> ResultsOkOrDeleteList { get; set; } = [];
	public bool Collections { get; set; }
}
