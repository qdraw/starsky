using System;
using System.Globalization;
using System.Runtime.Serialization;
using System.Text;
using starsky.Helpers;
using starsky.Interfaces;
using starsky.Models;
using XmpCore;

namespace starsky.Services
{
    
    public partial class ReadMeta : IReadMeta
    {
        private readonly AppSettings _appSettings;

        public ReadMeta(AppSettings appSettings = null)
        {
            _appSettings = appSettings;
        }
        
        public FileIndexItem XmpGetSidecarFile(FileIndexItem databaseItem, string singleFilePath)
        {
            if(_appSettings == null) throw new InvalidDataContractException("AppSettings in XmpSelectSidecarFile is null");
            
            // Read content from sidecar xmp file
            if (Files.IsXmpSidecarRequired(singleFilePath))
            {
                // Parse an xmp file for this location
                var xmpFilePath = Files.GetXmpSidecarFileWhenRequired(
                    singleFilePath,
                    _appSettings.ExifToolXmpPrefix);
                if (Files.IsFolderOrFile(xmpFilePath) == FolderOrFileModel.FolderOrFileTypeList.File)
                {
                    // Read the text-content of the xmp file.
                    var xmp = new PlainTextFileHelper().ReadFile(xmpFilePath);
                    // Get the data from the xmp
                    databaseItem = GetDataFromString(xmp,databaseItem);
                }
            }
            return databaseItem;
        }
        
        public FileIndexItem GetDataFromString(string xmpDataAsString, FileIndexItem databaseItem = null)
        {
            // Does not require appsettings
            
            if(databaseItem == null) databaseItem = new FileIndexItem();
            // ContentNameSpace is for example : Namespace=http://...
            databaseItem = GetDataContentNameSpaceTypes(xmpDataAsString, databaseItem);
            // NullNameSpace is for example : string.Empty
            databaseItem = GetDataNullNameSpaceTypes(xmpDataAsString, databaseItem);
            return databaseItem;
        }

        /// <summary>
        /// Get the value for items with the name without namespace
        /// </summary>
        /// <param name="property">IXmpPropertyInfo read from string</param>
        /// <param name="xmpName">xmpName, for example dc:subject[1]</param>
        /// <returns>value or null</returns>
        private string GetNullNameSpace(IXmpPropertyInfo property, string xmpName)
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
        private string GetContentNameSpace(IXmpPropertyInfo property, string xmpName)
        {
            if (property.Path == xmpName && !string.IsNullOrEmpty(property.Value) 
                                         && !string.IsNullOrEmpty(property.Namespace) )
                // the difference it this ^^!^^
            {
                return property.Value;
            }
            return null;
        }

        private double GpsPreParseAndConvertDegreeAngleToDouble(string gpsLatOrLong)
        {
            // get ref North, South, East West
            string refGps = gpsLatOrLong.Substring(gpsLatOrLong.Length-1, 1);
            return ConvertDegreeMinutesToDouble(gpsLatOrLong, refGps);
        }
                
        private FileIndexItem GetDataNullNameSpaceTypes(string xmpDataAsString, FileIndexItem item)
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

//                Console.WriteLine($"Path={property.Path} Namespace={property.Namespace} Value={property.Value}");

            }

            return item;
        }

        private FileIndexItem GetDataContentNameSpaceTypes(string xmpDataAsString, FileIndexItem item)
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
                
                //   Path=photomechanic:ColorClass Namespace=http://ns.camerabits.com/photomechanic/1.0/ Value=1
                var colorClass = GetContentNameSpace(property, "photomechanic:ColorClass");
                if (colorClass != null)
                {
                    item.SetColorClass(colorClass);
                }
                
                // Path=tiff:Orientation Namespace=http://ns.adobe.com/tiff/1.0/ Value=6
                var rotation = GetContentNameSpace(property, "tiff:Orientation");
                if (rotation != null)
                {
                    item.SetAbsoluteOrientation(rotation);
                }

            }
            return item;
        }
        
    }
}