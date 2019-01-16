﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;
using starsky.Helpers;
using starsky.Models;
using starskycore.Helpers;
using starskycore.Models;

namespace starsky.core.Services
{
    public partial class ReadMeta
    {
	    private const string GpxXmlNameSpaceName = "http://www.topografix.com/GPX/1/1"; 
	    
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
        
        /// <summary>
        /// Read full gpx file, or return after trackpoint
        /// </summary>
        /// <param name="fullFilePath"></param>
        /// <param name="geoList"></param>
        /// <param name="returnAfter">default complete file, but can be used to read only the first point</param>
        /// <returns></returns>
        public List<GeoListItem> ReadGpxFile(string fullFilePath, List<GeoListItem> geoList = null, int returnAfter = int.MaxValue)
        {
            if (geoList == null) geoList = new List<GeoListItem>();

            
            XmlDocument gpxDoc = new XmlDocument();
            
            // Some files are having problems with gpxDoc.Load()
            var fileString = new PlainTextFileHelper().ReadFile(fullFilePath);
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