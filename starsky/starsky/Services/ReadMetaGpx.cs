using System;
using System.Collections.Generic;
using System.Globalization;
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
                return ReadGpxFileReturnAfterFirstFieldDirect(fullFilePath);
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
        
        /// <summary>
        /// Direct api, please use with exeption handeling
        /// </summary>
        /// <param name="fullFilePath"></param>
        /// <returns></returns>
        private FileIndexItem ReadGpxFileReturnAfterFirstFieldDirect(string fullFilePath)
        {
            
            XmlDocument gpxDoc = new XmlDocument();
            gpxDoc.Load(fullFilePath);
            
            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(gpxDoc.NameTable);
            namespaceManager.AddNamespace("x", "http://www.topografix.com/GPX/1/1");
            
            XmlNodeList nodeList = gpxDoc.SelectNodes("//x:trkpt", namespaceManager);

            foreach (XmlElement node in nodeList)
            {
                var longitudeString = node.GetAttribute("lon");
                var latitudeString = node.GetAttribute("lat");

                var longitude = double.Parse(longitudeString, 
                    NumberStyles.Currency, CultureInfo.InvariantCulture);
                var latitude = double.Parse(latitudeString, 
                    NumberStyles.Currency, CultureInfo.InvariantCulture);

                foreach (XmlElement childNode in node.ChildNodes)
                {
                    // childNode.Name == "ele" > elevation
                    if (childNode.Name != "time") continue;
                    var datetimeString = childNode.InnerText;
                    
                    DateTime.TryParse(datetimeString, out var dateTime);
                    
                    return new FileIndexItem
                    {
                        Title = GetTrkName(gpxDoc, namespaceManager),
                        DateTime = dateTime, //.ToUniversalTime(),
                        Latitude = latitude,
                        Longitude = longitude,
                        Tags = string.Empty,
                        ColorClass = FileIndexItem.Color.None
                    };
                }
            }
            return new FileIndexItem();
        }


        /// <summary>
        /// Read full gpx file
        /// </summary>
        /// <param name="fullFilePath"></param>
        /// <param name="geoList">
        /// </param>
        /// <returns></returns>
        public List<GeoListItem> ReadGpxFile(string fullFilePath, List<GeoListItem> geoList = null)
        {
            if (geoList == null) geoList = new List<GeoListItem>();

            XmlDocument gpxDoc = new XmlDocument();
            gpxDoc.Load(fullFilePath);
            
            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(gpxDoc.NameTable);
            namespaceManager.AddNamespace("x", "http://www.topografix.com/GPX/1/1");
            
            XmlNodeList nodeList = gpxDoc.SelectNodes("//x:trkpt", namespaceManager);

            foreach (XmlElement node in nodeList)
            {
                var longitudeString = node.GetAttribute("lon");
                var latitudeString = node.GetAttribute("lat");

                var longitude = double.Parse(longitudeString, 
                    NumberStyles.Currency, CultureInfo.InvariantCulture);
                var latitude = double.Parse(latitudeString, 
                    NumberStyles.Currency, CultureInfo.InvariantCulture);

                DateTime dateTime = DateTime.MinValue;
                foreach (XmlElement childNode in node.ChildNodes)
                {
                         
                    if (childNode.Name == "ele")
                    {
                        var elevation = childNode.InnerText;
                    }
                    
                    if (childNode.Name != "time") continue;
                    var datetimeString = childNode.InnerText;
                    
                    // 2018-08-21T19:15:41Z
                    DateTime.TryParseExact(datetimeString, 
                        "yyyy-MM-ddTHH:mm:ssZ", 
                        CultureInfo.InvariantCulture, 
                        DateTimeStyles.None, 
                        out dateTime);

                    dateTime = TimeZoneInfo.ConvertTime(dateTime, TimeZoneInfo.Local);

                }
                
                geoList.Add(new GeoListItem
                {
                    DateTime = dateTime,
                    Latitude = latitude,
                    Longitude = longitude,
                });
                
                
            }
            return geoList;
        }
    }
}