﻿using System;
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
        public FileIndexItem ReadGpxFromFile(string fullFilePath)
        {
            
            if (Files.IsFolderOrFile(fullFilePath) != FolderOrFileModel.FolderOrFileTypeList.File) return new FileIndexItem();

            try
            {
                return ReadGpxFileReturnAfterFirstField(fullFilePath);
            }
            catch (XmlException e)
            {
                Console.WriteLine(e);
                return new FileIndexItem { ColorClass = FileIndexItem.Color.None};
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
                    };
                }
            }
            return string.Empty;
        }
        
        private FileIndexItem ReadGpxFileReturnAfterFirstField(string fullFilePath)
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
    }
}