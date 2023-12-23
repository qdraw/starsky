using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.georealtime.Models.GeoJson;
using starsky.foundation.platform.JsonConverter;

namespace starskytest.starsky.foundation.georealtime.Models;

[TestClass]
public class ModelsParserTest
{
	[TestMethod]
	public void LineStringParser()
	{
		//  [longitude, latitude, altitude]
		const string geoJson = @"
        {
            ""type"": ""FeatureCollection"",
            ""features"": [
                {
                    ""type"": ""Feature"",
                    ""geometry"": {
                        ""type"": ""LineString"",
                        ""coordinates"": [
                            [5.485941, 51.809360, 7.263],
                            [5.485724, 51.807968, 7.772],
                            [5.485631, 51.807019, 9.957],
                            [5.485610, 51.806702, 10.808],
                            [5.485633, 51.805663, 10.866],
                            [5.485738, 51.805500, 9.242],
                            [5.486056, 51.805115, 9.122]
                        ]
                    }
                }
            ]
        }";

		// Deserialize GeoJSON string to C# objects
		var featureCollection = System.Text.Json.JsonSerializer.Deserialize<FeatureCollectionModel>(geoJson, 
			DefaultJsonSerializer.CamelCase);
		
		// Assert
		Assert.IsNotNull(featureCollection);
		Assert.AreEqual(FeatureCollectionType.FeatureCollection, featureCollection.Type);

		Assert.IsNotNull(featureCollection.Features);
		Assert.AreEqual(1, featureCollection.Features.Count);

		var feature = featureCollection.Features[0];
		Assert.IsNotNull(feature);
		Assert.AreEqual(FeatureType.Feature, feature.Type);

		var geometry = feature.Geometry as GeometryLineStringModel;
		Assert.IsNotNull(geometry);
		Assert.AreEqual(GeometryType.LineString, geometry.Type);

		var coordinates = geometry.Coordinates;
		Assert.IsNotNull(coordinates);
		Assert.AreEqual(7, coordinates.Count);
	}

	[TestMethod]
	public void PointParser()
	{
		//  [longitude, latitude, altitude]
		const string geoJson = @"
        {
            ""type"": ""FeatureCollection"",
            ""features"": [
                {
                    ""type"": ""Feature"",
                    ""geometry"": {
                        ""type"": ""Point"",
                        ""coordinates"": 
                            [5.485941, 51.809360, 7.263]
                    }
                }
            ]
        }";

		// Deserialize GeoJSON string to C# objects
		var featureCollection = System.Text.Json.JsonSerializer.Deserialize<FeatureCollectionModel>(geoJson, 
			DefaultJsonSerializer.CamelCase);
		
		// Assert
		Assert.IsNotNull(featureCollection);
		Assert.AreEqual(FeatureCollectionType.FeatureCollection, featureCollection.Type);

		Assert.IsNotNull(featureCollection.Features);
		Assert.AreEqual(1, featureCollection.Features.Count);

		var feature = featureCollection.Features[0];
		Assert.IsNotNull(feature);
		Assert.AreEqual(FeatureType.Feature, feature.Type);

		var geometry = feature.Geometry as GeometryPointModel;
		Assert.IsNotNull(geometry);
		Assert.AreEqual(GeometryType.Point, geometry.Type);

		var coordinates = geometry.Coordinates;
		Assert.IsNotNull(coordinates);
		Assert.AreEqual(3, coordinates.Count);
		
		Assert.AreEqual(5.485941, coordinates[0]);
		Assert.AreEqual(51.809360, coordinates[1]);
		Assert.AreEqual(7.263, coordinates[2]);
	}

	[TestMethod]
	public void LineStringAndPointParser()
	{
		//  [longitude, latitude, altitude]
		const string geoJson = @"
        {
            ""type"": ""FeatureCollection"",
            ""features"": [
                {
                    ""type"": ""Feature"",
                    ""geometry"": {
                        ""type"": ""LineString"",
                        ""coordinates"": [
                            [5.485941, 51.809360, 7.263],
                            [5.485724, 51.807968, 7.772]
                        ]
                    }
                },
                {
                    ""type"": ""Feature"",
                    ""geometry"": {
                        ""type"": ""Point"",
                        ""coordinates"": 
                            [5.485941, 51.809360, 7.263]
                    }
                }
            ]
        }";
		
		var featureCollection = System.Text.Json.JsonSerializer.Deserialize<FeatureCollectionModel>(geoJson, 
			DefaultJsonSerializer.CamelCase);
		
		// First LineString
		var feature0 = featureCollection.Features[0];
		Assert.IsNotNull(feature0);
		Assert.AreEqual(FeatureType.Feature, feature0.Type);

		var geometry0 = feature0.Geometry as GeometryLineStringModel;
		Assert.IsNotNull(geometry0);
		Assert.AreEqual(GeometryType.LineString, geometry0.Type);
		
		// Second Point
		var feature1 = featureCollection.Features[1];
		Assert.IsNotNull(feature1);
		Assert.AreEqual(FeatureType.Feature, feature1.Type);

		var geometry1 = feature1.Geometry as GeometryPointModel;
		Assert.IsNotNull(geometry1);
		Assert.AreEqual(GeometryType.Point, geometry1.Type);

	}
}
