using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using starskycore.Helpers;
using starskycore.Services;

namespace starskycore.Models
{
    public class FileIndexItem
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
		public int Id { get; set; }

	    /// <summary>
	    /// Internal API for storing full file path's
	    /// </summary>
        private string FilePathPrivate { get; set; } = string.Empty;

		/// <summary>
		/// Get a concatenated subpath style filepath to find the location
		/// </summary>
		/// <value>
		/// The file path.
		/// </value>
		[Column(Order = 2)]
        public string FilePath
        {
            get { return PathHelper.RemoveLatestSlash(ParentDirectory) + PathHelper.PrefixDbSlash(FileName); }
            set
            {
                // For legacy reasons
                FilePathPrivate = PathHelper.RemoveLatestSlash(ParentDirectory) + PathHelper.PrefixDbSlash(FileName);
            } 
        }

		/// <summary>
		/// Sets the file path as Filename and ParentDirectory
		/// </summary>
		/// <param name="value">The value.</param>
		public void SetFilePath(string value)
        {
	        _parentDirectory = Breadcrumbs.BreadcrumbHelper(value).LastOrDefault();
	        
			_fileName = PathHelper.GetFileName(value);
			// filenames are without starting slash
	        _fileName = PathHelper.RemovePrefixDbSlash(_fileName);
        }

	    /// <summary>
	    /// Internal API: Do not save null in database for FileName
	    /// </summary>
        private string _fileName;

		/// <summary>
		/// Get or Set FileName with extension
		/// Without first slash or path
		/// </summary>
		/// <value>
		/// The name of the file with extension
		/// </value>
		/// <example>/folder/image.jpg</example>
		[Column(Order = 1)]
        public string FileName
        {
            get => _fileName ?? string.Empty;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    _fileName = string.Empty;
                    return;
                }
                _fileName = value;
            }
        }

		/// <summary>
		/// Base32 hash to identify this file. Gets or sets the file hash.
		/// </summary>
		/// <value>
		/// The file hash.
		/// </value>
		/// <example>OZHCK4I47QPHOT53QBRE7Z4RLI</example>
		public string FileHash { get; set; }

		/// <summary>
		/// GetFileNameWithoutExtension
		/// </summary>
		/// <value>
		/// The name of the file collection.
		/// </value>
		/// <example>filenameWithoutExtension</example>
		[NotMapped]
        public string FileCollectionName {
            get
            {
                return Path.GetFileNameWithoutExtension(FileName);
            } 
        }

        /// <summary>
        /// Internal API: Do not save null in database for Parent Directory
        /// </summary>
	    private string _parentDirectory;

		/// <summary>
		/// Get/Set Relative path of the parent Directory
		/// </summary>
		/// <value>
		/// The parent directory in subpath style
		/// </value>
		/// <example>/folder</example>
		public string ParentDirectory
        {
            get => _parentDirectory ?? string.Empty;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    _parentDirectory = string.Empty;
                    return;
                }
                _parentDirectory = value;
            }
        }

		/// <summary>
		/// Gets or sets a value indicating whether this instance is directory.
		/// </summary>
		/// <value>
		///   <c>true</c> if this instance is directory; otherwise (then is a file), <c>false</c>.
		/// </value>
		/// <example>true</example>
		public bool IsDirectory { get; set; }

		/// <summary>
		/// Get/Set a HashList with Tags (stores as string under Tags)
		/// </summary>
		/// <value>
		/// The keywords as List/HashList
		/// </value>
		[NotMapped]
        public HashSet<string> Keywords {
			get => HashSetHelper.StringToHashSet(Tags.Trim());
			set
			{
				if (value == null) return;
				_tags = HashSetHelper.HashSetToString(value);
			} 
		}

        
	    /// <summary>
	    /// Internal API: Do not save null in database for tags
	    /// </summary>
        private string _tags;

		/// <summary>
		/// Get/Set a comma separated string of unique tags.
		/// Duplicate keywords are case sensitive removed
		/// </summary>
		/// <value>
		/// The tags.
		/// </value>
		/// <example>tag1, tag2</example>
		public string Tags
        {
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
            Default,
            NotFoundNotInIndex,
            NotFoundSourceMissing,
            NotFoundIsDir,
	        OperationNotSupported,
            DirReadOnly,
            ReadOnly,
            Unauthorized,
            Ok
        }

		/// <summary>
		/// Gets or sets the display file status. (eg. NotFoundNotInIndex, Ok)
		/// </summary>
		/// <value>
		/// The display file status. (eg. NotFoundNotInIndex, Ok).
		/// </value>
		//[JsonConverter(typeof(StringEnumConverter))]
		[JsonConverter(typeof(JsonStringEnumConverter))]
		[NotMapped]
        public ExifStatus Status { get; set; } = ExifStatus.Default;
        
        /// <summary>
        /// Internal API: to store description
        /// </summary>
        private string _description;

		/// <summary>
		/// Gets or sets the iptc:Caption-Abstract or xmp:description.
		/// </summary>
		/// <value>
		/// The description as string
		/// </value>
		public string Description
        {
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
	    /// Internal API: to store Title
	    /// </summary>
        private string _title;

		/// <summary>
		/// Gets or sets the iptc:Object Name or xmp:title.
		/// </summary>
		/// <value>
		/// The title as string
		/// </value>
		public string Title
        {
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
        public string LocationCity { get; set; } = string.Empty;

		/// <summary>
		/// The name of the nearest state or province (Max length is 40 chars)
		/// </summary>
		/// <value>
		/// The state of the location.
		/// </value>
		[MaxLength(40)]
        public string LocationState { get; set; } = string.Empty;

		/// <summary>
		/// The name of the nearest country (Max length is 40 chars)
		/// </summary>
		/// <value>
		/// The location country.
		/// </value>
		[MaxLength(40)] 
        public string LocationCountry { get; set; } = string.Empty;

		/// <summary>
		/// Use a int value to get the ColorClass enum. The number input is between 1 and 8
		/// </summary>
		/// <param name="colorclassString">The colorclass string.</param>
		/// <returns></returns>
		public Color GetColorClass(string colorclassString = "0")
        {

            switch (colorclassString)
            {
                case "0":
                    return Color.None;
                case "8":
                    return  Color.Trash;
                case "7":
                    return Color.Extras;
                case "6":
                    return Color.TypicalAlt;
                case "5":
                    return Color.Typical;
                case "4":
                    return Color.SuperiorAlt;
                case "3":
                    return Color.Superior;
                case "2":
                    return Color.WinnerAlt;
                case "1":
                    return Color.Winner;
                default:
                    return Color.DoNotChange;
            }
        }

		/// <summary>
		/// Comma separated list of color class numbers to create a list of enums
		/// Used to create custom files to sort a combination of color classes
		/// </summary>
		/// <param name="colorclassString">The color class string.</param>
		/// <returns></returns>
		public List<Color> GetColorClassList(string colorclassString = null)
        {
            if (string.IsNullOrWhiteSpace(colorclassString)) return new List<Color>();

            var colorclassStringList = new List<string>();

            if (!colorclassString.Contains(","))
            {
                if (!int.TryParse(colorclassString, out var parsedInt)) return new List<Color>();
                colorclassStringList.Add(parsedInt.ToString());
            }
            if (colorclassString.Contains(",")) {
                colorclassStringList = colorclassString.Split(",".ToCharArray()).ToList();
            }
            var colorclassList = new HashSet<Color>();
            foreach (var colorclassStringItem in colorclassStringList)
            {
                colorclassList.Add(GetColorClass(colorclassStringItem));
            }
            return colorclassList.ToList();
        }


		/// <summary>
		/// Create an List of all String, bool, Datetime, ImageFormat based database fields		
		/// </summary>
		/// <returns> Files the index property list.</returns>
		public List<string> FileIndexPropList()
        {
            var fileIndexPropList = new List<string>();
            // only for types String in FileIndexItem()

            foreach (var propertyInfo in new FileIndexItem().GetType().GetProperties())
            {
                if ((
						propertyInfo.PropertyType == typeof(bool) || 
						propertyInfo.PropertyType == typeof(string) || 
						propertyInfo.PropertyType == typeof(DateTime) ||
						propertyInfo.PropertyType == typeof(ExtensionRolesHelper.ImageFormat)
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
		public Color ColorClass { get; set; } = Color.DoNotChange;

		/// <summary>
		/// ColorClass enum, used to filter images
		/// </summary>
		public enum Color
        {
            // Display name: used in -xmp:Label
            [Display(Name = "Winner")]
            Winner = 1, // Paars - purple
            [Display(Name = "Winner Alt")]
            WinnerAlt = 2, // rood - Red -
            [Display(Name = "Superior")]
            Superior = 3, // Oranje - orange
            [Display(Name = "Superior Alt")]
            SuperiorAlt = 4, //Geel - yellow
            [Display(Name = "Typical")]
            Typical = 5, // Groen - groen
            [Display(Name = "Typical Alt")]
            TypicalAlt = 6, // Turquoise
            [Display(Name = "Extras")]
            Extras = 7, // Blauw - blue
            [Display(Name = "")]
            Trash = 8, // grijs - Grey
            None = 0, // donkergrijs Dark Grey
            DoNotChange = -1
        }

		/// <summary>
		/// Gets or sets the image orientation.
		/// </summary>
		/// <value>
		/// The orientation as enum item
		/// </value>
		//[JsonConverter(typeof(StringEnumConverter))]
		[JsonConverter(typeof(JsonStringEnumConverter))]
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
            
            var currentOrentation = _orderRotation.FindIndex(i => i == Orientation);
            
            if (currentOrentation >= 0 && currentOrentation+relativeRotation < _orderRotation.Count && currentOrentation+relativeRotation >= 0)
            {
                return _orderRotation[currentOrentation + relativeRotation];
            }
            if (currentOrentation + relativeRotation == -1) {
                return _orderRotation[_orderRotation.Count-1]; //changed
            }
            if (currentOrentation+relativeRotation >= _orderRotation.Count) {
                return _orderRotation[0];
            }
            
            return Rotation.DoNotChange;
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
            int.TryParse(imageWidth, out var parsedInt);
            SetImageWidth(parsedInt);
        }

		/// <summary>
		/// Sets the width of the image. (saved as ushort 0-65535)
		/// </summary>
		/// <param name="imageWidth">Width of the image.</param>
		public void SetImageWidth(int imageWidth)
        {
            if(imageWidth >= 1 && imageWidth <= ushort.MaxValue ) 
                ImageWidth = (ushort) imageWidth;
        }

		/// <summary>
		/// Sets the height of the image. (saved as ushort 0-65535)
		/// </summary>
		/// <param name="imageHeight">Height of the image.</param>
		public void SetImageHeight(string imageHeight)
        {
            int.TryParse(imageHeight, out var parsedInt);
            SetImageHeight(parsedInt);
        }

		/// <summary>
		/// Sets the height of the image. (saved as ushort 0-65535)
		/// </summary>
		/// <param name="imageHeight">Height of the image.</param>
		public void SetImageHeight(int imageHeight)
        {
            if(imageHeight >= 1 && imageHeight <= ushort.MaxValue ) 
                ImageHeight = (ushort) imageHeight;
        }


		/// <summary>
		/// Gets all items of the enum color, eg Winner, WinnerAlt.
		/// </summary>
		/// <returns>List with enum-item</returns>
		public static IEnumerable<Color> GetAllColor()
        {
            return Enum.GetValues(typeof(Color)).Cast<Color>().Where(p => (int)p >= 0).OrderBy(p => (int)p );
        }

		/// <summary>
		/// The enum Color in Dutch
		/// </summary>
		public enum ColorUserInterface
        {
            Paars = 1, // Paars - purple
            Rood = 2, // rood - Red -
            Oranje = 3, // Oranje - orange
            Geel = 4, //Geel - yellow
            Groen = 5, // Groen - groen
            Turquoise = 6, // Turquoise
            Blauw = 7, // Blauw - blue
            Grijs = 8, // grijs - Grey
            Geen = 0, // donkergrijs Dark Grey
            DoNotChange = -1
        }

		/// <summary>
		/// Gets all color user interface. In Dutch: Paars, Rood, etc.
		/// </summary>
		/// <returns></returns>
		public static IEnumerable<ColorUserInterface> GetAllColorUserInterface()
        {
            return Enum.GetValues(typeof(ColorUserInterface)).Cast<ColorUserInterface>().Where(p => (int)p >= 0).OrderBy(p => (int)p );
        }

		/// <summary>
		/// Gets or sets the image format. (eg: jpg, tiff)
		/// There are types for: notfound = -1, and	unknown = 0,
		/// </summary>
		/// <value>
		/// The image format as enum item
		/// </value>
		//[JsonConverter(typeof(StringEnumConverter))]
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public ExtensionRolesHelper.ImageFormat ImageFormat { get; set; }

		/// <summary>
		/// To show location of files with the same Filename without extension
		/// </summary>
		/// <value>
		/// The collection paths, relative to the database (subpath style)
		/// </value>
		[NotMapped]
        public List<string> CollectionPaths { get; set; } = new List<string>();

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
	    public string ShutterSpeed {
		    get
		    {
			    if ( string.IsNullOrEmpty(_shutterSpeed) ) return string.Empty;
			    return _shutterSpeed;
		    }
			set
			{
				if ( string.IsNullOrEmpty(_shutterSpeed) ) _shutterSpeed = string.Empty;
				if ( value.Length <= 20 ) _shutterSpeed = value;
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
		    int.TryParse(isoSpeed, out var parsedInt);
		    SetIsoSpeed(parsedInt);
	    }

		/// <summary>
		/// Sets the iso speed. (saved as ushort 0-65535)
		/// </summary>
		/// <param name="isoSpeed">The iso speed.</param>
		public void SetIsoSpeed(int isoSpeed)
	    {
		    if(isoSpeed >= 1 && isoSpeed <= ushort.MaxValue ) 
			    IsoSpeed = (ushort) isoSpeed;
	    }

	    [NotMapped] 
	    public string Software { get; set; }
	    
	    
		/// <summary>
		/// Private field to store MakeModel Data
		/// </summary>
	    private string _makeModel = string.Empty;

	    /// <summary>
	    /// Camera make and Model (saved comma separated String)
	    /// </summary>
	    /// <value>
	    /// Camera brand and type
	    /// </value>
	    public string MakeModel {
		    get
		    {
			    if ( string.IsNullOrEmpty(_makeModel) ) return string.Empty;
			    return _makeModel;
		    }
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
			    var makeModelList = MakeModel.Split("|".ToCharArray());
			    if ( makeModelList.Length != MakeModelFixedLength ) return string.Empty;
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
			    var makeModelList = MakeModel.Split("|".ToCharArray());
				if( makeModelList.Length != MakeModelFixedLength ) return string.Empty;
				return makeModelList[1];
		    }
	    }
	    
	    
	    /// <summary>
	    /// The Zoom of the camera
	    /// </summary>
	    [NotMapped]
	    public double FocalLength { get; set; }

	    
	    /// <summary>
	    /// To add Make (without comma and TitleCase) and second follow by Model (same as input)
	    /// </summary>
	    /// <param name="addedValue">the text to add</param>
	    /// <param name="fieldIndex">the indexer of the array (the place in the array)</param>
	    public void SetMakeModel(string addedValue, int fieldIndex)
	    {
		    if ( fieldIndex > MakeModelFixedLength ) throw new AggregateException("index is higher than MakeModelFixedLength");

			var titleValue = addedValue.Replace("|", string.Empty);

		    var makeModelList = _makeModel.Split("|".ToCharArray()).ToList();
		    if ( makeModelList.Count != MakeModelFixedLength )
		    {
			    makeModelList = new List<string>();
				for ( int i = 0; i < MakeModelFixedLength; i++ )
			    {
					makeModelList.Add(string.Empty);
				}
		    }

			makeModelList[fieldIndex] = titleValue;

			// Store Make: APPLE as Apple in the database
			if ( fieldIndex == 0 ) makeModelList[fieldIndex] = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(titleValue.ToLowerInvariant());

			_makeModel = FixedListToString(makeModelList);
	    }

	    /// <summary>
	    /// Convert list to fixed length string
	    /// </summary>
	    /// <param name="listKeywords">list</param>
	    /// <returns>string with fixed length</returns>
	    private static string FixedListToString(List<string> listKeywords)
	    {

		    if ( listKeywords == null )
		    {
			    return string.Empty;
		    }

		    var toBeAddedKeywordsStringBuilder = new StringBuilder();

		    for ( int i = 0; i < listKeywords.Count; i++ )
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


	} // end class
	
	
    
//	Make,
//	CameraModelName,
//	LensInfo,
//	LensModel,
//	Aperture,
//	ShutterSpeed,
//	ExposureMode
}
