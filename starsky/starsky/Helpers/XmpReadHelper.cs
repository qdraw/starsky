using System;
using System.Globalization;
using System.Text;
using starsky.Models;
using starsky.Services;
using XmpCore;

namespace starsky.Helpers
{
    public static class XmpReadHelper
    {
        public static FileIndexItem GetDataFromString(string xmpDataAsString)
        {
            var item = new FileIndexItem();
            // ContentNameSpace is for example : Namespace=http://...
            item = GetDataContentNameSpaceTypes(xmpDataAsString, item);
            // NullNameSpace is for example : string.Empty
            item = GetDataNullNameSpaceTypes(xmpDataAsString, item);
            return item;
        }

        /// <summary>
        /// Get the value for items with the name without namespace
        /// </summary>
        /// <param name="property">IXmpPropertyInfo read from string</param>
        /// <param name="xmpName">xmpName, for example dc:subject[1]</param>
        /// <returns>value or null</returns>
        private static string GetNullNameSpace(IXmpPropertyInfo property, string xmpName)
        {
            if (property.Path == xmpName && !string.IsNullOrEmpty(property.Value) 
                                                     && string.IsNullOrEmpty(property.Namespace) )
            {
               return property.Value;
            }
            return null;
        }

        /// <summary>
        /// Get the value for items with the name with a namespace
        /// </summary>
        /// <param name="property">IXmpPropertyInfo read from string</param>
        /// <param name="xmpName">xmpName, for example dc:subject[1]</param>
        /// <returns>value or null</returns>
        private static string GetContentNameSpace(IXmpPropertyInfo property, string xmpName)
        {
            if (property.Path == xmpName && !string.IsNullOrEmpty(property.Value) 
                                         && !string.IsNullOrEmpty(property.Namespace) )
                // the difference it this ^^!^^
            {
                return property.Value;
            }
            return null;
        }

        private static double GpsPreParseAndConvertDegreeAngleToDouble(string gpsLatOrLong)
        {
            // get ref North, South, East West
            string refGps = gpsLatOrLong.Substring(gpsLatOrLong.Length-1, 1);
            var point = gpsLatOrLong.Replace(",", " ").Replace(".", " ");
            return ExifRead.ConvertDegreeMinutesSecondsToDouble(point, refGps);
        }
                
        private static FileIndexItem GetDataNullNameSpaceTypes(string xmpDataAsString, FileIndexItem item)
        {
            var xmp = XmpMetaFactory.ParseFromString(xmpDataAsString);
            foreach (var property in xmp.Properties)
            {
                
                //   Path=dc:description Namespace=http://purl.org/dc/elements/1.1/ Value=
                //   Path=dc:description[1] Namespace= Value=caption
                //   Path=dc:description[1]/xml:lang Namespace=http...
                var description = GetNullNameSpace(property, "dc:description[1]");
                if (description != null) item.Description = description;
                
                // Path=dc:subject Namespace=http://purl.org/dc/elements/1.1/ Value=
                // Path=dc:subject[1] Namespace= Value=keyword
                var tags = GetNullNameSpace(property, "dc:subject[1]");
                if (tags != null) item.Tags = tags;
                
                // Path=dc:subject[2] Namespace= Value=keyword2
                if ( !string.IsNullOrEmpty(property.Path) && 
                     property.Path.Contains("dc:subject[") && 
                     property.Path != "dc:subject[1]" && 
                     !string.IsNullOrEmpty(property.Value) && 
                     string.IsNullOrEmpty(property.Namespace) )
                {
                    StringBuilder tagsStringBuilder = new StringBuilder();
                    tagsStringBuilder.Append(item.Tags);
                    tagsStringBuilder.Append(", ");
                    tagsStringBuilder.Append(property.Value);
                    item.Tags = tagsStringBuilder.ToString();
                }
                
                // Path=dc:title Namespace=http://purl.org/dc/elements/1.1/ Value=
                // Path=dc:title[1] Namespace= Value=The object name
                //    Path=dc:title[1]/xml:lang Namespace=http://www.w3...
                var title = GetNullNameSpace(property, "dc:title[1]");
                if (title != null) item.Title = title;

                
                
                
                Console.WriteLine($"Path={property.Path} Namespace={property.Namespace} Value={property.Value}");

            }

            return item;
        }

        private static FileIndexItem GetDataContentNameSpaceTypes(string xmpDataAsString, FileIndexItem item)
        {
            var xmp = XmpMetaFactory.ParseFromString(xmpDataAsString);
            foreach (var property in xmp.Properties)
            {

                // Path=exif:GPSLatitude Namespace=http://ns.adobe.com/exif/1.0/ Value=52,20.708N
                var gpsLatitude = GetContentNameSpace(property, "exif:GPSLatitude");
                if (gpsLatitude != null)
                {
                    item.Latitude = GpsPreParseAndConvertDegreeAngleToDouble(gpsLatitude);
                }

                // Path=exif:GPSLongitude Namespace=http://ns.adobe.com/exif/1.0/ Value=5,55.840E
                var gpsLongitude = GetContentNameSpace(property, "exif:GPSLongitude");
                if (gpsLongitude != null)
                {
                    item.Longitude = GpsPreParseAndConvertDegreeAngleToDouble(gpsLongitude);
                }

                // Path=exif:DateTimeOriginal Namespace=http://ns.adobe.com/exif/1.0/ Value=2018-07-18T19:44:27
                var dateTimeOriginal = GetContentNameSpace(property, "exif:DateTimeOriginal");
                if (dateTimeOriginal != null)
                {
                    DateTime.TryParseExact(property.Value,
                        "yyyy-MM-dd\\THH:mm:ss",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out var dateTime);
                    item.DateTime = dateTime;
                }
            }
            return item;
        }
        
    }
}