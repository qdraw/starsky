#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;
using starsky.foundation.readmeta.Models;
using starsky.foundation.storage.Helpers;

namespace starsky.foundation.readmeta.ReadMetaHelpers
{
    public static class ReadMetaGpx
    {
	    private const string GpxXmlNameSpaceName = "http://www.topografix.com/GPX/1/1"; 
	    
        public static FileIndexItem ReadGpxFromFileReturnAfterFirstField(Stream? stream, string subPath)
        {
	        if ( stream == null )
	        {
		        var returnItem = new FileIndexItem(subPath){Status = FileIndexItem.ExifStatus.OperationNotSupported};
		        return returnItem;
	        }

	        var readGpxFile = ReadGpxFile(stream, null, 1);

	        if ( !readGpxFile.Any() )
	        {
		        return new FileIndexItem(subPath)
		        {
			        Tags = "SystemXmlXmlException",
			        ColorClass = ColorClassParser.Color.None
		        };
	        }

	        var title = readGpxFile.FirstOrDefault()?.Title ?? string.Empty;
	        var dateTime = readGpxFile.FirstOrDefault()?.DateTime ?? new DateTime();
	        var latitude = readGpxFile.FirstOrDefault()?.Latitude ?? 0d;
	        var longitude = readGpxFile.FirstOrDefault()?.Longitude ?? 0d;
	        var altitude = readGpxFile.FirstOrDefault()?.Altitude ?? 0d;

	        return new FileIndexItem(subPath)
	        {
		        Title = title,
		        DateTime = dateTime,
		        Latitude = latitude,
		        Longitude = longitude,
		        LocationAltitude = altitude,
		        ColorClass = ColorClassParser.Color.None,
		        ImageFormat = ExtensionRolesHelper.ImageFormat.gpx
	        };
        }

        private static string GetTrkName(XmlNode? gpxDoc, XmlNamespaceManager namespaceManager)
        {
	        var trkNodeList = gpxDoc?.SelectNodes("//x:trk",  namespaceManager);
            if ( trkNodeList == null ) return string.Empty;
            var trkName = new StringBuilder();
            foreach (XmlElement node in trkNodeList)
            {
                foreach (XmlElement childNode in node.ChildNodes)
                {
	                if ( childNode.Name != "name" ) continue;
	                trkName.Append(childNode.InnerText);
	                return trkName.ToString();
                }
            }
            return string.Empty;
        }

	    /// <summary>
	    /// Read full gpx file, or return after trackPoint
	    /// </summary>
	    /// <param name="stream"></param>
	    /// <param name="geoList"></param>
	    /// <param name="returnAfter">default complete file, but can be used to read only the first point</param>
	    /// <returns></returns>
	    public static List<GeoListItem> ReadGpxFile(Stream stream, List<GeoListItem>? geoList = null, int returnAfter = int.MaxValue)
        {
            if (geoList == null) geoList = new List<GeoListItem>();

	        // Some files are having problems with gpxDoc.Load()
	        var fileString = new PlainTextFileHelper().StreamToString(stream);
	        
	        try
	        {
		        return ParseGpxString(fileString, geoList, returnAfter);
	        }
	        catch ( XmlException e )
	        {
		        Console.WriteLine($"XmlException>>\n{e}\n <<XmlException");
		        return geoList;
	        }

        }

	    /// <summary>
	    /// Parse XML as XmlDocument
	    /// </summary>
	    /// <param name="fileString">input as string</param>
	    /// <returns>parsed xml document</returns>
	    internal static XmlDocument ParseXml(string fileString)
	    {
		    XmlDocument gpxDoc = new XmlDocument();
		    gpxDoc.LoadXml(fileString);
		    return gpxDoc;
	    }

	    /// <summary>
	    /// Parse the gpx string
	    /// </summary>
	    /// <param name="fileString">string with xml</param>
	    /// <param name="geoList">object to add</param>
	    /// <param name="returnAfter">return after number of values; default return all</param>
	    /// <returns></returns>
	    private static List<GeoListItem> ParseGpxString(string fileString, List<GeoListItem>? geoList = null, 
		    int returnAfter = int.MaxValue)
	    {
		    var gpxDoc = ParseXml(fileString);
            
            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(gpxDoc.NameTable);
			namespaceManager.AddNamespace("x", GpxXmlNameSpaceName);
            
            XmlNodeList? nodeList = gpxDoc.SelectNodes("//x:trkpt", namespaceManager);
            if ( nodeList == null ) return new List<GeoListItem>();
            geoList ??= new List<GeoListItem>();

            var title = GetTrkName(gpxDoc, namespaceManager);

            var count = 0;
            foreach (XmlElement node in nodeList)
            {
                var longitudeString = node.GetAttribute("lon");
                var latitudeString = node.GetAttribute("lat");

                var longitude = double.Parse(longitudeString, 
                    NumberStyles.Currency, CultureInfo.InvariantCulture);
                var latitude = double.Parse(latitudeString, 
                    NumberStyles.Currency, CultureInfo.InvariantCulture);

                DateTime dateTime = DateTime.MinValue;

                var elevation = 0d;

                foreach (XmlElement childNode in node.ChildNodes)
                {
                    if (childNode.Name == "ele")
                    {
                        elevation = double.Parse(childNode.InnerText, CultureInfo.InvariantCulture);
                    }
                    
                    if (childNode.Name != "time") continue;
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
                
                if(returnAfter == count) return geoList;
                count++;
                
            }
            return geoList;
	    }
    }
}
