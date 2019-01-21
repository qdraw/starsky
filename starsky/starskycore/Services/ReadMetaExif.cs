using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using MetadataExtractor;
using starskycore.Models;

namespace starskycore.Services
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
            item.ColorClass = item.GetColorClass();
            
            // Set the default value
            item.Orientation = item.SetAbsoluteOrientation("1");

            item.Latitude = GetGeoLocationLatitude(allExifItems);
            item.Longitude = GetGeoLocationLongitude(allExifItems);
            item.LocationAltitude = GetGeoLocationAltitude(allExifItems);
            item.SetImageWidth(GetImageWidthHeight(allExifItems,true));
            item.SetImageHeight(GetImageWidthHeight(allExifItems,false));
            
            
            foreach (var exifItem in allExifItems)
            {
                //  exifItem.Tags
                var tags = GetExifKeywords(exifItem);
                if(tags != null) // null = is not the right tag or empty tag
                {
                    item.Tags = tags;
                }
                // Colour Class => ratings
                var colorClassString = GetColorClassString(exifItem);
                if(colorClassString != null) // null = is not the right tag or empty tag
                {
                    item.ColorClass = item.GetColorClass(colorClassString);
                }
                
                // [IPTC] Caption/Abstract
                var caption = GetCaptionAbstract(exifItem);
                if(caption != null) // null = is not the right tag or empty tag
                {
                    item.Description = caption;
                }    
                
                // [IPTC] Object Name = Title
                var title = GetObjectName(exifItem);
                if(title != null) // null = is not the right tag or empty tag
                {
                     item.Title = title;
                }
                
                // DateTime of image
                var dateTime = GetExifDateTime(exifItem);
                if(dateTime.Year > 2) // 0 = is not the right tag or empty tag
                {
                    item.DateTime = dateTime;
                }
                
                // DateTime of image
                var orientation = GetOrientation(exifItem);
                if (orientation != FileIndexItem.Rotation.DoNotChange)
                {
                    item.Orientation = orientation;
                }

                ///    [IPTC] City = Diepenveen
                var locationCity = GetLocationPlaces(exifItem, "City");
                if(locationCity != null) // null = is not the right tag or empty tag
                {
                    item.LocationCity = locationCity;
                }
                
                ///    [IPTC] Province/State = Overijssel
                var locationState = GetLocationPlaces(exifItem, "Province/State");
                if(locationState != null) // null = is not the right tag or empty tag
                {
                    item.LocationState = locationState;
                }
                
                ///    [IPTC] Country/Primary Location Name = Nederland
                var locationCountry = GetLocationPlaces(exifItem, "Country/Primary Location Name");
                if(locationCountry != null) // null = is not the right tag or empty tag
                {
                    item.LocationCountry = locationCountry;
                }
	            
	            ///    [Exif SubIFD] Aperture Value = f/2.2
	            var aperture = GetAperture(exifItem);
	            if(Math.Abs(aperture) > 0) // 0f = is not the right tag or empty tag
	            {
		            item.Aperture = aperture;
	            }
	            
	            // [Exif SubIFD] Shutter Speed Value = 1/2403 sec
	            var shutterSpeed = GetShutterSpeedValue(exifItem);
	            if(shutterSpeed != string.Empty) // string.Empty = is not the right tag or empty tag
	            {
		            item.ShutterSpeed = shutterSpeed;
	            }
	            
	            // [Exif SubIFD] ISO Speed Ratings = 25
	            var isoSpeed = GetIsoSpeedValue(exifItem);
	            if(isoSpeed != 0) // 0 = is not the right tag or empty tag
	            {
		            item.SetIsoSpeed(isoSpeed);
	            }
	            
	            
	            var make = GetMakeModel(exifItem,true);
	            if (make != string.Empty) // string.Empty = is not the right tag or empty tag
	            {
		            item.SetMakeModel(make,0);
	            }
	            
	            var model = GetMakeModel(exifItem,false);
	            if (model != string.Empty) // string.Empty = is not the right tag or empty tag
	            {
		            item.SetMakeModel(model,1);
	            }        


            }
            
            return item;
        }

        private FileIndexItem.Rotation GetOrientation(MetadataExtractor.Directory exifItem)
        {
            var tCounts = exifItem.Tags.Count(p => p.DirectoryName == "Exif IFD0" && p.Name == "Orientation");
            if (tCounts < 1) return FileIndexItem.Rotation.DoNotChange;
            
            var caption = exifItem.Tags.FirstOrDefault(
                p => p.DirectoryName == "Exif IFD0" 
                     && p.Name == "Orientation")?.Description;

            // Not unit tested :(
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



	    private string GetMakeModel(MetadataExtractor.Directory exifItem, bool isMake)
	    {
		    // [Exif IFD0] Make = SONY
		    // [Exif IFD0] Model = SLT-A58

		    var makeModel = isMake ? "Make" : "Model";
		    
		    var tCounts =
			    exifItem.Tags.Count(p => p.DirectoryName == "Exif IFD0" && p.Name == makeModel);
		    if ( tCounts < 1 ) return string.Empty;

		    var caption = exifItem.Tags.FirstOrDefault(
			    p => p.DirectoryName == "Exif IFD0"
			         && p.Name == makeModel)?.Description;
		    
		    return caption;
	    }


	    private static void DisplayAllExif(IEnumerable<MetadataExtractor.Directory> allExifItems)
        {
//            foreach (var exifItem in allExifItems) {
//                foreach (var tag in exifItem.Tags) Console.WriteLine($"[{exifItem.Name}] {tag.Name} = {tag.Description}");
//            }
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
                         && p.Name == "Keywords")?.Description;
                if (!string.IsNullOrWhiteSpace(tags))
                {
                    tags = tags.Replace(";", ", ");
                }

                return tags;
            }
            return null;
        }

        private static string GetColorClassString(MetadataExtractor.Directory exifItem)
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
                    var prefsTagSplit = prefsTag.Split(":".ToCharArray());
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
        
        private double GetGeoLocationLatitude(List<MetadataExtractor.Directory> allExifItems)
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
        
        private double GetGeoLocationAltitude(List<MetadataExtractor.Directory> allExifItems)
        {
            //    [GPS] GPS Altitude Ref = Below sea level
            //    [GPS] GPS Altitude = 2 metres

            var altitudeString = string.Empty;
            var altitudeRef = string.Empty;
            
            foreach (var exifItem in allExifItems)
            {
                var longitudeRefLocal = exifItem.Tags.FirstOrDefault(
                    p => p.DirectoryName == "GPS" 
                         && p.Name == "GPS Altitude Ref")?.Description;
                
                if (longitudeRefLocal != null)
                {
                    altitudeRef = longitudeRefLocal;
                }
                
                var altitudeLocal = exifItem.Tags.FirstOrDefault(
                    p => p.DirectoryName == "GPS" 
                         && p.Name == "GPS Altitude")?.Description;

                if (altitudeLocal != null)
                {
                    altitudeString = altitudeLocal.Replace(" metres",string.Empty);
                    // space metres
                }
            }

            if (string.IsNullOrWhiteSpace(altitudeString) ||
                (altitudeRef != "Below sea level" && altitudeRef != "Sea level")) return 0;
            
            var altitude = int.Parse(altitudeString, CultureInfo.InvariantCulture);
            // this value is always an int
            
            if (altitudeRef == "Below sea level") altitude = altitude * -1;
                
            return altitude;
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

        private int GetImageWidthHeightMaxCount(string dirName, List<MetadataExtractor.Directory> allExifItems)
        {
            var maxcount =  6;
            if(dirName == "Exif SubIFD") maxcount = 30; // on header place 17&18
            if (allExifItems.Count <= 5) maxcount = allExifItems.Count;
            return maxcount;
        }
            
        public int GetImageWidthHeight(List<MetadataExtractor.Directory> allExifItems, bool isWidth)
        {
            // The size lives normaly in the first 5 headers
            // > "Exif IFD0" .dng
            // [Exif SubIFD] > arw; on header place 17&18
            var directoryNames = new[] {"JPEG", "PNG-IHDR", "BMP Header", "GIF Header", "Exif IFD0", "Exif SubIFD"};
            foreach (var dirName in directoryNames)
            {
                var typeName = "Image Height";
                if (isWidth) typeName = "Image Width";

                var maxcount = GetImageWidthHeightMaxCount(dirName, allExifItems);
                
                for (int i = 0; i < maxcount; i++)
                {
                    var exifItem = allExifItems[i];

                    var ratingCountsJpeg =
                        exifItem.Tags.Count(p => p.DirectoryName == dirName && p.Name.Contains(typeName));
                    if (ratingCountsJpeg >= 1)
                    {
                        var widthTag = exifItem.Tags
                            .FirstOrDefault(p => p.DirectoryName == dirName && p.Name.Contains(typeName))
                            ?.Description;
                        widthTag = widthTag?.Replace(" pixels", string.Empty);
                        int.TryParse(widthTag, out var widthInt);
                        return widthInt >= 1 ? widthInt : 0; // (widthInt >= 1) return widthInt)
                    }
                }
            }
            return 0;
        }
        

        /// <summary>
        ///     For the location element
        ///    [IPTC] City = Diepenveen
        ///    [IPTC] Province/State = Overijssel
        ///    [IPTC] Country/Primary Location Name = Nederland
        /// </summary>
        /// <param name="exifItem"></param>
        /// <param name="name">City, State or Country</param>
        /// <returns></returns>
        public string GetLocationPlaces(MetadataExtractor.Directory exifItem, string name)
        {
            var tCounts = exifItem.Tags.Count(p => p.DirectoryName == "IPTC" && p.Name == name);
            if (tCounts < 1) return null;
            
            var locationCity = exifItem.Tags.FirstOrDefault(
                p => p.DirectoryName == "IPTC" 
                     && p.Name == name)?.Description;
            return locationCity;
        }
	    
	    public double GetAperture(MetadataExtractor.Directory exifItem)
	    {
		    var apertureString = string.Empty;

		    var dtCounts = exifItem.Tags.Count(p => p.DirectoryName == "Exif SubIFD" && p.Name == "Aperture Value");
		    if (dtCounts >= 1)
		    {
			    apertureString = exifItem.Tags.FirstOrDefault(p => p.DirectoryName == "Exif SubIFD" && p.Name == "Aperture Value")?.Description;
		    }

		    dtCounts = exifItem.Tags.Count(p => p.DirectoryName == "Exif SubIFD" && p.Name == "F-Number");
		    if (dtCounts >= 1)
		    {
			    apertureString = exifItem.Tags.FirstOrDefault(p => p.DirectoryName == "Exif SubIFD" && p.Name == "F-Number")?.Description;
		    }
		    
		    if(apertureString == null) return 0d; 
		    apertureString = apertureString.Replace("f/", string.Empty);
		    float.TryParse(apertureString, NumberStyles.Number, CultureInfo.InvariantCulture, out var aperture);
		    return aperture;
		    
	    }
	    
	    // [Exif SubIFD] Shutter Speed Value = 1/2403 sec
	    public string GetShutterSpeedValue(Directory exifItem)
	    {
		    string shutterSpeedString = string.Empty;
		    var dtCounts = exifItem.Tags.Count(p => p.DirectoryName == "Exif SubIFD" && p.Name == "Shutter Speed Value");
		    if (dtCounts >= 1)
		    {
			    shutterSpeedString = exifItem.Tags.FirstOrDefault(p => p.DirectoryName == "Exif SubIFD" && p.Name == "Shutter Speed Value")?.Description;
		    }
		    
		    dtCounts = exifItem.Tags.Count(p => p.DirectoryName == "Exif SubIFD" && p.Name == "Exposure Time");
		    if (dtCounts >= 1)
		    {
			    shutterSpeedString = exifItem.Tags.FirstOrDefault(p => p.DirectoryName == "Exif SubIFD" && p.Name == "Exposure Time")?.Description;
		    }
		    
		    if(shutterSpeedString == null) return string.Empty; 
		    // the database has a 20 char limit
		    if(shutterSpeedString.Length >= 20) return string.Empty;
		    
		    // in xmp there is only a field with for example: 1/33
		    shutterSpeedString = shutterSpeedString.Replace(" sec", string.Empty);
		    return shutterSpeedString;
	    }

	    public int GetIsoSpeedValue(Directory exifItem)
	    {
		    var isoSpeedString = string.Empty;

		    var dtCounts = exifItem.Tags.Count(p => p.DirectoryName == "Exif SubIFD" && p.Name == "ISO Speed Ratings");
		    if (dtCounts >= 1)
		    {
			    isoSpeedString = exifItem.Tags.FirstOrDefault(p => p.DirectoryName == "Exif SubIFD" && p.Name == "ISO Speed Ratings")?.Description;
		    }
		    
		    if(isoSpeedString == null) return 0; 
		    int.TryParse(isoSpeedString, NumberStyles.Number, CultureInfo.InvariantCulture, out var isoSpeed);
		    return isoSpeed;
	    }

    }
}
