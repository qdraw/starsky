#if SYSTEM_TEXT_ENABLED
using System.Text.Json.Serialization;
#else
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
#endif
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
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
			// Main Storage for source files (default)
            StorageFolder = Path.Combine(BaseDirectoryProject, "storageFolder");
            // Temp folder, should be cleaned
            TempFolder = Path.Combine(BaseDirectoryProject, "temp");

            try
            {
	            CreateDefaultFolders();
            }
            catch ( FileNotFoundException e )
            {
	            Console.WriteLine("> Not allowed to create default folders: ");
	            Console.WriteLine(e);
            }
            
            // Set the default write to appSettings file
            AppSettingsPath = Path.Combine(BaseDirectoryProject, "appsettings.patch.json");
            
            // AddMemoryCache defaults in prop
        }

        /// <summary>
        /// @see: https://tomasherceg.com/blog/post/azure-app-service-cannot-create-directories-and-write-to-filesystem-when-deployed-using-azure-devops
        /// </summary>
        private void CreateDefaultFolders()
        {
	        if(!Directory.Exists(BaseDirectoryProject)) Directory.CreateDirectory(BaseDirectoryProject);

	        // Cache for thumbs
	        if(!Directory.Exists(ThumbnailTempFolder)) Directory.CreateDirectory(ThumbnailTempFolder);

	        // default location to store source images. you should change this
	        if(!Directory.Exists(StorageFolder)) Directory.CreateDirectory(StorageFolder);

	        // may be cleaned after restart (not implemented)
	        if(!Directory.Exists(TempFolder)) Directory.CreateDirectory(TempFolder);
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
		
		/// <summary>
		/// Get the Application Version of Starsky
		/// </summary>
		public string AppVersion
		{
			get
			{
				var assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
				return new Regex("\\.0$").Replace(assemblyVersion, string.Empty);
			}
		}
		public DateTime AppVersionBuildDateTime => DateAssembly.GetBuildDate(Assembly.GetExecutingAssembly());

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
        public string GetWebSafeReplacedName(string input, string name)
        {
            // Included slash dd the end of this file
            return PathHelper.AddSlash(input.Replace("{name}", GenerateSlug(name,true)));
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
	            var connection = ReplaceEnvironmentVariable(value);
                _databaseConnection = SqLiteFullPath(connection, BaseDirectoryProject);
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

        /// <summary>
        /// To Check if the structure is any good
        /// </summary>
        /// <param name="structure"></param>
        /// <exception cref="ArgumentException"></exception>
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

        /// <summary>
        /// Private: Location of storage of Thumbnails
        /// </summary>
        private string _thumbnailTempFolder;
        
        /// <summary>
        /// Location of storage of Thumbnails
        /// </summary>
        public string ThumbnailTempFolder
        {
	        get => _thumbnailTempFolder;
	        set
	        {
		        var thumbnailTempFolder = ReplaceEnvironmentVariable(value);
		        _thumbnailTempFolder = PathHelper.AddBackslash(thumbnailTempFolder);
	        }
        }
        
        /// <summary>
        /// Private: Location of temp folder
        /// </summary>
        private string _tempFolder;

        /// <summary>
        /// Location of temp folder
        /// </summary>
        public string TempFolder
        {
	        get => AssemblyDirectoryReplacer(_tempFolder);
	        set
	        {
		        var tempFolder = ReplaceEnvironmentVariable(value);
		        _tempFolder = PathHelper.AddBackslash(tempFolder);
	        }
        }

        /// <summary>
        /// Private: Location of AppSettings Path
        /// </summary>
        private string _appSettingsPathPrivate;
        
        /// <summary>
        /// To store the settings by user in the AppData folder
        /// Used by the Desktop App
        /// </summary>
        public string AppSettingsPath
        {
	        get => AssemblyDirectoryReplacer(_appSettingsPathPrivate); 
	        // ReSharper disable once MemberCanBePrivate.Global
	        set => _appSettingsPathPrivate = value; // set by ctor
        }
        
        /// <summary>
        /// Is the host of the Application Windows
        /// </summary>
        public bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        /// <summary>
        /// Private Location of ExifTool.exe
        /// </summary>
        private string ExifToolPathPrivate { get; set; }
        
        /// <summary>
        /// Location of ExifTool.exe
        /// </summary>
        public string ExifToolPath {
	        get
	        {
		        if (IsWindows && string.IsNullOrEmpty(ExifToolPathPrivate)  )
		        {
			        return Path.Combine(TempFolder, "exiftool-windows", "exiftool.exe");
		        }
		        if (!IsWindows && string.IsNullOrEmpty(ExifToolPathPrivate) )
		        {
			         return Path.Combine(TempFolder, "exiftool-unix", "exiftool");
		        }
		        return ExifToolPathPrivate;
	        }
	        set => ExifToolPathPrivate = value;
        }
        
        // C# 6+ required for this
        public bool ExifToolImportXmpCreate { get; set; } = true; // -x -clean command

	    // fallback in constructor
	    // use env variable: app__ReadOnlyFolders__0 - value
        public List<string> ReadOnlyFolders { get; set; }

        /// <summary>
        /// Is the file read only (only by folder name)
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
	    /// Private field: Publishing profiles
	    /// </summary>
	    private Dictionary<string, List<AppSettingsPublishProfiles>> PublishProfilesPrivate { get; set; } =
		    new Dictionary<string, List<AppSettingsPublishProfiles>>();

	    /// <summary>
	    /// Publishing profiles used within the publishing module (Order by Key)
	    /// </summary>
	    public Dictionary<string, List<AppSettingsPublishProfiles>> PublishProfiles {
		    get => PublishProfilesPrivate;
		    set
		    {
			    if ( value == null ) return;
			    PublishProfilesPrivate = value.OrderBy(obj => obj.Key)
				    .ToDictionary(obj => obj.Key, 
					    obj => obj.Value);
		    } 
	    } 
	    
	    /// <summary>
	    /// Set this value to `true` to keep `/account/register` open for everyone. (Security Issue)
	    /// This setting is by default false. The only 2 build-in exceptions are when there are no accounts or you already logged in
	    /// </summary>
	    public bool IsAccountRegisterOpen { get; set; } = false;

	    /// <summary>
	    /// When a new account is created, which Account Role is assigned 
	    /// </summary>
#if SYSTEM_TEXT_ENABLED
	    [JsonConverter(typeof(JsonStringEnumConverter))]
#else
	    [JsonConverter(typeof(StringEnumConverter))]
#endif
	    public AccountRoles.AppAccountRoles AccountRegisterDefaultRole { get; set; } = AccountRoles.AppAccountRoles.User;
	    
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

	    public int MaxDegreesOfParallelism => 6;
	    
	    /// <summary>
	    /// Set to false when running on http-only service.
	    /// You should enable this when going to production
	    /// Ignored in Debug/Develop mode
	    /// </summary>
	    public bool UseHttpsRedirection { get; set; } = false;

	    public bool UseRealtime { get; set; }

	    // -------------------------------------------------
	    // ------------------- Modifiers -------------------
	    // -------------------------------------------------

	    private string AssemblyDirectoryReplacer(string value)
	    {
		    return value.Replace("{AssemblyDirectory}", BaseDirectoryProject);
	    }
	    
	    /// <summary>
	    /// Used for CloneToDisplay
	    /// </summary>
	    public const string CloneToDisplaySecurityWarning =
		    "warning: The field is not empty but for security reasons it is not shown";
	    
	    /// <returns>AppSettings duplicated</returns>
		/// <summary>
		/// Duplicate this item in memory. AND remove _databaseConnection 
		/// </summary>
		/// <returns>AppSettings duplicated></returns>
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

		/// <summary>
		/// StorageFolders ends always with a backslash
		/// </summary>
		/// <param name="subpath">in OS Style, StorageFolder ends with backslash</param>
		/// <returns></returns>
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
        /// Used to reference other environment variables in the config
        /// </summary>
        /// <param name="input">the input, the env should start with a $</param>
        /// <returns>the value or the input when nothing is found</returns>
        internal string ReplaceEnvironmentVariable(string input)
        {
	        if ( string.IsNullOrEmpty(input) || !input.StartsWith("$") ) return input;
	        var value = Environment.GetEnvironmentVariable(input.Remove(0, 1));
	        return string.IsNullOrEmpty(value) ? input : value;
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
