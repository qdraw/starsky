using System.Collections.Generic;
using starsky.foundation.georealtime.Models.GeoJson;

namespace starsky.foundation.georealtime.Helpers;

public static class DefaultGeoJson
{
	public static FeatureCollectionModel CreateDefaultGeoJson(List<List<double>> coordinates)
	{
		return new FeatureCollectionModel
		{
			Type = FeatureCollectionType.FeatureCollection,
			Features = new List<FeatureModel>
			{
				new FeatureModel
				{
					Type = FeatureType.Feature,
					Geometry = new GeometryModel
					{
						Coordinates = coordinates
					},
					Properties = new PropertiesModel()
				}
			}
		};
	}
}
