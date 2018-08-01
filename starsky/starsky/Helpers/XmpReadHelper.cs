using System;
using System.Globalization;
using starsky.Models;
using starsky.Services;
using XmpCore;

namespace starsky.Helpers
{
    public static class XmpReadHelper
    {
        public static FileIndexItem GetData(string xmpDataAsString)
        {
            var item = new FileIndexItem();
            var xmp = XmpMetaFactory.ParseFromString(xmpDataAsString);
            foreach (var property in xmp.Properties)
            {
                
                //   Path=dc:description Namespace=http://purl.org/dc/elements/1.1/ Value=
                //   Path=dc:description[1] Namespace= Value=caption
                //   Path=dc:description[1]/xml:lang Namespace=http...
                if (property.Path == "dc:description[1]" && !string.IsNullOrEmpty(property.Value) 
                                                         && string.IsNullOrEmpty(property.Namespace) )
                {
                    item.Description = property.Value;
                }
                
                // Path=dc:subject Namespace=http://purl.org/dc/elements/1.1/ Value=
                // Path=dc:subject[1] Namespace= Value=keyword
                if (property.Path == "dc:subject[1]" && !string.IsNullOrEmpty(property.Value) 
                                                     && string.IsNullOrEmpty(property.Namespace) )
                {
                    item.Tags = property.Value;
                }
                
                // Path=dc:subject[2] Namespace= Value=keyword2
                if ( !string.IsNullOrEmpty(property.Path) && 
                     property.Path.Contains("dc:subject[") && 
                     property.Path != "dc:subject[1]" && 
                     !string.IsNullOrEmpty(property.Value) && string.IsNullOrEmpty(property.Namespace) )
                {
                    item.Tags += ", " + property.Value;
                }
                
                // Path=dc:title Namespace=http://purl.org/dc/elements/1.1/ Value=
                // Path=dc:title[1] Namespace= Value=The object name
                //    Path=dc:title[1]/xml:lang Namespace=http://www.w3...
                if (property.Path == "dc:title[1]" && !string.IsNullOrEmpty(property.Value) 
                                                   && string.IsNullOrEmpty(property.Namespace) )
                {
                    item.Title = property.Value;
                }
                
                // Path=exif:GPSLatitude Namespace=http://ns.adobe.com/exif/1.0/ Value=52,20.708N
                if (property.Path == "exif:GPSLatitude" && !string.IsNullOrEmpty(property.Value) 
                                                        && !string.IsNullOrEmpty(property.Namespace) )
                {
                    // get ref;
                    string refGps = property.Value.Substring(property.Value.Length-1, 1);

                    var point = property.Value.Replace(",", " ").Replace(".", " ");
                    item.Latitude = ExifRead.ConvertDegreeAngleToDouble(point, refGps);
                }
                
                // Path=exif:GPSLongitude Namespace=http://ns.adobe.com/exif/1.0/ Value=5,55.840E
                if (property.Path == "exif:GPSLongitude" && !string.IsNullOrEmpty(property.Value) 
                                                        && !string.IsNullOrEmpty(property.Namespace) )
                {
                    // get ref;
                    string refGps = property.Value.Substring(property.Value.Length-1, 1);

                    var point = property.Value.Replace(",", " ").Replace(".", " ");
                    item.Longitude = ExifRead.ConvertDegreeAngleToDouble(point, refGps);
                }            
                
                // Path=exif:DateTimeOriginal Namespace=http://ns.adobe.com/exif/1.0/ Value=2018-07-18T19:44:27
                if (property.Path == "exif:DateTimeOriginal" && !string.IsNullOrEmpty(property.Value) 
                                                         && !string.IsNullOrEmpty(property.Namespace) )
                {
                    DateTime.TryParseExact(property.Value, 
                        "yyyy-MM-dd\\THH:mm:ss",
                        CultureInfo.InvariantCulture, 
                        DateTimeStyles.None, 
                        out var dateTime);
                    item.DateTime = dateTime;
                }       
                
                
                Console.WriteLine($"Path={property.Path} Namespace={property.Namespace} Value={property.Value}");

            }

            return item;
        }
    }
}