using System.Text.Json.Serialization;
using starsky.foundation.georealtime.Converter;

namespace starsky.foundation.georealtime.Models.GeoJson;

[JsonConverter(typeof(GeometryBaseModelConverter))]
public abstract class GeometryBaseModel
{
	public virtual GeometryType? Type { get; set; }
}
