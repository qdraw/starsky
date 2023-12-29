using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using starsky.foundation.georealtime.Models;
using starsky.foundation.platform.Helpers;

namespace starsky.foundation.georealtime.Helpers;

public static class Kml2IntermediateModelConverter
{
	
	public static List<LatitudeLongitudeAltDateTimeModel> ParseKml(XContainer kml)
	{
		var coordinateIntermediateModel = new List<LatitudeLongitudeAltDateTimeModel>();
		
		XNamespace kmlNamespace = "http://www.opengis.net/kml/2.2";
		var placeMarks = kml.Descendants(kmlNamespace + "Placemark");
		
		foreach (var placeMark in placeMarks)
		{
			var timeUtc = GetOpenGisDateTime(placeMark);
			
			// Extract coordinates from Point element
			var coordinatesNode = placeMark.Descendants(kmlNamespace + "coordinates").FirstOrDefault();
			if (coordinatesNode != null)
			{
				string[] coordinates = coordinatesNode.Value.Split(',');

				// Extract latitude, longitude, and altitude
				var latitude = GetDoubleValue(coordinates[1].Trim());
				var longitude = GetDoubleValue(coordinates[0].Trim());
				var altitude = GetDoubleValue(coordinates[2].Trim());
				
				if ( latitude == null || 
				     longitude == null || 
				     !ValidateLocation.ValidateLatitudeLongitude(latitude.Value, longitude.Value) || 
					 coordinateIntermediateModel.Any(p => Equals(p.Latitude, latitude) && Equals(p.Longitude, longitude)))
				{
					continue;
				}
				
				coordinateIntermediateModel.Add(new LatitudeLongitudeAltDateTimeModel
				{
					Longitude = longitude.Value,
					Latitude = latitude.Value,
					Altitude = altitude,
					DateTime = timeUtc
				});

				Console.WriteLine($"Latitude: {latitude}, Longitude: {longitude}, Altitude: {altitude}");
			}

		}
		return coordinateIntermediateModel;
	}

	private static double? GetDoubleValue(string input)
	{
		if ( double.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out var value) )
		{
			return value;
		}
		return null;
	}
	
	private static DateTime GetOpenGisDateTime(XContainer placeMark)
	{
		var timeUtc = DateTime.UtcNow;
		var timeUtcElement = placeMark.Descendants("{http://www.opengis.net/kml/2.2}Data")
			.FirstOrDefault(e => e.Attribute("name")?.Value == "Time UTC");

		var timeUtcString = timeUtcElement?.Element("{http://www.opengis.net/kml/2.2}value")?.Value;
		if ( !string.IsNullOrEmpty(timeUtcString) )
		{
			timeUtc = DateTime.ParseExact(timeUtcString, "M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture);
		}
		return timeUtc;
	}
}
