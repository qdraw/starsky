using System;
using starsky.Models;
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
                if (property.Path == "dc:description[1]" && !string.IsNullOrEmpty(property.Value) && string.IsNullOrEmpty(property.Namespace) )
                {
                    item.Description = property.Value;
                }
                
                if (property.Path == "dc:subject[1]" && !string.IsNullOrEmpty(property.Value) && string.IsNullOrEmpty(property.Namespace) )
                {
                    item.Tags = property.Value;
                }
                
                if ( !string.IsNullOrEmpty(property.Path) && 
                     property.Path.Contains("dc:subject[") && 
                     property.Path != "dc:subject[1]" && 
                     !string.IsNullOrEmpty(property.Value) && string.IsNullOrEmpty(property.Namespace) )
                {
                    item.Tags += ", " + property.Value;
                }
                
                if (property.Path == "dc:title[1]" && !string.IsNullOrEmpty(property.Value) && string.IsNullOrEmpty(property.Namespace) )
                {
                    item.Title = property.Value;
                }
                
//                if (property.Path == "exif:GPSLatitude" && !string.IsNullOrEmpty(property.Value) && !string.IsNullOrEmpty(property.Namespace) )
//                {
//                    ConvertDegreeAngleToDouble(string point, string refGps)
//                        
//                    item.Title = property.Value;
//                }
                
                
                
                
                
                Console.WriteLine($"Path={property.Path} Namespace={property.Namespace} Value={property.Value}");

            }

            return item;
        }
    }
}