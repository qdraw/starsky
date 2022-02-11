using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.Exif.Makernotes;
using MetadataExtractor.Formats.QuickTime;
using MetadataExtractor.Formats.Xmp;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starsky.foundation.readmeta.Helpers;
using starsky.foundation.storage.Interfaces;
using Directory = MetadataExtractor.Directory;

[assembly: InternalsVisibleTo("starsky.foundation.metathumbnail.Services")]
[assembly: InternalsVisibleTo("starskytest")]
namespace starsky.foundation.readmeta.ReadMetaHelpers
{
	public class ReadMetaExif
	{
		private readonly IStorage _iStorage;
		private readonly AppSettings _appSettings;

		public ReadMetaExif(IStorage iStorage, AppSettings appSettings = null)
		{
			_iStorage = iStorage;
			_appSettings = appSettings;
		}
		public FileIndexItem ReadExifFromFile(string subPath, 
			FileIndexItem existingFileIndexItem = null) // use null to create an object
        {
            List<Directory> allExifItems;

	        // Used to overwrite feature
	        if (existingFileIndexItem == null)
	        {
		        existingFileIndexItem = new FileIndexItem(subPath);
	        }

	        var defaultErrorResult = new FileIndexItem(subPath)
	        {
		        ColorClass = ColorClassParser.Color.None,
		        ImageFormat = ExtensionRolesHelper.ImageFormat.unknown,
		        Status = FileIndexItem.ExifStatus.OperationNotSupported,
		        Tags = nameof(ImageProcessingException).ToLowerInvariant(),
		        Orientation = FileIndexItem.Rotation.Horizontal
	        };
	        
	        using ( var stream = _iStorage.ReadStream(subPath) )
	        {
		        if ( stream == Stream.Null ) return defaultErrorResult;
				try
				{
					allExifItems = ImageMetadataReader.ReadMetadata(stream).ToList();
					DisplayAllExif(allExifItems);
				}
				catch (Exception)
				{
					// ImageProcessing or System.Exception: Handler moved stream beyond end of atom
					stream.Dispose();
					return defaultErrorResult;
				}
	        }
	        
            return ParseExifDirectory(allExifItems, existingFileIndexItem);
        }

        private FileIndexItem ParseExifDirectory(List<MetadataExtractor.Directory> allExifItems, FileIndexItem item)
        {
            // Used to overwrite feature
            if (item == null)
            {
                throw new ArgumentException("need to fill item with filepath");
            }
            
            // Set the default value
            item.ColorClass = ColorClassParser.GetColorClass();
            
            // Set the default value
            item.Orientation = item.SetAbsoluteOrientation("1");

            item.Latitude = GetGeoLocationLatitude(allExifItems);
            item.Longitude = GetGeoLocationLongitude(allExifItems);
            item.LocationAltitude = GetGeoLocationAltitude(allExifItems);
            item.SetImageWidth(GetImageWidthHeight(allExifItems,true));
            item.SetImageHeight(GetImageWidthHeight(allExifItems,false));

	        // Update imageFormat based on Exif data
	        var imageFormat = GetFileSpecificTags(allExifItems);
	        if ( imageFormat != ExtensionRolesHelper.ImageFormat.unknown )
		        item.ImageFormat = imageFormat;
            
            foreach (var exifItem in allExifItems)
            {
                //  exifItem.Tags
                var tags = GetExifKeywords(exifItem);
                if(!string.IsNullOrEmpty(tags)) // null = is not the right tag or empty tag
                {
                    item.Tags = tags;
                }
                // Colour Class => ratings
                var colorClassString = GetColorClassString(exifItem);
                if(!string.IsNullOrEmpty(colorClassString)) // null = is not the right tag or empty tag
                {
                    item.ColorClass = ColorClassParser.GetColorClass(colorClassString);
                }
                
                // [IPTC] Caption/Abstract
                var caption = GetCaptionAbstract(exifItem);
                if(!string.IsNullOrEmpty(caption)) // null = is not the right tag or empty tag
                {
                    item.Description = caption;
                }    
                
                // [IPTC] Object Name = Title
                var title = GetObjectName(exifItem);
                if(!string.IsNullOrEmpty(title)) // null = is not the right tag or empty tag
                {
                     item.Title = title;
                }
                
                // Orientation of image
                var orientation = GetOrientationFromExifItem(exifItem);
                if (orientation != FileIndexItem.Rotation.DoNotChange)
                {
                    item.Orientation = orientation;
                }

                //    [IPTC] City = Diepenveen
                var locationCity = GetLocationPlaces(exifItem, "City","photoshop:City");
                if(!string.IsNullOrEmpty(locationCity)) // null = is not the right tag or empty tag
                {
                    item.LocationCity = locationCity;
                }
                
                //    [IPTC] Province/State = Overijssel
                var locationState = GetLocationPlaces(exifItem, 
	                "Province/State","photoshop:State");
                if(!string.IsNullOrEmpty(locationState)) // null = is not the right tag or empty tag
                {
                    item.LocationState = locationState;
                }
                
                //    [IPTC] Country/Primary Location Name = Nederland
                var locationCountry = GetLocationPlaces(exifItem, 
	                "Country/Primary Location Name","photoshop:Country");
                if(!string.IsNullOrEmpty(locationCountry)) // null = is not the right tag or empty tag
                {
                    item.LocationCountry = locationCountry;
                }
	            
	            //    [Exif SubIFD] Aperture Value = f/2.2
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

	            var lensModel = GetMakeLensModel(exifItem);
	            if (lensModel != string.Empty)
	            {
		            item.SetMakeModel(lensModel,2);
	            }
	            
	            // [Exif SubIFD] Focal Length = 200 mm
	            var focalLength = GetFocalLength(exifItem);
	            if (Math.Abs(focalLength) > 0.00001) 
	            {
		            item.FocalLength = focalLength;
	            }


            }
            
            var make = GetMakeModel(allExifItems,true);
            if (make != string.Empty) // string.Empty = is not the right tag or empty tag
            {
	            item.SetMakeModel(make,0);
            }
	            
            var model = GetMakeModel(allExifItems,false);
            if (model != string.Empty) // string.Empty = is not the right tag or empty tag
            {
	            item.SetMakeModel(model,1);
            }
            

            item.Software = GetSoftware(allExifItems);
            
            // last & out of the loop
            var sonyLensModel = GetSonyMakeLensModel(allExifItems, item.LensModel);
            if ( !string.IsNullOrEmpty(sonyLensModel) )
            {
	            item.SetMakeModel(sonyLensModel,2);
            }
            
            item.ImageStabilisation = GetImageStabilisation(allExifItems);
            
            // DateTime of image
            var dateTime = GetExifDateTime(allExifItems, new CameraMakeModel(item.Make, item.Model));
            if ( dateTime != null )
            {
	            item.DateTime = (DateTime)dateTime;
            }

            return item;
        }
        
        /// <summary>
        /// Currently only for Sony cameras
        /// </summary>
        /// <param name="allExifItems">all items</param>
        /// <returns>Enum</returns>
        private static ImageStabilisationType GetImageStabilisation(IEnumerable<Directory> allExifItems)
        {
	        var sonyDirectory = allExifItems.OfType<SonyType1MakernoteDirectory>().FirstOrDefault();
	        var imageStabilisation = sonyDirectory?.GetDescription(SonyType1MakernoteDirectory.TagImageStabilisation);
	        // 0 	0x0000	Off
	        // 1 	0x0001	On
	        switch ( imageStabilisation )
	        {
		        case "Off":
			        return ImageStabilisationType.Off;
		        case "On":
			        return ImageStabilisationType.On;
	        }
	        return ImageStabilisationType.Unknown;
        }

        private string GetSonyMakeLensModel(List<Directory> allExifItems, string lensModel)
        {
	        // only if there is nothing yet
	        if ( !string.IsNullOrEmpty(lensModel) ) return string.Empty;
	        var sonyDirectory = allExifItems.OfType<SonyType1MakernoteDirectory>().FirstOrDefault();
	        var lensId = sonyDirectory?.GetDescription(SonyType1MakernoteDirectory.TagLensId);
	        
	        return string.IsNullOrEmpty(lensId) ? string.Empty : new SonyLensIdConverter().GetById(lensId);
        }

		private ExtensionRolesHelper.ImageFormat GetFileSpecificTags(List<Directory> allExifItems)
		{
			if ( allExifItems.Any(p => p.Name == "JPEG") )
				return ExtensionRolesHelper.ImageFormat.jpg;
				
			if ( allExifItems.Any(p => p.Name == "PNG-IHDR") )
				return ExtensionRolesHelper.ImageFormat.png;
			
			if ( allExifItems.Any(p => p.Name == "BMP Header") )
				return ExtensionRolesHelper.ImageFormat.bmp;	
			
			if ( allExifItems.Any(p => p.Name == "GIF Header") )
				return ExtensionRolesHelper.ImageFormat.gif;	
				
			return ExtensionRolesHelper.ImageFormat.unknown;
		}

		public FileIndexItem.Rotation GetOrientationFromExifItem(Directory exifItem)
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

		private string GetSoftware(List<Directory> allExifItems)
		{
			// [Exif IFD0] Software = 10.3.2
			var exifIfd0Directory = allExifItems.OfType<ExifIfd0Directory>().FirstOrDefault();
			var tagSoftware = exifIfd0Directory?.GetDescription(ExifDirectoryBase.TagSoftware);
			return tagSoftware;
		}


	    private string GetMakeModel(List<Directory> allExifItems, bool isMake)
	    {
		    // [Exif IFD0] Make = SONY
		    // [Exif IFD0] Model = SLT-A58

		    var exifIfd0Directory = allExifItems.OfType<ExifIfd0Directory>().FirstOrDefault();
		    var tagMakeModelExif = isMake ? ExifDirectoryBase.TagMake : ExifDirectoryBase.TagModel;
		    
		    var captionExifIfd0 = exifIfd0Directory?.GetDescription(tagMakeModelExif);
		    if ( !string.IsNullOrEmpty(captionExifIfd0) )
		    {
			    return captionExifIfd0;
		    }
		    
		    var quickTimeMetaDataDirectory = allExifItems.OfType<QuickTimeMetadataHeaderDirectory>().FirstOrDefault();
		    var tagMakeModelQuickTime = isMake ? QuickTimeMetadataHeaderDirectory.TagMake : QuickTimeMetadataHeaderDirectory.TagModel;
		    
		    var captionQuickTime = quickTimeMetaDataDirectory?.GetDescription(tagMakeModelQuickTime);
		    return !string.IsNullOrEmpty(captionQuickTime) ? captionQuickTime : string.Empty;
	    }

	    /// <summary>
	    /// [Exif SubIFD] Lens Model = E 18-200mm F3.5-6.3 OSS LE
	    /// </summary>
	    /// <param name="exifItem"></param>
	    /// <returns></returns>
	    private string GetMakeLensModel(Directory exifItem)
	    {
		    var lensModel = exifItem.Tags.FirstOrDefault(
			    p => p.DirectoryName == "Exif SubIFD"
			         && p.Name == "Lens Model")?.Description;

		    return lensModel == "----" ? string.Empty : lensModel;
	    }


	    private void DisplayAllExif(List<Directory> allExifItems)
        {
	        if(_appSettings == null || !_appSettings.IsVerbose()) return;
	        
            foreach (var exifItem in allExifItems) {
                foreach (var tag in exifItem.Tags) Console.WriteLine($"[{exifItem.Name}] {tag.Name} = {tag.Description}");
                // for xmp notes
                if (exifItem is XmpDirectory xmpDirectory && xmpDirectory.XmpMeta != null)
                {
	                foreach (var property in xmpDirectory.XmpMeta.Properties.Where(
		                p => !string.IsNullOrEmpty(p.Path)))
	                {
		                Console.WriteLine($"{exifItem.Name},{property.Namespace},{property.Path},{property.Value}");
	                }
                }
            }
        }
	    

	    /// <summary>
	    /// Read "dc:subject" values from XMP
	    /// </summary>
	    /// <param name="exifItem">item</param>
	    /// <returns></returns>
	    public string GetXmpDataSubject(Directory exifItem) // 
	    {
		    if ( !( exifItem is XmpDirectory xmpDirectory ) || xmpDirectory.XmpMeta == null )
			    return string.Empty;
		    
		    var tagsList = new HashSet<string>();
		    foreach (var property in xmpDirectory.XmpMeta.Properties.Where(p => !string.IsNullOrEmpty(p.Value)))
		    {
			    if ( property.Path.StartsWith("dc:subject[") )
			    {
				    tagsList.Add(property.Value);
			    }
		    }
		    return HashSetHelper.HashSetToString(tagsList);
	    }

	    public string GetXmpData(Directory exifItem, string propertyPath)
	    {
		    // for xmp notes
		    if ( !( exifItem is XmpDirectory xmpDirectory ) || xmpDirectory.XmpMeta == null )
			    return string.Empty;

		    return ( from property in xmpDirectory.XmpMeta.Properties.Where(p => !string.IsNullOrEmpty(p.Value)) 
			    where property.Path == propertyPath select property.Value ).FirstOrDefault();
	    }

        public string GetObjectName (Directory exifItem)
        {
            var tCounts = exifItem.Tags.Count(p => p.DirectoryName == "IPTC" && p.Name == "Object Name");
            
            // Xmp readings
            if ( tCounts == 0 ) return GetXmpData(exifItem, "dc:title[1]");
            
            var caption = exifItem.Tags.FirstOrDefault(
                p => p.DirectoryName == "IPTC" 
                     && p.Name == "Object Name")?.Description;
            return caption;
        }

        
        public string GetCaptionAbstract(Directory exifItem)
        {
            var tCounts = exifItem.Tags.Count(p => p.DirectoryName == "IPTC" && p.Name == "Caption/Abstract");

            // Xmp readings
            if ( tCounts == 0 ) return GetXmpData(exifItem, "dc:description[1]");

            var caption = exifItem.Tags.FirstOrDefault(
	            p => p.DirectoryName == "IPTC" 
	                 && p.Name == "Caption/Abstract")?.Description;
            return caption;
        }
        
        public string GetExifKeywords(Directory exifItem)
        {
            var tCounts = exifItem.Tags.Count(p => p.DirectoryName == "IPTC" && p.Name == "Keywords");
            
            if ( tCounts == 0 ) return GetXmpDataSubject(exifItem);

            var tags = exifItem.Tags.FirstOrDefault(
                p => p.DirectoryName == "IPTC" 
                     && p.Name == "Keywords")?.Description;
            if (!string.IsNullOrWhiteSpace(tags))
            {
                tags = tags.Replace(";", ", ");
            }

            return tags;
        }

        private string GetColorClassString(MetadataExtractor.Directory exifItem)
        {
	        var colorClassSting = string.Empty;
            var ratingCounts = exifItem.Tags.Count(p => p.DirectoryName == "IPTC" && p.Name.Contains("0x02dd"));
            if (ratingCounts >= 1)
            {
                var prefsTag = exifItem.Tags.FirstOrDefault(p => 
	                p.DirectoryName == "IPTC" && p.Name.Contains("0x02dd"))?.Description;
    
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
            
            // Xmp readings
            colorClassSting = GetXmpData(exifItem, "photomechanic:ColorClass");
            return colorClassSting;
        }

        /// <summary>
        /// Get the EXIF/SubIFD or PNG Created Datetime
        /// </summary>
        /// <param name="allExifItems">Directory</param>
        /// <param name="cameraMakeModel">cameraMakeModel</param>
        /// <returns>Datetime</returns>
        internal DateTime? GetExifDateTime(List<Directory> allExifItems, CameraMakeModel cameraMakeModel = null)
        {
	        var provider = CultureInfo.InvariantCulture;

	        var itemDateTimeSubIfd = ParseSubIfdDateTime(allExifItems, provider);
	        if ( itemDateTimeSubIfd.Year >= 2 )
	        {
		        return itemDateTimeSubIfd;
	        }
			
	        var itemDateTimeQuickTime = ParseQuickTimeDateTime(cameraMakeModel, allExifItems, provider);

	        // to avoid errors scanning gpx files (with this it would be Local)

	        if ( itemDateTimeQuickTime.Year >= 1970 )
	        {
		        return itemDateTimeQuickTime;
	        }
	        
	        // 1970-01-01T02:00:03 formatted
	        var xmpDirectory = allExifItems.OfType<XmpDirectory>().FirstOrDefault();

	        var photoShopDateCreated = GetXmpData(xmpDirectory, "photoshop:DateCreated");
	        DateTime.TryParseExact(photoShopDateCreated, "yyyy-MM-ddTHH:mm:ss", provider, 
		        DateTimeStyles.AdjustToUniversal, out var xmpItemDateTime);
	        
	        if ( xmpItemDateTime.Year >= 2 )
	        {
		        return xmpItemDateTime;
	        }
	        
	        return null;
        }

        internal DateTime ParseSubIfdDateTime(IEnumerable<Directory> allExifItems, IFormatProvider provider)
        {
	        var pattern = "yyyy:MM:dd HH:mm:ss";

	        // Raw files have multiple ExifSubIfdDirectories
	        var exifSubIfdList = allExifItems.OfType<ExifSubIfdDirectory>().ToList();
	        foreach ( var exifSubIfd in exifSubIfdList )
	        {
		        //     https://odedcoster.com/blog/2011/12/13/date-and-time-format-strings-in-net-understanding-format-strings/
		        //     2018:01:01 11:29:36
		        var tagDateTimeDigitized = exifSubIfd?.GetDescription(ExifDirectoryBase.TagDateTimeDigitized);
		        DateTime.TryParseExact(tagDateTimeDigitized, pattern, provider, DateTimeStyles.AdjustToUniversal, out var itemDateTimeDigitized);
		        if ( itemDateTimeDigitized.Year >= 2 )
		        {
			        return itemDateTimeDigitized;
		        }
	        
		        var tagDateTimeOriginal = exifSubIfd?.GetDescription(ExifDirectoryBase.TagDateTimeOriginal);
		        DateTime.TryParseExact(tagDateTimeOriginal, pattern, provider, DateTimeStyles.AdjustToUniversal, out var itemDateTimeOriginal);
		        if ( itemDateTimeOriginal.Year >= 2 )
		        {
			        return itemDateTimeOriginal;
		        }
	        }

	        return new DateTime();
        }

        internal DateTime ParseQuickTimeDateTime(CameraMakeModel cameraMakeModel,
	        IEnumerable<Directory> allExifItems, IFormatProvider provider)
        {
	        if ( _appSettings == null ) Console.WriteLine("[ParseQuickTimeDateTime] app settings is null");
	        if ( cameraMakeModel == null ) cameraMakeModel = new CameraMakeModel();
	        var useUseLocalTime = _appSettings?.VideoUseLocalTime?.Any(p =>
		        string.Equals(p.Make, cameraMakeModel.Make, StringComparison.InvariantCultureIgnoreCase) && (
			        string.Equals(p.Model, cameraMakeModel.Model, StringComparison.InvariantCultureIgnoreCase) ||
			        string.IsNullOrEmpty(p.Model) ));
	        
	        
	        var quickTimeDirectory = allExifItems.OfType<QuickTimeMovieHeaderDirectory>().FirstOrDefault();

	        var quickTimeCreated = quickTimeDirectory?.GetDescription(QuickTimeMovieHeaderDirectory.TagCreated);

	        var dateTimeStyle = useUseLocalTime == true
		        ? DateTimeStyles.AdjustToUniversal
		        : DateTimeStyles.AssumeLocal;
	        
	        // [QuickTime Movie Header] Created = Tue Oct 11 09:40:04 2011 or Sat Mar 20 21:29:11 2010 // time is in UTC
	        DateTime.TryParseExact(quickTimeCreated, "ddd MMM dd HH:mm:ss yyyy", provider, 
		        dateTimeStyle, out var itemDateTimeQuickTime);

	        // ReSharper disable once InvertIf
	        if ( useUseLocalTime != true && _appSettings?.CameraTimeZoneInfo != null)
	        {
		        itemDateTimeQuickTime = DateTime.SpecifyKind(itemDateTimeQuickTime, DateTimeKind.Utc);
		        itemDateTimeQuickTime =  TimeZoneInfo.ConvertTime(itemDateTimeQuickTime, 
			        TimeZoneInfo.Utc, _appSettings?.CameraTimeZoneInfo); 
	        }

	        return itemDateTimeQuickTime;
        }
        
        /// <summary>
        /// 51° 57' 2.31"
        /// </summary>
        /// <param name="allExifItems"></param>
        /// <returns></returns>
        private double GetGeoLocationLatitude(List<Directory> allExifItems)
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
                    continue;
                }

                var locationQuickTime = exifItem.Tags.FirstOrDefault(
	                p => p.DirectoryName == "QuickTime Metadata Header" 
	                     && p.Name == "GPS Location")?.Description;
                if ( locationQuickTime != null)
                {
	                return GeoParser.ParseIsoString(locationQuickTime).Latitude;
                }
            }

            if ( string.IsNullOrWhiteSpace(latitudeString) )
	            return GetXmpGeoData(allExifItems, "exif:GPSLatitude");
            
            var latitude = GeoParser.ConvertDegreeMinutesSecondsToDouble(latitudeString, latitudeRef);
            return  Math.Floor(latitude * 10000000000) / 10000000000;

        }

        private double GetXmpGeoData(List<Directory> allExifItems, string propertyPath)
        {
	        var latitudeString = string.Empty;
	        var latitudeRef = string.Empty;
	        
	        foreach ( var exifItem in allExifItems )
	        {
		        // exif:GPSLatitude,45,33.615N
		        var latitudeLocal = GetXmpData(exifItem, propertyPath);
		        if(string.IsNullOrEmpty(latitudeLocal)) continue;
		        var split = Regex.Split(latitudeLocal, "[NSWE]");
		        if(split.Length != 2) continue;
		        latitudeString = split[0];
		        latitudeRef = latitudeLocal[latitudeLocal.Length-1].ToString();
	        }

	        if ( string.IsNullOrWhiteSpace(latitudeString) ) return 0;
            
	        var latitudeDegreeMinutes = GeoParser.ConvertDegreeMinutesToDouble(latitudeString, latitudeRef);
	        return Math.Floor(latitudeDegreeMinutes * 10000000000) / 10000000000; 
        }
        
        private double GetGeoLocationAltitude(List<Directory> allExifItems)
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
	                // space metres
                    altitudeString = altitudeLocal.Replace(" metres",string.Empty);
                }
            }

            // this value is always an int but current culture formatted
            var parsedAltitudeString = double.TryParse(altitudeString, 
	            NumberStyles.Number, CultureInfo.CurrentCulture, out var altitude);
            
            if (altitudeRef == "Below sea level" ) altitude *= -1;

            // Read xmp if altitudeString is string.Empty
            if (!parsedAltitudeString)
            {
	            return GetXmpGeoAlt(allExifItems);
            }

            return (int) altitude;
        }

        private double GetXmpGeoAlt(List<Directory> allExifItems)
        {
	        var altitudeRef = true;
	        var altitude = 0d;
	        
	        // example:
	        // +1
	        // XMP,http://ns.adobe.com/exif/1.0/,exif:GPSAltitude,1/1
	        // XMP,http://ns.adobe.com/exif/1.0/,exif:GPSAltitudeRef,0

	        // -10
	        // XMP,http://ns.adobe.com/exif/1.0/,exif:GPSAltitude,10/1
	        // XMP,http://ns.adobe.com/exif/1.0/,exif:GPSAltitudeRef,1
	        
	        foreach ( var exifItem in allExifItems )
	        {
		        if ( !( exifItem is XmpDirectory xmpDirectory ) || xmpDirectory.XmpMeta == null )
			        continue;

		        foreach (var property in xmpDirectory.XmpMeta.Properties.Where(p =>
			        !string.IsNullOrEmpty(p.Value)) )
		        {
			        switch ( property.Path )
			        {
				        case "exif:GPSAltitude":
					        altitude = new MathFraction().Fraction(property.Value);
					        break;
				        case "exif:GPSAltitudeRef":
					        altitudeRef = property.Value == "1";
					        break;
			        }
		        }
	        }
	        
	        // no -0 as result
	        if(Math.Abs(altitude) < 0.001) return 0;
	        
	        if (altitudeRef) altitude *= -1;
	        return altitude;
        }
        
        
        private double GetGeoLocationLongitude(List<Directory> allExifItems)
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
                    continue;
                }

                var locationQuickTime = exifItem.Tags.FirstOrDefault(
	                p => p.DirectoryName == "QuickTime Metadata Header" 
	                     && p.Name == "GPS Location")?.Description;
                if ( locationQuickTime != null)
                {
	                return GeoParser.ParseIsoString(locationQuickTime).Longitude;
                }
            }

            if (!string.IsNullOrWhiteSpace(longitudeString))
            {
                var longitude = GeoParser.ConvertDegreeMinutesSecondsToDouble(longitudeString, longitudeRef);
                longitude = Math.Floor(longitude * 10000000000) / 10000000000; 
                return longitude;
            }
            
            return GetXmpGeoData(allExifItems, "exif:GPSLongitude");
        }

        private int GetImageWidthHeightMaxCount(string dirName, List<MetadataExtractor.Directory> allExifItems)
        {
            var maxCount =  6;
            if(dirName == "Exif SubIFD") maxCount = 30; // on header place 17&18
            if (allExifItems.Count <= 5) maxCount = allExifItems.Count;
            return maxCount;
        }
            
        public int GetImageWidthHeight(List<Directory> allExifItems, bool isWidth)
        {
            // The size lives normally in the first 5 headers
            // > "Exif IFD0" .dng
            // [Exif SubIFD] > arw; on header place 17&18
            var directoryNames = new[] {"JPEG", "PNG-IHDR", "BMP Header", "GIF Header", 
	            "QuickTime Track Header", "Exif IFD0", "Exif SubIFD"};
            foreach (var dirName in directoryNames)
            {
                var typeName = "Image Height";
                if(dirName == "QuickTime Track Header") typeName = "Height";
                
                if (isWidth) typeName = "Image Width";
                if(isWidth && dirName == "QuickTime Track Header") typeName = "Width";

                var maxCount = GetImageWidthHeightMaxCount(dirName, allExifItems);
                
                for (var i = 0; i < maxCount; i++)
                {
	                if(i >= allExifItems.Count) continue;
                    var exifItem = allExifItems[i];
                    var value = GetImageSizeInsideLoop(exifItem, dirName, typeName);
                    if ( value != 0 ) return value;
                }
            }
            return 0;
        }

        private int GetImageSizeInsideLoop( Directory exifItem, string dirName, string typeName)
        {
	        
	        var ratingCountsJpeg =
		        exifItem.Tags.Count(p => p.DirectoryName == dirName 
		                                 && p.Name.Contains(typeName) && p.Description != "0");
	        if (ratingCountsJpeg >= 1)
	        {
		        var widthTag = exifItem.Tags
			        .FirstOrDefault(p => p.DirectoryName == dirName 
			                             && p.Name.Contains(typeName) && p.Description != "0")
			        ?.Description;
		        widthTag = widthTag?.Replace(" pixels", string.Empty);
		        int.TryParse(widthTag, out var widthInt);
		        return widthInt >= 1 ? widthInt : 0; // (widthInt >= 1) return widthInt)
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
        /// <param name="iptcName">City, State or Country</param>
        /// <param name="xmpPropertyPath">photoshop:State</param>
        /// <returns></returns>
        public string GetLocationPlaces(Directory exifItem, string iptcName, string xmpPropertyPath)
        {
            var tCounts = exifItem.Tags.Count(p => p.DirectoryName == "IPTC" && p.Name == iptcName);
            if ( tCounts < 1 )
            {
	            return GetXmpData(exifItem, xmpPropertyPath);
            }
            
            var locationCity = exifItem.Tags.FirstOrDefault(
                p => p.DirectoryName == "IPTC" 
                     && p.Name == iptcName)?.Description;
            return locationCity;
        }

        /// <summary>
        /// [Exif SubIFD] Focal Length
        /// </summary>
        /// <returns></returns>
        public double GetFocalLength(Directory exifItem)
        {
	        var focalLengthString = exifItem.Tags.FirstOrDefault(
		        p => p.DirectoryName == "Exif SubIFD" 
		             && p.Name == "Focal Length")?.Description;
	        
	        // XMP,http://ns.adobe.com/exif/1.0/,exif:FocalLength,11/1
	        var focalLengthXmp = GetXmpData(exifItem, "exif:FocalLength");
	        if (string.IsNullOrEmpty(focalLengthString) && !string.IsNullOrEmpty(focalLengthXmp))
	        {
		        return Math.Round(new MathFraction().Fraction(focalLengthXmp), 5);
	        }
	        
	        if ( string.IsNullOrWhiteSpace(focalLengthString) ) return 0d;

	        focalLengthString = focalLengthString.Replace(" mm", string.Empty);
	        
	        // Note: focalLengthString: (Dutch) 2,2 or (English) 2.2 based CultureInfo.CurrentCulture
	        float.TryParse(focalLengthString, NumberStyles.Number, CultureInfo.CurrentCulture, out var focalLength);
	        
	        return Math.Round(focalLength, 5);
        }

        
	    public double GetAperture(Directory exifItem)
	    {
		    var apertureString = exifItem.Tags.FirstOrDefault(p => 
			    p.DirectoryName == "Exif SubIFD" && p.Name == "Aperture Value")?.Description;

		    if (string.IsNullOrEmpty(apertureString))
		    {
			    apertureString = exifItem.Tags.FirstOrDefault(p => 
				    p.DirectoryName == "Exif SubIFD" && p.Name == "F-Number")?.Description;
		    }
		    
		    // XMP,http://ns.adobe.com/exif/1.0/,exif:FNumber,9/1
		    var fNumberXmp = GetXmpData(exifItem, "exif:FNumber");
		    if (string.IsNullOrEmpty(apertureString) && !string.IsNullOrEmpty(fNumberXmp))
		    {
			    return new MathFraction().Fraction(fNumberXmp);
		    }
		    
		    if(apertureString == null) return 0d; 
		    
		    apertureString = apertureString.Replace("f/", string.Empty);
		    // Note: apertureString: (Dutch) 2,2 or (English) 2.2 based CultureInfo.CurrentCulture
		    float.TryParse(apertureString, NumberStyles.Number, CultureInfo.CurrentCulture, out var aperture);
		    
		    return aperture;
	    }
	    
	    // [Exif SubIFD] Shutter Speed Value = 1/2403 sec
	    public string GetShutterSpeedValue(Directory exifItem)
	    {
		    var shutterSpeedString = exifItem.Tags.FirstOrDefault(p => 
			    p.DirectoryName == "Exif SubIFD" && p.Name == "Shutter Speed Value")?.Description;

		    if (string.IsNullOrEmpty(shutterSpeedString))
		    {
			    shutterSpeedString = exifItem.Tags.FirstOrDefault(p => 
				    p.DirectoryName == "Exif SubIFD" && p.Name == "Exposure Time")?.Description;
		    }
		    
		    // XMP,http://ns.adobe.com/exif/1.0/,exif:ExposureTime,1/20
		    var exposureTimeXmp = GetXmpData(exifItem, "exif:ExposureTime");
		    if (string.IsNullOrEmpty(shutterSpeedString) && !string.IsNullOrEmpty(exposureTimeXmp) && exposureTimeXmp.Length <= 20)
		    {
			    return exposureTimeXmp;
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
		    var isoSpeedString = exifItem.Tags.FirstOrDefault(p => 
			    p.DirectoryName == "Exif SubIFD" && p.Name == "ISO Speed Ratings")?.Description;

		    if ( string.IsNullOrEmpty(isoSpeedString) )
		    {
			    isoSpeedString = exifItem.Tags.FirstOrDefault(p => 
				    p.DirectoryName == "Canon Makernote" && p.Name == "Iso")?.Description;
			    if ( isoSpeedString == "Auto" )
			    {
				    // src: https://github.com/exiftool/exiftool/blob/
				    // 6b994069d52302062b9d7a462dc27082c4196d95/lib/Image/ExifTool/Canon.pm#L8882
				    var autoIso = exifItem.Tags.FirstOrDefault(p => 
					    p.DirectoryName == "Canon Makernote" && p.Name == "Auto ISO")?.Description;
				    var baseIso = exifItem.Tags.FirstOrDefault(p => 
					    p.DirectoryName == "Canon Makernote" && p.Name == "Base ISO")?.Description;
				    if ( !string.IsNullOrEmpty(autoIso) && !string.IsNullOrEmpty(baseIso) )
				    {
					    int.TryParse(autoIso, NumberStyles.Number, CultureInfo.InvariantCulture, out var autoIsoSpeed);
					    int.TryParse(baseIso, NumberStyles.Number, CultureInfo.InvariantCulture, out var baseIsoSpeed);
					    return baseIsoSpeed * autoIsoSpeed / 100;
				    }
			    }
		    }
		    
		    // XMP,http://ns.adobe.com/exif/1.0/,exif:ISOSpeedRatings,
		    // XMP,,exif:ISOSpeedRatings[1],101
		    // XMP,,exif:ISOSpeedRatings[2],101
		    var isoSpeedXmp = GetXmpData(exifItem, "exif:ISOSpeedRatings[1]");
		    if (string.IsNullOrEmpty(isoSpeedString) && !string.IsNullOrEmpty(isoSpeedXmp))
		    {
			    isoSpeedString = isoSpeedXmp;
		    }
		    
		    int.TryParse(isoSpeedString, NumberStyles.Number, CultureInfo.InvariantCulture, out var isoSpeed);
		    return isoSpeed;
	    }
		
	}
}
