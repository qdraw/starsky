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

        // public Color ColorClass { get; set; }

        public enum Color
        {
            None = 0, // donkergrijs Dark Grey
            Trash = 1, // grijs - Grey
            Extras = 2, // Blauw - blue
            TypicalAlt = 3, // Turquoise
            Typical = 4, // Groen - groen
            SuperiorAlt = 5, //Geel - yellow
            Superior = 6, // Oranje - orange
            WinnerAlt = 7, // rood - Red -
            Winner = 8 // Paars - purple
        }

         /*
         0:1:0:-00001
         0:2:0:-00001
         0:3:0:-00001
         0:4:0:-00001
         0:5:0:-00001
         0:7:0:-00001
         0:8:0:-00001*/


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
