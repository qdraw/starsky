using System.Text.Json.Serialization;

namespace starsky.foundation.georealtime.Models.GeoJson;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum FeatureType
{
	Feature
}
