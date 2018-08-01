﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using starsky.Helpers;
using starsky.Services;

namespace starsky.Models
{
    public class FileIndexItem
    {
        public int Id { get; set; }


        private string _filePath { get; set; }
        [Column(Order = 2)]
        public string FilePath
        {
            get { return ConfigRead.RemoveLatestSlash(ParentDirectory) + ConfigRead.PrefixDbSlash(FileName); }
            set {_filePath = ConfigRead.RemoveLatestSlash(ParentDirectory) + ConfigRead.PrefixDbSlash(FileName);} // For legacy reasons
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
        
        public Files.ImageFormat ImageFormat { get; set; }

        
    }
}
