using System;
using System.Globalization;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using starskycore.Helpers;
using starskycore.Interfaces;
using starskycore.Models;
using XmpCore;

namespace starskycore.Services
{
	public class ReadMetaXmp
	{
		private readonly IStorage _iStorage;

		public ReadMetaXmp(IStorage iStorage, IMemoryCache memoryCache = null)
		{
			_iStorage = iStorage;
		}
		
        public FileIndexItem XmpGetSidecarFile(FileIndexItem databaseItem)
        {
	        if(databaseItem == null) databaseItem = new FileIndexItem();

            // Read content from sidecar xmp file
            if (ExtensionRolesHelper.IsExtensionForceXmp(databaseItem.FilePath))
            {
                // Parse an xmp file for this location
	            var xmpSubPath =
		            ExtensionRolesHelper.ReplaceExtensionWithXmp(databaseItem.FilePath);
                if ( _iStorage.ExistFile(xmpSubPath) )
                {
                    // Read the text-content of the xmp file.
                    var xmp = new PlainTextFileHelper().StreamToString(_iStorage.ReadStream(xmpSubPath));
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
	        
	        try
	        {
		        var xmp = XmpMetaFactory.ParseFromString(xmpDataAsString);
		        // ContentNameSpace is for example : Namespace=http://...
		        databaseItem = GetDataContentNameSpaceTypes(xmp, databaseItem);
		        // NullNameSpace is for example : string.Empty
		        databaseItem = GetDataNullNameSpaceTypes(xmp, databaseItem);
	        }
	        catch ( XmpException e )
	        {
		        Console.WriteLine($"XmpException {databaseItem.FilePath} >>\n{e}\n <<XmpException");
		        databaseItem.Tags = "XmpException";
		        databaseItem.ColorClass = FileIndexItem.Color.None;
	        }
	        
	        
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
            return GeoDistanceTo.ConvertDegreeMinutesToDouble(gpsLatOrLong, refGps);
        }
                
        private FileIndexItem GetDataNullNameSpaceTypes(IXmpMeta xmp, FileIndexItem item)
        {
	        
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
	            
	            
	            // Path=exif:ISOSpeedRatings Namespace=http://ns.adobe.com/exif/1.0/ Value=
	            // Path=exif:ISOSpeedRatings[1] Namespace= Value=25
	            var isoSpeed = GetNullNameSpace(property, "exif:ISOSpeedRatings[1]");
	            if ( isoSpeed != null ) item.SetIsoSpeed(isoSpeed);
	            
//				Console.WriteLine($"Path={property.Path} Namespace={property.Namespace} Value={property.Value}");


            }

            return item;
        }

        private void GpsAltitudeRef(IXmpMeta xmp, FileIndexItem item)
        {
            string gpsAltitude = null;
            string gpsAltitudeRef = null;
            foreach (var property in xmp.Properties)
            {
                // Path=exif:GPSAltitude Namespace=http://ns.adobe.com/exif/1.0/ Value=627/10
                // Path=exif:GPSAltitudeRef Namespace=http://ns.adobe.com/exif/1.0/ Value=0
                var gpsAltitudeLocal = GetContentNameSpace(property, "exif:GPSAltitude");
                if (gpsAltitudeLocal != null)
                {
                    gpsAltitude = gpsAltitudeLocal;
                }
                
                var gpsAltitudeRefLocal = GetContentNameSpace(property, "exif:GPSAltitudeRef");
                if (gpsAltitudeRefLocal != null)
                {
                    gpsAltitudeRef = gpsAltitudeRefLocal;
                }
            }
            if(gpsAltitude == null || gpsAltitudeRef == null) return;
            if(!gpsAltitude.Contains("/")) return;

			var locationAltitude = new MathFraction().Fraction(gpsAltitude);
	        if(Math.Abs(locationAltitude) < 0) return;
	        item.LocationAltitude = locationAltitude;
	        
            //For items under the sea level
            if (gpsAltitudeRef == "1") item.LocationAltitude = item.LocationAltitude * -1;
        }




	    /// <summary>
	    /// ContentNameSpace is for example : Namespace=http://...
	    /// </summary>
	    /// <param name="xmp"></param>
	    /// <param name="item"></param>
	    /// <returns></returns>
	    private FileIndexItem GetDataContentNameSpaceTypes(IXmpMeta xmp, FileIndexItem item)
	    {
     
            GpsAltitudeRef(xmp, item);
                
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

	            // Option 1 (Datetime)
	            // Path=exif:DateTimeOriginal Namespace=http://ns.adobe.com/exif/1.0/ Value=2018-07-18T19:44:27
	            var dateTimeOriginal = GetContentNameSpace(property, "exif:DateTimeOriginal");
	            if ( dateTimeOriginal != null )
	            {
		            DateTime.TryParseExact(dateTimeOriginal,
			            "yyyy-MM-dd\\THH:mm:ss",
			            CultureInfo.InvariantCulture,
			            DateTimeStyles.None,
			            out var dateTime);
		            if ( dateTime.Year >= 3 ) item.DateTime = dateTime;
	            }

	            // Option 2 (Datetime)
	            // Path=xmp:CreateDate Namespace=http://ns.adobe.com/xap/1.0/ Value=2019-03-02T11:29:18+01:00
	            // Path=xmp:CreateDate Namespace=http://ns.adobe.com/xap/1.0/ Value=2019-03-02T11:29:18
	            var createDate = GetContentNameSpace(property, "xmp:CreateDate");
	            if (createDate != null)
	            {
		            DateTime.TryParseExact(createDate,
			            "yyyy-MM-dd\\THH:mm:sszzz",
			            CultureInfo.InvariantCulture,
			            DateTimeStyles.None,
			            out var dateTime);
		            
		            // The other option
		            if ( dateTime.Year <= 3 )
		            {
			            DateTime.TryParseExact(createDate,
				            "yyyy-MM-dd\\THH:mm:ss",
				            CultureInfo.InvariantCulture,
				            DateTimeStyles.None,
				            out dateTime);
		            }
		            // write it back
		            item.DateTime = dateTime;
	            }
                
                //   Path=photomechanic:ColorClass Namespace=http://ns.camerabits.com/photomechanic/1.0/ Value=1
                var colorClass = GetContentNameSpace(property, "photomechanic:ColorClass");
                if (colorClass != null)
                {
                    item.ColorClass = item.GetColorClass(colorClass);
                }
                
                // Path=tiff:Orientation Namespace=http://ns.adobe.com/tiff/1.0/ Value=6
                var rotation = GetContentNameSpace(property, "tiff:Orientation");
                if (rotation != null)
                {
                    item.SetAbsoluteOrientation(rotation);
                }
                
                //  Path=tiff:ImageLength Namespace=http://ns.adobe.com/tiff/1.0/ Value=13656
                var height = GetContentNameSpace(property, "tiff:ImageLength");
                if (height != null)
                {
                    item.SetImageHeight(height);
                }

                //  Path=tiff:ImageWidth Namespace=http://ns.adobe.com/tiff/1.0/ Value=15504
                var width = GetContentNameSpace(property, "tiff:ImageWidth");
                if (width != null)
                {
                    item.SetImageWidth(width);
                }
                
                // Path=photoshop:City Namespace=http://ns.adobe.com/photoshop/1.0/ Value=Epe
                var locationCity = GetContentNameSpace(property, "photoshop:City");
                if (locationCity != null) item.LocationCity = locationCity;

                // Path=photoshop:State Namespace=http://ns.adobe.com/photoshop/1.0/ Value=Gelderland
                var locationState = GetContentNameSpace(property, "photoshop:State");
                if (locationState != null) item.LocationState = locationState;
                
                // Path=photoshop:Country Namespace=http://ns.adobe.com/photoshop/1.0/ Value=Nederland
                var locationCountry = GetContentNameSpace(property, "photoshop:Country");
                if (locationCountry != null) item.LocationCountry = locationCountry;
	            
	            // exif:ExposureTime http://ns.adobe.com/exif/1.0/
	            var shutterSpeed = GetContentNameSpace(property, "exif:ExposureTime");
	            if (shutterSpeed != null) item.ShutterSpeed = shutterSpeed;
	            
	            // exif:FNumber http://ns.adobe.com/exif/1.0/
	            var aperture = GetContentNameSpace(property, "exif:FNumber");
	            if (aperture != null) item.Aperture =  new MathFraction().Fraction(aperture);
	            
	            // Path=tiff:Make Namespace=http://ns.adobe.com/tiff/1.0/ Value=SONY
	            var make = GetContentNameSpace(property, "tiff:Make");
	            if (make != null) item.SetMakeModel(make,0);

				// Path=tiff:Model Namespace=http://ns.adobe.com/tiff/1.0/ Value=SLT-A58
	            var model = GetContentNameSpace(property, "tiff:Model");
	            if (model != null) item.SetMakeModel(model,1);

				// Path=exif:FocalLength Namespace=http://ns.adobe.com/exif/1.0/ Value=200/1
				// Path=exif:FocalLength Namespace=http://ns.adobe.com/exif/1.0/ Value=18/1
				var focalLength = GetContentNameSpace(property, "exif:FocalLength");
				if ( focalLength != null ) item.FocalLength =  new MathFraction().Fraction(focalLength);
	            
				// Path=xmp:CreatorTool Namespace=http://ns.adobe.com/xap/1.0/ Value=SLT-A58 v1.00
				var software = GetContentNameSpace(property, "xmp:CreatorTool");
				if ( software != null ) item.Software = software;
            }
	        
            return item;
        }
	}
}
