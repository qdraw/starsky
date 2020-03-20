using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;

namespace starskycore.Helpers
{
	public class ArgsHelper
	{
		// Table of Content
		
		// -j > free
		// -k > free
		// -l > free
		// -q > free
		// -w > free
		// -y > free
		// -z > free
		// --verbose -v
		// --databasetype -d
		// --connection -c
		// --basepath -b
		// --thumbnailtempfolder -f
		// --exiftoolpath -e
		// --help -h
		// --index -i
		// --path -p
		// --subpath -s
		// --subpathrelative -g
		// --thumbnail -t
		// --orphanfolder -o
		// --move -m
		// --all -a
		// --recruisive -r 
		// -rf --readonlyfolders // no need to use in cli/importercli
		// -u --structure
		// -n --name
		// -x --clean
		// --colorclass (no shorthand)
		
		/// <summary>
		/// Simple injection
		/// </summary>
		public ArgsHelper()
		{
		}
		
		/// <summary>
		/// Use with appsettings
		/// </summary>
		/// <param name="appSettings">appsettings</param>
		public ArgsHelper(AppSettings appSettings)
		{
			_appSettings = appSettings;
		}
		
		/// <summary>
		/// Appsettings
		/// </summary>
		private readonly AppSettings _appSettings;
		
		/// <summary>
		/// Show debug information
		/// </summary>
		/// <param name="args">input args</param>
		/// <returns></returns>
		public bool NeedVerbose(IReadOnlyList<string> args)
		{
			var needDebug = false;
			for (int arg = 0; arg < args.Count; arg++)
			{
				if ((args[arg].ToLower() == "--verbose" || args[arg].ToLower() == "-v") && (arg + 1) != args.Count)
				{
					bool.TryParse(args[arg + 1], out needDebug);
				}
				if ((args[arg].ToLower() == "--verbose" || args[arg].ToLower() == "-v"))
				{
					needDebug = true;
				}
			}
			return needDebug;
		}

		/// <summary>
		/// short input args, use the same order as 'LongNameList' and 'EnvNameList'
		/// </summary>
		public readonly IEnumerable<string> ShortNameList = new List<string>
		{
			"-d","-c","-b","-f","-e","-u","-g","-n", "-x"
		}.AsReadOnly();

		/// <summary>
		/// Long input args, use this order as 'ShortNameList' and 'EnvNameList'
		/// </summary>
		public readonly IEnumerable<string> LongNameList = new List<string>
		{
			"--databasetype","--connection","--basepath","--thumbnailtempfolder",
			"--exiftoolpath","--structure","--subpathrelative","--name", "--clean"
		}
		.AsReadOnly();

		/// <summary>
		/// name of the env__ (__=:) use this order as 'LongNameList' and 'ShortNameList'
		/// </summary>
		public readonly IEnumerable<string> EnvNameList = new List<string>
		{
			"app__DatabaseType","app__DatabaseConnection","app__StorageFolder","app__ThumbnailTempFolder",
			"app__ExifToolPath", "app__Structure", "app__subpathrelative", "app__name", "app__ExifToolImportXmpCreate"
		}.AsReadOnly();

		/// <summary>
		/// SetEnvironmentByArgs
		/// </summary>
		/// <param name="args"></param>
		public void SetEnvironmentByArgs(IReadOnlyList<string> args)
		{
			var shortNameList = ShortNameList.ToArray();
			var longNameList = LongNameList.ToArray();
			var envNameList = EnvNameList.ToArray();
			for (int i = 0; i < ShortNameList.Count(); i++)
			{
				for (int arg = 0; arg < args.Count; arg++)
				{
					if ((args[arg].ToLower() == longNameList[i] || 
						args[arg].ToLower() == shortNameList[i]) && (arg + 1) != args.Count)
					{
						Environment.SetEnvironmentVariable(envNameList[i],args[arg+1]);
					}
				}
			}
		}

		/// <summary>
		/// Set Environment Variables to appSettings (not used in .net core), used by framework app
		/// </summary>
		/// <exception cref="FieldAccessException">use with _appsettings</exception>
		public void SetEnvironmentToAppSettings()
		{
			if ( _appSettings == null ) throw new FieldAccessException("use with _appsettings");
				
			var envNameList = EnvNameList.ToArray();
			foreach ( var envUnderscoreName in envNameList )
			{
				var envValue = Environment.GetEnvironmentVariable(envUnderscoreName);
				var envName = envUnderscoreName.Replace("app__", string.Empty);
				if ( !string.IsNullOrEmpty(envValue) )
				{
					PropertyInfo propertyObject = _appSettings.GetType().GetProperty(envName);
					if(propertyObject == null) continue;
					var type = propertyObject.PropertyType;
					
					// for enums
					if ( propertyObject.PropertyType.IsEnum )
					{
						var envTypedObject = Enum.Parse(type, envValue);
						propertyObject.SetValue(_appSettings, envTypedObject, null);
						continue;
					}
					
					dynamic envTypedDynamic = Convert.ChangeType(envValue, type);
					propertyObject.SetValue(_appSettings, envTypedDynamic, null);
				}
			}
		}
		
				
		/// <summary>
		/// Based on args get the -h or --help commandline input
		/// </summary>
		/// <param name="args">args input</param>
		/// <returns>bool, true if --help</returns>
		public bool NeedHelp(IReadOnlyList<string> args)
		{
			var needHelp = false;
			for (int arg = 0; arg < args.Count; arg++)
			{
				if ((args[arg].ToLower() == "--help" || args[arg].ToLower() == "-h") && (arg + 1) != args.Count)
				{
					bool.TryParse(args[arg + 1], out needHelp);
				}
				if ((args[arg].ToLower() == "--help" || args[arg].ToLower() == "-h"))
				{
					needHelp = true;
				}
			}
			return needHelp;
		}
		
		/// <summary>
		/// Show Help dialog
		/// </summary>
		/// <exception cref="FieldAccessException">use appsettings</exception>
		public void NeedHelpShowDialog()
		{
			if (_appSettings == null) throw new FieldAccessException("use with _appsettings");
			
			Console.WriteLine("Starksy " + _appSettings.ApplicationType + " Cli ~ Help:");
			Console.WriteLine("--help or -h == help (this window)");
			
			switch (_appSettings.ApplicationType)
			{
				case AppSettings.StarskyAppType.Admin:
					Console.WriteLine("--name or -n == string ; username / email");
					break;
				case AppSettings.StarskyAppType.Geo:
					// When this change please update ./readme.md
					Console.WriteLine("--path or -p == parameter: (string) ; without addition is current directory, full path (all locations are supported) ");
					Console.WriteLine("--subpath or -s == parameter: (string) ; relative path in the database ");
					Console.WriteLine("--subpathrelative or -g == Overwrite subpath to use relative days to select a folder" +
						", use for example '1' to select yesterday. (structure is required)");
					Console.WriteLine("-p, -s, -g == you need to select one of those tags");
					Console.WriteLine("--all or -a == overwrite reverse geotag location tags " +
						"(default: false / ignore already taged files) ");
					Console.WriteLine("--index or -i == parameter: (bool) ; gpx feature to index geo location, default true");
				break;
				case AppSettings.StarskyAppType.WebHtml:
					// When this change please update ./readme.md
					Console.WriteLine("--path or -p == parameter: (string) ; fullpath (select a folder), use '-p' for current directory");
					Console.WriteLine("--name or -n == parameter: (string) ; name of blogitem ");
				break;
				case AppSettings.StarskyAppType.Importer:
					// When this change please update ./readme.md
					Console.WriteLine("--path or -p == parameter: (string) ; full path");
					Console.WriteLine("                can be an folder or file, use '-p' for current directory");
					Console.WriteLine("--move or -m == delete file after importing (default false / copy file)");
					Console.WriteLine("--all or -a == import all files including files older than 2 years" +
						" (default: false / ignore old files) ");
					Console.WriteLine("--recursive or -r == Import Directory recursive " +
						"(default: false / only the selected folder) ");
					Console.WriteLine("--structure == overwrite appsettings with filedirectory structure "+
						"based on exif and filename create datetime");
					Console.WriteLine("--index or -i == parameter: (bool) ; indexing, false is always copy, true is check if exist in db, default true");
					Console.WriteLine("--clean or -x == true is to add a xmp sidecar file for raws, default true");
					Console.WriteLine("--colorclass == update colorclass to this number value, default don't change");
				break;
				case AppSettings.StarskyAppType.Sync:
					// When this change please update ./readme.md
					Console.WriteLine("--path or -p == parameter: (string) ; " +
					"'full path', only child items of the database folder are supported," +
					"search and replace first part of the filename, '/', use '-p' for current directory ");
					Console.WriteLine("--subpath or -s == parameter: (string) ; relative path in the database");
					Console.WriteLine("--subpathrelative or -g == Overwrite subpath to use relative days to select a folder" +
						", use for example '1' to select yesterday. (structure is required)");
					Console.WriteLine("-p, -s, -g == you need to select one of those tags");
					Console.WriteLine("--index or -i == parameter: (bool) ; enable indexing, default true");
					Console.WriteLine("--thumbnail or -t == parameter: (bool) ; enable thumbnail, default false");
					Console.WriteLine("--clean or -x == parameter: (bool) ; enable checks in thumbnailtempfolder if thumbnails are needed, delete unused files");
					Console.WriteLine("--orphanfolder or -o == To delete files without a parent folder " +
						"(heavy cpu usage), default false");
					Console.WriteLine("--verbose or -v == verbose, more detailed info");
					Console.WriteLine("--databasetype or -d == Overwrite EnvironmentVariable for DatabaseType");
					Console.WriteLine("--basepath or -b == Overwrite EnvironmentVariable for StorageFolder");
					Console.WriteLine("--connection or -c == Overwrite EnvironmentVariable for DatabaseConnection");
					Console.WriteLine("--thumbnailtempfolder or -f == Overwrite EnvironmentVariable for ThumbnailTempFolder");
					Console.WriteLine("--exiftoolpath or -e == Overwrite EnvironmentVariable for ExifToolPath");
				break;
			}
			
			Console.WriteLine("--verbose or -v == verbose, more detailed info");
			Console.WriteLine("  use -v -help to show settings: ");
			
			if (!_appSettings.Verbose) return;
			
			Console.WriteLine("");
			Console.WriteLine("AppSettings:");
			Console.WriteLine("Database Type (-d --databasetype) "+ _appSettings.DatabaseType);
			Console.WriteLine("DatabaseConnection (-c --connection) " + _appSettings.DatabaseConnection);
			Console.WriteLine($"StorageFolder (-b --basepath) {_appSettings.StorageFolder} ");
			Console.WriteLine($"ThumbnailTempFolder (-f --thumbnailtempfolder) {_appSettings.ThumbnailTempFolder} ");
			Console.WriteLine($"ExifToolPath  (-e --exiftoolpath) {_appSettings.ExifToolPath} ");
			Console.WriteLine("Structure  (-u --structure) "+ _appSettings.Structure);
			Console.WriteLine("Name " + _appSettings.Name);
			Console.WriteLine("CameraTimeZone "+ _appSettings.CameraTimeZone);

			if ( _appSettings.ApplicationType == AppSettings.StarskyAppType.Importer)
				Console.WriteLine("Create xmp on import (ExifToolImportXmpCreate): " + _appSettings.ExifToolImportXmpCreate);
			
			if ( _appSettings.ApplicationType == AppSettings.StarskyAppType.WebFtp) 
				Console.WriteLine("WebFtp " + _appSettings.WebFtp);
			
			Console.WriteLine("-- Appsettings.json locations -- ");
			
			var machineName = Environment.MachineName.ToLowerInvariant();
			
			Console.WriteLine("Config is read in this order: \n" +
				$"1. {Path.Combine(_appSettings.BaseDirectoryProject, "appsettings.patch.json")}\n" +
				$"2. {Path.Combine(_appSettings.BaseDirectoryProject, "appsettings." + machineName + ".patch.json")}  ");
			Console.WriteLine($"3. {Path.Combine(_appSettings.BaseDirectoryProject, "appsettings.json")}\n" +
				$"4. {Path.Combine(_appSettings.BaseDirectoryProject, "appsettings." + machineName + ".json")} ");
			
			switch ( _appSettings.ApplicationType )
			{
				case AppSettings.StarskyAppType.WebHtml:
					Console.WriteLine($"Config for {_appSettings.ApplicationType}");
					foreach ( var publishProfiles in _appSettings.PublishProfiles )
					{
						Console.WriteLine("--- " +
						$"Path: {publishProfiles.Path} " +
						$"Append: {publishProfiles.Append} " +
						$"Copy: {publishProfiles.Copy} " +
						$"Folder: {publishProfiles.Folder} " +
						$"Prepend: {publishProfiles.Prepend} " +
						$"Template: {publishProfiles.Template} " +
						$"ContentType: {publishProfiles.ContentType} " +
						$"MetaData: {publishProfiles.MetaData} " +
						$"OverlayMaxWidth: {publishProfiles.OverlayMaxWidth} " +
						$"SourceMaxWidth: {publishProfiles.SourceMaxWidth} ");
				}
				break;
			}
			
			var framework = Assembly
				.GetEntryAssembly()?
				.GetCustomAttribute<TargetFrameworkAttribute>()?
				.FrameworkName;
			Console.WriteLine($".NET Version - {framework}");
		}
		
		/// <summary>
		/// Default On
		/// Based on args get the -i or --index commandline input
		/// </summary>
		/// <param name="args">args input</param>
		/// <returns>bool, true if --index</returns>
		public bool GetIndexMode(IReadOnlyList<string> args)
		{
			var isIndexMode = true;
			
			for (int arg = 0; arg < args.Count; arg++)
			{
				if ((args[arg].ToLower() == "--index" || args[arg].ToLower() == "-i") && (arg + 1) != args.Count)
				{
					bool.TryParse(args[arg + 1], out isIndexMode);
				}
			}
		
			return isIndexMode;
		}
	 
		/// <summary>
		/// Get path from args
		/// </summary>
		/// <param name="args">args</param>
		/// <param name="dbStyle">convert to subpath style, default=true</param>
		/// <returns>string path</returns>
		/// <exception cref="ArgumentNullException">appsettings is missing</exception>
		public string GetPathFormArgs(IReadOnlyList<string> args, bool dbStyle = true)
		{
			if ( _appSettings == null ) throw new FieldAccessException("use with _appsettings");
			var path = string.Empty;
			
			for (int arg = 0; arg < args.Count; arg++)
			{
				if ((args[arg].ToLower() == "--path" || args[arg].ToLower() == "-p") && (arg + 1) != args.Count )
				{
					path = args[arg + 1];
				}
			}
			
			// To use only with -p or --path > current directory
			if ( (args.Contains("-p") || args.Contains("--path") ) && (path == string.Empty || path[0] == "-"[0]))
			{
				var currentDirectory = Directory.GetCurrentDirectory();
				if ( currentDirectory != _appSettings.BaseDirectoryProject )
				{
					path = currentDirectory;
					if ( _appSettings.Verbose ) Console.WriteLine($">> currentDirectory: {currentDirectory}");
				}
			}
			
			if ( dbStyle)
			{
				path = _appSettings.FullPathToDatabaseStyle(path);
			}
			
			return path;
		}
	 
		/// <summary>
		/// Get --subpath from args
		/// </summary>
		/// <param name="args">args</param>
		/// <returns>subpath string</returns>
		public string GetSubpathFormArgs(IReadOnlyList<string> args)
		{
			var subpath = "/";
			
			for (int arg = 0; arg < args.Count; arg++)
			{
				if ((args[arg].ToLower() == "--subpath" || args[arg].ToLower() == "-s") && (arg + 1) != args.Count)
				{
					subpath = args[arg + 1];
				}
			}
			return subpath;
		}

		/// <summary>
		/// Get subPathRelative, so a structured url based on relative datetime
		/// </summary>
		/// <param name="args">args[]</param>
		/// <returns>relative subpath</returns>
		/// <exception cref="FieldAccessException">missing appsettings</exception>
		public string GetSubpathRelative(IReadOnlyList<string> args)
		{
			if (_appSettings == null) throw new FieldAccessException("use with _appsettings");
			string subpathRelative = string.Empty;
			
			for (int arg = 0; arg < args.Count; arg++)
			{
				if ((args[arg].ToLower() == "--subpathrelative" || 
					args[arg].ToLower() == "-g") && (arg + 1) != args.Count)
				{
					subpathRelative = args[arg + 1];
				}
			}
			
			if (string.IsNullOrWhiteSpace(subpathRelative)) return subpathRelative; // null
			
			int.TryParse(subpathRelative, out var subPathInt);
			if(subPathInt >= 1) subPathInt = subPathInt * -1; //always in the past
			
			// Fallback for dates older than 24-11-1854 to avoid a exception.
			if ( subPathInt < -60000 ) subPathInt = 0;

			// the model to test
			var importmodel = new ImportIndexItem(_appSettings)
			{
				DateTime = DateTime.Today.AddDays(subPathInt), 
				SourceFullFilePath = "notimplemented.jpg"
			};
			
			// expect something like this: /2018/09/2018_09_02/
			return importmodel.ParseSubfolders(false);
		}
		
		/// <summary>
		/// Know if --subpath or --path
		/// </summary>
		/// <param name="args">input[]</param>
		/// <returns>bool --path(true), false --subpath</returns>
		public bool IfSubpathOrPath(IReadOnlyList<string> args)
		{
			// To use only with -p or --path > current directory
			if ( args.Any(arg => (arg.ToLower() == "--path" || arg.ToLower() == "-p")) )
			{
				return false;
			}
			
			// Detect if a input is a fullpath or a subpath.
			for (int arg = 0; arg < args.Count; arg++)
			{
				if ((args[arg].ToLower() == "--subpath" || args[arg].ToLower() == "-s") && (arg + 1) != args.Count)
				{
					return true;
				}
			}
			return true;
		}
	 
		/// <summary>
		/// --thumbnail bool
		/// </summary>
		/// <param name="args">args input</param>
		/// <returns>bool</returns>
		public bool GetThumbnail(IReadOnlyList<string> args)
		{
			var isThumbnail = false;
			
			for (int arg = 0; arg < args.Count; arg++)
			{
				if ((args[arg].ToLower() == "--thumbnail" || args[arg].ToLower() == "-t") && (arg + 1) != args.Count)
				{
					bool.TryParse(args[arg + 1], out isThumbnail);
				}
			}
			
			if (_appSettings.Verbose) Console.WriteLine(">> GetThumbnail " + isThumbnail);
			return isThumbnail;
		}
	 
		/// <summary>
		/// Check for parent/subitems feature
		/// </summary>
		/// <param name="args">args input</param>
		/// <returns>bool</returns>
		public bool GetOrphanFolderCheck(IReadOnlyList<string> args)
		{
			var isOrphanFolderCheck = false;
			
			for (int arg = 0; arg < args.Count; arg++)
			{
				if ((args[arg].ToLower() == "--orphanfolder" || args[arg].ToLower() == "-o") && (arg + 1) != args.Count)
				{
					bool.TryParse(args[arg + 1], out isOrphanFolderCheck);
				}
			}
			
			if (_appSettings.Verbose) Console.WriteLine(">> isOrphanFolderCheck " + isOrphanFolderCheck);
			return isOrphanFolderCheck;
		}
	 
		/// <summary>
		/// Move files
		/// </summary>
		/// <param name="args">args input</param>
		/// <returns>bool, true=move</returns>
		public bool GetMove(IReadOnlyList<string> args)
		{
			var getMove = false;
			
			for (int arg = 0; arg < args.Count; arg++)
			{
				if ((args[arg].ToLower() == "--move" 
					|| args[arg].ToLower() == "-m") 
					&& (arg + 1) != args.Count)
				{
					bool.TryParse(args[arg + 1], out getMove);
				}
				
				if ((args[arg].ToLower() == "--move" || args[arg].ToLower() == "-m"))
				{
					getMove = true;
				}
			}
			return getMove;
		}
	 
		/// <summary>
		/// Get all --all true
		/// </summary>
		/// <param name="args">input args</param>
		/// <returns>bool</returns>
		public bool GetAll(IReadOnlyList<string> args)
		{
			// default false
			var getAll = false;
			
			for (int arg = 0; arg < args.Count; arg++)
			{
				if ((args[arg].ToLower() == "--all" || args[arg].ToLower() == "-a"))
				{
					getAll = true;
				}
				
				if ( ( args[arg].ToLower() != "--all" && args[arg].ToLower() != "-a" ) ||
					( arg + 1 ) == args.Count ) continue;
				
				if (args[arg + 1].ToLower() == "false") getAll = false;
			}
			return getAll;
		}
	 
		/// <summary>
		/// Recursive scan for folders
		/// </summary>
		/// <param name="args">input args</param>
		/// <returns>bool</returns>
		public bool NeedRecursive(IReadOnlyList<string> args)
		{
			bool needRecursive = false;
			
			for (int arg = 0; arg < args.Count; arg++)
			{
				if ((args[arg].ToLower() == "--recursive" || args[arg].ToLower() == "-r"))
				{
					needRecursive = true;
				}
			}
			return needRecursive;
		}
	 
		/// <summary>
		/// Need to remove caches
		/// </summary>
		/// <param name="args">input args</param>
		/// <returns>bool</returns>
		public bool NeedCleanup(IReadOnlyList<string> args)
		{
			// -x --clean
			bool needCacheCleanup = false;
			
			for (int arg = 0; arg < args.Count; arg++)
			{
				if ((args[arg].ToLower() == "--clean" || args[arg].ToLower() == "-x"))
				{
					needCacheCleanup = true;
				}
			}
			return needCacheCleanup;
		}

		/// <summary>
		/// Get colorClass value from args
		/// </summary>
		/// <param name="args">input args</param>
		/// <returns>number, but valid with colorClass</returns>
		public int GetColorClass(IReadOnlyList<string> args)
		{	
			// --colorclass
			var colorClass = -1;
			
			for (var arg = 0; arg < args.Count; arg++)
			{
				if ( args[arg].ToLower() != "--colorclass" || ( arg + 1 ) == args.Count ) continue;
				var colorClassString = args[arg + 1];
				var color =  ColorClassParser.GetColorClass(colorClassString);
				colorClass = (int) color;
			}
			return colorClass;
		}
	}
}
