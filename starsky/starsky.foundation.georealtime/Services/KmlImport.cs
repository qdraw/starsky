using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using starsky.foundation.georealtime.Interfaces;
using starsky.foundation.georealtime.Models;
using starsky.foundation.http.Interfaces;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;

namespace starsky.foundation.georealtime.Services;

public sealed class KmlImport : IKmlImport
{
	private readonly IHttpClientHelper _httpClientHelper;
	private readonly IStorage _hostStorage;

	public KmlImport(IHttpClientHelper httpClientHelper, ISelectorStorage selectorStorage)
	{
		_httpClientHelper = httpClientHelper;
		_hostStorage =
			selectorStorage.Get(SelectorStorage.StorageServices.HostFilesystem);
	}
	
	public async Task Import(string kmlPathOrUrl)
	{

		var readString = await _httpClientHelper.ReadString(kmlPathOrUrl);
		if ( !readString.Key )
		{
			return;
		}

		var xDocument = XmlParse(readString.Value);
		ParseKml(xDocument);
	}

	private static XDocument XmlParse(string content)
	{
		return XDocument.Parse(content);
	}

	private static List<LatitudeLongitudeAltDateTimeModel> ParseKml(XContainer kml)
	{
		var coordinates = new List<LatitudeLongitudeAltDateTimeModel>();
		DateTime timeUtc = DateTime.UtcNow;
		
		foreach (var placemark in kml.Descendants("{http://www.opengis.net/kml/2.2}Placemark"))
		{
			var timeUtcElement = placemark.Descendants("{http://www.opengis.net/kml/2.2}Data")
				.FirstOrDefault(e => e.Attribute("name")?.Value == "Time UTC");
			if (timeUtcElement != null)
			{
				var timeUtcString = timeUtcElement.Element("{http://www.opengis.net/kml/2.2}value")?.Value;
				Console.WriteLine(timeUtcString);
				timeUtc = DateTime.ParseExact(timeUtcString, "M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture);
			}
			
			// now get the coordinates
			
			var lineString = placemark.Element("{http://www.opengis.net/kml/2.2}LineString");
			var coordinatesElement = lineString?.Element("{http://www.opengis.net/kml/2.2}coordinates");
			
			if ( coordinatesElement == null ) continue;
			
			var coordinatesString = coordinatesElement.Value;
			var coordinatesArray = coordinatesString.Split(' ');
			// ReSharper disable once LoopCanBeConvertedToQuery
			foreach (var coordinate in coordinatesArray)
			{
				var coordinateArray = coordinate.Split(',');
				coordinates.Add(new LatitudeLongitudeAltDateTimeModel
				{
					Longitude = coordinateArray[0],
					Latitude = coordinateArray[1],
					Altitude = coordinateArray[2],
					DateTime = timeUtc
				});
			}
		}
		return coordinates;
	}
}
