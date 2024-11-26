using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using GeoTimeZone;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.readmeta.Models;
using starsky.foundation.storage.Helpers;

namespace starsky.foundation.readmeta.ReadMetaHelpers;

public sealed class ReadMetaGpx
{
	private const string GpxXmlNameSpaceName = "http://www.topografix.com/GPX/1/1";
	private readonly IWebLogger _logger;

	public ReadMetaGpx(IWebLogger logger)
	{
		_logger = logger;
	}

	public async Task<FileIndexItem> ReadGpxFromFileReturnAfterFirstFieldAsync(Stream? stream,
		string subPath, bool useLocal = true)
	{
		if ( stream == null )
		{
			var returnItem = new FileIndexItem(subPath)
			{
				Status = FileIndexItem.ExifStatus.OperationNotSupported
			};
			return returnItem;
		}

		var readGpxFile = await ReadGpxFileAsync(stream, null, 1);

		if ( readGpxFile.Count == 0 )
		{
			_logger.LogInformation($"[ReadMetaGpx] SystemXmlXmlException for {subPath}");
			return new FileIndexItem(subPath)
			{
				Tags = "SystemXmlXmlException", ColorClass = ColorClassParser.Color.None
			};
		}

		var latitude = readGpxFile[0].Latitude;
		var longitude = readGpxFile[0].Longitude;
		var altitude = readGpxFile[0].Altitude;

		return new FileIndexItem(subPath)
		{
			Title = readGpxFile[0].Title,
			DateTime = ConvertDateTime(readGpxFile[0].DateTime, useLocal, latitude, longitude),
			Latitude = latitude,
			Longitude = longitude,
			LocationAltitude = altitude,
			ColorClass = ColorClassParser.Color.None,
			ImageFormat = ExtensionRolesHelper.ImageFormat.gpx
		};
	}

	internal static DateTime ConvertDateTime(DateTime dateTime, bool useLocal,
		double latitude, double longitude)
	{
		if ( !useLocal )
		{
			return dateTime;
		}

		var localTimeZoneNameResult = TimeZoneLookup
			.GetTimeZone(latitude, longitude).Result;
		var localTimeZone =
			TimeZoneInfo.FindSystemTimeZoneById(localTimeZoneNameResult);
		return TimeZoneInfo.ConvertTimeFromUtc(dateTime, localTimeZone);
	}

	private static string GetTrkName(XmlNode? gpxDoc, XmlNamespaceManager namespaceManager)
	{
		var trkNodeList = gpxDoc?.SelectNodes("//x:trk", namespaceManager);
		if ( trkNodeList == null )
		{
			return string.Empty;
		}

		var trkName = new StringBuilder();
		foreach ( XmlElement node in trkNodeList )
		{
			foreach ( XmlElement childNode in node.ChildNodes )
			{
				if ( childNode.Name != "name" )
				{
					continue;
				}

				trkName.Append(childNode.InnerText);
				return trkName.ToString();
			}
		}

		return string.Empty;
	}

	/// <summary>
	///     Read full gpx file, or return after trackPoint
	/// </summary>
	/// <param name="stream"></param>
	/// <param name="geoList"></param>
	/// <param name="returnAfter">default complete file, but can be used to read only the first point</param>
	/// <returns></returns>
	public async Task<List<GeoListItem>> ReadGpxFileAsync(Stream stream,
		List<GeoListItem>? geoList = null, int returnAfter = int.MaxValue)
	{
		geoList ??= new List<GeoListItem>();

		// Some files are having problems with gpxDoc.Load()
		// Stream is disposed in the StreamToStringHelper
		var fileString = await StreamToStringHelper.StreamToStringAsync(stream);

		try
		{
			return ParseGpxString(fileString, geoList, returnAfter);
		}
		catch ( XmlException e )
		{
			_logger.LogInformation($"XmlException for {e}");
			return geoList;
		}
	}

	/// <summary>
	///     Parse XML as XmlDocument
	/// </summary>
	/// <param name="fileString">input as string</param>
	/// <returns>parsed xml document</returns>
	internal static XmlDocument ParseXml(string fileString)
	{
		var gpxDoc = new XmlDocument();
		gpxDoc.LoadXml(fileString);
		return gpxDoc;
	}

	/// <summary>
	///     Parse the gpx string
	/// </summary>
	/// <param name="fileString">string with xml</param>
	/// <param name="geoList">object to add</param>
	/// <param name="returnAfter">return after number of values; default return all</param>
	/// <returns></returns>
	private static List<GeoListItem> ParseGpxString(string fileString,
		List<GeoListItem>? geoList = null,
		int returnAfter = int.MaxValue)
	{
		var gpxDoc = ParseXml(fileString);

		var namespaceManager = new XmlNamespaceManager(gpxDoc.NameTable);
		namespaceManager.AddNamespace("x", GpxXmlNameSpaceName);

		var nodeList = gpxDoc.SelectNodes("//x:trkpt", namespaceManager);
		if ( nodeList == null )
		{
			return new List<GeoListItem>();
		}

		geoList ??= new List<GeoListItem>();

		var title = GetTrkName(gpxDoc, namespaceManager);

		var count = 0;
		foreach ( XmlElement node in nodeList )
		{
			var longitudeString = node.GetAttribute("lon");
			var latitudeString = node.GetAttribute("lat");

			var longitude = double.Parse(longitudeString,
				NumberStyles.Currency, CultureInfo.InvariantCulture);
			var latitude = double.Parse(latitudeString,
				NumberStyles.Currency, CultureInfo.InvariantCulture);

			var dateTime = DateTime.MinValue;

			var elevation = 0d;

			foreach ( XmlElement childNode in node.ChildNodes )
			{
				if ( childNode.Name == "ele" )
				{
					elevation = double.Parse(childNode.InnerText, CultureInfo.InvariantCulture);
				}

				if ( childNode.Name != "time" )
				{
					continue;
				}

				var datetimeString = childNode.InnerText;

				// 2018-08-21T19:15:41Z
				DateTime.TryParseExact(datetimeString,
					"yyyy-MM-ddTHH:mm:ssZ",
					CultureInfo.InvariantCulture,
					DateTimeStyles.AdjustToUniversal,
					out dateTime);

				dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
			}


			geoList.Add(new GeoListItem
			{
				Title = title,
				DateTime = dateTime,
				Latitude = latitude,
				Longitude = longitude,
				Altitude = elevation
			});

			if ( returnAfter == count )
			{
				return geoList;
			}

			count++;
		}

		return geoList;
	}
}
