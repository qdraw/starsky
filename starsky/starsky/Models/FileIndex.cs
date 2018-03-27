using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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

        public string ParentDirectory { get; set; }

        public bool IsDirectory { get; set; }

        public string Tags { get; set; }

        public string Description { get; set; }

        public DateTime DateTime { get; set; }

        public DateTime AddToDatabase { get; set; }
        
        private Color _colorClass;

        public Color SetColorClass(string colorclassString = "-1")
        {

            switch (colorclassString)
            {
                case "0":
                    _colorClass = Color.None;
                    return _colorClass;
                case "1":
                    _colorClass = Color.Trash;
                    return _colorClass;
                case "2":
                    _colorClass = Color.Extras;
                    return _colorClass;
                case "3":
                    _colorClass = Color.TypicalAlt;
                    return _colorClass;
                case "4":
                    _colorClass = Color.Typical;
                    return _colorClass;
                case "5":
                    _colorClass = Color.SuperiorAlt;
                    return _colorClass;
                case "6":
                    _colorClass = Color.Superior;
                    return _colorClass;
                case "7":
                    _colorClass = Color.WinnerAlt;
                    return _colorClass;
                case "8":
                    _colorClass = Color.Winner;
                    return _colorClass;
                default:
                    _colorClass = Color.DoNotChange;
                    return _colorClass;
            }
        }

        public List<Color> GetColorClassList(string colorclassString = null)
        {
            if (string.IsNullOrWhiteSpace(colorclassString)) return null;


            var colorclassStringList = new List<string>();
            if (!colorclassString.Contains(","))
            {
                if (!int.TryParse(colorclassString, out var parsedInt)) return null;
                colorclassStringList.Add(parsedInt.ToString());
            }
            if (colorclassString.Contains(",")) {
                colorclassStringList = colorclassString.Split(",").ToList();
            }
            var colorclassList = new List<Color>();
            foreach (var colorclassStringItem in colorclassStringList)
            {
                colorclassList.Add(SetColorClass(colorclassStringItem));
            }
            return colorclassList;
        }


        public Color ColorClass { 
            get => _colorClass;
            set => _colorClass = value;
        }

        public enum Color
        {
            Winner = 8, // Paars - purple
            WinnerAlt = 7, // rood - Red -
            Superior = 6, // Oranje - orange
            SuperiorAlt = 5, //Geel - yellow
            Typical = 4, // Groen - groen
            TypicalAlt = 3, // Turquoise
            Extras = 2, // Blauw - blue
            Trash = 1, // grijs - Grey
            None = 0, // donkergrijs Dark Grey
            DoNotChange = -1
        }
        
        public static IEnumerable<Color> GetAllColor()
        {
            return Enum.GetValues(typeof(Color)).Cast<Color>().Where(p => (int)p >= 0).OrderByDescending(p => (int)p );
        }
        
        public enum ColorUserInterface
        {
            Paars = 8, // Paars - purple
            Rood = 7, // rood - Red -
            Oranje = 6, // Oranje - orange
            Geel = 5, //Geel - yellow
            Groen = 4, // Groen - groen
            Turquoise = 3, // Turquoise
            Blauw = 2, // Blauw - blue
            Grijs = 1, // grijs - Grey
            GeenKleur = 0, // donkergrijs Dark Grey
            DoNotChange = -1
        }

        public static IEnumerable<ColorUserInterface> GetAllColorUserInterface()
        {
            return Enum.GetValues(typeof(ColorUserInterface)).Cast<ColorUserInterface>().Where(p => (int)p >= 0).OrderByDescending(p => (int)p );
        }
        

        // From System full path => database relative path
        public string FullPathToDatabaseStyle() //PathToSys
        {
            return FullPathToDatabaseStyle(FilePath);
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
        public static string DatabasePathToFilePath(string databaseFilePath)
        {
            var filepath = AppSettingsProvider.BasePath + databaseFilePath;

            filepath = _pathToFilePathStyle(filepath);

            var fileexist = File.Exists(filepath) ? filepath : null;
            if (fileexist != null)
            {
                return fileexist;
            }
            return Directory.Exists(filepath) ? filepath : null;
        }

        public string DatabasePathToFilePath()
        {
            return DatabasePathToFilePath(FilePath);
        }
    }
}
