using System.Text.Json.Serialization;

namespace starsky.foundation.platform.Models;

public class MetadataContainer
{
	/// <summary>
	/// Only for JsonSchema
	/// And Keep on top of the class
	/// </summary>
	[JsonPropertyName("$id")]
	public string Id { get; set; } = "https://docs.qdraw.nl/schema/meta-data-container.json";
	
	[JsonPropertyName("$schema")]
	public string Schema { get; set; } = "https://json-schema.org/draft/2020-12/schema";

	public object Item { get; set; }
}
