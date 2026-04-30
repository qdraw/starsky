using System.Text.Json.Serialization;
using starsky.foundation.database.JsonConverters;
using starsky.foundation.database.Models;

namespace starsky.feature.trash.Models;

public sealed class MoveToTrashPayload
{
	[JsonConverter(typeof(FileIndexItemWithIdJsonConverterFactory))]
	public List<FileIndexItem> MoveToTrashList { get; set; } = [];

	public bool IsSystemTrashEnabled { get; set; }
	public Dictionary<string, List<string>> ChangedFileIndexItemName { get; set; } = new();

	[JsonConverter(typeof(FileIndexItemWithIdJsonConverterFactory))]
	public List<FileIndexItem> FileIndexResultsList { get; set; } = [];

	[JsonConverter(typeof(FileIndexItemWithIdJsonConverterFactory))]
	public FileIndexItem InputModel { get; set; } = new();

	public bool Collections { get; set; }
}
