using System.Text.Json.Serialization;
using starsky.foundation.database.JsonConverters;
using starsky.foundation.database.Models;

namespace starsky.foundation.metaupdate.Models;

public sealed class MetaReplaceBackgroundPayload
{
	/// <summary>
	///     Pre-computed map of filePath → changed field names, so the job does not
	///     have to re-read from a potentially stale cache.
	/// </summary>
	public Dictionary<string, List<string>> ChangedFileIndexItemName { get; set; } = new();

	/// <summary>
	///     The already-modified items (with the replaced values) to be written to
	///     EXIF / the database by the background job.
	/// </summary>
	[JsonConverter(typeof(FileIndexItemWithIdJsonConverterFactory))]
	public List<FileIndexItem> FileIndexResultsList { get; set; } = new();
}
