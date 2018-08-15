using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using MetadataExtractor;
using starsky.Interfaces;
using starsky.Models;

namespace starsky.Services
{
    // Reading Exif using MetadataExtractor
    public partial class ReadMeta // IReadMeta
    {
        
        public FileIndexItem ReadExifFromFile(string fileFullPath, FileIndexItem existingFileIndexItem = null) // use null to create an object
        {
            List<MetadataExtractor.Directory> allExifItems;
            
            try
            {
                allExifItems = ImageMetadataReader.ReadMetadata(fileFullPath).ToList();
                DisplayAllExif(allExifItems);
            }
            catch (ImageProcessingException)
            {
                var item = new FileIndexItem {Tags = "ImageProcessingException".ToLower()};
                return item;
            }

            return ParseExifDirectory(allExifItems, existingFileIndexItem);
        }

        private FileIndexItem ParseExifDirectory(List<MetadataExtractor.Directory> allExifItems, FileIndexItem item)
        {
            // Used to overwrite feature
            if (item == null)
            {
                item = new FileIndexItem();
            }
            
            // Set the default value
            item.SetColorClass();

            item.Latitude = GetGeoLocationLatitude(allExifItems);
            item.Longitude = GetGeoLocationLongitude(allExifItems);
            

            foreach (var exifItem in allExifItems)
            {
                //  exifItem.Tags
                var tags = GetExifKeywords(exifItem);
                if(tags != null) // null = is not the right tag or emthy tag
                {
                    item.Tags = tags;
                }
                // Colour Class => ratings
                var colorClassString = _getColorClassString(exifItem);
                if(colorClassString != null) // null = is not the right tag or emthy tag
                {
                    item.SetColorClass(colorClassString);
                }
                
                // [IPTC] Caption/Abstract
                var caption = GetCaptionAbstract(exifItem);
                if(caption != null) // null = is not the right tag or emthy tag
                {
                    item.Description = caption;
                }    
                
                // [IPTC] Object Name = Title
                var title = GetObjectName(exifItem);
                if(title != null) // null = is not the right tag or emthy tag
                {
                     item.Title = title;
                }
                
                // DateTime of image
                var dateTime = GetExifDateTime(exifItem);
                if(dateTime.Year > 2) // 0 = is not the right tag or emthy tag
                {
                    item.DateTime = dateTime;
                }
                
                // DateTime of image
                var orientation = GetOrientation(exifItem);
                if (orientation != FileIndexItem.Rotation.DoNotChange)
                {
                    item.Orientation = orientation;
                }

            }
            
            return item;
        }

        private static FileIndexItem.Rotation GetOrientation(MetadataExtractor.Directory exifItem)
        {
            var tCounts = exifItem.Tags.Count(p => p.DirectoryName == "Exif IFD0" && p.Name == "Orientation");
            if (tCounts < 1) return FileIndexItem.Rotation.DoNotChange;
            
            var caption = exifItem.Tags.FirstOrDefault(
                p => p.DirectoryName == "Exif IFD0" 
                     && p.Name == "Orientation")?.Description;

            switch (caption)
            {
                case "Top, left side (Horizontal / normal)":
                    return FileIndexItem.Rotation.Horizontal;
                case "Right side, top (Rotate 90 CW)":
                    return FileIndexItem.Rotation.Rotate90Cw;
                case "Bottom, right side (Rotate 180)":
                    return FileIndexItem.Rotation.Rotate180;
                case "Left side, bottom (Rotate 270 CW)":
                    return FileIndexItem.Rotation.Rotate270Cw;
                default:
                    return FileIndexItem.Rotation.Horizontal;
            }
        }

        private static void DisplayAllExif(IEnumerable<MetadataExtractor.Directory> allExifItems)
        {
            foreach (var exifItem in allExifItems) {
                foreach (var tag in exifItem.Tags) Console.WriteLine($"[{exifItem.Name}] {tag.Name} = {tag.Description}");
            }
        }

        public string GetObjectName (MetadataExtractor.Directory exifItem)
        {
            var tCounts = exifItem.Tags.Count(p => p.DirectoryName == "IPTC" && p.Name == "Object Name");
            if (tCounts < 1) return null;
            
            var caption = exifItem.Tags.FirstOrDefault(
                p => p.DirectoryName == "IPTC" 
                     && p.Name == "Object Name")?.Description;
            return caption;
        }

        
        public string GetCaptionAbstract(MetadataExtractor.Directory exifItem)
        {
            var tCounts = exifItem.Tags.Count(p => p.DirectoryName == "IPTC" && p.Name == "Caption/Abstract");
            if (tCounts < 1) return null;
            
            var caption = exifItem.Tags.FirstOrDefault(
                p => p.DirectoryName == "IPTC" 
                     && p.Name == "Caption/Abstract")?.Description;
            return caption;
        }
        
        public string GetExifKeywords(MetadataExtractor.Directory exifItem)
        {
            var tCounts = exifItem.Tags.Count(p => p.DirectoryName == "IPTC" && p.Name == "Keywords");
            if (tCounts >= 1)
            {
                var tags = exifItem.Tags.FirstOrDefault(
                    p => p.DirectoryName == "IPTC" 
                         && p.Name == "Keywords")?.Description.ToLower();
                if (!string.IsNullOrWhiteSpace(tags))
                {
                    tags = tags.Replace(";", ", ");
                }

                return tags;
            }
            return null;
        }

        private static string _getColorClassString(MetadataExtractor.Directory exifItem)
        {
            var ratingCounts = exifItem.Tags.Count(p => p.DirectoryName == "IPTC" && p.Name.Contains("0x02dd"));
            if (ratingCounts >= 1)
            {
                var colorClassSting = string.Empty;
                var prefsTag = exifItem.Tags.FirstOrDefault(p => p.DirectoryName == "IPTC" && p.Name.Contains("0x02dd"))?.Description;
    
                // Results for example
                //     0:1:0:-00001
                //     ~~~~~~
                //     0:8:0:-00001
                        
                if (!string.IsNullOrWhiteSpace(prefsTag))
                {
                    var prefsTagSplit = prefsTag.Split(":");
                    colorClassSting = prefsTagSplit[1];     
                }
                return colorClassSting;
            }

            return null;
        }

        public DateTime GetExifDateTime(MetadataExtractor.Directory exifItem)
        {
            var itemDateTime = new DateTime();
            
            string pattern = "yyyy:MM:dd HH:mm:ss";
            CultureInfo provider = CultureInfo.InvariantCulture;
            
            var dtCounts = exifItem.Tags.Count(p => p.DirectoryName == "Exif SubIFD" && p.Name == "Date/Time Digitized");
            if (dtCounts >= 1)
            {
                var dateString = exifItem.Tags.FirstOrDefault(p => p.DirectoryName == "Exif SubIFD" && p.Name == "Date/Time Digitized")?.Description;
    
                // https://odedcoster.com/blog/2011/12/13/date-and-time-format-strings-in-net-understanding-format-strings/
                //2018:01:01 11:29:36
                DateTime.TryParseExact(dateString, pattern, provider, DateTimeStyles.AdjustToUniversal, out itemDateTime);
            }

            if (itemDateTime.Year != 1 || itemDateTime.Month != 1) return itemDateTime;

            var dateStringOriginal = exifItem.Tags.FirstOrDefault(p => p.DirectoryName == "Exif SubIFD" && p.Name == "Date/Time Original")?.Description;
            DateTime.TryParseExact(dateStringOriginal, pattern, provider, DateTimeStyles.AdjustToUniversal, out itemDateTime);

            return itemDateTime;
        }
        
        private  double GetGeoLocationLatitude(List<MetadataExtractor.Directory> allExifItems)
        {
            var latitudeString = string.Empty;
            var latitudeRef = string.Empty;
            
            foreach (var exifItem in allExifItems)
            {
                var latitudeRefLocal = exifItem.Tags.FirstOrDefault(
                    p => p.DirectoryName == "GPS" 
                    && p.Name == "GPS Latitude Ref")?.Description;
                
                if (latitudeRefLocal != null)
                {
                    latitudeRef = latitudeRefLocal;
                }
                
                var latitudeLocal = exifItem.Tags.FirstOrDefault(
                    p => p.DirectoryName == "GPS" 
                         && p.Name == "GPS Latitude")?.Description;

                if (latitudeLocal != null)
                {
                    latitudeString = latitudeLocal;
                }
            }

            if (!string.IsNullOrWhiteSpace(latitudeString))
            {
                var latitude = ConvertDegreeMinutesSecondsToDouble(latitudeString, latitudeRef);
                latitude = Math.Floor(latitude * 10000000000) / 10000000000; 
                return latitude;
            }
            return 0;
        }
        
         private double GetGeoLocationLongitude(List<MetadataExtractor.Directory> allExifItems)
        {
            var longitudeString = string.Empty;
            var longitudeRef = string.Empty;
            
            foreach (var exifItem in allExifItems)
            {
                var longitudeRefLocal = exifItem.Tags.FirstOrDefault(
                    p => p.DirectoryName == "GPS" 
                         && p.Name == "GPS Longitude Ref")?.Description;
                
                if (longitudeRefLocal != null)
                {
                    longitudeRef = longitudeRefLocal;
                }
                
                var longitudeLocal = exifItem.Tags.FirstOrDefault(
                    p => p.DirectoryName == "GPS" 
                         && p.Name == "GPS Longitude")?.Description;

                if (longitudeLocal != null)
                {
                    longitudeString = longitudeLocal;
                }
            }

            if (!string.IsNullOrWhiteSpace(longitudeString))
            {
                var longitude = ConvertDegreeMinutesSecondsToDouble(longitudeString, longitudeRef);
                longitude = Math.Floor(longitude * 10000000000) / 10000000000; 
                return longitude;
            }
            return 0;
        }

        public double ConvertDegreeMinutesSecondsToDouble(string point, string refGps)
        {
            //Example: 17.21.18S
            // DD°MM’SS.s” usage
            
            var multiplier = (refGps.Contains("S") || refGps.Contains("W")) ? -1 : 1; //handle south and west

            point = Regex.Replace(point, "[^0-9\\., ]", "", RegexOptions.CultureInvariant); //remove the characters

            // When you use an localisation where commas are used instead of a dot
            point = point.Replace(",", ".");

            var pointArray = point.Split(' '); //split the string.

            //Decimal degrees = 
            //   whole number of degrees, 
            //   plus minutes divided by 60, 
            //   plus seconds divided by 3600

            var degrees = double.Parse(pointArray[0], CultureInfo.InvariantCulture);
            var minutes = double.Parse(pointArray[1], CultureInfo.InvariantCulture) / 60;
            var seconds = double.Parse(pointArray[2],CultureInfo.InvariantCulture) / 3600;

            return (degrees + minutes + seconds) * multiplier;
        }

        public double ConvertDegreeMinutesToDouble(string point, string refGps)
        {
            // "5,55.840E"
            var multiplier = (refGps.Contains("S") || refGps.Contains("W")) ? -1 : 1; //handle south and west

            point = point.Replace(",", " ");
            point = Regex.Replace(point, "[^0-9\\., ]", "", RegexOptions.CultureInvariant); //remove the characters

            var pointArray = point.Split(' '); //split the string.
            var degrees = double.Parse(pointArray[0], CultureInfo.InvariantCulture);
            var minutes = double.Parse(pointArray[1], CultureInfo.InvariantCulture) / 60;
            
            return (degrees + minutes) * multiplier;
        }


    }
}
