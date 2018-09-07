using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using starsky.Helpers;
using starsky.Services;

namespace starsky.Models
{
    public class FileIndexItem
    {
        public int Id { get; set; }

        private string _filePath { get; set; } = string.Empty;
        [Column(Order = 2)]
        public string FilePath
        {
            get { return ConfigRead.RemoveLatestSlash(ParentDirectory) + ConfigRead.PrefixDbSlash(FileName); }
            set
            {
                // For legacy reasons
                _filePath = ConfigRead.RemoveLatestSlash(ParentDirectory) + ConfigRead.PrefixDbSlash(FileName);
            } 
        }

        public void SetFilePath(string value)
        {
                _parentDirectory = Breadcrumbs.BreadcrumbHelper(value).LastOrDefault();
                _fileName = value.Replace(Breadcrumbs.BreadcrumbHelper(value).LastOrDefault(),string.Empty);
        }
        
        // Do not save null in database for FileName
        private string _fileName;
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

        public string FileHash { get; set; }
        
        [NotMapped]
        public string FileCollectionName {
            get
            {
                return Path.GetFileNameWithoutExtension(FileName);
            } 
        }
        // Do not save null in database for Parent Directory
        private string _parentDirectory;
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
        
        public bool IsDirectory { get; set; }

        [NotMapped]
        public HashSet<string> Keywords {
            get => HashSetHelper.StringToHashSet(_tags);
            set => _tags = HashSetHelper.HashSetToString(value);
        }

        
        // Do not save null in database for tags
        private string _tags;
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
        
        // Used to display file status
        public enum ExifStatus
        {
            Default,
            NotFoundNotInIndex,
            NotFoundSourceMissing,
            NotFoundIsDir,
            ReadOnly,
            Ok,
        }

        [JsonConverter(typeof(StringEnumConverter))]
        [NotMapped]
        public ExifStatus Status { get; set; } = ExifStatus.Default;
        

        [System.ComponentModel.DefaultValue("")]
        // add default value (6#+)
        public string Description { get; set; } = string.Empty;
        
        // Do not save null in database for Title
        private string _title;
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
        
        public DateTime DateTime { get; set; }

        public DateTime AddToDatabase { get; set; }
        
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        
        

        
        
        private Color _colorClass;

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
                colorclassStringList = colorclassString.Split(",").ToList();
            }
            var colorclassList = new HashSet<Color>();
            foreach (var colorclassStringItem in colorclassStringList)
            {
                colorclassList.Add(GetColorClass(colorclassStringItem));
            }
            return colorclassList.ToList();
        }

        
        // Creat an List of all String based database fields
        public List<string> FileIndexPropList()
        {
            var fileIndexPropList = new List<string>();
            // only for types String in FileIndexItem()

            foreach (var propertyInfo in new FileIndexItem().GetType().GetProperties())
            {
                if (propertyInfo.PropertyType == typeof(string) && propertyInfo.CanRead)
                {
                    fileIndexPropList.Add(propertyInfo.Name);
                }
            }
            return fileIndexPropList;
        }

      
        // Always display int, because the Update api uses ints to parse
        public Color ColorClass { 
            get => _colorClass == Color.DoNotChange ? Color.None : _colorClass;
            set
            {
                if (value == Color.DoNotChange) return;
                _colorClass = value;
            }
        }


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


        [JsonConverter(typeof(StringEnumConverter))]
        public Rotation Orientation { get; set; } = Rotation.DoNotChange;

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

        // More than 1 and order by logic order instead of exif order
        private readonly List<Rotation> _orderRotation = new List<Rotation>
        {
            Rotation.Horizontal,
            Rotation.Rotate90Cw,
            Rotation.Rotate180,
            Rotation.Rotate270Cw
        };

        public static bool IsRelativeOrientation(int rotateClock)
        {
            return rotateClock == -1 || rotateClock == 1; // rotateClock == -1 || rotateClock == 1 true
        }

        public void SetRelativeOrientation(int relativeRotation = 0)
        {
            Orientation = RelativeOrientation(relativeRotation);
        }
        
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
        
        // Set Rotation value
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

        // Unexpeted side effect: when rotating; this may not be updated
        // type > ushort 0-65535
        public ushort ImageWidth { get; set; }
        public ushort ImageHeight { get; set; }

        public void SetImageWidth(string imageWidth)
        {
            int.TryParse(imageWidth, out var parsedInt);
            SetImageWidth(parsedInt);
        }

        public void SetImageWidth(int imageWidth)
        {
            if(imageWidth >= 1 && imageWidth <= ushort.MaxValue ) 
                ImageWidth = (ushort) imageWidth;
        }

        public void SetImageHeight(string imageHeight)
        {
            int.TryParse(imageHeight, out var parsedInt);
            SetImageHeight(parsedInt);
        }
        
        public void SetImageHeight(int imageHeight)
        {
            if(imageHeight >= 1 && imageHeight <= ushort.MaxValue ) 
                ImageHeight = (ushort) imageHeight;
        }
        
        

        public static string GetDisplayName(Enum enumValue)
        {
            var name = enumValue.GetType()?
                .GetMember(enumValue.ToString())?
                .First()?
                .GetCustomAttribute<DisplayAttribute>()?
                .Name;
            return name;
        }
        
      
        public static IEnumerable<Color> GetAllColor()
        {
            return Enum.GetValues(typeof(Color)).Cast<Color>().Where(p => (int)p >= 0).OrderBy(p => (int)p );
        }
        
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

        public static IEnumerable<ColorUserInterface> GetAllColorUserInterface()
        {
            return Enum.GetValues(typeof(ColorUserInterface)).Cast<ColorUserInterface>().Where(p => (int)p >= 0).OrderBy(p => (int)p );
        }
        
        [JsonConverter(typeof(StringEnumConverter))]
        public Files.ImageFormat ImageFormat { get; set; }
        
        [NotMapped]
        public List<string> CollectionPaths { get; set; } = new List<string>();

        public FileIndexItem Clone()
        {
            return (FileIndexItem) MemberwiseClone();
        }
    }
}
