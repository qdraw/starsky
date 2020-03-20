#if SYSTEM_TEXT_ENABLED
using System.Text.Json.Serialization;
#else
using Newtonsoft.Json.Converters;
#endif
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using starsky.foundation.platform.Helpers;
using TimeZoneConverter;

namespace starsky.foundation.platform.Models
{
    public class AppSettings
    {
        public AppSettings()
        {
            ReadOnlyFolders = new List<string>();
            DatabaseConnection = SqLiteFullPath("Data Source=data.db",BaseDirectoryProject);
            
            // Cache for thumbs
            ThumbnailTempFolder = Path.Combine(BaseDirectoryProject, "thumbnailTempFolder");
            if(!Directory.Exists(ThumbnailTempFolder)) Directory.CreateDirectory(ThumbnailTempFolder);

            StorageFolder = Path.Combine(BaseDirectoryProject, "storageFolder");
            if(!Directory.Exists(StorageFolder)) Directory.CreateDirectory(StorageFolder);

            // may be cleaned after restart (not implemented)
            TempFolder = Path.Combine(BaseDirectoryProject, "temp");
            if(!Directory.Exists(TempFolder)) Directory.CreateDirectory(TempFolder);
            
            // AddMemoryCache defaults in prop
        }

	    public string BaseDirectoryProject => AppDomain.CurrentDomain.BaseDirectory
		    .Replace("starskyadmincli", "starsky")
		    .Replace("starskysynccli", "starsky")
		    .Replace("starsky.foundation.database", "starsky")
		    .Replace("starskyImporterNetFrameworkCli", "starsky")
		    .Replace("netframework-msbuild", "starsky")
		    .Replace("starskySyncNetFrameworkCli", "starsky")
		    .Replace("starskyimportercli", "starsky")
		    .Replace("starskywebftpcli", "starsky")
		    .Replace("starskywebhtmlcli", "starsky")
		    .Replace("starskygeocli", "starsky")
		    .Replace("starskytest", "starsky");

#if SYSTEM_TEXT_ENABLED
	    [JsonConverter(typeof(JsonStringEnumConverter))]
#else
	    [JsonConverter(typeof(StringEnumConverter))]
#endif
	    public StarskyAppType ApplicationType { get; set; }

		public enum StarskyAppType
        {
            WebController = 0,
            Importer = 1,
            Sync = 2,
            WebHtml = 3,
            Geo = 4,
	        WebFtp = 5,
	        Admin = 6,
        }

        // Can be used in the cli session to select files out of the file database system
        private string _storageFolder; // in old versions: basePath 
        public string StorageFolder
        {
            get { return _storageFolder; }
            set
            {
                _storageFolder = PathHelper.AddBackslash(value);
            }
        }

        public bool Verbose { get; set; }

        // Used in the webhtmlcli to store the log item name
        // used for the url
        private string _name;
        public string Name
        {
            get => _name ?? "Starsky"; // defaults to this
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    _name = string.Empty;
                    return;
                }
                _name = value;
            }
        }
        
        // Used to template config > appsettingsPubProfile
        public string GetWebSafeReplacedName(string input)
        {
            // Included slash dd the end of this file
            return PathHelper.AddSlash(input.Replace("{name}", GenerateSlug(Name,true)));
        }
        
        /// <summary>
        /// Generates a permalink slug for passed string
        /// </summary>
        /// <param name="phrase"></param>
        /// <param name="allowUnderScore">to allow underscores in slug</param>
        /// <returns>clean slug string (ex. "some-cool-topic")</returns>
        public string GenerateSlug(string phrase, bool allowUnderScore = false)
        {
            var s = phrase.ToLowerInvariant();
            
            var matchNotRegexString = @"[^a-z0-9\s-]";
            if(allowUnderScore) matchNotRegexString = @"[^a-z0-9\s-_]";     // allow underscores
            
            s = Regex.Replace(s,matchNotRegexString, "");                   // remove invalid characters
            s = Regex.Replace(s, @"\s+", " ").Trim();                       // single space
            s = s.Substring(0, s.Length <= 45 ? s.Length : 45).Trim();      // cut and trim
            s = Regex.Replace(s, @"\s", "-");                               // insert hyphens
            return s.ToLower();
        }
        

        /// <summary>
        /// Type of the database, sqlite, mysql or inmemory
        /// </summary>
#if SYSTEM_TEXT_ENABLED
		[JsonConverter(typeof(JsonStringEnumConverter))]
#else
        [JsonConverter(typeof(StringEnumConverter))]
#endif
        public DatabaseTypeList DatabaseType { get; set; } = DatabaseTypeList.Sqlite;


	    public enum DatabaseTypeList
        {
            Mysql = 1,
            Sqlite = 2,
            InMemoryDatabase = 3
        }
        
        // DatabaseType > above this one
        private string _databaseConnection;

        public string DatabaseConnection
        {
            get { return _databaseConnection; }
            set
            {
                _databaseConnection = SqLiteFullPath(value,BaseDirectoryProject);
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
                var structure = PathHelper.PrefixDbSlash(value);
                _structure = PathHelper.RemoveLatestBackslash(structure);
                // Struture regex check
                StructureCheck(_structure);
            }
        }

        // Used for syncing gpx files
        public string CameraTimeZone
        {
            get
            {
                if (CameraTimeZoneInfo == null) return string.Empty; 
                return CameraTimeZoneInfo.Id; 
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    CameraTimeZoneInfo = TimeZoneInfo.Local;
                    return;
                }
                CameraTimeZoneInfo = TZConvert.GetTimeZoneInfo(value); 
            }
        }

        [JsonIgnore]
        public TimeZoneInfo CameraTimeZoneInfo { get; set; }

        // To Check if the structure is any good
        public static void StructureCheck(string structure)
        {
            // Unescaped regex:
            //      ^(\/.+)?\/([\/_ A-Z0-9*{}\.\\-]+(?=\.ext))\.ext$
            
            Regex structureRegex = new Regex( 
                "^(\\/.+)?\\/([\\/_ A-Z0-9*{}\\.\\\\-]+(?=\\.ext))\\.ext$", 
                RegexOptions.IgnoreCase);

            if (structureRegex.Match(structure).Success) return;

            throw new ArgumentException("(StructureCheck) Structure is not confirm regex - " + structure);
        }

        private string _thumbnailTempFolder;
        public string ThumbnailTempFolder
        {
            get => _thumbnailTempFolder;
	        set => _thumbnailTempFolder = PathHelper.AddBackslash(value);
        }
        
        private string _tempFolder;
        public string TempFolder
        {
            get => _tempFolder;
	        set => _tempFolder = PathHelper.AddBackslash(value);
        }


        public string ExifToolPath { get; set; }
        
        // C# 6+ required for this
        public bool ExifToolImportXmpCreate { get; set; } = true; // -x -clean command

	    // fallback in contructor
	    // use env variable: app__ReadOnlyFolders__0 - value
        public List<string> ReadOnlyFolders { get; set; }

        /// <summary>
        /// Is the file read only
        /// </summary>
        /// <param name="f">filepath</param>
        /// <returns>true = don't edit</returns>
        public bool IsReadOnly(string f)
        {
            if (!ReadOnlyFolders.Any() ) return false;
            
            var result = ReadOnlyFolders.FirstOrDefault(f.Contains);
            return result != null;
        }
        
        // C# 6+ required for this
        public bool AddMemoryCache { get; set; } = true;
        
	    /// <summary>
	    /// Display swagger pages
	    /// </summary>
	    public bool AddSwagger { get; set; } = false;

	    /// <summary>
	    /// Export swagger pages (use AddSwagger and AddSwaggerExport to export)
	    /// </summary>	    
	    public bool AddSwaggerExport { get; set; } = false;

	    public bool AddLegacyOverwrite { get; set; } = Type.GetType("Mono.Runtime") != null;
	    
	    private string _webftp; 
	    public string WebFtp
	    {
		    get
		    {
			    if ( string.IsNullOrEmpty(_webftp) ) return string.Empty;
			    return _webftp;
		    }
		    set
		    {
			    // Anonymous FTP is not supported
			    // Make sure that '@' in username is '%40'
			    if ( string.IsNullOrEmpty(value) ) return;
			    Uri uriAddress = new Uri (value);
			    if ( uriAddress.UserInfo.Split(":".ToCharArray()).Length == 2 
			         && uriAddress.Scheme == "ftp" 
			         && uriAddress.LocalPath.Length >= 1 )
			    {
				    _webftp = value;
			    }

		    }
	    }

	    /// <summary>
	    /// Publishing profiles used within the publishing module
	    /// </summary>
	    public List<AppSettingsPublishProfiles> PublishProfiles { get; set; } = new List<AppSettingsPublishProfiles>();
	    
	    /// <summary>
	    /// Set this value to `true` to keep `/account/register` open for everyone. (Security Issue)
	    /// This setting is by default false. The only 2 build-in exceptions are when there are no accounts or you already logged in
	    /// </summary>
	    public bool IsAccountRegisterOpen { get; set; } = false;

	    /// <summary>
	    /// Private storage for Application Insights InstrumentationKey
	    /// </summary>
	    private string ApplicationInsightsInstrumentationKeyPrivate { get; set; } = "";
	    
	    /// <summary>
	    /// Insert the Application Insights InstrumentationKey here or use environment variable: APPINSIGHTS_INSTRUMENTATIONKEY
	    /// </summary>
	    public string ApplicationInsightsInstrumentationKey {
		    get
		    {
			    if ( string.IsNullOrWhiteSpace(ApplicationInsightsInstrumentationKeyPrivate) )
			    {
				    return Environment.GetEnvironmentVariable("APPINSIGHTS_INSTRUMENTATIONKEY");
			    }
			    return ApplicationInsightsInstrumentationKeyPrivate;
		    }
		    set => ApplicationInsightsInstrumentationKeyPrivate = value;
	    }

	    // -------------------------------------------------
	    // ------------------- Modifiers -------------------
	    // -------------------------------------------------
	    
	    /// <summary>
	    /// Used for CloneToDisplay
	    /// </summary>
	    public const string CloneToDisplaySecurityWarning =
		    "warning: The field is not empty but for security reasons it is not shown";
	    
	    /// <summary>
	    /// Duplicate this item in memory. AND remove _databaseConnection 
	    /// </summary>
	    /// <returns>AppSettings duplicated</returns>
	    public AppSettings CloneToDisplay()
	    {

		    var appSettings = (AppSettings) MemberwiseClone();
		    
		    if ( appSettings.DatabaseType != DatabaseTypeList.Sqlite )
		    {
			    appSettings.DatabaseConnection = CloneToDisplaySecurityWarning;
		    }

		    if ( !string.IsNullOrEmpty(appSettings.ApplicationInsightsInstrumentationKey) )
		    {
			    appSettings.ApplicationInsightsInstrumentationKey = CloneToDisplaySecurityWarning;
		    }

		    if ( !string.IsNullOrEmpty(appSettings.WebFtp) )
		    {
			    appSettings._webftp = CloneToDisplaySecurityWarning;
		    }
		    return appSettings;
	    }

		public string FullPathToDatabaseStyle(string subpath)
		{
			var databaseFilePath = subpath.Replace(StorageFolder, string.Empty);
			databaseFilePath = _pathToDatabaseStyle(databaseFilePath);
			
			return PathHelper.PrefixDbSlash(databaseFilePath);
		}
	    
	    
	    /// <summary>
	    /// Rename a list to database style (short style)
	    /// </summary>
	    /// <param name="localSubFolderList"></param>
	    /// <returns></returns>
	    public List<string> RenameListItemsToDbStyle(List<string> localSubFolderList)
	    {
		    var localSubFolderListDatabaseStyle = new List<string>();

		    foreach (var item in localSubFolderList)
		    {
			    localSubFolderListDatabaseStyle.Add(FullPathToDatabaseStyle(item));
		    }

		    return localSubFolderListDatabaseStyle;
	    }
        
	    /// <summary>
	    ///  Replace windows \\ > /
	    /// </summary>
	    /// <param name="subPath">path to replace</param>
	    /// <returns>replaced output</returns>
        private string _pathToDatabaseStyle(string subPath)
        {
            if (Path.DirectorySeparatorChar.ToString() == "\\")
            {
                subPath = subPath.Replace("\\", "/");
            }
            return subPath;
        }

	    /// <summary>
	    ///  Replace windows \\ > /
	    /// </summary>
	    /// <param name="subPath">path to replace</param>
	    /// <returns>replaced output</returns>
        private string _pathToFilePathStyle(string subPath)
        {
            if (Path.DirectorySeparatorChar.ToString() == "\\")
            {
                subPath = subPath.Replace("/", "\\");
            }
            return subPath;
        }

        /// <summary>
        /// from relative database path => file location path 
        /// </summary>
        /// <param name="databaseFilePath">databaseFilePath</param>
        /// <param name="checkIfExist">checkIfExist</param>
        /// <returns></returns>
        public string DatabasePathToFilePath(string databaseFilePath, bool checkIfExist = true)
        {
            var filepath = StorageFolder + databaseFilePath;

            filepath = _pathToFilePathStyle(filepath);

            // Used for deleted files
            if (!checkIfExist) return filepath;
            
            var fileExist = File.Exists(filepath) ? filepath : null;
            if (fileExist != null)
            {
                return fileExist;
            }
            return Directory.Exists(filepath) ? filepath : null;
        }

        /// <summary>
        /// Replaces a SQLite url with a full directory path in the connection string
        /// </summary>
        /// <param name="connectionString">SQLite</param>
        /// <param name="baseDirectoryProject">path</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">The 'DatabaseConnection' field is null or empty or missing Data Source in connection string</exception>
        public string SqLiteFullPath(string connectionString, string baseDirectoryProject)
        {
            if (DatabaseType == DatabaseTypeList.Mysql && string.IsNullOrWhiteSpace(connectionString)) 
                throw  new ArgumentException("The 'DatabaseConnection' field is null or empty");

            if(DatabaseType != DatabaseTypeList.Sqlite) return connectionString; // mysql does not need this
            if(Verbose) Console.WriteLine(connectionString);            

            if(!connectionString.Contains("Data Source=")) throw 
                new ArgumentException("missing Data Source in connection string");

            var databaseFileName = connectionString.Replace("Data Source=", "");
            
            // Check if path is not absolute already
            if (databaseFileName.Contains("/") || databaseFileName.Contains("\\")) return connectionString;

            // Return if running in Microsoft.EntityFrameworkCore.Sqlite (location is now root folder)
            if(baseDirectoryProject.Contains("entityframeworkcore")) return connectionString;

            var dataSource = "Data Source=" + baseDirectoryProject + 
                             Path.DirectorySeparatorChar+  databaseFileName;
            if(Verbose) Console.WriteLine(dataSource);
            return dataSource;
        }

    }

}
