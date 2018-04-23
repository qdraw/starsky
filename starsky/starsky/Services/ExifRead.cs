using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using MetadataExtractor;
using starsky.Models;

namespace starsky.Services
{
    // Reading Exif using MetadataExtractor
    public static class ExifRead
    {
        public static FileIndexItem ReadExifFromFile(string fileFullPath)
        {
            var item = new FileIndexItem();
            // Set the default value
            item.SetColorClass();

            List<Directory> allExifItems;
            try
            {
                allExifItems = ImageMetadataReader.ReadMetadata(fileFullPath).ToList();
                _displayAllExif(allExifItems);
            }
            catch (ImageProcessingException)
            {
                item.Tags = "ImageProcessingException".ToLower();
                return item;
            }

            item.Latitude = GetGeoLocationLatitude(allExifItems);
            item.Longitude = GetGeoLocationLongitude(allExifItems);
            

            foreach (var exifItem in allExifItems)
            {
                //  exifItem.Tags
                var tags = _getExifTags(exifItem);
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
                var caption = _getCaptionAbstract(exifItem);
                if(caption != null) // null = is not the right tag or emthy tag
                {
                    item.Description = caption;
                }    
                
                // [IPTC] Object Name = Title
                var title = _getObjectName(exifItem);
                if(title != null) // null = is not the right tag or emthy tag
                {
                     item.Title = title;
                }
                
               
                // DateTime of image
                var dateTime = _getDateTime(exifItem);
                if(dateTime.Year > 2) // 0 = is not the right tag or emthy tag
                {
                    item.DateTime = dateTime;
                }
            }
            
            return item;
        }

        private static void _displayAllExif(IEnumerable<Directory> allExifItems)
        {
            if (!AppSettingsProvider.Verbose) return;
            foreach (var exifItem in allExifItems) {
                foreach (var tag in exifItem.Tags) Console.WriteLine($"[{exifItem.Name}] {tag.Name} = {tag.Description}");
            }
        }

        // Update Database structure first
        private static string _getObjectName (Directory exifItem)
        {
            var tCounts = exifItem.Tags.Count(p => p.DirectoryName == "IPTC" && p.Name == "Object Name");
            if (tCounts < 1) return null;
            
            var caption = exifItem.Tags.FirstOrDefault(
                p => p.DirectoryName == "IPTC" 
                     && p.Name == "Object Name")?.Description;
            return caption;
        }

        
        private static string _getCaptionAbstract(Directory exifItem)
        {
            var tCounts = exifItem.Tags.Count(p => p.DirectoryName == "IPTC" && p.Name == "Caption/Abstract");
            if (tCounts < 1) return null;
            
            var caption = exifItem.Tags.FirstOrDefault(
                p => p.DirectoryName == "IPTC" 
                     && p.Name == "Caption/Abstract")?.Description;
            return caption;
            
        }
        
        private static string _getExifTags(Directory exifItem)
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

        private static string _getColorClassString(Directory exifItem)
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

        private static DateTime _getDateTime(Directory exifItem)
        {
            var itemDateTime = new DateTime();
            var dtCounts = exifItem.Tags.Count(p => p.DirectoryName == "Exif SubIFD" && p.Name == "Date/Time Digitized");
            if (dtCounts >= 1)
            {
    
                var dateString = exifItem.Tags.FirstOrDefault(p => p.DirectoryName == "Exif SubIFD" && p.Name == "Date/Time Digitized")?.Description;
    
                // https://odedcoster.com/blog/2011/12/13/date-and-time-format-strings-in-net-understanding-format-strings/
                //2018:01:01 11:29:36
                string pattern = "yyyy:MM:dd HH:mm:ss";
                CultureInfo provider = CultureInfo.InvariantCulture;
                DateTime.TryParseExact(dateString, pattern, provider, DateTimeStyles.AdjustToUniversal, out itemDateTime);
    
                if (itemDateTime.Year == 1 && itemDateTime.Month == 1)
                {
                    dateString = exifItem.Tags.FirstOrDefault(p => p.DirectoryName == "Exif SubIFD" && p.Name == "Date/Time Original")?.Description;
                    DateTime.TryParseExact(dateString, pattern, provider, DateTimeStyles.AdjustToUniversal, out itemDateTime);
                }
            }

            return itemDateTime;
        }
        
        private static double GetGeoLocationLatitude(List<Directory> allExifItems)
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
                var latitude = ConvertDegreeAngleToDouble(latitudeString, latitudeRef);
                latitude = Math.Floor(latitude * 10000000000) / 10000000000; 
                return latitude;
            }
            return 0;
        }
        
         private static double GetGeoLocationLongitude(List<Directory> allExifItems)
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
                var longitude = ConvertDegreeAngleToDouble(longitudeString, longitudeRef);
                longitude = Math.Floor(longitude * 10000000000) / 10000000000; 
                return longitude;
            }
            return 0;
        }
        
        public static double ConvertDegreeAngleToDouble(string point, string refGps)
        {
            //Example: 17.21.18S

            var multiplier = (refGps.Contains("S") || refGps.Contains("W")) ? -1 : 1; //handle south and west

            point = Regex.Replace(point, "[^0-9. ]", ""); //remove the characters

            var pointArray = point.Split(' '); //split the string.

            //Decimal degrees = 
            //   whole number of degrees, 
            //   plus minutes divided by 60, 
            //   plus seconds divided by 3600

            var degrees = Double.Parse(pointArray[0]);
            var minutes = Double.Parse(pointArray[1]) / 60;
            var seconds = Double.Parse(pointArray[2]) / 3600;

            return (degrees + minutes + seconds) * multiplier;
        }
        
        
    }
}
