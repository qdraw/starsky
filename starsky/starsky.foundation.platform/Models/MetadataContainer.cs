using System.Text.Json.Serialization;

namespace starsky.foundation.platform.Models;

public class MetadataContainer
{
	[JsonPropertyName("$id")]
	public string Id { get; set; } = "https://docs.qdraw.nl/openapi/schema.json";
	
	[JsonPropertyName("$schema")]
	public string Schema { get; set; } = "https://json-schema.org/draft/2020-12/schema";

	public object Item { get; set; }
}
