using System.Text.Json.Serialization;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using starsky.foundation.platform.Helpers;

namespace starsky.foundation.database.Models
{
	public sealed class FileIndexItem
	{
		/// <summary>
		/// Default
		/// </summary>
		public FileIndexItem()
		{
			SetLastEdited();
			if ( AddToDatabase.Year == 0 ) SetAddToDatabase();
		}

		/// <summary>
		/// Make new FileIndexItem with set subPath
		/// </summary>
		/// <param name="subPath">the subPath</param>
		public FileIndexItem(string subPath)
		{
			SetFilePath(subPath);
			IsDirectory = false;
			SetLastEdited();
		}
	    
		/// <summary>
		/// Unique database Id, not used and Json Ignored due the fact that files that are moved could have a new Id 
		/// </summary>
		/// <value>
		/// The identifier.
		/// </value>
		[JsonIgnore]
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		[SuppressMessage("ReSharper", "PropertyCanBeMadeInitOnly.Global")]
		public int Id { get; set; }

		/// <summary>
		/// Internal API for storing full file path's
		/// </summary>
		private string FilePathPrivate { get; set; } = string.Empty;

		/// <summary>
		/// Get a concatenated subPath style filepath to find the location
		/// </summary>
		/// <value>
		/// The file path.
		/// </value>
		[Column(Order = 2)]
		[MaxLength(380)]
		public string? FilePath
		{
			get
			{
				if ( string.IsNullOrEmpty(FilePathPrivate)  )
				{
					return "/";
				}
				return FilePathPrivate;
			}
			set => SetFilePath(value);
		}

		/// <summary>
		/// Sets the file path as Filename and ParentDirectory
		/// </summary>
		/// <param name="value">The value.</param>
		public void SetFilePath(string? value)
		{
			var parentPath = FilenamesHelper.GetParentPath(value);
			if ( string.IsNullOrEmpty(parentPath)) parentPath = "/";
			// Home has no parentDirectory and filename slash
			if ( value != "/" ) _parentDirectory = parentPath;
			
			_fileName = PathHelper.GetFileName(value);
			// filenames are without starting slash
			_fileName = PathHelper.RemovePrefixDbSlash(_fileName);
			
			FilePathPrivate = PathHelper.RemoveLatestSlash(ParentDirectory) + PathHelper.PrefixDbSlash(FileName);
		}

		/// <summary>
		/// Internal API: Do not save null in database for FileName
		/// </summary>
		private string? _fileName;

		/// <summary>
		/// Get or Set FileName with extension
		/// Without first slash or path
		/// </summary>
		/// <value>
		/// The name of the file with extension
		/// </value>
		/// <example>/folder/image.jpg</example>
		[Column(Order = 1)]
		[MaxLength(190)] // Index column size too large. The maximum column size is 767 bytes (767/4)
		public string? FileName
		{
			// ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
			get => _fileName ?? string.Empty;
			set
			{
				if (string.IsNullOrWhiteSpace(value))
				{
					_fileName = string.Empty;
					return;
				}
				_fileName = value;
				FilePathPrivate = PathHelper.RemoveLatestSlash(ParentDirectory) +
				                  PathHelper.PrefixDbSlash(value);
			}
		}

		/// <summary>
		/// Base32 hash to identify this file. Gets or sets the file hash.
		/// </summary>
		/// <value>
		/// The file hash.
		/// </value>
		/// <example>OZHCK4I47QPHOT53QBRE7Z4RLI</example>
		[MaxLength(190)] // Index column size too large. The maximum column size is 767 bytes (767/4)
		public string? FileHash { get; set; } = string.Empty;
		
		
		/// <summary>
		/// GetFileNameWithoutExtension (only a getter)
		/// </summary>
		/// <value>
		/// The name of the file collection.
		/// </value>
		/// <example>filenameWithoutExtension</example>
		[NotMapped]
		public string? FileCollectionName => Path.GetFileNameWithoutExtension(FileName);

		/// <summary>
		/// Internal API: Do not save null in database for Parent Directory
		/// </summary>
		private string _parentDirectory = string.Empty;

		/// <summary>
		/// Get/Set Relative path of the parent Directory
		/// </summary>
		/// <value>
		/// The parent directory in subPath style
		/// </value>
		/// <example>/folder</example>
		[MaxLength(190)] // Index column size too large. The maximum column size is 767 bytes (767/4)
		public string? ParentDirectory
		{
			// ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
			get => _parentDirectory ?? string.Empty;
			set
			{
				if (string.IsNullOrWhiteSpace(value))
				{
					_parentDirectory = string.Empty;
					return;
				}
				_parentDirectory = value;
				FilePathPrivate = PathHelper.RemoveLatestSlash(value) +
				                  PathHelper.PrefixDbSlash(FileName);
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether this instance is directory.
		/// </summary>
		/// <value>
		///   <c>true</c> if this instance is directory; otherwise (then is a file), <c>false</c>.
		/// </value>
		/// <example>true</example>
		public bool? IsDirectory { get; set; }

		/// <summary>
		/// Get/Set a HashList with Tags (stores as string under Tags)
		/// </summary>
		/// <value>
		/// The keywords as List/HashList
		/// </value>
		[NotMapped]
		[JsonIgnore] // <== gives conversion errors with jsonParser
		internal HashSet<string>? Keywords {
			get => HashSetHelper.StringToHashSet(Tags?.Trim());
			set
			{
				if (value == null) return;
				_tags = HashSetHelper.HashSetToString(value);
			} 
		}
        
		/// <summary>
		/// Private API: Do not save null in database for tags
		/// </summary>
		private string? _tags;

		/// <summary>
		/// Get/Set a comma separated string of unique tags.
		/// Duplicate keywords are case sensitive removed
		/// </summary>
		/// <value>
		/// The tags.
		/// </value>
		/// <example>tag1, tag2</example>
		[MaxLength(1024)]
		public string? Tags
		{
			// ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
			get => _tags ?? string.Empty;
			set
			{
				if (string.IsNullOrWhiteSpace(value))
				{
					_tags = string.Empty;
					return;
				}
				// So remove duplicate keywords
				Keywords = HashSetHelper.StringToHashSet(value.Trim());
				_tags = HashSetHelper.HashSetToString(Keywords);
			}
		}

		/// <summary>
		/// Used to display file status (eg. NotFoundNotInIndex, Ok)
		/// </summary>
		public enum ExifStatus
		{
			/// <summary>
			/// Default (should update)
			/// </summary>
			Default,
			/// <summary>
			/// Writing is not supported
			/// </summary>
			ExifWriteNotSupported,
			/// <summary>
			/// This file is not in the database (should be added)
			/// </summary>
			NotFoundNotInIndex,
			/// <summary>
			/// Source is missing on disk (should be removed from index)
			/// </summary>
			NotFoundSourceMissing,
			/// <summary>
			/// The operation is not supported
			/// </summary>
			OperationNotSupported,
			/// <summary>
			/// Directory is read only
			/// </summary>
			DirReadOnly,
			/// <summary>
			/// Not allowed to edit
			/// </summary>
			ReadOnly,
			/// <summary>
			/// Not allowed
			/// </summary>
			Unauthorized,
			/// <summary>
			/// Everything is Good
			/// </summary>
			Ok,
			/// <summary>
			/// File is in trash
			/// </summary>
			Deleted,
			
			/// <summary>
			/// Everything is good but not changed
			/// </summary>
			OkAndSame,
			
			/// <summary>
			/// File is in trash but not changed
			/// </summary>
			DeletedAndSame

		}

		/// <summary>
		/// Gets or sets the display file status. (eg. NotFoundNotInIndex, Ok)
		/// </summary>
		/// <value>
		/// The display file status. (eg. NotFoundNotInIndex, Ok).
		/// </value>
		// newtonsoft uses: StringEnumConverter
		[JsonConverter(typeof(JsonStringEnumConverter))]
		[NotMapped]
		public ExifStatus Status { get; set; } = ExifStatus.Default;
        
		/// <summary>
		/// Internal API: to store description
		/// </summary>
		private string _description = string.Empty;

		/// <summary>
		/// Gets or sets the iptc:Caption-Abstract or xmp:description.
		/// </summary>
		/// <value>
		/// The description as string
		/// </value>
		public string? Description
		{
			// ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
			get => _description ?? string.Empty;
			set
			{
				if (string.IsNullOrWhiteSpace(value))
				{
					_description = string.Empty;
					return;
				}
				_description = value.Trim();
			}
		}
        
		/// <summary>
		/// Private API: to store Title
		/// </summary>
		private string _title = string.Empty;

		/// <summary>
		/// Gets or sets the iptc:Object Name or xmp:title.
		/// </summary>
		/// <value>
		/// The title as string
		/// </value>
		public string? Title
		{
			// ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
			get => _title ?? string.Empty;
			set
			{
				if (string.IsNullOrWhiteSpace(value))
				{
					_title = string.Empty;
					return;
				}
				_title = value.Trim();
			}
		}

		/// <summary>
		/// Date/Time Digitized or Date/Time Original
		/// Assumes localTime
		/// </summary>
		/// <value>
		/// The DateTime object
		/// </value>
		public DateTime DateTime { get; set; }
		
		/// <summary>
		/// Datetime of when this item is added to the database
		/// </summary>
		/// <value>
		/// The add to database DateTime value
		/// </value>
		public DateTime AddToDatabase { get; set; }
	    
		/// <summary>
		/// Datetime of the last change to this object
		/// </summary>
		/// <value>
		/// The last edited DateTime value
		/// </value>
		public DateTime LastEdited { get; set; }

		/// <summary>
		/// Update te last edited time manual
		/// </summary>
		public void SetLastEdited()
		{
			LastEdited = DateTime.UtcNow;
		}
	    
		/// <summary>
		/// Update the add to Database Date
		/// </summary>
		public void SetAddToDatabase()
		{
			AddToDatabase = DateTime.UtcNow;
		}

		/// <summary>
		/// Latitude as decimal degrees using WGS84 (stored as double)
		/// </summary>
		/// <value>
		/// The latitude value
		/// </value>
		public double Latitude { get; set; }

		/// <summary>
		/// Longitude  as decimal degrees using WGS84 (stored as double)
		/// </summary>
		/// <value>
		/// The longitude value
		/// </value>
		public double Longitude { get; set; }

		/// <summary>
		/// Location altitude in meters. How how difference there is relative to WGS84 round earth.
		/// (This might be different than meters above sea level)
		/// </summary>
		/// <value>
		/// The location altitude.
		/// </value>
		public double LocationAltitude { get; set; } // in meters

		/// <summary>
		/// The name of the nearest city
		/// </summary>
		/// <value>
		/// The location city.
		/// </value>
		[MaxLength(40)]
		public string? LocationCity { get; set; } = string.Empty;

		/// <summary>
		/// The name of the nearest state or province (Max length is 40 chars)
		/// </summary>
		/// <value>
		/// The state of the location.
		/// </value>
		[MaxLength(40)]
		public string? LocationState { get; set; } = string.Empty;

		/// <summary>
		/// The name of the nearest country (Max length is 40 chars)
		/// </summary>
		/// <value>
		/// The location country.
		/// </value>
		[MaxLength(40)] 
		public string? LocationCountry { get; set; } = string.Empty;
		
		
		/// <summary>
		/// ISO-3166-1 alpha-3 (for example NLD for The Netherlands)
		/// @see: https://en.wikipedia.org/wiki/ISO_3166-1_alpha-3
		/// </summary>
		[MaxLength(3)]
		public string? LocationCountryCode { get; set; }


		/// <summary>
		/// Comma separated list of color class numbers to create a list of enums
		/// Used to create custom files to sort a combination of color classes
		/// </summary>
		/// <param name="colorClassString">The color class string.</param>
		/// <returns></returns>
		public static List<ColorClassParser.Color> GetColorClassList(string? colorClassString = null)
		{
			if (string.IsNullOrWhiteSpace(colorClassString)) return new List<ColorClassParser.Color>();

			var colorClassStringList = new List<string>();

			if (!colorClassString.Contains(',') )
			{
				if (!int.TryParse(colorClassString, out var parsedInt)) return new List<ColorClassParser.Color>();
				colorClassStringList.Add(parsedInt.ToString());
			}
			if (colorClassString.Contains(',')) {
				colorClassStringList = colorClassString.Split(",".ToCharArray()).ToList();
			}
			var colorClassList = new HashSet<ColorClassParser.Color>();
			foreach (var colorClassStringItem in colorClassStringList)
			{
				colorClassList.Add(ColorClassParser.GetColorClass(colorClassStringItem));
			}
			return colorClassList.ToList();
		}


		/// <summary>
		/// Create an List of all String, bool, Datetime, ImageFormat based database fields		
		/// </summary>
		/// <returns> Files the index property list.</returns>
		public static List<string> FileIndexPropList()
		{
			var fileIndexPropList = new List<string>();
			// only for types String in FileIndexItem()

			foreach (var propertyInfo in new FileIndexItem().GetType().GetProperties())
			{
				if ((
					propertyInfo.PropertyType == typeof(bool) || 
					propertyInfo.PropertyType == typeof(bool?) || 
					propertyInfo.PropertyType == typeof(string) || 
					propertyInfo.PropertyType == typeof(DateTime) ||
					propertyInfo.PropertyType == typeof(ExtensionRolesHelper.ImageFormat) || 
					propertyInfo.PropertyType == typeof(ColorClassParser.Color)
				) && propertyInfo.CanRead)
				{
					fileIndexPropList.Add(propertyInfo.Name);
				}
			}
			return fileIndexPropList;
		}

		/// <summary>
		/// Gets or sets the color class.
		/// Always display int, because the Update api uses ints to parse
		/// </summary>
		/// <value>
		/// The color class.
		/// </value>
		public ColorClassParser.Color ColorClass { get; set; } = ColorClassParser.Color.DoNotChange;

		/// <summary>
		/// Gets or sets the image orientation.
		/// </summary>
		/// <value>
		/// The orientation as enum item
		/// </value>
		[JsonConverter(typeof(JsonStringEnumConverter))]
		// newtonsoft uses: StringEnumConverter
		public Rotation Orientation { get; set; } = Rotation.DoNotChange;
		
		/// <summary>
		/// Exit Rotation values
		/// </summary>
		public enum Rotation
		{
			DoNotChange = -1,
            
			// There are more types:
			// https://www.daveperrett.com/articles/2012/07/28/exif-orientation-handling-is-a-ghetto/
            
			[Display(Name = "Horizontal (normal)")] 
			Horizontal = 1,

			[Display(Name = "Rotate 90 CW")]
			Rotate90Cw = 6,

			[Display(Name = "Rotate 180")]
			Rotate180 = 3,  

			[Display(Name = "Rotate 270 CW")]
			Rotate270Cw = 8
		}

		/// <summary>
		/// The logic order of the rotation. Used when rotate relative. And after the last one it starts with the first item.
		/// </summary>
		private readonly List<Rotation> _orderRotation = new List<Rotation>
		{
			Rotation.Horizontal,
			Rotation.Rotate90Cw,
			Rotation.Rotate180,
			Rotation.Rotate270Cw
		};

		/// <summary>
		/// Determines whether [is relative orientation] [the specified rotate clock].
		/// </summary>
		/// <param name="rotateClock">The rotate clock.</param>
		/// <returns>
		///   <c>true</c> if [is relative orientation] [the specified rotate clock]; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsRelativeOrientation(int rotateClock)
		{
			return rotateClock == -1 || rotateClock == 1; // rotateClock == -1 || rotateClock == 1 true
		}

		/// <summary>
		/// Sets the relative orientation.
		/// </summary>
		/// <param name="relativeRotation">The relative rotation.</param>
		public void SetRelativeOrientation(int relativeRotation = 0)
		{
			Orientation = RelativeOrientation(relativeRotation);
		}

		/// <summary>
		/// Use a relative value to get the next orientation value
		/// </summary>
		/// <param name="relativeRotation">The relative rotation. +1 left, -1 right</param>
		/// <returns>enum value of the output rotation</returns>
		public Rotation RelativeOrientation(int relativeRotation = 0)
		{
			if (Orientation == Rotation.DoNotChange) Orientation = Rotation.Horizontal;
            
			var currentOrientation = _orderRotation.FindIndex(i => i == Orientation);
            
			if (currentOrientation >= 0 && currentOrientation+relativeRotation < 
				_orderRotation.Count && currentOrientation+relativeRotation >= 0)
			{
				return _orderRotation[currentOrientation + relativeRotation];
			}
			if (currentOrientation + relativeRotation == -1) {
				// ReSharper disable once UseIndexFromEndExpression
				return _orderRotation[_orderRotation.Count-1]; //changed
			}
			return _orderRotation[0];
		}

		/// <summary>
		/// Sets the absolute orientation value for rotation.
		/// </summary>
		/// <param name="orientationString">The orientation string (used by exif 1,6,8)</param>
		/// <returns></returns>
		public Rotation SetAbsoluteOrientation(string orientationString = "0")
		{

			switch (orientationString)
			{
				case "1":
					Orientation = Rotation.Horizontal;
					return Orientation;
				case "6":
					Orientation = Rotation.Rotate90Cw;
					return Orientation;
				case "3":
					Orientation = Rotation.Rotate180;
					return Orientation;
				case "8":
					Orientation = Rotation.Rotate270Cw;
					return Orientation;
				default: // Fallback by for example 'null'
					Orientation = Rotation.DoNotChange;
					return Orientation;
			}
		}

		/// <summary>
		/// Gets or sets the width of the image. (saved as ushort 0-65535)
		/// Side effect: when rotating; this may not be updated
		/// </summary>
		/// <value>
		/// The width of the image.
		/// </value>
		public ushort ImageWidth { get; set; }

		/// <summary>
		/// Gets or sets the height of the image. (saved as ushort 0-65535)
		///  Side effect: when rotating; this may not be updated
		/// </summary>
		/// <value>
		/// The height of the image.
		/// </value>
		public ushort ImageHeight { get; set; }

		/// <summary>
		/// Sets the width of the image. (saved as ushort 0-65535)
		/// </summary> 
		/// <param name="imageWidth">Width of the image.</param>
		public void SetImageWidth(string imageWidth)
		{
			if ( int.TryParse(imageWidth, out var parsedInt) )
			{
				SetImageWidth(parsedInt);
			}
		}

		/// <summary>
		/// Sets the width of the image. (saved as ushort 0-65535)
		/// </summary>
		/// <param name="imageWidth">Width of the image.</param>
		public void SetImageWidth(int imageWidth)
		{
			if(imageWidth is >= 1 and <= ushort.MaxValue ) 
				ImageWidth = (ushort) imageWidth;
		}

		/// <summary>
		/// Sets the height of the image. (saved as ushort 0-65535)
		/// </summary>
		/// <param name="imageHeight">Height of the image.</param>
		public void SetImageHeight(string imageHeight)
		{
			if ( int.TryParse(imageHeight, out var parsedInt) )
			{
				SetImageHeight(parsedInt);
			}
		}

		/// <summary>
		/// Sets the height of the image. (saved as ushort 0-65535)
		/// </summary>
		/// <param name="imageHeight">Height of the image.</param>
		public void SetImageHeight(int imageHeight)
		{
			if(imageHeight is >= 1 and <= ushort.MaxValue ) 
				ImageHeight = (ushort) imageHeight;
		}
		

		/// <summary>
		/// Gets or sets the image format. (eg: jpg, tiff)
		/// There are types for: notfound = -1, and	unknown = 0,
		/// </summary>
		/// <value>
		/// The image format as enum item
		/// </value>
		[JsonConverter(typeof(JsonStringEnumConverter))]
		// newtonsoft uses: StringEnumConverter
		public ExtensionRolesHelper.ImageFormat ImageFormat { get; set; }

		/// <summary>
		/// To show location of files with the same Filename without extension
		/// </summary>
		/// <value>
		/// The collection paths, relative to the database (subPath style)
		/// </value>
		[NotMapped]
		public List<string> CollectionPaths { get; set; } = new List<string>();

		/// <summary>
		/// Database string to store sidecar paths
		/// </summary>
		[JsonIgnore]
		public string? SidecarExtensions { get; set; } = "";

		/// <summary>
		/// List of Sidecar files
		/// </summary>
		[NotMapped]
		public HashSet<string> SidecarExtensionsList
		{
			get
			{
				if ( string.IsNullOrWhiteSpace(SidecarExtensions) ) return new HashSet<string>();
				return 
					new HashSet<string>(
						SidecarExtensions.Split('|')
							.Where(p => !string.IsNullOrWhiteSpace(p)));
			}
		}
		
		/// <summary>
		/// Add extensions without dot
		/// </summary>
		/// <param name="ext">ext without dot</param>
		public void AddSidecarExtension(string ext)
		{
			var current = SidecarExtensionsList;
			current.Add(ext);
			SidecarExtensions = string.Join("|", current);
		}
		
		/// <summary>
		/// Remove extensions without dot
		/// </summary>
		/// <param name="ext">ext without dot</param>
		public void RemoveSidecarExtension(string ext)
		{
			var current = SidecarExtensionsList;
			current.Remove(ext);
			SidecarExtensions = string.Join("|", current);
		}

		
		/// <summary>
		/// Duplicate this item in memory.
		/// </summary>
		/// <returns>FileIndexItem duplicated</returns>
		public FileIndexItem Clone()
		{
			return (FileIndexItem) MemberwiseClone();
		}

		/// <summary>
		/// Internal API: To store aperture
		/// </summary>
		private double _aperture;

		/// <summary>
		/// Gets or sets the aperture. (round to 5 decimals)
		/// </summary>
		/// <value>
		/// The aperture.
		/// </value>
		public double Aperture {
			get => Math.Round(_aperture,5);
			set => _aperture = Math.Round(value, 5);
		}

		/// <summary>
		/// Private field to store ShutterSpeed Data
		/// </summary>
		private string _shutterSpeed = string.Empty;
	    
		/// <summary>
		/// Shutter speed as string so '1/2' or '1' 
		/// </summary>
		/// <value>
		/// The shutter speed as string
		/// </value>
		[MaxLength(20)]
		public string? ShutterSpeed {
			get => string.IsNullOrEmpty(_shutterSpeed) ? string.Empty : _shutterSpeed;
			set
			{
				if ( string.IsNullOrEmpty(_shutterSpeed) ) _shutterSpeed = string.Empty;
				if ( value?.Length <= 20 ) _shutterSpeed = value;
			}
		}
	    
		/// <summary>
		/// Gets or sets the iso speed used by cameras  (saved as ushort 0-65535)
		/// </summary>
		/// <value>
		/// The iso speed.
		/// </value>
		public ushort IsoSpeed { get; set; }

		/// <summary>
		/// Sets the iso speed. (saved as ushort 0-65535)
		/// </summary>
		/// <param name="isoSpeed">The iso speed.</param>
		public void SetIsoSpeed(string isoSpeed)
		{
			if ( int.TryParse(isoSpeed, out var parsedInt) )
			{
				SetIsoSpeed(parsedInt);
			}
		}

		/// <summary>
		/// Sets the iso speed. (saved as ushort 0-65535)
		/// </summary>
		/// <param name="isoSpeed">The iso speed.</param>
		public void SetIsoSpeed(int isoSpeed)
		{
			if(isoSpeed is >= 1 and <= ushort.MaxValue ) 
				IsoSpeed = (ushort) isoSpeed;
		}
		
		/// <summary>
		/// Private field to store Software Data
		/// </summary>
		private string _softwareData = string.Empty;

		/// <summary>
		/// Edited with this program
		/// </summary>
		[MaxLength(40)]
		public string? Software {
			get => string.IsNullOrEmpty(_softwareData) ? string.Empty : _softwareData;
			set => _softwareData = string.IsNullOrEmpty(value) ? string.Empty : value;
		}

		/// <summary>
		/// Private field to store MakeModel Data
		/// </summary>
		private string? _makeModel = string.Empty;

		/// <summary>
		/// Camera Make, Model and Lens (saved pipe separated string)
		/// </summary>
		/// <value>
		/// Camera brand and type (incl. lens)
		/// </value>
		public string? MakeModel {
			get => string.IsNullOrEmpty(_makeModel) ? string.Empty : _makeModel;
			// ReSharper disable once PropertyCanBeMadeInitOnly.Global
			set => _makeModel = string.IsNullOrEmpty(value) ? string.Empty : value;
		}

		/// <summary>
		/// Internal API: to [0,0,0]
		/// </summary>
		private const int MakeModelFixedLength = 3;


		/// <summary>
		/// Get the Make (Camera Brand) of the _makeModel 
		/// </summary>
		[NotMapped]
		public string Make
		{
			get
			{
				if ( string.IsNullOrEmpty(_makeModel) ) return string.Empty;
				var makeModelList = MakeModel?.Split("|".ToCharArray());
				if ( makeModelList?.Length != MakeModelFixedLength ) return string.Empty;
				return makeModelList[0];
			}
		}

		/// <summary>
		/// Get the Model (Camera Model) of the _makeModel 
		/// </summary>
		[NotMapped]
		public string Model
		{
			get
			{
				if ( string.IsNullOrEmpty(_makeModel) ) return string.Empty;
				var makeModelList = MakeModel?.Split("|".ToCharArray());
				return makeModelList?.Length != MakeModelFixedLength ? string.Empty : makeModelList[1];
			}
		}
	    
		/// <summary>
		/// Get the Lens info (from MakeModel)
		/// </summary>
		[NotMapped]
		public string LensModel
		{
			get
			{
				if ( string.IsNullOrEmpty(_makeModel) ) return string.Empty;
				var makeModelList = MakeModel?.Split("|".ToCharArray());
				if( makeModelList?.Length != MakeModelFixedLength ) return string.Empty;
				// ReSharper disable once ConvertIfStatementToReturnStatement
				if ( string.IsNullOrEmpty(Model) ) return makeModelList[2];
				// It replaces the Camera Model in the lens
				var lensModel = makeModelList[2].Replace(Model,string.Empty).Trim();
				return lensModel;
			}
		}
	    
	    
		/// <summary>
		/// The Zoom of the camera (that is currently used)
		/// </summary>
		public double FocalLength { get; set; }

		/// <summary>
		/// Private field to store the ByteSize Data
		/// </summary>
		private long _size;
		
		/// <summary>
		/// Size of the file in bytes (BigInt)
		/// </summary>
		public long Size
		{
			get => _size;
			set {
				if ( value < 0 )
				{
					_size = 0;
					return;
				}
				_size = value;
			} 
		}

		/// <summary>
		/// To add Make (without comma and TitleCase) and second follow by Model (same as input)
		/// Followed by index 2: Lens info
		/// (
		///		Example Data: Sony|ILCE-6600|E 18-200mm F3.5-6.3 OSS LE
		///		Index 1: Sony
		///		Index 2: ILCE-6600
		///		Index 3: E 18-200mm F3.5-6.3 OSS LE
		/// )
		/// </summary>
		/// <param name="addedValue">the text to add</param>
		/// <param name="fieldIndex">the indexer of the array (the place in the array)</param>
		public void SetMakeModel(string addedValue, int fieldIndex)
		{
			if ( string.IsNullOrWhiteSpace(addedValue) || addedValue == "----" )
			{
				return;
			}
			if ( fieldIndex > MakeModelFixedLength ) throw new AggregateException("index is higher than MakeModelFixedLength");

			var titleValue = addedValue.Replace("|", string.Empty);

			var makeModelList = _makeModel?.Split("|".ToCharArray()).ToList();
			if ( makeModelList?.Count != MakeModelFixedLength )
			{
				makeModelList = new List<string>();
				for ( var i = 0; i < MakeModelFixedLength; i++ )
				{
					makeModelList.Add(string.Empty);
				}
			}

			makeModelList[fieldIndex] = titleValue;

			// Store Make: APPLE as Apple in the database
			if ( fieldIndex == 0 ) makeModelList[fieldIndex] = 
				CultureInfo.InvariantCulture.TextInfo.ToTitleCase(titleValue.ToLowerInvariant());

			_makeModel = FixedListToString(makeModelList);
		}

		/// <summary>
		/// Convert list to fixed length string
		/// </summary>
		/// <param name="listKeywords">list</param>
		/// <returns>string with fixed length</returns>
		internal static string FixedListToString(IReadOnlyList<string>? listKeywords)
		{
			if ( listKeywords == null )
			{
				return string.Empty;
			}

			var toBeAddedKeywordsStringBuilder = new StringBuilder();

			for ( var i = 0; i < listKeywords.Count; i++ )
			{
				var keyword = listKeywords[i];

				if ( i != listKeywords.Count - 1 )
				{
					toBeAddedKeywordsStringBuilder.Append(keyword + "|");
				}
				else
				{
					toBeAddedKeywordsStringBuilder.Append(keyword);
				}
			}

			var toBeAddedKeywords = toBeAddedKeywordsStringBuilder.ToString();

			return toBeAddedKeywords;
		}
		
		/// <summary>
		/// Is ImageStabilisation unknown, on or off
		/// Currently Sony only
		/// </summary>
		// newtonsoft uses: StringEnumConverter
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public ImageStabilisationType ImageStabilisation { get; set; }

		/// <summary>
		/// Fields that are changed in the last request
		/// </summary>
		[NotMapped]
		public List<string> LastChanged { get; set; } = new List<string>();

	}

	// end class
	
	
    
//	Make,
//	CameraModelName,
//	LensInfo,
//	LensModel,
//	Aperture,
//	ShutterSpeed,
//	ExposureMode
}
