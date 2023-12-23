using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using starsky.foundation.georealtime.Models;
using starsky.foundation.georealtime.Models.GeoJson;

namespace starsky.foundation.georealtime.Helpers;

public static class IntermediateModelConverter
{
	public static FeatureCollectionModel Covert2GeoJson(
		List<LatitudeLongitudeAltDateTimeModel> inputModel, bool addPoints = true)
	{

		// todo add splliter by day - inactive
		
		var featureCollection = ConvertLineGeoJson(inputModel, new FeatureCollectionModel
		{
			Type = FeatureCollectionType.FeatureCollection,
			Features = new List<FeatureModel>()
		});

		return !addPoints ? featureCollection : ConvertPointsGeoJson(inputModel, featureCollection);
	}

	private static FeatureCollectionModel ConvertLineGeoJson(
		List<LatitudeLongitudeAltDateTimeModel> inputModel,
		FeatureCollectionModel featureCollection)
	{
		var lineStringModel = new FeatureModel
		{
			Type = FeatureType.Feature,
			Geometry = new GeometryLineStringModel()
			{
				Type = GeometryType.LineString,
				Coordinates =
					new List<List<double>>()
			},
			Properties = new PropertiesModel
			{
				DateTime = inputModel.FirstOrDefault()?.DateTime ?? DateTime.UtcNow
			}
		};
		
		foreach ( var model in inputModel )
		{
			(lineStringModel.Geometry as GeometryLineStringModel)!.Coordinates!.Add(new List<double>
			{
				model.Latitude,
				model.Longitude,
				model.Altitude ?? 0
			});
		}
		
		featureCollection.Features.Add(lineStringModel);
		
		return featureCollection;
	}

	private static FeatureCollectionModel ConvertPointsGeoJson(List<LatitudeLongitudeAltDateTimeModel> inputModel, FeatureCollectionModel featureCollection)
	{
		foreach ( var model in inputModel )
		{
			featureCollection.Features.Add(new FeatureModel
			{
				Type = FeatureType.Feature,
				Geometry = new GeometryPointModel()
				{
					Type = GeometryType.Point,
					Coordinates = 
					new List<double>
					{
						model.Latitude,
						model.Longitude,
						model.Altitude ?? 0
					}
				},
				Properties = new PropertiesModel
				{
					DateTime = model.DateTime
				}
			});
		}

		return featureCollection;
	}

	public static string ConvertToGpx(List<LatitudeLongitudeAltDateTimeModel> waypoints)
	{
		var settings = new XmlWriterSettings
		{
			Indent = true,
			IndentChars = "    "
		};

		using var stringWriter = new StringWriter();
		using var xmlWriter = XmlWriter.Create(stringWriter, settings);
		xmlWriter.WriteStartElement("gpx", "http://www.topografix.com/GPX/1/1");
		xmlWriter.WriteAttributeString("version", "1.1");
		xmlWriter.WriteAttributeString("creator", $"Starsky {Assembly.GetExecutingAssembly().GetName().Version?.ToString()}");

		foreach (var waypoint in waypoints)
		{
			xmlWriter.WriteStartElement("wpt");
			xmlWriter.WriteAttributeString("lat", waypoint.Latitude.ToString(CultureInfo.InvariantCulture));
			xmlWriter.WriteAttributeString("lon", waypoint.Longitude.ToString(CultureInfo.InvariantCulture));

			if (waypoint.Altitude.HasValue)
			{
				xmlWriter.WriteElementString("ele", waypoint.Altitude.Value.ToString(CultureInfo.InvariantCulture));
			}

			xmlWriter.WriteElementString("time", waypoint.DateTime.ToString("yyyy-MM-ddTHH:mm:ssZ"));

			xmlWriter.WriteEndElement(); // Close wpt element
		}

		xmlWriter.WriteEndElement(); // Close gpx element
		xmlWriter.Flush();

		return stringWriter.ToString();
	}
}
