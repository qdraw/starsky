using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using starsky.Services;

namespace starsky.Models
{
    public class FileIndexItem
    {
        public int Id { get; set; }

        [Column(Order = 2)]
        public string FilePath { get; set; }

        [Column(Order = 1)]
        public string FileName { get; set; }

        public string FileHash { get; set; }

        // Do not save null in database for tags
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
                _tags = value;
            }
        }

        [System.ComponentModel.DefaultValue("")]
        public string Description { get; set; }
        
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
                _title = value;
            }
        }
        
        public DateTime DateTime { get; set; }

        public DateTime AddToDatabase { get; set; }
        
        private Color _colorClass;
        
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public Color SetColorClass(string colorclassString = "0")
        {

            switch (colorclassString)
            {
                case "0":
                    _colorClass = Color.None;
                    return _colorClass;
                case "8":
                    _colorClass = Color.Trash;
                    return _colorClass;
                case "7":
                    _colorClass = Color.Extras;
                    return _colorClass;
                case "6":
                    _colorClass = Color.TypicalAlt;
                    return _colorClass;
                case "5":
                    _colorClass = Color.Typical;
                    return _colorClass;
                case "4":
                    _colorClass = Color.SuperiorAlt;
                    return _colorClass;
                case "3":
                    _colorClass = Color.Superior;
                    return _colorClass;
                case "2":
                    _colorClass = Color.WinnerAlt;
                    return _colorClass;
                case "1":
                    _colorClass = Color.Winner;
                    return _colorClass;
                default:
                    _colorClass = Color.DoNotChange;
                    return _colorClass;
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
                colorclassList.Add(SetColorClass(colorclassStringItem));
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

      

        public Color ColorClass { 
            get => _colorClass;
            set => _colorClass = value;
        }


        public enum Color
        {
            Winner = 1, // Paars - purple
            WinnerAlt = 2, // rood - Red -
            Superior = 3, // Oranje - orange
            SuperiorAlt = 4, //Geel - yellow
            Typical = 5, // Groen - groen
            TypicalAlt = 6, // Turquoise
            Extras = 7, // Blauw - blue
            Trash = 8, // grijs - Grey
            None = 0, // donkergrijs Dark Grey
            DoNotChange = -1
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
        
        public static string FullPathToDatabaseStyle(string subpath)
        {
            var databaseFilePath = subpath.Replace(AppSettingsProvider.BasePath, "");
            databaseFilePath = _pathToDatabaseStyle(databaseFilePath);
            return databaseFilePath;
        }
        
        // Replace windows \\ > /
        private static string _pathToDatabaseStyle(string subPath)
        {
            if (Path.DirectorySeparatorChar.ToString() == "\\")
            {
                subPath = subPath.Replace("\\", "/");
            }
            return subPath;
        }

        // Replace windows \\ > /
        private static string _pathToFilePathStyle(string subPath)
        {
            if (Path.DirectorySeparatorChar.ToString() == "\\")
            {
                subPath = subPath.Replace("/", "\\");
            }
            return subPath;
        }


        // from relative database path => file location path 
        public static string DatabasePathToFilePath(string databaseFilePath, bool checkIfExist = true)
        {
            var filepath = AppSettingsProvider.BasePath + databaseFilePath;

            filepath = _pathToFilePathStyle(filepath);

            // Used for deleted files
            if (!checkIfExist) return filepath;
            
            var fileexist = File.Exists(filepath) ? filepath : null;
            if (fileexist != null)
            {
                return fileexist;
            }
            return Directory.Exists(filepath) ? filepath : null;
        }

//        // todo: remove this?
//        public string DatabasePathToFilePath()
//        {
//            return DatabasePathToFilePath(FilePath);
//        }

        // Depends on App Settings for storing values
        // Depends on BasePathConfig for setting default values
        public string ParseFileName()
        {
            if (string.IsNullOrWhiteSpace(FilePath)) return string.Empty;

            var fileExtenstion = Files.GetImageFormat(FilePath).ToString();
            var fileNameStructureList = AppSettingsProvider.StructureFilenamePattern;

            fileNameStructureList = _parseListDateFormat(fileNameStructureList, DateTime);
                
            foreach (var item in fileNameStructureList)
            {
                Regex rgx = new Regex(".ex[A-Z]$");
                var result = rgx.Replace(item, "." + fileExtenstion);
                return result;
            }
            return string.Empty;
        }

        public List<string> ParseSubFolders()
        {
            if (string.IsNullOrWhiteSpace(FilePath)) return new List<string>();
            {
                var directoryStructureList = AppSettingsProvider.StructureDirectoryPattern;

                var subFoldersList = _parseListDateFormat(directoryStructureList, DateTime);
                
                ParseCommentsFolder(subFoldersList);
                
                return subFoldersList;
            }
        }

        public List<string> ParseCommentsFolder(List<string> subFoldersList)
        {
            if (subFoldersList.FirstOrDefault() == "/") return null;

            subFoldersList.Insert(0, string.Empty);
          
            var fullPathBase = DatabasePathToFilePath(subFoldersList.FirstOrDefault(),false);
            var newSubFoldersList = new List<string>();
            foreach (var folder in subFoldersList)
            {
                fullPathBase += folder.Replace("*", string.Empty) + Path.DirectorySeparatorChar;
                if (!folder.Contains("*")
                    || Directory.Exists(fullPathBase))
                {
                    newSubFoldersList.Add(folder);
                    continue;
                }
                
                SearchAsteriskFolder(fullPathBase);

            }

            Console.WriteLine("newSubFoldersList");
            Console.WriteLine(newSubFoldersList);
            return newSubFoldersList;
        }

        public string SearchAsteriskFolder(string fullFolderPath)
        {
            Console.WriteLine(fullFolderPath);
            
//            var parrentFolder = Breadcrumbs.BreadcrumbHelper(fullFolderPath).LastOrDefault();
//            fullFolderPath.Split("/")
//            var t = Directory.GetDirectories(parrentFolder, "*", SearchOption.AllDirectories);
//            Console.WriteLine();
            return null;
        }
        
        


    
        
        private static List<string> _parseListDateFormat(List<string> patternList, DateTime fileDateTime)
        {
            var parseListDate = new List<string>();
            foreach (var patternItem in patternList)
            {
                if (patternItem == "/") return patternList;
                var item = fileDateTime.ToString(patternItem, CultureInfo.InvariantCulture);
                parseListDate.Add(item);
            }
            return parseListDate;
        }
    }
}
