using System.Collections.Generic;

namespace starsky.foundation.georealtime.Models.GeoJson;

public class GeometryPointModel : GeometryBaseModel
{
	public override GeometryType? Type { get; set; } = GeometryType.Point;

	public List<double>? Coordinates { get; set; } = new List<double>();

}
