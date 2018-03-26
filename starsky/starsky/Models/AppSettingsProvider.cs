using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace starsky.Models
{
    public static class AppSettingsProvider
    {
        public static string DbConnectionString { get; set; }
        public static string BasePath { get; set; }

        public static DatabaseTypeList DatabaseType { get; set; }
        public static string ThumbnailTempFolder { get; set; }
        public static string ExifToolPath { get; set; }
        public static bool Verbose { get; set; }

        public enum DatabaseTypeList
        {
            Mysql = 1,
            Sqlite = 2
        }

        
    }
}
