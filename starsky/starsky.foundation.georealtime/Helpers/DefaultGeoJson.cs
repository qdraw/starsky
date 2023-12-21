using starsky.foundation.georealtime.Models.GeoJson;

namespace starsky.foundation.georealtime.Helpers;

public class DefaultGeoJson
{
	public void CreateDefaultGeoJson()
	{
		var featureCollection = new FeatureCollectionModel
		{
			Type = FeatureCollectionType.FeatureCollection
		};
	}
}
