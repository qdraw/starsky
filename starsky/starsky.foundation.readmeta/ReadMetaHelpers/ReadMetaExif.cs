using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.Exif.Makernotes;
using MetadataExtractor.Formats.Iptc;
using MetadataExtractor.Formats.QuickTime;
using MetadataExtractor.Formats.Xmp;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.readmeta.Helpers;
using starsky.foundation.storage.Interfaces;
using Directory = MetadataExtractor.Directory;

[assembly: InternalsVisibleTo("starsky.foundation.thumbnailmeta.Services")]
[assembly: InternalsVisibleTo("starskytest")]

namespace starsky.foundation.readmeta.ReadMetaHelpers;

[SuppressMessage("Usage", "S3966: Resource '_iStorage.ReadStream' has " +
                          "already been disposed explicitly or through a using statement implicitly. " +
                          "Remove the redundant disposal.")]
public sealed class ReadMetaExif
{
	private readonly AppSettings? _appSettings;
	private readonly IStorage _iStorage;
	private readonly IWebLogger _logger;

	public ReadMetaExif(IStorage iStorage, AppSettings appSettings, IWebLogger logger)
	{
		_iStorage = iStorage;
		_appSettings = appSettings;
		_logger = logger;
	}

	public FileIndexItem ReadExifFromFile(string subPath,
		FileIndexItem? existingFileIndexItem = null) // use null to create an object
	{
		List<Directory> allExifItems;

		// Used to overwrite feature
		if ( existingFileIndexItem == null )
		{
			existingFileIndexItem = new FileIndexItem(subPath);
		}

		var defaultErrorResult = new FileIndexItem(subPath)
		{
			ColorClass = ColorClassParser.Color.None,
			ImageFormat = ExtensionRolesHelper.ImageFormat.unknown,
			Status = FileIndexItem.ExifStatus.OperationNotSupported,
			Tags = string.Empty,
			Orientation = ImageRotation.Rotation.Horizontal
		};

		using ( var stream = _iStorage.ReadStream(subPath) )
		{
			if ( stream == Stream.Null )
			{
				return defaultErrorResult;
			}

			try
			{
				allExifItems = ImageMetadataReader.ReadMetadata(stream).ToList();
				DisplayAllExif(allExifItems);
			}
			catch ( Exception )
			{
				// ImageProcessing or System.Exception: Handler moved stream beyond end of atom
				return defaultErrorResult;
			}
		}

		return ParseExifDirectory(allExifItems, existingFileIndexItem);
	}

	internal FileIndexItem ParseExifDirectory(List<Directory> allExifItems, FileIndexItem? item)
	{
		// Used to overwrite feature
		if ( item == null )
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

		item.SetImageWidth(GetImageWidthHeight(allExifItems, true));
		item.SetImageHeight(GetImageWidthHeight(allExifItems, false));

		// Update imageFormat based on Exif data
		var imageFormat = GetFileSpecificTags(allExifItems);
		if ( imageFormat != ExtensionRolesHelper.ImageFormat.unknown )
		{
			item.ImageFormat = imageFormat;
		}

		SetArrayBasedItemsTagsDescriptionTitle(allExifItems, item);
		SetArrayBasedItemsApertureShutterSpeedIso(allExifItems, item);
		SetArrayBasedItemsLocationPlaces(allExifItems, item);
		SetArrayBasedItemsOrientation(allExifItems, item);
		SetArrayBasedItemsLens(allExifItems, item);
		SetArrayBasedItemsMakeModel(allExifItems, item);
		SetArrayBasedItemSerial(allExifItems, item);
		SetArrayBasedItemsSoftwareStabilization(allExifItems, item);

		return item;
	}

	/// <summary>
	///     Combination setter Tags Description Title ColorClass
	/// </summary>
	/// <param name="allExifItems">list of items</param>
	/// <param name="item">output item</param>
	private static void SetArrayBasedItemsTagsDescriptionTitle(
		List<Directory> allExifItems, FileIndexItem item)
	{
		//  exifItem.Tags
		var tags = GetExifKeywords(allExifItems);
		if ( !string.IsNullOrEmpty(tags) ) // null = is not the right tag or empty tag
		{
			item.Tags = tags;
		}

		// Colour Class => ratings
		var colorClassString = GetColorClassString(allExifItems);
		if ( !string.IsNullOrEmpty(
			    colorClassString) ) // null = is not the right tag or empty tag
		{
			item.ColorClass = ColorClassParser.GetColorClass(colorClassString);
		}

		// [IPTC] Caption/Abstract
		var caption = GetCaptionAbstract(allExifItems);
		if ( !string.IsNullOrEmpty(caption) ) // null = is not the right tag or empty tag
		{
			item.Description = caption;
		}

		// [IPTC] Object Name = Title
		var title = GetObjectName(allExifItems);
		if ( !string.IsNullOrEmpty(title) ) // null = is not the right tag or empty tag
		{
			item.Title = title;
		}
	}


	/// <summary>
	///     Combination setter Orientation
	/// </summary>
	/// <param name="allExifItems">list of items</param>
	/// <param name="item">output item</param>
	private static void SetArrayBasedItemsOrientation(
		IEnumerable<Directory> allExifItems, FileIndexItem item)
	{
		// Orientation of image
		var orientation = GetOrientationFromExifItem(allExifItems);
		if ( orientation != ImageRotation.Rotation.DoNotChange )
		{
			item.Orientation = orientation;
		}
	}

	/// <summary>
	///     Combination setter Aperture Shutter SpeedIso
	/// </summary>
	/// <param name="allExifItems">list of items</param>
	/// <param name="item">output item</param>
	private static void SetArrayBasedItemsApertureShutterSpeedIso(List<Directory> allExifItems,
		FileIndexItem item)
	{
		//    [Exif SubIFD] Aperture Value = f/2.2
		var aperture = GetAperture(allExifItems);
		if ( Math.Abs(aperture) > 0 ) // 0f = is not the right tag or empty tag
		{
			item.Aperture = aperture;
		}

		// [Exif SubIFD] Shutter Speed Value = 1/2403 sec
		var shutterSpeed = GetShutterSpeedValue(allExifItems);
		if ( shutterSpeed != string.Empty ) // string.Empty = is not the right tag or empty tag
		{
			item.ShutterSpeed = shutterSpeed;
		}

		// [Exif SubIFD] ISO Speed Ratings = 25
		var isoSpeed = GetIsoSpeedValue(allExifItems);
		if ( isoSpeed != 0 ) // 0 = is not the right tag or empty tag
		{
			item.SetIsoSpeed(isoSpeed);
		}
	}

	/// <summary>
	///     Combination setter Location Places
	/// </summary>
	/// <param name="allExifItems">list of items</param>
	/// <param name="item">output item</param>
	private static void SetArrayBasedItemsLocationPlaces(
		List<Directory> allExifItems, FileIndexItem item)
	{
		//    [IPTC] City = Diepenveen
		var locationCity = GetLocationPlaces(allExifItems, "City", "photoshop:City");
		if ( !string.IsNullOrEmpty(locationCity) ) // null = is not the right tag or empty tag
		{
			item.LocationCity = locationCity;
		}

		//    [IPTC] Province/State = Overijssel
		var locationState = GetLocationPlaces(allExifItems,
			"Province/State", "photoshop:State");
		if ( !string.IsNullOrEmpty(locationState) ) // null = is not the right tag or empty tag
		{
			item.LocationState = locationState;
		}

		//    [IPTC] Country/Primary Location Name = Nederland
		var locationCountry = GetLocationPlaces(allExifItems,
			"Country/Primary Location Name", "photoshop:Country");
		if ( !string.IsNullOrEmpty(
			    locationCountry) ) // null = is not the right tag or empty tag
		{
			item.LocationCountry = locationCountry;
		}
	}

	/// <summary>
	///     Combination setter Focal Length and lens model
	/// </summary>
	/// <param name="allExifItems">list of items</param>
	/// <param name="item">output item</param>
	private static void SetArrayBasedItemsLens(
		List<Directory> allExifItems, FileIndexItem item)
	{
		// [Exif SubIFD] Focal Length = 200 mm
		var focalLength = GetFocalLength(allExifItems);
		if ( Math.Abs(focalLength) > 0.00001 )
		{
			item.FocalLength = focalLength;
		}

		// LENS Model
		var sonyLensModel = GetSonyMakeLensModel(allExifItems, item.LensModel);
		if ( !string.IsNullOrEmpty(sonyLensModel) )
		{
			item.SetMakeModel(sonyLensModel, 2);
		}

		var lensModel = GetMakeLensModel(allExifItems);
		if ( lensModel != string.Empty )
		{
			item.SetMakeModel(lensModel, 2);
		}
	}

	private static void SetArrayBasedItemSerial(List<Directory> allExifItems, FileIndexItem item)
	{
		var serialNumber = GetMakeSerial(allExifItems);

		if ( serialNumber != string.Empty ) // string.Empty = is not the right tag or empty tag
		{
			item.SetMakeModel(serialNumber, 3);
		}
	}

	/// <summary>
	///     Combination setter for Make and Model
	/// </summary>
	/// <param name="allExifItems">list of items</param>
	/// <param name="item">output item</param>
	private static void SetArrayBasedItemsMakeModel(
		List<Directory> allExifItems, FileIndexItem item)
	{
		var make = GetMakeModel(allExifItems, true);
		if ( make != string.Empty ) // string.Empty = is not the right tag or empty tag
		{
			// sometimes a make can have a space at the end
			item.SetMakeModel(make.Trim(), 0);
		}

		var model = GetMakeModel(allExifItems, false);
		if ( model != string.Empty ) // string.Empty = is not the right tag or empty tag
		{
			item.SetMakeModel(model, 1);
		}
	}

	/// <summary>
	///     Combination setter Software Stabilization
	/// </summary>
	/// <param name="allExifItems">list of meta data</param>
	/// <param name="item">single item</param>
	private void SetArrayBasedItemsSoftwareStabilization(List<Directory> allExifItems,
		FileIndexItem item)
	{
		item.Software = GetSoftware(allExifItems);

		item.ImageStabilisation = GetImageStabilisation(allExifItems);
		item.LocationCountryCode = GetLocationCountryCode(allExifItems);

		// DateTime of image
		var dateTime =
			GetExifDateTime(allExifItems, new CameraMakeModel(item.Make, item.Model));
		if ( dateTime != null )
		{
			item.DateTime = ( DateTime ) dateTime;
		}
	}

	/// <summary>
	///     Currently only for Sony cameras
	/// </summary>
	/// <param name="allExifItems">all items</param>
	/// <returns>Enum</returns>
	private static ImageStabilisationType GetImageStabilisation(
		IEnumerable<Directory> allExifItems)
	{
		var sonyDirectory = allExifItems.OfType<SonyType1MakernoteDirectory>().FirstOrDefault();
		var imageStabilisation =
			sonyDirectory?.GetDescription(SonyType1MakernoteDirectory.TagImageStabilisation);
		// 0 	0x0000	Off
		// 1 	0x0001	On
		return imageStabilisation switch
		{
			"Off" => ImageStabilisationType.Off,
			"On" => ImageStabilisationType.On,
			_ => ImageStabilisationType.Unknown
		};
	}

	internal static string GetLocationCountryCode(List<Directory> allExifItems)
	{
		var iptcDirectory = allExifItems.OfType<IptcDirectory>().FirstOrDefault();
		var countryCodeIptc =
			iptcDirectory?.GetDescription(IptcDirectory.TagCountryOrPrimaryLocationCode);

		if ( !string.IsNullOrEmpty(countryCodeIptc) )
		{
			return countryCodeIptc;
		}

		// XMP,http://iptc.org/std/Iptc4xmpCore/1.0/xmlns/,Iptc4xmpCore:CountryCode,NLD
		var xmpDirectory = allExifItems.OfType<XmpDirectory>().FirstOrDefault();
		var countryCodeXmp = GetXmpData(xmpDirectory, "Iptc4xmpCore:CountryCode");

		return countryCodeXmp;
	}

	private static string GetSonyMakeLensModel(IEnumerable<Directory> allExifItems,
		string lensModel)
	{
		// only if there is nothing yet
		if ( !string.IsNullOrEmpty(lensModel) )
		{
			return string.Empty;
		}

		var sonyDirectory = allExifItems.OfType<SonyType1MakernoteDirectory>().FirstOrDefault();
		var lensId = sonyDirectory?.GetDescription(SonyType1MakernoteDirectory.TagLensId);
		if ( SonyLensIdConverter.IsGenericEMountTMountOtherLens(lensId) )
		{
			return string.Empty;
		}

		return string.IsNullOrEmpty(lensId)
			? string.Empty
			: SonyLensIdConverter.GetById(lensId);
	}

	private static ExtensionRolesHelper.ImageFormat GetFileSpecificTags(
		List<Directory> allExifItems)
	{
		if ( allExifItems.Exists(p => p.Name == "JPEG") )
		{
			return ExtensionRolesHelper.ImageFormat.jpg;
		}

		if ( allExifItems.Exists(p => p.Name == "PNG-IHDR") )
		{
			return ExtensionRolesHelper.ImageFormat.png;
		}

		if ( allExifItems.Exists(p => p.Name == "BMP Header") )
		{
			return ExtensionRolesHelper.ImageFormat.bmp;
		}

		if ( allExifItems.Exists(p => p.Name == "GIF Header") )
		{
			return ExtensionRolesHelper.ImageFormat.gif;
		}

		if ( allExifItems.Exists(p => p.Name == "WebP") )
		{
			return ExtensionRolesHelper.ImageFormat.webp;
		}

		return ExtensionRolesHelper.ImageFormat.unknown;
	}

	public static ImageRotation.Rotation GetOrientationFromExifItem(
		IEnumerable<Directory> allExifItems)
	{
		var exifItem = allExifItems.OfType<ExifIfd0Directory>().FirstOrDefault();

		var caption = exifItem?.Tags
			.FirstOrDefault(p => p.Type == ExifDirectoryBase.TagOrientation)
			?.Description;
		if ( caption == null )
		{
			return ImageRotation.Rotation.DoNotChange;
		}

		switch ( caption )
		{
			case "Top, left side (Horizontal / normal)":
				return ImageRotation.Rotation.Horizontal;
			case "Right side, top (Rotate 90 CW)":
				return ImageRotation.Rotation.Rotate90Cw;
			case "Bottom, right side (Rotate 180)":
				return ImageRotation.Rotation.Rotate180;
			case "Left side, bottom (Rotate 270 CW)":
				return ImageRotation.Rotation.Rotate270Cw;
			default:
				return ImageRotation.Rotation.Horizontal;
		}
	}

	private static string GetSoftware(IEnumerable<Directory> allExifItems)
	{
		// [Exif IFD0] Software = 10.3.2
		var exifIfd0Directory = allExifItems.OfType<ExifIfd0Directory>().FirstOrDefault();
		var tagSoftware = exifIfd0Directory?.GetDescription(ExifDirectoryBase.TagSoftware);
		tagSoftware ??= string.Empty;
		return tagSoftware;
	}

	private static string GetMakeModel(List<Directory> allExifItems, bool isMake)
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

		var quickTimeMetaDataDirectory = allExifItems.OfType<QuickTimeMetadataHeaderDirectory>()
			.FirstOrDefault();
		var tagMakeModelQuickTime = isMake
			? QuickTimeMetadataHeaderDirectory.TagMake
			: QuickTimeMetadataHeaderDirectory.TagModel;

		var captionQuickTime =
			quickTimeMetaDataDirectory?.GetDescription(tagMakeModelQuickTime);
		return !string.IsNullOrEmpty(captionQuickTime) ? captionQuickTime : string.Empty;
	}

	/// <summary>
	///     [Exif SubIFD] Lens Model = E 18-200mm F3.5-6.3 OSS LE
	/// </summary>
	/// <param name="allExifItems"></param>
	/// <returns></returns>
	private static string GetMakeLensModel(List<Directory> allExifItems)
	{
		var exifIfd0Directories = allExifItems.OfType<ExifSubIfdDirectory>();

		var lensModel = string.Empty;
		foreach ( var exifIfd0Directory in exifIfd0Directories )
		{
			var directoryItem = exifIfd0Directory.GetDescription(ExifDirectoryBase.TagLensModel);
			if ( !string.IsNullOrEmpty(directoryItem) && directoryItem != "----" )
			{
				lensModel = directoryItem;
			}
		}

		return lensModel;
	}

	private static string GetMakeSerial(List<Directory> allExifItems)
	{
		var exifIfd0Directories = allExifItems.OfType<ExifSubIfdDirectory>();
		var cameraSerial = string.Empty;
		foreach ( var exifIfd0Directory in exifIfd0Directories )
		{
			var directoryItem =
				exifIfd0Directory.GetDescription(ExifDirectoryBase.TagBodySerialNumber);
			if ( !string.IsNullOrEmpty(directoryItem) && directoryItem != "----" )
			{
				cameraSerial = directoryItem;
			}
		}

		return cameraSerial;
	}

	private void DisplayAllExif(IEnumerable<Directory> allExifItems)
	{
		if ( _appSettings == null || !_appSettings.IsVerbose() )
		{
			return;
		}

		foreach ( var exifItem in allExifItems )
		{
			foreach ( var tag in exifItem.Tags )
			{
				_logger.LogDebug($"[{exifItem.Name}] {tag.Name} = {tag.Description}");
			}

			// for xmp notes
			if ( exifItem is not XmpDirectory xmpDirectory ||
			     xmpDirectory.XmpMeta == null )
			{
				continue;
			}

			foreach ( var property in xmpDirectory.XmpMeta.Properties.Where(p =>
				         !string.IsNullOrEmpty(p.Path)) )
			{
				_logger.LogDebug(
					$"{exifItem.Name},{property.Namespace},{property.Path},{property.Value}");
			}
		}
	}

	/// <summary>
	///     Read "dc:subject" values from XMP
	/// </summary>
	/// <param name="exifItem">item</param>
	/// <returns></returns>
	private static string GetXmpDataSubject(Directory? exifItem)
	{
		if ( !( exifItem is XmpDirectory xmpDirectory ) || xmpDirectory.XmpMeta == null )
		{
			return string.Empty;
		}

		var tagsList = new HashSet<string>();
		foreach ( var property in xmpDirectory.XmpMeta.Properties.Where(p =>
			         !string.IsNullOrEmpty(p.Value)
			         && p.Path.StartsWith("dc:subject[")) )
		{
			tagsList.Add(property.Value);
		}

		return HashSetHelper.HashSetToString(tagsList);
	}

	private static string GetXmpData(Directory? exifItem, string propertyPath)
	{
		// for xmp notes
		if ( exifItem is not XmpDirectory xmpDirectory || xmpDirectory.XmpMeta == null )
		{
			return string.Empty;
		}

		var result =
			( from property in xmpDirectory.XmpMeta.Properties.Where(p =>
					!string.IsNullOrEmpty(p.Value))
				where property.Path == propertyPath
				select property.Value ).FirstOrDefault();
		result ??= string.Empty;
		return result;
	}

	public static string GetObjectName(List<Directory> allExifItems)
	{
		// Xmp readings
		var xmpDirectory = allExifItems.OfType<XmpDirectory>().FirstOrDefault();
		var xmpTitle = GetXmpData(xmpDirectory, "dc:title[1]");
		if ( !string.IsNullOrEmpty(xmpTitle) )
		{
			return xmpTitle;
		}

		var iptcDirectory = allExifItems.OfType<IptcDirectory>().FirstOrDefault();
		var iptcObjectName = iptcDirectory?.Tags.FirstOrDefault(p => p.Name == "Object Name")
			?.Description;
		iptcObjectName ??= string.Empty;

		return iptcObjectName;
	}

	public static string? GetCaptionAbstract(List<Directory> allExifItems)
	{
		var xmpDirectory = allExifItems.OfType<XmpDirectory>().FirstOrDefault();
		var xmpCaption = GetXmpData(xmpDirectory, "dc:description[1]");

		if ( !string.IsNullOrEmpty(xmpCaption) )
		{
			return xmpCaption;
		}

		var iptcDirectory = allExifItems.OfType<IptcDirectory>().FirstOrDefault();
		var caption = iptcDirectory?.GetDescription(IptcDirectory.TagCaption);
		return caption;
	}

	public static string GetExifKeywords(List<Directory> allExifItems)
	{
		var iptcDirectory = allExifItems.OfType<IptcDirectory>().FirstOrDefault();
		var keyWords = iptcDirectory?.GetDescription(IptcDirectory.TagKeywords);

		if ( string.IsNullOrEmpty(keyWords) )
		{
			var xmpDirectory = allExifItems.OfType<XmpDirectory>().FirstOrDefault();
			return GetXmpDataSubject(xmpDirectory);
		}

		if ( !string.IsNullOrWhiteSpace(keyWords) )
		{
			keyWords = keyWords.Replace(";", ", ");
		}

		return keyWords;
	}

	private static string GetColorClassString(List<Directory> allExifItems)
	{
		var exifItem = allExifItems.OfType<IptcDirectory>().FirstOrDefault();
		var colorClassSting = string.Empty;
		var ratingCounts =
			exifItem?.Tags.Count(p => p.DirectoryName == "IPTC" && p.Name.Contains("0x02dd"));
		if ( ratingCounts >= 1 )
		{
			var prefsTag = exifItem!.Tags.FirstOrDefault(p =>
				p.DirectoryName == "IPTC" && p.Name.Contains("0x02dd"))?.Description;

			// Results for example
			//     0:1:0:-00001
			//     ~~~~~~
			//     0:8:0:-00001

			if ( !string.IsNullOrWhiteSpace(prefsTag) )
			{
				var prefsTagSplit = prefsTag.Split(":".ToCharArray());
				colorClassSting = prefsTagSplit[1];
			}

			return colorClassSting;
		}

		// Xmp readings
		var xmpDirectory = allExifItems.OfType<XmpDirectory>().FirstOrDefault();
		colorClassSting = GetXmpData(xmpDirectory, "photomechanic:ColorClass");
		return colorClassSting;
	}

	/// <summary>
	///     Get the EXIF/SubIFD or PNG Created Datetime
	/// </summary>
	/// <param name="allExifItems">Directory</param>
	/// <param name="cameraMakeModel">cameraMakeModel</param>
	/// <returns>Datetime</returns>
	internal DateTime? GetExifDateTime(List<Directory> allExifItems,
		CameraMakeModel? cameraMakeModel = null)
	{
		var provider = CultureInfo.InvariantCulture;

		var itemDateTimeSubIfd = ParseSubIfdDateTime(allExifItems, provider);
		if ( itemDateTimeSubIfd.Year >= 2 )
		{
			return itemDateTimeSubIfd;
		}

		var itemDateTimeQuickTime = ParseQuickTimeDateTime(cameraMakeModel, allExifItems);

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

	internal static DateTime ParseSubIfdDateTime(IEnumerable<Directory> allExifItems,
		IFormatProvider provider)
	{
		var pattern = "yyyy:MM:dd HH:mm:ss";

		// Raw files have multiple ExifSubIfdDirectories
		var exifSubIfdList = allExifItems.OfType<ExifSubIfdDirectory>().ToList();
		foreach ( var exifSubIfd in exifSubIfdList )
		{
			// https://odedcoster.com/blog/2011/12/13/date-and-time-format-strings-in-net-understanding-format-strings/
			// 2018:01:01 11:29:36
			var tagDateTimeDigitized =
				exifSubIfd.GetDescription(ExifDirectoryBase.TagDateTimeDigitized);
			DateTime.TryParseExact(tagDateTimeDigitized,
				pattern, provider, DateTimeStyles.AdjustToUniversal,
				out var itemDateTimeDigitized);
			if ( itemDateTimeDigitized.Year >= 2 )
			{
				return itemDateTimeDigitized;
			}

			var tagDateTimeOriginal =
				exifSubIfd.GetDescription(ExifDirectoryBase.TagDateTimeOriginal);
			DateTime.TryParseExact(tagDateTimeOriginal,
				pattern, provider, DateTimeStyles.AdjustToUniversal,
				out var itemDateTimeOriginal);
			if ( itemDateTimeOriginal.Year >= 2 )
			{
				return itemDateTimeOriginal;
			}
		}

		return new DateTime(0, DateTimeKind.Utc);
	}

	internal DateTime ParseQuickTimeDateTime(CameraMakeModel? cameraMakeModel,
		IEnumerable<Directory> allExifItems)
	{
		if ( _appSettings == null )
		{
			Console.WriteLine("[ParseQuickTimeDateTime] app settings is null");
		}

		cameraMakeModel ??= new CameraMakeModel();

		if ( _appSettings is { VideoUseLocalTime: null } )
		{
			_appSettings.VideoUseLocalTime = new List<CameraMakeModel>();
		}

		var useUseLocalTime = _appSettings?.VideoUseLocalTime.Exists(p =>
			string.Equals(p.Make, cameraMakeModel.Make,
				StringComparison.InvariantCultureIgnoreCase) && (
				string.Equals(p.Model, cameraMakeModel.Model,
					StringComparison.InvariantCultureIgnoreCase) ||
				string.IsNullOrEmpty(p.Model) ));


		var quickTimeDirectory =
			allExifItems.OfType<QuickTimeMovieHeaderDirectory>().FirstOrDefault();

		var quickTimeCreated =
			quickTimeDirectory?.GetDescription(QuickTimeMovieHeaderDirectory.TagCreated);

		var dateTimeStyle = useUseLocalTime == true
			? DateTimeStyles.AdjustToUniversal
			: DateTimeStyles.AssumeLocal;

		// [QuickTime Movie Header] Created = Tue Oct 11 09:40:04 2011 or Sat Mar 20 21:29:11 2010 // time is in UTC
		// Or Dutch (NL-nl) "zo mrt. 29 13:10:07 2020"
		DateTime.TryParseExact(quickTimeCreated, "ddd MMM dd HH:mm:ss yyyy",
			CultureInfo.CurrentCulture,
			dateTimeStyle, out var itemDateTimeQuickTime);

		// ReSharper disable once InvertIf
		if ( useUseLocalTime != true && _appSettings?.CameraTimeZoneInfo != null )
		{
			itemDateTimeQuickTime =
				DateTime.SpecifyKind(itemDateTimeQuickTime, DateTimeKind.Utc);
			itemDateTimeQuickTime = TimeZoneInfo.ConvertTime(itemDateTimeQuickTime,
				TimeZoneInfo.Utc, _appSettings.CameraTimeZoneInfo);
		}

		return itemDateTimeQuickTime;
	}

	/// <summary>
	///     51° 57' 2.31"
	/// </summary>
	/// <param name="allExifItems"></param>
	/// <returns></returns>
	private static double GetGeoLocationLatitude(List<Directory> allExifItems)
	{
		var latitudeString = string.Empty;
		var latitudeRef = string.Empty;

		foreach ( var exifItemTag in allExifItems.Select(p => p.Tags) )
		{
			var latitudeRefLocal = exifItemTag.FirstOrDefault(p => p.DirectoryName == "GPS"
				&& p.Name == "GPS Latitude Ref")?.Description;

			if ( latitudeRefLocal != null )
			{
				latitudeRef = latitudeRefLocal;
			}

			var latitudeLocal = exifItemTag.FirstOrDefault(p => p.DirectoryName == "GPS"
			                                                    && p.Name == "GPS Latitude")
				?.Description;

			if ( latitudeLocal != null )
			{
				latitudeString = latitudeLocal;
				continue;
			}

			var locationQuickTime = exifItemTag.FirstOrDefault(p =>
				p.DirectoryName == "QuickTime Metadata Header"
				&& p.Name == "GPS Location")?.Description;
			if ( locationQuickTime != null )
			{
				return GeoParser.ParseIsoString(locationQuickTime).Latitude;
			}
		}

		if ( string.IsNullOrWhiteSpace(latitudeString) )
		{
			return GetXmpGeoData(allExifItems, "exif:GPSLatitude");
		}

		var latitude =
			GeoParser.ConvertDegreeMinutesSecondsToDouble(latitudeString, latitudeRef);
		return Math.Floor(latitude * 10000000000) / 10000000000;
	}

	internal static double GetXmpGeoData(List<Directory> allExifItems, string propertyPath)
	{
		var latitudeString = string.Empty;
		var latitudeRef = string.Empty;

		foreach ( var exifItem in allExifItems )
		{
			// exif:GPSLatitude,45,33.615N
			var latitudeLocal = GetXmpData(exifItem, propertyPath);
			if ( string.IsNullOrEmpty(latitudeLocal) )
			{
				continue;
			}

			var split = Regex.Split(latitudeLocal, "[NSWE]",
				RegexOptions.None, TimeSpan.FromMilliseconds(100));
			if ( split.Length != 2 )
			{
				continue;
			}

			latitudeString = split[0];
			latitudeRef = latitudeLocal[^1].ToString();
		}

		if ( string.IsNullOrWhiteSpace(latitudeString) )
		{
			return 0;
		}

		var latitudeDegreeMinutes =
			GeoParser.ConvertDegreeMinutesToDouble(latitudeString, latitudeRef);
		return Math.Floor(latitudeDegreeMinutes * 10000000000) / 10000000000;
	}

	private static double GetGeoLocationAltitude(List<Directory> allExifItems)
	{
		//    [GPS] GPS Altitude Ref = Below sea level
		//    [GPS] GPS Altitude = 2 metres

		var altitudeString = string.Empty;
		var altitudeRef = string.Empty;

		foreach ( var exifItemTags in allExifItems.Select(p => p.Tags) )
		{
			var longitudeRefLocal = exifItemTags.FirstOrDefault(p => p.DirectoryName == "GPS"
				&& p.Name == "GPS Altitude Ref")?.Description;

			if ( longitudeRefLocal != null )
			{
				altitudeRef = longitudeRefLocal;
			}

			var altitudeLocal = exifItemTags.FirstOrDefault(p => p.DirectoryName == "GPS"
			                                                     && p.Name == "GPS Altitude")
				?.Description;

			if ( altitudeLocal != null )
			{
				// space metres
				altitudeString = altitudeLocal.Replace(" metres", string.Empty);
			}
		}

		// this value is always an int but current culture formatted
		var parsedAltitudeString = double.TryParse(altitudeString,
			NumberStyles.Number, CultureInfo.CurrentCulture, out var altitude);

		if ( altitudeRef == "Below sea level" )
		{
			altitude *= -1;
		}

		// Read xmp if altitudeString is string.Empty
		if ( !parsedAltitudeString )
		{
			return GetXmpGeoAlt(allExifItems);
		}

		return ( int ) altitude;
	}

	internal static double GetXmpGeoAlt(List<Directory> allExifItems)
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
			{
				continue;
			}

			foreach ( var property in xmpDirectory.XmpMeta.Properties.Where(p =>
				         !string.IsNullOrEmpty(p.Value)) )
			{
				switch ( property.Path )
				{
					case "exif:GPSAltitude":
						altitude = MathFraction.Fraction(property.Value);
						break;
					case "exif:GPSAltitudeRef":
						altitudeRef = property.Value == "1";
						break;
				}
			}
		}

		// no -0 as result
		if ( Math.Abs(altitude) < 0.001 )
		{
			return 0;
		}

		if ( altitudeRef )
		{
			altitude *= -1;
		}

		return altitude;
	}


	private static double GetGeoLocationLongitude(List<Directory> allExifItems)
	{
		var longitudeString = string.Empty;
		var longitudeRef = string.Empty;

		foreach ( var exifItemTags in allExifItems.Select(p => p.Tags) )
		{
			var longitudeRefLocal = exifItemTags.FirstOrDefault(p => p.DirectoryName == "GPS"
				&& p.Name == "GPS Longitude Ref")?.Description;

			if ( longitudeRefLocal != null )
			{
				longitudeRef = longitudeRefLocal;
			}

			var longitudeLocal = exifItemTags.FirstOrDefault(p => p.DirectoryName == "GPS"
				&& p.Name == "GPS Longitude")?.Description;

			if ( longitudeLocal != null )
			{
				longitudeString = longitudeLocal;
				continue;
			}

			var locationQuickTime = exifItemTags.FirstOrDefault(p =>
				p.DirectoryName == "QuickTime Metadata Header"
				&& p.Name == "GPS Location")?.Description;
			if ( locationQuickTime != null )
			{
				return GeoParser.ParseIsoString(locationQuickTime).Longitude;
			}
		}

		if ( !string.IsNullOrWhiteSpace(longitudeString) )
		{
			var longitude =
				GeoParser.ConvertDegreeMinutesSecondsToDouble(longitudeString, longitudeRef);
			longitude = Math.Floor(longitude * 10000000000) / 10000000000;
			return longitude;
		}

		return GetXmpGeoData(allExifItems, "exif:GPSLongitude");
	}

	private static int GetImageWidthHeightMaxCount(string dirName,
		ICollection<Directory> allExifItems)
	{
		var maxCount = 6;
		if ( dirName == "Exif SubIFD" )
		{
			maxCount = 30; // on header place 17&18
		}

		if ( allExifItems.Count <= 5 )
		{
			maxCount = allExifItems.Count;
		}

		return maxCount;
	}

	private static string GetImageWidthTypeName(string dirName,
		bool isWidth)
	{
		var typeName = "Image Height";
		if ( dirName == "QuickTime Track Header" )
		{
			typeName = "Height";
		}

		if ( isWidth )
		{
			typeName = "Image Width";
		}

		if ( isWidth && dirName == "QuickTime Track Header" )
		{
			typeName = "Width";
		}

		return typeName;
	}

	public static int GetImageWidthHeight(List<Directory> allExifItems, bool isWidth)
	{
		// The size lives normally in the first 5 headers
		// > "Exif IFD0" .dng
		// [Exif SubIFD] > arw; on header place 17&18
		var directoryNames = new[]
		{
			"JPEG", "PNG-IHDR", "BMP Header", "GIF Header", "QuickTime Track Header",
			"Exif IFD0", "Exif SubIFD", "WebP"
		};
		foreach ( var dirName in directoryNames )
		{
			var typeName = GetImageWidthTypeName(dirName, isWidth);
			var maxCount = GetImageWidthHeightMaxCount(dirName, allExifItems);

			for ( var i = 0; i < maxCount; i++ )
			{
				if ( i >= allExifItems.Count )
				{
					continue;
				}

				var exifItem = allExifItems[i];
				var value = GetImageSizeInsideLoop(exifItem, dirName, typeName);
				if ( value != 0 )
				{
					return value;
				}
			}
		}

		return 0;
	}

	private static int GetImageSizeInsideLoop(Directory exifItem, string dirName,
		string typeName)
	{
		var ratingCountsJpeg =
			exifItem.Tags.Count(p => p.DirectoryName == dirName
			                         && p.Name.Contains(typeName) && p.Description != "0");
		if ( ratingCountsJpeg >= 1 )
		{
			var widthTag = exifItem.Tags
				.FirstOrDefault(p => p.DirectoryName == dirName
				                     && p.Name.Contains(typeName) && p.Description != "0")
				?.Description;
			widthTag = widthTag?.Replace(" pixels", string.Empty);
			if ( int.TryParse(widthTag, out var widthInt) )
			{
				return widthInt >= 1 ? widthInt : 0; // (widthInt >= 1) return widthInt)
			}
		}

		return 0;
	}


	/// <summary>
	///     For the location element
	///     [IPTC] City = Diepenveen
	///     [IPTC] Province/State = Overijssel
	///     [IPTC] Country/Primary Location Name = Nederland
	/// </summary>
	/// <param name="allExifItems"></param>
	/// <param name="iptcName">City, State or Country</param>
	/// <param name="xmpPropertyPath">photoshop:State</param>
	/// <returns></returns>
	private static string GetLocationPlaces(List<Directory> allExifItems, string iptcName,
		string xmpPropertyPath)
	{
		var iptcDirectoryDirectory = allExifItems.OfType<IptcDirectory>().FirstOrDefault();

		var tCounts =
			iptcDirectoryDirectory?.Tags.Count(p =>
				p.DirectoryName == "IPTC" && p.Name == iptcName);
		if ( tCounts < 1 )
		{
			var xmpDirectory = allExifItems.OfType<XmpDirectory>().FirstOrDefault();
			return GetXmpData(xmpDirectory, xmpPropertyPath);
		}

		var locationCity = iptcDirectoryDirectory?.Tags
			.FirstOrDefault(p => p.Name == iptcName)?.Description;
		locationCity ??= string.Empty;
		return locationCity;
	}

	/// <summary>
	///     [Exif SubIFD] Focal Length
	/// </summary>
	/// <returns></returns>
	private static double GetFocalLength(List<Directory> allExifItems)
	{
		var exifSubIfdDirectory = allExifItems.OfType<ExifSubIfdDirectory>().FirstOrDefault();
		var focalLengthString =
			exifSubIfdDirectory?.GetDescription(ExifDirectoryBase.TagFocalLength);

		var xmpDirectory = allExifItems.OfType<XmpDirectory>().FirstOrDefault();

		// XMP,http://ns.adobe.com/exif/1.0/,exif:FocalLength,11/1
		var focalLengthXmp = GetXmpData(xmpDirectory, "exif:FocalLength");
		if ( string.IsNullOrEmpty(focalLengthString) && !string.IsNullOrEmpty(focalLengthXmp) )
		{
			return Math.Round(MathFraction.Fraction(focalLengthXmp), 5);
		}

		if ( string.IsNullOrWhiteSpace(focalLengthString) )
		{
			return 0d;
		}

		focalLengthString = focalLengthString.Replace(" mm", string.Empty);

		// Note: focalLengthString: (Dutch) 2,2 or (English) 2.2 based CultureInfo.CurrentCulture
		float.TryParse(focalLengthString, NumberStyles.Number,
			CultureInfo.CurrentCulture, out var focalLength);

		return Math.Round(focalLength, 5);
	}


	private static double GetAperture(List<Directory> allExifItems)
	{
		var exifItem = allExifItems.OfType<ExifSubIfdDirectory>().FirstOrDefault();

		// "Exif SubIFD"
		var apertureString = exifItem?.Tags.FirstOrDefault(p =>
			p.Name == "Aperture Value")?.Description;

		if ( string.IsNullOrEmpty(apertureString) )
		{
			apertureString = exifItem?.Tags.FirstOrDefault(p =>
				p.Name == "F-Number")?.Description;
		}

		// XMP,http://ns.adobe.com/exif/1.0/,exif:FNumber,9/1
		var xmpDirectory = allExifItems.OfType<XmpDirectory>().FirstOrDefault();
		var fNumberXmp = GetXmpData(xmpDirectory, "exif:FNumber");

		if ( string.IsNullOrEmpty(apertureString) && !string.IsNullOrEmpty(fNumberXmp) )
		{
			return MathFraction.Fraction(fNumberXmp);
		}

		if ( apertureString == null )
		{
			return 0d;
		}

		apertureString = apertureString.Replace("f/", string.Empty);
		// Note: apertureString: (Dutch) 2,2 or (English) 2.2 based CultureInfo.CurrentCulture
		float.TryParse(apertureString, NumberStyles.Number, CultureInfo.CurrentCulture,
			out var aperture);

		return aperture;
	}

	/// <summary>
	///     [Exif SubIFD] Shutter Speed Value = 1/2403 sec
	/// </summary>
	/// <param name="allExifItems">item to look in</param>
	/// <returns>value</returns>
	private static string GetShutterSpeedValue(List<Directory> allExifItems)
	{
		var exifItem = allExifItems.OfType<ExifSubIfdDirectory>().FirstOrDefault();

		// Exif SubIFD
		var shutterSpeedString = exifItem?.Tags.FirstOrDefault(p =>
			p.Name == "Shutter Speed Value")?.Description;

		if ( string.IsNullOrEmpty(shutterSpeedString) )
		{
			// Exif SubIFD
			shutterSpeedString = exifItem?.Tags.FirstOrDefault(p =>
				p.Name == "Exposure Time")?.Description;
		}

		// XMP,http://ns.adobe.com/exif/1.0/,exif:ExposureTime,1/20
		var xmpDirectory = allExifItems.OfType<XmpDirectory>().FirstOrDefault();

		var exposureTimeXmp = GetXmpData(xmpDirectory, "exif:ExposureTime");
		if ( string.IsNullOrEmpty(shutterSpeedString) &&
		     !string.IsNullOrEmpty(exposureTimeXmp) && exposureTimeXmp.Length <= 20 )
		{
			return exposureTimeXmp;
		}

		if ( shutterSpeedString == null )
		{
			return string.Empty;
		}

		// the database has a 20 char limit
		if ( shutterSpeedString.Length >= 20 )
		{
			return string.Empty;
		}

		// in xmp there is only a field with for example: 1/33
		shutterSpeedString = shutterSpeedString.Replace(" sec", string.Empty);
		return shutterSpeedString;
	}

	internal static int GetIsoSpeedValue(List<Directory> allExifItems)
	{
		var subIfdItem = allExifItems.OfType<ExifSubIfdDirectory>().FirstOrDefault();

		// Exif SubIFD
		var isoSpeedString = subIfdItem?.Tags.FirstOrDefault(p =>
			p.Name == "ISO Speed Ratings")?.Description;

		if ( string.IsNullOrEmpty(isoSpeedString) )
		{
			var canonMakerNoteDirectory =
				allExifItems.OfType<CanonMakernoteDirectory>().FirstOrDefault();

			// Canon Makernote
			isoSpeedString = canonMakerNoteDirectory?.Tags.FirstOrDefault(p =>
				p.Name == "Iso")?.Description;
			if ( isoSpeedString == "Auto" )
			{
				// src: https://github.com/exiftool/exiftool/blob/
				// 6b994069d52302062b9d7a462dc27082c4196d95/lib/Image/ExifTool/Canon.pm#L8882
				var autoIso = canonMakerNoteDirectory!.Tags.FirstOrDefault(p =>
					p.Name == "Auto ISO")?.Description;
				var baseIso = canonMakerNoteDirectory.Tags.FirstOrDefault(p =>
					p.Name == "Base ISO")?.Description;
				if ( !string.IsNullOrEmpty(autoIso) && !string.IsNullOrEmpty(baseIso) )
				{
					int.TryParse(autoIso, NumberStyles.Number,
						CultureInfo.InvariantCulture, out var autoIsoSpeed);
					int.TryParse(baseIso, NumberStyles.Number,
						CultureInfo.InvariantCulture, out var baseIsoSpeed);
					return baseIsoSpeed * autoIsoSpeed / 100;
				}
			}
		}

		// XMP,http://ns.adobe.com/exif/1.0/,exif:ISOSpeedRatings,
		// XMP,,exif:ISOSpeedRatings[1],101
		// XMP,,exif:ISOSpeedRatings[2],101
		var xmpDirectory = allExifItems.OfType<XmpDirectory>().FirstOrDefault();
		var isoSpeedXmp = GetXmpData(xmpDirectory, "exif:ISOSpeedRatings[1]");
		if ( string.IsNullOrEmpty(isoSpeedString) && !string.IsNullOrEmpty(isoSpeedXmp) )
		{
			isoSpeedString = isoSpeedXmp;
		}

		int.TryParse(isoSpeedString, NumberStyles.Number, CultureInfo.InvariantCulture,
			out var isoSpeed);
		return isoSpeed;
	}
}
