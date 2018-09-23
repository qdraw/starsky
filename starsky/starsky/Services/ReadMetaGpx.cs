using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;
using starsky.Helpers;
using starsky.Models;

namespace starsky.Services
{
    public partial class ReadMeta
    {
        public FileIndexItem ReadGpxFromFileReturnAfterFirstField(string fullFilePath)
        {
            if (Files.IsFolderOrFile(fullFilePath) != FolderOrFileModel.FolderOrFileTypeList.File) 
                return new FileIndexItem();

            try
            {
                var readGpxFile = ReadGpxFile(fullFilePath, null, 1);
                return new FileIndexItem
                {
                    Title = readGpxFile.FirstOrDefault().Title,
                    DateTime = readGpxFile.FirstOrDefault().DateTime,
                    Latitude = readGpxFile.FirstOrDefault().Latitude,
                    Longitude = readGpxFile.FirstOrDefault().Longitude,
                    LocationAltitude = readGpxFile.FirstOrDefault().Altitude,
                    ColorClass = FileIndexItem.Color.None
                };
            }
            catch (XmlException e)
            {
                Console.WriteLine(e);
                return new FileIndexItem
                {
                    Tags = "SystemXmlXmlException",
                    ColorClass = FileIndexItem.Color.None
                };
            }
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
        
//        /// <summary>
//        /// Direct api, please use with exeption handeling
//        /// </summary>
//        /// <param name="fullFilePath"></param>
//        /// <returns></returns>
//        private FileIndexItem ReadGpxFileReturnAfterFirstFieldDirect(string fullFilePath)
//        {
//            
//            XmlDocument gpxDoc = new XmlDocument();
//            gpxDoc.Load(fullFilePath);
//            
//            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(gpxDoc.NameTable);
//            namespaceManager.AddNamespace("x", "http://www.topografix.com/GPX/1/1");
//            
//            XmlNodeList nodeList = gpxDoc.SelectNodes("//x:trkpt", namespaceManager);
//
//            foreach (XmlElement node in nodeList)
//            {
//                var longitudeString = node.GetAttribute("lon");
//                var latitudeString = node.GetAttribute("lat");
//
//                var longitude = double.Parse(longitudeString, 
//                    NumberStyles.Currency, CultureInfo.InvariantCulture);
//                var latitude = double.Parse(latitudeString, 
//                    NumberStyles.Currency, CultureInfo.InvariantCulture);
//
//                foreach (XmlElement childNode in node.ChildNodes)
//                {
//                    // childNode.Name == "ele" > elevation
//                    if (childNode.Name != "time")
//                    {
//                        var elevationString = childNode.InnerText;
//                        var locationAlitude = int.Parse(elevationString);
//                    }
//                        
//                    if (childNode.Name != "time") continue;
//                    var datetimeString = childNode.InnerText;
//                    
//                    DateTime.TryParse(datetimeString, out var dateTime);
//                    dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
//
//                    return new FileIndexItem
//                    {
//                        Title = GetTrkName(gpxDoc, namespaceManager),
//                        DateTime = dateTime,
//                        Latitude = latitude,
//                        Longitude = longitude,
//                        Tags = string.Empty,
//                        ColorClass = FileIndexItem.Color.None
//                    };
//                }
//            }
//            return new FileIndexItem();
//        }


        /// <summary>
        /// Read full gpx file
        /// </summary>
        /// <param name="fullFilePath"></param>
        /// <param name="geoList">
        /// </param>
        /// <returns></returns>
        public List<GeoListItem> ReadGpxFile(string fullFilePath, List<GeoListItem> geoList = null, int returnAfter = int.MaxValue)
        {
            if (geoList == null) geoList = new List<GeoListItem>();

            XmlDocument gpxDoc = new XmlDocument();
            gpxDoc.Load(fullFilePath);
            
            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(gpxDoc.NameTable);
            namespaceManager.AddNamespace("x", "http://www.topografix.com/GPX/1/1");
            
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