using System.Text.Json.Serialization;
using starsky.foundation.database.JsonConverters;
using starsky.foundation.database.Models;

namespace starsky.foundation.metaupdate.Models;

public sealed class MetaUpdateBackgroundPayload
{
	public Dictionary<string, List<string>> ChangedFileIndexItemName { get; set; } = new();

	[JsonConverter(typeof(FileIndexItemWithIdJsonConverterFactory))]
	public List<FileIndexItem> FileIndexResultsList { get; set; } = [];

	public bool Collections { get; set; }
	public bool Append { get; set; }
	public int RotateClock { get; set; }
}
