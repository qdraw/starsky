using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace starsky.Models
{
    public static class AppSettingsProvider
    {
        public static string BasePath { get; set; }

        public static DatabaseTypeList DatabaseType { get; set; }
        public static string ThumbnailTempFolder { get; set; }
        public static string ExifToolPath { get; set; }
        public static bool Verbose { get; set; }

        private static string _dbConnectionString;
        public static string DbConnectionString
        {
            get { return _dbConnectionString; }
            set
            {
                _dbConnectionString = SqliteFullPath(value);
            }
        }
        public static List<string> ReadOnlyFolders { get; set; }

        // doing the same trick as in 
        // todo: merge with: BasePathConfig.cs
        private static string _structure;
        public static string Structure
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_structure))
                {
                    return _structure;
                }
                return new BasePathConfig().Structure;
            }
            set
            {
                _structure = new BasePathConfig().Structure = value;
            }
        }

        public enum DatabaseTypeList
        {
            mysql = 1,
            sqlite = 2,
            inmemorydatabase = 3
        }
        
        
        private static string SqliteFullPath(string connectionString)
        {
            if(string.IsNullOrWhiteSpace(connectionString)) throw new ArgumentException(">> Connection string IsNullOrWhiteSpace ");
            if(DatabaseType != DatabaseTypeList.sqlite) return connectionString; // mysql does not need this
            if(Verbose) Console.WriteLine(connectionString);
            if(!connectionString.Contains("Data Source=")) throw new ArgumentException("missing Data Source in connection string");
            var databaseFileName = connectionString.Replace("Data Source=", "");
            // Check if path is not absolute already
            if (databaseFileName.Contains("/") || databaseFileName.Contains("\\")) return connectionString;
          
            var fullDbPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "data.db");

            // Return if running in Microsoft.EntityFrameworkCore.Sqlite (location is now root folder)
            if(fullDbPath.Contains("entityframeworkcore")) return connectionString;
            
            // Replace cli database ==> normal database
            if (fullDbPath.Contains(Path.DirectorySeparatorChar + "starsky-cli" + Path.DirectorySeparatorChar ))
            {
                fullDbPath = fullDbPath.Replace(
                    Path.DirectorySeparatorChar + "starsky-cli" + Path.DirectorySeparatorChar,
                    Path.DirectorySeparatorChar + "starsky" + Path.DirectorySeparatorChar);
            }
            
            // Replace starskyimportercli database ==> normal database
            if (fullDbPath.Contains(Path.DirectorySeparatorChar + "starskyimportercli" + Path.DirectorySeparatorChar ))
            {
                fullDbPath = fullDbPath.Replace(
                    Path.DirectorySeparatorChar + "starskyimportercli" + Path.DirectorySeparatorChar,
                    Path.DirectorySeparatorChar + "starsky" + Path.DirectorySeparatorChar);
            }

            var datasource = "Data Source=" + fullDbPath;
            if(Verbose) Console.WriteLine(datasource);
            return datasource;
        }

        
    }
}
