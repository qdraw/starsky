using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Models;

namespace starsky.foundation.import.Models;

public sealed class ImportIndexJsonContainer
{
	[JsonPropertyName("$id")]
	public string Id { get; set; } =
		"https://docs.qdraw.nl/schema/import-index-container.json";

	[JsonPropertyName("$schema")]
	public string Schema { get; set; } = "https://json-schema.org/draft/2020-12/schema";

	public DateTime ExportedAtUtc { get; set; } = DateTime.UtcNow;

	public string Version { get; set; } = string.Empty;

	public AppSettingsStructureModel Structure { get; set; } = new();

	public List<ImportIndexItem> Items { get; set; } = [];
}