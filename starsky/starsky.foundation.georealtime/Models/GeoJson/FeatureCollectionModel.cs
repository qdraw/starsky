using System.Collections.Generic;

namespace starsky.foundation.georealtime.Models.GeoJson;

public class FeatureCollectionModel
{
	public FeatureCollectionType? Type { get; set; }
	public List<FeatureModel> Features { get; set; }
}
