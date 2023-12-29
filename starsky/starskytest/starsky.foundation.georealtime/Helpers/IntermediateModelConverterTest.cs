using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.georealtime.Helpers;
using starsky.foundation.georealtime.Models;
using starsky.foundation.georealtime.Models.GeoJson;
using starsky.foundation.platform.JsonConverter;

namespace starskytest.starsky.foundation.georealtime.Helpers
{
	[TestClass]
	public class IntermediateModelConverterTests
	{
		[TestMethod]
		public void Convert2GeoJson_ReturnsFeatureCollection_WithPoint()
		{
			// Arrange
			var inputModel = new List<LatitudeLongitudeAltDateTimeModel>
			{
				// Populate with test data
				new LatitudeLongitudeAltDateTimeModel
				{
					Longitude = 52.377956,
					Latitude = 4.897070, // amsterdam
					Altitude = 7.4,
					DateTime = DateTime.Now
				},
			};

			// Act
			var result = IntermediateModelConverter.Covert2GeoJson(inputModel);
			
			// Assert
			Assert.IsNotNull(result);
			
			Assert.AreEqual(2, result.Features.Count);

			Assert.AreEqual(GeometryType.LineString, result.Features.FirstOrDefault()?.Geometry?.Type);
			
			var lineStringModel = result.Features.FirstOrDefault()?.
				Geometry as GeometryLineStringModel;
			//  [longitude, latitude, altitude]
			Assert.AreEqual(4.897070 , lineStringModel?.Coordinates?.FirstOrDefault()?[0]);
			Assert.AreEqual(52.377956, lineStringModel?.Coordinates?.FirstOrDefault()?[1]);
			Assert.AreEqual(7.4, lineStringModel?.Coordinates?.FirstOrDefault()?[2]);
			
			var pointModel = result.Features[1]?.
				Geometry as GeometryPointModel;
			
			Assert.AreEqual(GeometryType.Point, result.Features[1].Geometry?.Type);
			Assert.AreEqual(4.89707, pointModel?.Coordinates?[0]);
			Assert.AreEqual(52.377956, pointModel?.Coordinates?[1]);
			Assert.AreEqual(7.4, pointModel?.Coordinates?[2]);
		}
		
		[TestMethod]
		public void Convert2GeoJson_ReturnsFeatureCollection_NoPoint()
		{
			// Arrange
			var inputModel = new List<LatitudeLongitudeAltDateTimeModel>
			{
				// Populate with test data
				new LatitudeLongitudeAltDateTimeModel
				{
					Longitude = 52.377956,
					Latitude = 4.897070, // amsterdam
					Altitude = 7.4,
					DateTime = DateTime.Now
				},
			};

			// Act
			var result = IntermediateModelConverter.Covert2GeoJson(inputModel, false);

			// Assert
			Assert.IsNotNull(result);
			
			Assert.AreEqual(1, result.Features.Count);

			Assert.AreEqual(GeometryType.LineString, result.Features.FirstOrDefault()?.Geometry?.Type);
			
			var lineStringModel = result.Features.FirstOrDefault()?.
				Geometry as GeometryLineStringModel;

			Assert.AreEqual(4.897070 , lineStringModel?.Coordinates?.FirstOrDefault()?[0]);
			Assert.AreEqual(52.377956, lineStringModel?.Coordinates?.FirstOrDefault()?[1]);
			Assert.AreEqual(7.4, lineStringModel?.Coordinates?.FirstOrDefault()?[2]);
		}
		
		[TestMethod]
		public void TwoPoints()
		{
			var input = new List<LatitudeLongitudeAltDateTimeModel>
			{
				new LatitudeLongitudeAltDateTimeModel
				{
					Longitude = 42.448770,
					Latitude = 1.2,
					Altitude = 170,
					DateTime = new DateTime(2023, 6, 29, 13, 15, 16)
				},
				new LatitudeLongitudeAltDateTimeModel
				{
					Longitude = 42.448769,
					Latitude = 1.2,
					Altitude = 170,
					DateTime = new DateTime(2023, 6, 29, 13, 15, 16)
				}
			};

			var result = IntermediateModelConverter.Covert2GeoJson(input, true);
		
			Assert.IsNotNull(result);
			
			Assert.AreEqual(3, result.Features.Count);

			Assert.AreEqual(GeometryType.LineString, result.Features.FirstOrDefault()?.Geometry?.Type);
			
			var lineStringModel = result.Features.FirstOrDefault()?.
				Geometry as GeometryLineStringModel;
			
			//  [longitude, latitude, altitude]
			Assert.AreEqual(1.2 , lineStringModel?.Coordinates?.FirstOrDefault()?[0]);
			Assert.AreEqual(42.448770, lineStringModel?.Coordinates?.FirstOrDefault()?[1]);
			Assert.AreEqual(170, lineStringModel?.Coordinates?.FirstOrDefault()?[2]);
			
			Assert.AreEqual(1.2 , lineStringModel?.Coordinates?[1]?[0]);
			Assert.AreEqual(42.448769, lineStringModel?.Coordinates?[1]?[1]);
			Assert.AreEqual(170, lineStringModel?.Coordinates?[1]?[2]);
			
			var pointModel1 = result.Features[1]?.
				Geometry as GeometryPointModel;
			
			Assert.AreEqual(GeometryType.Point, result.Features[1].Geometry?.Type);
			Assert.AreEqual(1.2, pointModel1?.Coordinates?[0]);
			Assert.AreEqual(42.44877, pointModel1?.Coordinates?[1]);
			Assert.AreEqual(170, pointModel1?.Coordinates?[2]);
			
			var pointModel2 = result.Features[2]?.
				Geometry as GeometryPointModel;
			
			Assert.AreEqual(GeometryType.Point, result.Features[1].Geometry?.Type);
			Assert.AreEqual(1.2, pointModel2?.Coordinates?[0]);
			Assert.AreEqual(42.448769, pointModel2?.Coordinates?[1]);
			Assert.AreEqual(170, pointModel2?.Coordinates?[2]);
		}

		[TestMethod]
		public void TwoPointsJson()
		{
			var input = new List<LatitudeLongitudeAltDateTimeModel>
			{
				new LatitudeLongitudeAltDateTimeModel
				{
					Longitude = 42.448770,
					Latitude = 1.2,
					Altitude = 170,
					DateTime = new DateTime(2023, 6, 29, 13, 15, 16)
				},
				new LatitudeLongitudeAltDateTimeModel
				{
					Longitude = 42.448769,
					Latitude = 1.2,
					Altitude = 170,
					DateTime = new DateTime(2023, 6, 29, 13, 15, 16)
				}
			};

			var result = IntermediateModelConverter.Covert2GeoJson(input, true);
			var jsonResult = JsonSerializer.Serialize(result, DefaultJsonSerializer.CamelCaseNoEnters);
			
			Assert.AreEqual("{\"type\":\"FeatureCollection\",\"features\":" +
			                "[{\"type\":\"Feature\",\"geometry\":{\"type\":\"LineString\"," +
			                "\"coordinates\":[[1.2,42.44877,170],[1.2,42.448769,170]]}," +
			                "\"properties\":{\"name\":null,\"dateTime\":\"2023-06-29T13:15:16\"}}," +
			                "{\"type\":\"Feature\",\"geometry\":{\"type\":\"Point\",\"coordinates\":" +
			                "[1.2,42.44877,170]},\"properties\":{\"name\":null,\"dateTime\":\"2023-06-29T13:15:16\"}}," +
			                "{\"type\":\"Feature\",\"geometry\":{\"type\":\"Point\",\"coordinates\":[1.2,42.448769,170]}," +
			                "\"properties\":{\"name\":null,\"dateTime\":\"2023-06-29T13:15:16\"}}]}",jsonResult);

		}

		[TestMethod]
		public void ConvertToGpx_ReturnsValidGpxString()
		{
			// Arrange
			var waypoints = new List<LatitudeLongitudeAltDateTimeModel>
			{
				// Populate with test data
				new LatitudeLongitudeAltDateTimeModel
				{
					Longitude = 0.0,
					Latitude = 0.0,
					Altitude = 100.0,
					DateTime = DateTime.Now
				},
				// Add more test data as needed
			};

			// Act
			var result = IntermediateModelConverter.ConvertToGpx(waypoints);

			// Assert
			Assert.IsNotNull(result);
			// Add more specific assertions based on your expectations
		}
	}
}
