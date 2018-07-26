using System;
using System.IO;
using System.Reflection;
using starsky.Services;

namespace starsky.Models
{
    public class AppSettings
    {
        public string BaseDirectoryProject => AppDomain.CurrentDomain.BaseDirectory
            .Replace("starsky-cli", "starsky")
            .Replace("starskyimportercli", "starsky");


        public string StorageFolder { get; set; }
        public bool Verbose { get; set; }
                
        
        // Database

        public DatabaseTypeList DatabaseType { get; set; }
        public enum DatabaseTypeList
        {
            Mysql = 1,
            Sqlite = 2,
            InMemoryDatabase = 3
        }
        
        // DatabaseType > above this one
        private  string _databaseConnection;
        public  string DatabaseConnection
        {
            get { return _databaseConnection; }
            set
            {
                _databaseConnection = SqliteFullPath(value);
            }
        }

        
        private string _structure;
        public string Structure
        {
            get
            {
                if (string.IsNullOrEmpty(_structure))
                {
                    //   - dd 	            The day of the month, from 01 through 31.
                    //   - MM 	            The month, from 01 through 12.
                    //   - yyyy 	        The year as a four-digit number.
                    //   - HH 	            The hour, using a 24-hour clock from 00 to 23.
                    //   - mm 	            The minute, from 00 through 59.
                    //   - ss 	            The second, from 00 through 59.
                    //   - https://docs.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings
                    //   - \\               (double escape sign or double backslash); to escape dd use this: \\d\\d 
                    //   - /                (slash); is split in folder (Windows / Linux / Mac)
                    //   - .ext             (dot ext); extension for example: .jpg
                    //   - (nothing)        extension is forced
                    //   - {filenamebase}   use the orginal filename without extension
                    //   - *                (asterisk); match anything
                    //   - *starksy*        Match the folder match that contains the word 'starksy'
                    
                    
                    //    Please update /starskyimportercli/readme.md when this changes
                    
                    return "/yyyy/MM/yyyy_MM_dd/yyyyMMdd_HHmmss_{filenamebase}.ext";
                }
                return _structure;
            }
            set // using Json importer
            {
                if (string.IsNullOrEmpty(value) || value == "/") return;
                var structure = ConfigRead.PrefixDbSlash(value);
                _structure = ConfigRead.RemoveLatestBackslash(structure);
            }
        }
        
        
         // Replaces a SQLite url with a full path in the connection string
        public string SqliteFullPath(string connectionString, string fullDbPath = null)
        {
            if (string.IsNullOrWhiteSpace(connectionString)) return connectionString;
//            if(string.IsNullOrWhiteSpace(connectionString)) throw new ArgumentException(">> Connection string IsNullOrWhiteSpace ");
            if(DatabaseType != DatabaseTypeList.Sqlite) return connectionString; // mysql does not need this
            if(Verbose) Console.WriteLine(connectionString);
            if(!connectionString.Contains("Data Source=")) throw new ArgumentException("missing Data Source in connection string");
            var databaseFileName = connectionString.Replace("Data Source=", "");
            // Check if path is not absolute already
            if (databaseFileName.Contains("/") || databaseFileName.Contains("\\")) return connectionString;

            // Overwrite to default when not using the fullDbPath option
            if (fullDbPath == null)
            {
                fullDbPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "data.db");
            }

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