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

namespace starsky.foundation.readmeta.Services
{
    public class ReadMetaGpx
    {
	    private const string GpxXmlNameSpaceName = "http://www.topografix.com/GPX/1/1"; 
	    
        public FileIndexItem ReadGpxFromFileReturnAfterFirstField(Stream stream, string subPath)
        {
	        if ( stream == null )
	        {
		        var returnItem = new FileIndexItem(subPath){Status = FileIndexItem.ExifStatus.OperationNotSupported};
		        return returnItem;
	        }

	        var readGpxFile = ReadGpxFile(stream, null, 1);
	        
	        if ( readGpxFile.Any() )
	        {
		        return new FileIndexItem(subPath)
		        {
			        Title = readGpxFile.FirstOrDefault().Title,
			        DateTime = readGpxFile.FirstOrDefault().DateTime,
			        Latitude = readGpxFile.FirstOrDefault().Latitude,
			        Longitude = readGpxFile.FirstOrDefault().Longitude,
			        LocationAltitude = readGpxFile.FirstOrDefault().Altitude,
			        ColorClass = ColorClassParser.Color.None,
			        ImageFormat = ExtensionRolesHelper.ImageFormat.gpx
		        }; 
	        }
	        
	        return new FileIndexItem(subPath)
	        {
		        Tags = "SystemXmlXmlException",
		        ColorClass = ColorClassParser.Color.None
	        };
	        
        }

        private string GetTrkName(XmlDocument gpxDoc, XmlNamespaceManager namespaceManager)
        {
            
            XmlNodeList trkNodeList = gpxDoc.SelectNodes("//x:trk",  namespaceManager);
            var trkName = new StringBuilder();
            foreach (XmlElement node in trkNodeList)
            {
                foreach (XmlElement childNode in node.ChildNodes)
                {
                    if (childNode.Name == "name")
                    {
                        trkName.Append(childNode.InnerText);
                        return trkName.ToString();
                    }
                }
            }
            return string.Empty;
        }

	    /// <summary>
	    /// Read full gpx file, or return after trackpoint
	    /// </summary>
	    /// <param name="stream"></param>
	    /// <param name="geoList"></param>
	    /// <param name="returnAfter">default complete file, but can be used to read only the first point</param>
	    /// <returns></returns>
	    public List<GeoListItem> ReadGpxFile(Stream stream, List<GeoListItem> geoList = null, int returnAfter = int.MaxValue)
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
	    /// Parse the gpx string
	    /// </summary>
	    /// <param name="fileString">string with xml</param>
	    /// <param name="geoList">object to add</param>
	    /// <param name="returnAfter">return after number of values; default return all</param>
	    /// <returns></returns>
	    private List<GeoListItem> ParseGpxString(string fileString, List<GeoListItem> geoList = null, 
		    int returnAfter = int.MaxValue)
	    {
		    XmlDocument gpxDoc = new XmlDocument();
            
            gpxDoc.LoadXml(fileString);
            
            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(gpxDoc.NameTable);
			namespaceManager.AddNamespace("x", GpxXmlNameSpaceName);
            
            XmlNodeList nodeList = gpxDoc.SelectNodes("//x:trkpt", namespaceManager);

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
