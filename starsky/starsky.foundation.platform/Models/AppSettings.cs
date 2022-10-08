using System.Text.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using starsky.foundation.platform.Attributes;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.JsonConverter;
using TimeZoneConverter;

namespace starsky.foundation.platform.Models
{
	[SuppressMessage("ReSharper", "CA1822")]
	public class AppSettings
	{
		public AppSettings()
		{
			ReadOnlyFolders = new List<string>();
			DatabaseConnection = SqLiteFullPath("Data Source=data.db",BaseDirectoryProject);

			// Cache for thumbs
			ThumbnailTempFolder = Path.Combine(BaseDirectoryProject, "thumbnailTempFolder");

			// Temp folder, should be cleaned
			TempFolder = Path.Combine(BaseDirectoryProject, "temp");

			DependenciesFolder = Path.Combine(BaseDirectoryProject, "dependencies");

			ExifToolPathDefaultPrivate = GetDefaultExifToolPath();
			
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
		/// @see: https://tomasherceg.com/blog/post/
		/// azure-app-service-cannot-create-directories-and-write-to-filesystem-when-deployed-using-azure-devops
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
			
			if(!Directory.Exists(DependenciesFolder)) Directory.CreateDirectory(DependenciesFolder);
		}

		/// <summary>
		/// Root of the project with replaced value
		/// </summary>
		public string BaseDirectoryProject => AppDomain.CurrentDomain
			.BaseDirectory
			.Replace("starskyadmincli", "starsky")
			.Replace("starskysynchronizecli", "starsky")
			.Replace("starskythumbnailcli", "starsky")
			.Replace("starskythumbnailmetacli", "starsky")
			.Replace("starskysynccli", "starsky")
			.Replace("starsky.foundation.database", "starsky")
			.Replace("starskyImporterNetFrameworkCli", "starsky")
			.Replace("netframework-msbuild", "starsky")
			.Replace("starskySyncNetFrameworkCli", "starsky")
			.Replace("starskyimportercli", "starsky")
			.Replace("starskywebftpcli", "starsky")
			.Replace("starskywebhtmlcli", "starsky")
			.Replace("starskygeocli", "starsky")
			.Replace("starskytest", "starsky")
			.Replace("starskydiskwatcherworkerservice", "starsky");

		/// <summary>
		/// Application Type, defaults to WebController
		/// </summary>
		[JsonConverter(typeof(JsonStringEnumConverter))]
		// newtonsoft uses: StringEnumConverter
		public StarskyAppType ApplicationType { get; set; }

		/// <summary>
		/// What is the type of the application that is running different CLI types or Web
		/// </summary>
		public enum StarskyAppType
		{
			/// <summary>
			/// Mvc controller / Web interface
			/// </summary>
			WebController = 0,
			/// <summary>
			/// Importer CLI
			/// </summary>
			Importer = 1,
			/// <summary>
			/// Sync CLI
			/// </summary>
			Sync = 2,
			/// <summary>
			/// WebHTML CLI
			/// </summary>
			WebHtml = 3,
			/// <summary>
			/// Geo CLI
			/// </summary>
			Geo = 4,
			/// <summary>
			/// WebFTP CLI
			/// </summary>
			WebFtp = 5,
			/// <summary>
			/// Admin CLI
			/// </summary>
			Admin = 6,
			/// <summary>
			/// Thumbnail Generator CLI
			/// </summary>
			Thumbnail = 7,
			/// <summary>
			/// Meta Thumbnail Generator CLI
			/// </summary>
			MetaThumbnail = 8,
			
			/// <summary>
			/// DiskWatcherWorkerService
			/// </summary>
			DiskWatcherWorkerService = 9
		}
		
		/// <summary>
		/// Get the Application Version of Starsky
		/// </summary>
		public string AppVersion
		{
			get
			{
				var assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString();
				return string.IsNullOrEmpty(assemblyVersion) ? string.Empty : 
					new Regex("\\.0$").Replace(assemblyVersion, string.Empty);
			}
		}
		
		[PackageTelemetry]
		public DateTime AppVersionBuildDateTime => DateAssembly.GetBuildDate(Assembly.GetExecutingAssembly());

		// Can be used in the cli session to select files out of the file database system
		private string _storageFolder;
        
		/// <summary>
		/// Main Storage provider on disk
		/// </summary>
		public string StorageFolder
		{
			get
			{
				// ReSharper disable once ArrangeAccessorOwnerBody
				return string.IsNullOrEmpty(_storageFolder) ? Path.Combine(BaseDirectoryProject, "storageFolder") : _storageFolder;
			}
			set
			{
				var storageFolder = ReplaceEnvironmentVariable(value);
				// ReSharper disable once ArrangeAccessorOwnerBody
				_storageFolder = PathHelper.AddBackslash(storageFolder);
			}
		}

		/// <summary>
		/// Allow overwrite this name in AppSettingsController
		/// </summary>
		public bool StorageFolderAllowEdit =>
			string.IsNullOrEmpty(
				Environment.GetEnvironmentVariable("app__storageFolder"));

		[PackageTelemetry]
		public bool? Verbose { get; set; }

		public bool IsVerbose()
		{
			return Verbose == true;
		}

		// Used in the webHtmlCli to store the log item name
		// used for the url
		private string _name;
		
		[PackageTelemetry]
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
        
		// Used to template config > appSettingsPubProfile
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
		/// <param name="toLowerCase">change output to lowerCase</param>
		/// <param name="allowAtSign">allow @ (at sign) in name</param>
		/// <returns>clean slug string (ex. "some-cool-topic")</returns>
		public string GenerateSlug(string phrase, bool allowUnderScore = false, 
			bool toLowerCase = true, bool allowAtSign = false)
		{
			var text = toLowerCase ? phrase.ToLowerInvariant() : phrase;

			var charAllowLowerCase = allowUnderScore ? "_" : string.Empty;
			var charAllowAtSign = allowAtSign ? "@" : string.Empty;

			var matchNotRegexString = @"[^a-zA-Z0-9\s-" + charAllowLowerCase + charAllowAtSign + "]";
            
			text = Regex.Replace(text,matchNotRegexString, string.Empty);         
			//						^^^ remove invalid characters
			text = Regex.Replace(text, @"\s+", " ").Trim();                       // single space
			text = text.Substring(0, text.Length <= 65 ? text.Length : 65).Trim();      // cut and trim
			text = Regex.Replace(text, @"\s", "-");                               // insert hyphens
			text = text.Replace("---", "-"); // for example: "test[space]-[space]test"
			return text;
		}
        

		/// <summary>
		/// Type of the database, sqlite, mysql or inmemory
		/// </summary>
		[JsonConverter(typeof(JsonStringEnumConverter))]
		[PackageTelemetry]
		// newtonsoft uses: StringEnumConverter
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

		/// <summary>
		/// Internal Structure save location
		/// </summary>
		private string _structure;
		
		[PackageTelemetry]
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

		/// <summary>
		/// Used for syncing gpx files
		/// </summary>
		[PackageTelemetry]
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
		/// Private: Location of dependencies folder
		/// </summary>
		private string _dependenciesFolder;

		/// <summary>
		/// Location of dependencies folder
		/// </summary>
		public string DependenciesFolder
		{
			get => AssemblyDirectoryReplacer(_dependenciesFolder);
			set
			{
				var dependenciesFolder = ReplaceEnvironmentVariable(value);
				_dependenciesFolder = PathHelper.AddBackslash(dependenciesFolder);
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
		/// Set in ctor on startup
		/// </summary>
		private string ExifToolPathDefaultPrivate { get; }
		
		private string GetDefaultExifToolPath()
		{
			return IsWindows
				? Path.Combine(DependenciesFolder, "exiftool-windows", "exiftool.exe")
				: Path.Combine(DependenciesFolder, "exiftool-unix", "exiftool");
		}
		
		/// <summary>
		/// Location of ExifTool.exe
		/// </summary>
		public string ExifToolPath
		{
			get => string.IsNullOrEmpty(ExifToolPathPrivate) ? GetDefaultExifToolPath() : ExifToolPathPrivate;
			set
			{
				if ( value != ExifToolPathDefaultPrivate)
				{
					ExifToolPathPrivate = value;
				}
			}
		}

		[PackageTelemetry]
		public bool ExifToolImportXmpCreate { get; set; } = true; // -x -clean command

		/// <summary>
		/// fallback in constructor
		/// use env variable: app__ReadOnlyFolders__0 - value
		/// </summary>
		[PackageTelemetry]
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
        
		[PackageTelemetry]
		public bool? AddMemoryCache { get; set; } = true;
        
		/// <summary>
		/// Display swagger pages
		/// </summary>
		[PackageTelemetry]
		public bool? AddSwagger { get; set; } = false;

		/// <summary>
		/// Export swagger pages (use AddSwagger and AddSwaggerExport to export)
		/// </summary>	    
		[PackageTelemetry]
		public bool? AddSwaggerExport { get; set; } = false;

		/// <summary>
		/// Stop application after swagger Export.
		/// Need to set AddSwagger and AddSwaggerExport also to true to take effect
		/// </summary>
		[PackageTelemetry]
		public bool? AddSwaggerExportExitAfter { get; set; } = false;

		/// <summary>
		/// Set Meta Thumbnails on import
		/// </summary>
		[PackageTelemetry]
		public bool? MetaThumbnailOnImport { get; set; } = true;
		
		/// <summary>
		/// Internal location for webFtp credentials
		/// </summary>
		private string _webFtp; 
		
		public string WebFtp
		{
			get
			{
				if ( string.IsNullOrEmpty(_webFtp) ) return string.Empty;
				return _webFtp;
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
					_webFtp = value;
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
		[PackageTelemetry]
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
		[PackageTelemetry]
		public bool? IsAccountRegisterOpen { get; set; }

		/// <summary>
		/// Used for Desktop App, to allow localhost logins without password and use default account
		/// </summary>
		[PackageTelemetry]
		public bool? NoAccountLocalhost { get; set; } = false;
		
		/// <summary>
		/// When a new account is created, which Account Role is assigned 
		/// Defaults to User, but can also be Administrator
		/// </summary>
		[JsonConverter(typeof(JsonStringEnumConverter))]
		[PackageTelemetry]
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

		[PackageTelemetry]
		public bool? ApplicationInsightsLog { get; set; } = true;

		[PackageTelemetry]
		public bool? ApplicationInsightsDatabaseTracking { get; set; } = false;

		[PackageTelemetry]
		public int MaxDegreesOfParallelism { get; set; } = 6;
	    
		/// <summary>
		/// Set to false when running on http-only service.
		/// You should enable this when going to production
		/// Ignored in Debug/Develop mode
		/// </summary>
		[PackageTelemetry]
		public bool? UseHttpsRedirection { get; set; } = false;

		/// <summary>
		/// Set to false when running on http-only service.
		/// You should enable this when going to production
		/// Ignored in Debug/Develop mode
		/// </summary>
		[PackageTelemetry]
		public bool? HttpsOn { get; set; } = false;
		
		/// <summary>
		/// Use WebSockets to update the UI realtime
		/// </summary>
		[PackageTelemetry]
		public bool? UseRealtime { get; set; } = true;

		/// <summary>
		/// Watch the fileSystem for changes
		/// </summary>
		[PackageTelemetry]
		public bool? UseDiskWatcher { get; set; } = true;
	    
		/// <summary>
		/// Check if there are updates
		/// </summary>
		[PackageTelemetry]
		public bool? CheckForUpdates { get; set; } = true;

		/// <summary>
		/// Ignore the directories when running sync
		/// use env variable: app__SyncIgnore__0 - value
		/// Use always UNIX style
		/// </summary>
		public List<string> SyncIgnore { get; set; } = new List<string>
		{
			"/lost+found",
			"/.stfolder", 
			"/.git"
		};
		
		/// <summary>
		/// Ignore this part of a path while importing
		/// use env variable: app__importIgnore__0 - value
		/// Use always UNIX style
		/// </summary>
		public List<string> ImportIgnore { get; set; } = new List<string> {"lost+found", ".Trashes"};

		/// <summary>
		/// According to Phil Harvey (exiftool's creator),
		/// Quicktime timestamps are supposed to be set to UTC time as per the standard.
		/// But it seems a lot of cameras don't do this
		/// We assume that the standard is followed, and for Camera brands that don't follow the specs use this setting.
		/// </summary>
		public List<CameraMakeModel> VideoUseLocalTime { get; set; } = new List<CameraMakeModel>
		{
			new CameraMakeModel("Sony","A58")
		};

		private bool? EnablePackageTelemetryPrivate { get; set; } = null;

		public bool? EnablePackageTelemetry
		{
			get
			{
				// ReSharper disable once InvertIf
				if ( EnablePackageTelemetryPrivate == null )
				{
#pragma warning disable CS0162
#if(DEBUG)
					return false;
#endif
					return true;
#pragma warning restore CS0162
				}
				return EnablePackageTelemetryPrivate;
			}
			// ReSharper disable once PropertyCanBeMadeInitOnly.Global
			set
			{
				EnablePackageTelemetryPrivate = value;
			}
		}

		/// <summary>
		/// Show what is send in console/logger
		/// </summary>
		[PackageTelemetry]
		public bool? EnablePackageTelemetryDebug { get; set; } = false;

		public double UseDiskWatcherIntervalInMilliseconds { get; set; } = 20000;

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
		[SuppressMessage("ReSharper", "InvertIf")]
		public AppSettings CloneToDisplay()
		{
			var userProfileFolder = Environment.GetFolderPath(
				Environment.SpecialFolder.UserProfile);

			var appSettings = this.CloneViaJson();
		    
			if ( appSettings.DatabaseType != DatabaseTypeList.Sqlite )
			{
				appSettings.DatabaseConnection = CloneToDisplaySecurityWarning;
			}
			
			if ( appSettings.DatabaseType == DatabaseTypeList.Sqlite )
			{
				appSettings.DatabaseConnection = appSettings.DatabaseConnection.
					Replace(userProfileFolder, "~");
			}

			if ( !string.IsNullOrEmpty(appSettings.ApplicationInsightsInstrumentationKey) )
			{
				appSettings.ApplicationInsightsInstrumentationKey = CloneToDisplaySecurityWarning;
			}

			if ( !string.IsNullOrEmpty(appSettings.WebFtp) )
			{
				appSettings._webFtp = CloneToDisplaySecurityWarning;
			}
			
			if ( !string.IsNullOrEmpty(appSettings.AppSettingsPath) )
			{
				appSettings.AppSettingsPath =
					appSettings.AppSettingsPath.Replace(userProfileFolder, "~");
			}

			if ( appSettings.PublishProfiles != null )
			{
				foreach ( var value in appSettings.PublishProfiles.SelectMany(profile => profile.Value) )
				{
					ReplaceAppSettingsPublishProfilesCloneToDisplay(value);
				}
			}

			return appSettings;
		}

		private static void ReplaceAppSettingsPublishProfilesCloneToDisplay(AppSettingsPublishProfiles value)
		{
			if ( !string.IsNullOrEmpty(value.Path) && value.Path != AppSettingsPublishProfiles.GetDefaultPath() )
			{
				value.Path = CloneToDisplaySecurityWarning;
			}
			else if ( value.Path == AppSettingsPublishProfiles.GetDefaultPath() )
			{
				value.ResetPath();
			}

			if ( !string.IsNullOrEmpty(value.Prepend))
			{
				value.Prepend = CloneToDisplaySecurityWarning;
			}
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
		public List<string> RenameListItemsToDbStyle(IEnumerable<string> localSubFolderList)
		{
			return localSubFolderList.Select(FullPathToDatabaseStyle).ToList();
		}
		
		/// <summary>
		/// Rename a list to database style (short style)
		/// </summary>
		/// <param name="localSubFolderList"></param>
		/// <returns></returns>
		public IEnumerable<KeyValuePair<string, DateTime>> RenameListItemsToDbStyle(IEnumerable<KeyValuePair<string, DateTime>> localSubFolderList)
		{
			return localSubFolderList.Select(item => 
				new KeyValuePair<string, DateTime>(FullPathToDatabaseStyle(item.Key), item.Value)).ToList();
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
		/// <exception cref="ArgumentException">The 'DatabaseConnection' field is null or empty or
		/// missing Data Source in connection string</exception>
		public string SqLiteFullPath(string connectionString, string baseDirectoryProject)
		{
			if (DatabaseType == DatabaseTypeList.Mysql && string.IsNullOrWhiteSpace(connectionString)) 
				throw  new ArgumentException("The 'DatabaseConnection' field is null or empty");

			if(DatabaseType != DatabaseTypeList.Sqlite) return connectionString; // mysql does not need this
			if(IsVerbose()) Console.WriteLine(connectionString);            

			if(!connectionString.Contains("Data Source=")) throw 
				new ArgumentException("missing Data Source in connection string");

			var databaseFileName = connectionString.Replace("Data Source=", "");
            
			// Check if path is not absolute already
			if (databaseFileName.Contains('/') || databaseFileName.Contains('\\')) return connectionString;

			// Return if running in Microsoft.EntityFrameworkCore.Sqlite (location is now root folder)
			if(baseDirectoryProject.Contains("entityframeworkcore")) return connectionString;

			var dataSource = "Data Source=" + baseDirectoryProject + 
			                 Path.DirectorySeparatorChar+  databaseFileName;
			return dataSource;
		}

	}

}
