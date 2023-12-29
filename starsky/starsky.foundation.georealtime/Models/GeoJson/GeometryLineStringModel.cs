using System.Collections.Generic;

namespace starsky.foundation.georealtime.Models.GeoJson;

public class GeometryLineStringModel : GeometryBaseModel
{
	public override GeometryType? Type { get; set; } = GeometryType.LineString;

	public List<List<double>>? Coordinates { get; set; } = new List<List<double>>();

}
