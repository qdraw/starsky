using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.readmeta.Helpers;
using starsky.foundation.storage.Helpers;
using starsky.foundation.storage.Interfaces;
using XmpCore;

namespace starsky.foundation.readmeta.ReadMetaHelpers;

public sealed class ReadMetaXmp
{
	private readonly IStorage _iStorage;
	private readonly IWebLogger _logger;

	public ReadMetaXmp(IStorage iStorage, IWebLogger logger)
	{
		_iStorage = iStorage;
		_logger = logger;
	}

	public async Task<FileIndexItem> XmpGetSidecarFileAsync(FileIndexItem? databaseItem)
	{
		databaseItem ??= new FileIndexItem();

		// Parse an xmp file for this location
		var xmpSubPath =
			ExtensionRolesHelper.ReplaceExtensionWithXmp(databaseItem.FilePath);
		// also add when the file is a jpeg, we are not writing to it then
		if ( _iStorage.ExistFile(xmpSubPath) )
		{
			databaseItem.AddSidecarExtension("xmp");
		}

		// Read content from sidecar xmp file
		if ( !ExtensionRolesHelper.IsExtensionForceXmp(databaseItem.FilePath) ||
		     !_iStorage.ExistFile(xmpSubPath) )
		{
			return databaseItem;
		}

		// Read the text-content of the xmp file.
		var xmpStream = _iStorage.ReadStream(xmpSubPath);
		// xmpStream is disposed in StreamToStringAsync
		var xmp = await StreamToStringHelper.StreamToStringAsync(xmpStream);

		// Get the data from the xmp
		databaseItem = GetDataFromString(xmp, databaseItem);
		return databaseItem;
	}

	public FileIndexItem GetDataFromString(string xmpDataAsString,
		FileIndexItem? databaseItem = null)
	{
		// Does not require appSettings

		databaseItem ??= new FileIndexItem();

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
			_logger.LogInformation(
				$"XmpException {databaseItem.FilePath} >>\n{e}\n <<XmpException");
			databaseItem.Tags = string.Empty;
			databaseItem.Status =
				FileIndexItem.ExifStatus.OperationNotSupported;
			databaseItem.ColorClass = ColorClassParser.Color.None;
		}


		return databaseItem;
	}

	/// <summary>
	///     Get the value for items with the name without namespace
	/// </summary>
	/// <param name="property">IXmpPropertyInfo read from string</param>
	/// <param name="xmpName">xmpName, for example dc:subject[1]</param>
	/// <returns>value or null</returns>
	private static string? GetNullNameSpace(IXmpPropertyInfo property, string xmpName)
	{
		if ( property.Path == xmpName && !string.IsNullOrEmpty(property.Value)
		                              && string.IsNullOrEmpty(property.Namespace) )
		{
			return property.Value;
		}

		return null;
	}

	/// <summary>
	///     Get the value for items with the name with a namespace
	/// </summary>
	/// <param name="property">IXmpPropertyInfo read from string</param>
	/// <param name="xmpName">xmpName, for example dc:subject[1]</param>
	/// <returns>value or null</returns>
	private static string? GetContentNameSpace(IXmpPropertyInfo property, string xmpName)
	{
		if ( property.Path == xmpName && !string.IsNullOrEmpty(property.Value)
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
		var refGps = gpsLatOrLong.Substring(gpsLatOrLong.Length - 1, 1);
		return GeoParser.ConvertDegreeMinutesToDouble(gpsLatOrLong, refGps);
	}

	[SuppressMessage("Usage", "S125:Remove this commented out code")]
	private static FileIndexItem GetDataNullNameSpaceTypes(IXmpMeta xmp, FileIndexItem item)
	{
		foreach ( var property in xmp.Properties )
		{
			// dc:description[1] and dc:subject 
			SetCombinedDescriptionSubject(property, item);

			// Path=dc:title Namespace=http://purl.org/dc/elements/1.1/ Value=
			// Path=dc:title[1] Namespace= Value=The object name
			//    Path=dc:title[1]/xml:lang Namespace=http://www.w3...
			var title = GetNullNameSpace(property, "dc:title[1]");
			if ( title != null )
			{
				item.Title = title;
			}

			// Path=exif:ISOSpeedRatings Namespace=http://ns.adobe.com/exif/1.0/ Value=
			// Path=exif:ISOSpeedRatings[1] Namespace= Value=25
			var isoSpeed = GetNullNameSpace(property, "exif:ISOSpeedRatings[1]");
			if ( isoSpeed != null )
			{
				item.SetIsoSpeed(isoSpeed);
			}

			var height = GetContentNameSpace(property, "exif:PixelXDimension");
			if ( height != null )
			{
				item.SetImageHeight(height);
			}

			var width = GetContentNameSpace(property, "exif:PixelYDimension");
			if ( width != null )
			{
				item.SetImageWidth(width);
			}

			var lensModel = GetContentNameSpace(property, "exifEX:LensModel");
			if ( lensModel != null )
			{
				item.SetMakeModel(lensModel, 2);
			}

			var bodySerialNumber = GetContentNameSpace(property, "exifEX:BodySerialNumber");
			if ( bodySerialNumber != null )
			{
				item.SetMakeModel(bodySerialNumber, 3);
			}

			var countryCode = GetContentNameSpace(property, "Iptc4xmpCore:CountryCode");
			if ( countryCode != null )
			{
				item.LocationCountryCode = countryCode;
			}

			// ImageStabilisation is not found in XMP

			// don't show in production 
			// Console.WriteLine($"Path={property.Path} Namespace={property.Namespace} Value={property.Value}");
		}

		return item;
	}

	private static void SetCombinedDescriptionSubject(IXmpPropertyInfo property,
		FileIndexItem item)
	{
		//   Path=dc:description Namespace=http://purl.org/dc/elements/1.1/ Value=
		//   Path=dc:description[1] Namespace= Value=caption
		//   Path=dc:description[1]/xml:lang Namespace=http...
		var description = GetNullNameSpace(property, "dc:description[1]");
		if ( description != null )
		{
			item.Description = description;
		}

		// Path=dc:subject Namespace=http://purl.org/dc/elements/1.1/ Value=
		// Path=dc:subject[1] Namespace= Value=keyword
		var tags = GetNullNameSpace(property, "dc:subject[1]");
		if ( tags != null )
		{
			item.Tags = tags;
		}

		// Path=dc:subject[2] Namespace= Value=keyword2
		if ( !string.IsNullOrEmpty(property.Path) &&
		     property.Path.Contains("dc:subject[") &&
		     property.Path != "dc:subject[1]" &&
		     !string.IsNullOrEmpty(property.Value) &&
		     string.IsNullOrEmpty(property.Namespace) )
		{
			var tagsStringBuilder = new StringBuilder();
			tagsStringBuilder.Append(item.Tags);
			tagsStringBuilder.Append(", ");
			tagsStringBuilder.Append(property.Value);
			item.Tags = tagsStringBuilder.ToString();
		}
	}

	private static void GpsAltitudeRef(IXmpMeta xmp, FileIndexItem item)
	{
		string? gpsAltitude = null;
		string? gpsAltitudeRef = null;
		foreach ( var property in xmp.Properties )
		{
			// Path=exif:GPSAltitude Namespace=http://ns.adobe.com/exif/1.0/ Value=627/10
			// Path=exif:GPSAltitudeRef Namespace=http://ns.adobe.com/exif/1.0/ Value=0
			var gpsAltitudeLocal = GetContentNameSpace(property, "exif:GPSAltitude");
			if ( gpsAltitudeLocal != null )
			{
				gpsAltitude = gpsAltitudeLocal;
			}

			var gpsAltitudeRefLocal = GetContentNameSpace(property, "exif:GPSAltitudeRef");
			if ( gpsAltitudeRefLocal != null )
			{
				gpsAltitudeRef = gpsAltitudeRefLocal;
			}
		}

		if ( gpsAltitude == null || gpsAltitudeRef == null )
		{
			return;
		}

		if ( !gpsAltitude.Contains('/') )
		{
			return;
		}

		var locationAltitude = MathFraction.Fraction(gpsAltitude);
		if ( Math.Abs(locationAltitude) < 0 )
		{
			return;
		}

		item.LocationAltitude = locationAltitude;

		//For items under the sea level
		if ( gpsAltitudeRef == "1" )
		{
			item.LocationAltitude = item.LocationAltitude * -1;
		}
	}


	/// <summary>
	///     ContentNameSpace is for example : Namespace=http://...
	/// </summary>
	/// <param name="xmp"></param>
	/// <param name="item"></param>
	/// <returns></returns>
	private static FileIndexItem GetDataContentNameSpaceTypes(IXmpMeta xmp, FileIndexItem item)
	{
		GpsAltitudeRef(xmp, item);

		foreach ( var property in xmp.Properties )
		{
			SetCombinedLatLong(property, item);
			SetCombinedDateTime(property, item);
			SetCombinedColorClass(property, item);
			SetCombinedOrientation(property, item);
			SetCombinedImageHeightWidth(property, item);
			SetCombinedCityStateCountry(property, item);

			// exif:ExposureTime http://ns.adobe.com/exif/1.0/
			var shutterSpeed = GetContentNameSpace(property, "exif:ExposureTime");
			if ( shutterSpeed != null )
			{
				item.ShutterSpeed = shutterSpeed;
			}

			// exif:FNumber http://ns.adobe.com/exif/1.0/
			var aperture = GetContentNameSpace(property, "exif:FNumber");
			if ( aperture != null )
			{
				item.Aperture = MathFraction.Fraction(aperture);
			}

			// Path=tiff:Make Namespace=http://ns.adobe.com/tiff/1.0/ Value=SONY
			var make = GetContentNameSpace(property, "tiff:Make");
			if ( make != null )
			{
				item.SetMakeModel(make, 0);
			}

			// Path=tiff:Model Namespace=http://ns.adobe.com/tiff/1.0/ Value=SLT-A58
			var model = GetContentNameSpace(property, "tiff:Model");
			if ( model != null )
			{
				item.SetMakeModel(model, 1);
			}

			// Path=exif:FocalLength Namespace=http://ns.adobe.com/exif/1.0/ Value=200/1
			// Path=exif:FocalLength Namespace=http://ns.adobe.com/exif/1.0/ Value=18/1
			var focalLength = GetContentNameSpace(property, "exif:FocalLength");
			if ( focalLength != null )
			{
				item.FocalLength = MathFraction.Fraction(focalLength);
			}

			// Path=xmp:CreatorTool Namespace=http://ns.adobe.com/xap/1.0/ Value=SLT-A58 v1.00
			var software = GetContentNameSpace(property, "xmp:CreatorTool");
			if ( software != null )
			{
				item.Software = software;
			}
		}

		return item;
	}

	private static void SetCombinedOrientation(IXmpPropertyInfo property, FileIndexItem item)
	{
		// Path=tiff:Orientation Namespace=http://ns.adobe.com/tiff/1.0/ Value=6
		var rotation = GetContentNameSpace(property, "tiff:Orientation");
		if ( rotation != null )
		{
			item.SetAbsoluteOrientation(rotation);
		}
	}

	private static void SetCombinedColorClass(IXmpPropertyInfo property, FileIndexItem item)
	{
		//   Path=photomechanic:ColorClass Namespace=http://ns.camerabits.com/photomechanic/1.0/ Value=1
		var colorClass = GetContentNameSpace(property, "photomechanic:ColorClass");
		if ( colorClass != null )
		{
			item.ColorClass = ColorClassParser.GetColorClass(colorClass);
		}
	}

	private static void SetCombinedImageHeightWidth(IXmpPropertyInfo property,
		FileIndexItem item)
	{
		//  Path=tiff:ImageLength Namespace=http://ns.adobe.com/tiff/1.0/ Value=13656
		var height = GetContentNameSpace(property, "tiff:ImageLength");
		if ( height != null )
		{
			item.SetImageHeight(height);
		}

		//  Path=tiff:ImageWidth Namespace=http://ns.adobe.com/tiff/1.0/ Value=15504
		var width = GetContentNameSpace(property, "tiff:ImageWidth");
		if ( width != null )
		{
			item.SetImageWidth(width);
		}
	}

	private static void SetCombinedLatLong(
		IXmpPropertyInfo property, FileIndexItem item)
	{
		// Path=exif:GPSLatitude Namespace=http://ns.adobe.com/exif/1.0/ Value=52,20.708N
		var gpsLatitude = GetContentNameSpace(property, "exif:GPSLatitude");
		if ( gpsLatitude != null )
		{
			item.Latitude = GpsPreParseAndConvertDegreeAngleToDouble(gpsLatitude);
		}

		// Path=exif:GPSLongitude Namespace=http://ns.adobe.com/exif/1.0/ Value=5,55.840E
		var gpsLongitude = GetContentNameSpace(property, "exif:GPSLongitude");
		if ( gpsLongitude != null )
		{
			item.Longitude = GpsPreParseAndConvertDegreeAngleToDouble(gpsLongitude);
		}
	}

	private static void SetCombinedDateTime(
		IXmpPropertyInfo property, FileIndexItem item)
	{
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
			if ( dateTime.Year >= 3 )
			{
				item.DateTime = dateTime;
			}
		}

		// Option 2 (Datetime)
		// Path=xmp:CreateDate Namespace=http://ns.adobe.com/xap/1.0/ Value=2019-03-02T11:29:18+01:00
		// Path=xmp:CreateDate Namespace=http://ns.adobe.com/xap/1.0/ Value=2019-03-02T11:29:18
		var createDate = GetContentNameSpace(property, "xmp:CreateDate");
		if ( createDate != null )
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

			// and use this value
			item.DateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified);
		}
	}

	private static void SetCombinedCityStateCountry(
		IXmpPropertyInfo property, FileIndexItem item)
	{
		// Path=photoshop:City Namespace=http://ns.adobe.com/photoshop/1.0/ Value=Epe
		var locationCity = GetContentNameSpace(property, "photoshop:City");
		if ( locationCity != null )
		{
			item.LocationCity = locationCity;
		}

		// Path=photoshop:State Namespace=http://ns.adobe.com/photoshop/1.0/ Value=Gelderland
		var locationState = GetContentNameSpace(property, "photoshop:State");
		if ( locationState != null )
		{
			item.LocationState = locationState;
		}

		// Path=photoshop:Country Namespace=http://ns.adobe.com/photoshop/1.0/ Value=Nederland
		var locationCountry = GetContentNameSpace(property, "photoshop:Country");
		if ( locationCountry != null )
		{
			item.LocationCountry = locationCountry;
		}
	}
}
