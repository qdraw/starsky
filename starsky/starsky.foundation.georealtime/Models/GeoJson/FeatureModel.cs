namespace starsky.foundation.georealtime.Models.GeoJson;

public class FeatureModel
{
	public FeatureType? Type { get; set; }
	public GeometryModel Geometry { get; set; }
	public PropertiesModel? Properties { get; set; }
}
