using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Services;

namespace starsky.foundation.platform.Helpers
{
	public sealed class ArgsHelper
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
		// --tempfolder -tf
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
		/// Use with appSettings
		/// </summary>
		/// <param name="appSettings">appSettings</param>
		/// <param name="console">Console log</param>
		public ArgsHelper(AppSettings appSettings, IConsole? console = null)
		{
			_appSettings = appSettings;
			if ( console != null )
			{
				_console = console;
			}
		}
		
		/// <summary>
		/// Console abstraction, use this instead of Console
		/// </summary>
		private readonly IConsole _console = new ConsoleWrapper();

		/// <summary>
		/// AppSettings
		/// </summary>
		private readonly AppSettings _appSettings = new AppSettings();
		
		/// <summary>
		/// Show debug information
		/// </summary>
		/// <param name="args">input args</param>
		/// <returns></returns>
		public static bool NeedVerbose(IReadOnlyList<string> args)
		{
			var needDebug = false;
			for (var arg = 0; arg < args.Count; arg++)
			{
				if ((args[arg].Equals("--verbose", StringComparison.CurrentCultureIgnoreCase) 
				     || args[arg].Equals("-v", StringComparison.CurrentCultureIgnoreCase) ) 
				    && (arg + 1) != args.Count 
					&& bool.TryParse(args[arg + 1], out var needDebugParsed))
				{
					needDebug = needDebugParsed;
				}
				if ((args[arg].Equals("--verbose", StringComparison.CurrentCultureIgnoreCase) || 
				     args[arg].Equals("-v", StringComparison.CurrentCultureIgnoreCase) ) )
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
			"-d","-c","-b","-f","-e","-u","-g","-x","-tf", "-dep"
		}.AsReadOnly();

		/// <summary>
		/// Long input args, use this order as 'ShortNameList' and 'EnvNameList'
		/// </summary>
		public readonly IEnumerable<string> LongNameList = new List<string>
		{
			"--databasetype","--connection","--basepath","--thumbnailtempfolder",
			"--exiftoolpath","--structure","--subpathrelative", "--clean", "--tempfolder",
			"--dependencies"
		}
		.AsReadOnly();

		/// <summary>
		/// name of the env__ (__=:) use this order as 'LongNameList' and 'ShortNameList'
		/// </summary>
		public readonly IEnumerable<string> EnvNameList = new List<string>
		{
			"app__DatabaseType","app__DatabaseConnection","app__StorageFolder","app__ThumbnailTempFolder",
			"app__ExifToolPath", "app__Structure", "app__subpathrelative", "app__ExifToolImportXmpCreate", 
			"app__TempFolder", "app__DependenciesFolder"
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
			for (var i = 0; i < ShortNameList.Count(); i++)
			{
				for (var arg = 0; arg < args.Count; arg++)
				{
					if ((args[arg].Equals(longNameList[i], StringComparison.CurrentCultureIgnoreCase) || 
						args[arg].Equals(shortNameList[i], StringComparison.CurrentCultureIgnoreCase) ) && (arg + 1) != args.Count)
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
					var propertyObject = _appSettings.GetType().GetProperty(envName);
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
		public static bool NeedHelp(IReadOnlyList<string> args)
		{
			var needHelp = false;
			for (var arg = 0; arg < args.Count; arg++)
			{
				if ((args[arg].Equals("--help", StringComparison.CurrentCultureIgnoreCase) || 
				     args[arg].Equals("-h", StringComparison.CurrentCultureIgnoreCase) ) && (arg + 1) != args.Count 
				    && bool.TryParse(args[arg + 1], out var needHelp2) && needHelp2)
				{
					needHelp = true;
				}
				if (args[arg].Equals("--help", StringComparison.CurrentCultureIgnoreCase) ||
				    args[arg].Equals("-h", StringComparison.CurrentCultureIgnoreCase) )
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
		[SuppressMessage("Usage", "S2068:password detected here, make sure this is not a hard-coded credential")]
		public void NeedHelpShowDialog()
		{
			if ( _appSettings == null )
			{
				throw new FieldAccessException("use with _appSettings");
			}
			
			_console.WriteLine("Starsky " + _appSettings.ApplicationType + " Cli ~ Help:");
			_console.WriteLine("--help or -h == help (this window)");
			
			switch (_appSettings.ApplicationType)
			{
				case AppSettings.StarskyAppType.Thumbnail:
					
					_console.WriteLine("-t == enable thumbnail (default true)");
					_console.WriteLine("--path or -p == parameter: (string) ; " +
					                   "'full path', only child items of the database folder are supported," +
					                   "search and replace first part of the filename, '/', use '-p' for current directory ");
					_console.WriteLine("--subpath or -s == parameter: (string) ; relative path in the database");
					_console.WriteLine("--subpathrelative or -g == Overwrite sub-path to use relative days to select a folder" +
					                   ", use for example '1' to select yesterday. (structure is required)");
					_console.WriteLine("-p, -s, -g == you need to select one of those tags");
					
					_console.WriteLine("recursive is enabled by default");
					break;
				case AppSettings.StarskyAppType.MetaThumbnail:
					_console.WriteLine("--path or -p == parameter: (string) ; " +
					                   "'full path', only child items of the database folder are supported," +
					                   "search and replace first part of the filename, '/', use '-p' for current directory ");
					_console.WriteLine("--subpath or -s == parameter: (string) ; relative path in the database");
					_console.WriteLine("--subpathrelative or -g == Overwrite sub-path to use relative days to select a folder" +
					                   ", use for example '1' to select yesterday. (structure is required)");
					_console.WriteLine("-p, -s, -g == you need to select one of those tags");
					
					_console.WriteLine("recursive is enabled by default");
					break;
				case AppSettings.StarskyAppType.Admin:
					_console.WriteLine("--name or -n == string ; username / email");
					_console.WriteLine("--password == string ; password");
					break;
				case AppSettings.StarskyAppType.Geo:
					// When this change please update ./readme.md
					_console.WriteLine("--path or -p == parameter: (string) ; " +
					                   "without addition is current directory, full path (all locations are supported) ");
					_console.WriteLine("--subpath or -s == parameter: (string) ; relative path in the database ");
					_console.WriteLine("--subpathrelative or -g == Overwrite subpath to use relative days to select a folder" +
					                   ", use for example '1' to select yesterday. (structure is required)");
					_console.WriteLine("-p, -s, -g == you need to select one of those tags");
					_console.WriteLine("--all or -a == overwrite reverse geotag location tags " +
					                   "(default: false / ignore already taged files) ");
					_console.WriteLine("--index or -i == parameter: (bool) ; gpx feature to index geo location, default true");
				break;
				case AppSettings.StarskyAppType.WebHtml:
					// When this change please update ./readme.md
					_console.WriteLine("--path or -p == parameter: (string) ; full path (select a folder), " +
					                   "use '-p' for current directory");
					_console.WriteLine("--name or -n == parameter: (string) ; name of blog item ");
				break;
				case AppSettings.StarskyAppType.Importer:
					// When this change please update ./readme.md
					_console.WriteLine("--path or -p == parameter: (string) ; full path");
					_console.WriteLine("                can be an folder or file, use '-p' for current directory");
					_console.WriteLine("                for multiple items use dot comma (;) " +
					                   "to split and quotes (\") around the input string");
					_console.WriteLine("--move or -m == delete file after importing (default false / copy file)");
					_console.WriteLine("--recursive or -r == Import Directory recursive " +
					                   "(default: false / only the selected folder) ");
					_console.WriteLine("--structure == overwrite app-settings with file-directory structure "+
					                   "based on exif and filename create datetime");
					_console.WriteLine("--index or -i == parameter: (bool) ; indexing, false is always copy," +
					                   " true is check if exist in db, default true");
					_console.WriteLine("--clean or -x == true is to add a xmp sidecar file for raws, default true");
					_console.WriteLine("--colorclass == update color-class to this number value, default don't change");
				break;
				case AppSettings.StarskyAppType.Sync:
					// When this change please update ./readme.md
					_console.WriteLine("--path or -p == parameter: (string) ; " +
					                   "'full path', only child items of the database folder are supported," +
					                   "search and replace first part of the filename, '/', use '-p' for current directory ");
					_console.WriteLine("--subpath or -s == parameter: (string) ; relative path in the database");
					_console.WriteLine("--subpathrelative or -g == Overwrite sub-path to use relative days to select a folder" +
					                   ", use for example '1' to select yesterday. (structure is required)");
					_console.WriteLine("-p, -s, -g == you need to select one of those tags");
					_console.WriteLine("--index or -i == parameter: (bool) ; enable indexing, default true");
					_console.WriteLine("--thumbnail or -t == parameter: (bool) ; enable thumbnail, default false");
					_console.WriteLine("--clean or -x == parameter: (bool) ; enable checks in thumbnail-temp-folder" +
					                   " if thumbnails are needed, delete unused files");
					_console.WriteLine("--orphanfolder or -o == To delete files without a parent folder " +
					                   "(heavy cpu usage), default false");
					_console.WriteLine("--verbose or -v == verbose, more detailed info");
					_console.WriteLine("--databasetype or -d == Overwrite EnvironmentVariable for DatabaseType");
					_console.WriteLine("--basepath or -b == Overwrite EnvironmentVariable for StorageFolder");
					_console.WriteLine("--connection or -c == Overwrite EnvironmentVariable for DatabaseConnection");
					_console.WriteLine("--thumbnailtempfolder or -f == Overwrite EnvironmentVariable for ThumbnailTempFolder");
					_console.WriteLine("--exiftoolpath or -e == Overwrite EnvironmentVariable for ExifToolPath");
				break;
			}
			
			_console.WriteLine("--verbose or -v == verbose, more detailed info");
			_console.WriteLine("  use -v -help to show settings: ");
			
			if (!_appSettings.IsVerbose()) return;
			
			_console.WriteLine(string.Empty);
			_console.WriteLine("AppSettings: " + _appSettings.ApplicationType);
			_console.WriteLine("Database Type (-d --databasetype) "+ _appSettings.DatabaseType);
			_console.WriteLine("DatabaseConnection (-c --connection) " + _appSettings.DatabaseConnection);
			_console.WriteLine($"StorageFolder (-b --basepath) {_appSettings.StorageFolder} ");
			_console.WriteLine($"ThumbnailTempFolder (-f --thumbnailtempfolder) {_appSettings.ThumbnailTempFolder} ");
			_console.WriteLine($"ExifToolPath  (-e --exiftoolpath) {_appSettings.ExifToolPath} ");
			_console.WriteLine("Structure  (-u --structure) "+ _appSettings.Structure);
			_console.WriteLine("CameraTimeZone "+ _appSettings.CameraTimeZone);
			_console.WriteLine("Name " + _appSettings.Name);
			_console.WriteLine($"TempFolder {_appSettings.TempFolder} ");
			_console.WriteLine($"BaseDirectoryProject {_appSettings.BaseDirectoryProject} ");
			_console.WriteLine("ExiftoolSkipDownloadOnStartup "+ _appSettings.ExiftoolSkipDownloadOnStartup);
			_console.WriteLine("GeoFilesSkipDownloadOnStartup "+ _appSettings.GeoFilesSkipDownloadOnStartup);			
			
			_console.WriteLine($"MaxDegreesOfParallelism {_appSettings.MaxDegreesOfParallelism} ");

			_console.Write("SyncIgnore ");
			foreach ( var rule in _appSettings.SyncIgnore ) _console.Write($"{rule}, ");
			_console.Write("\n");
			
			_console.Write("ImportIgnore ");
			foreach ( var rule in _appSettings.ImportIgnore ) _console.Write($"{rule}, ");
			_console.Write("\n");
			
			if ( _appSettings.ApplicationType == AppSettings.StarskyAppType.Importer)
				_console.WriteLine("Create xmp on import (ExifToolImportXmpCreate): " + _appSettings.ExifToolImportXmpCreate);
			
			if ( _appSettings.ApplicationType == AppSettings.StarskyAppType.WebFtp) 
				_console.WriteLine("WebFtp " + _appSettings.WebFtp);

			if ( _appSettings.ApplicationType == AppSettings.StarskyAppType.Admin )
			{
				_console.WriteLine("NoAccountLocalhost " + _appSettings.NoAccountLocalhost);
			}
			
			_console.WriteLine("-- Appsettings.json locations -- ");
			
			var machineName = Environment.MachineName.ToLowerInvariant();
			
			_console.WriteLine("Config is read in this order: (latest is applied over lower numbers)");
			_console.WriteLine( $"1. {Path.Combine(_appSettings.BaseDirectoryProject, "appsettings.json")}");
			_console.WriteLine( $"2. {Path.Combine(_appSettings.BaseDirectoryProject, "appsettings.default.json")}");
			_console.WriteLine( $"3. {Path.Combine(_appSettings.BaseDirectoryProject, "appsettings.patch.json")}");
			_console.WriteLine( $"4. {Path.Combine(_appSettings.BaseDirectoryProject, "appsettings." + machineName + ".json")}");
			_console.WriteLine( $"5. {Path.Combine(_appSettings.BaseDirectoryProject,  "appsettings." + machineName + ".patch.json")}");
			_console.WriteLine( $"6. Environment variable: app__appsettingspath: {Environment.GetEnvironmentVariable("app__appsettingspath")}");
			_console.WriteLine("7. Specific environment variables for example app__storageFolder");

			AppSpecificHelp();

			ShowVersions();
		}

		private void AppSpecificHelp()
		{
			switch ( _appSettings.ApplicationType )
			{
				case AppSettings.StarskyAppType.WebHtml: 
					_console.WriteLine($"Config for {_appSettings.ApplicationType}");
					if ( _appSettings.PublishProfiles == null )
					{
						break;
					}
					
					foreach ( var publishProfiles in _appSettings.PublishProfiles )
					{
						_console.WriteLine($"ID: {publishProfiles.Key}" );
						foreach ( var publishProfile in publishProfiles.Value )
						{
							_console.WriteLine("--- " +
							                   $"Path: {publishProfile.Path} " +
							                   $"Append: {publishProfile.Append} " +
							                   $"Copy: {publishProfile.Copy} " +
							                   $"Folder: {publishProfile.Folder} " +
							                   $"Prepend: {publishProfile.Prepend} " +
							                   $"Template: {publishProfile.Template} " +
							                   $"ContentType: {publishProfile.ContentType} " +
							                   $"MetaData: {publishProfile.MetaData} " +
							                   $"OverlayMaxWidth: {publishProfile.OverlayMaxWidth} " +
							                   $"SourceMaxWidth: {publishProfile.SourceMaxWidth} ");
						}
					}
					break;
			}
		}

		/// <summary>
		/// Show in Console the .NET Version (Runtime) and Starsky Version
		/// @see: https://stackoverflow.com/a/58136318
		/// </summary>
		private void ShowVersions()
		{
			var version = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription;
			_console.WriteLine($".NET Version - {version}");
			_console.WriteLine($"Starsky Version - {_appSettings.AppVersion} " +
			                   "- build at: " +
			                   DateAssembly.GetBuildDate(Assembly.GetExecutingAssembly()).ToString(
				                   new CultureInfo("nl-NL")));
		}
		
		/// <summary>
		/// Default On
		/// Based on args get the -i or --index commandline input
		/// </summary>
		/// <param name="args">args input</param>
		/// <returns>bool, true if --index</returns>
		public static bool GetIndexMode(IReadOnlyList<string> args)
		{
			var isIndexMode = true;
			
			for (var arg = 0; arg < args.Count; arg++)
			{
				if ((args[arg].Equals("--index", StringComparison.CurrentCultureIgnoreCase) || 
				     args[arg].Equals("-i", StringComparison.CurrentCultureIgnoreCase) ) 
				    && (arg + 1) != args.Count 
				    && bool.TryParse(args[arg + 1], out var isIndexMode2))
				{
					isIndexMode = isIndexMode2;
				}
			}
		
			return isIndexMode;
		}
	 
		/// <summary>
		/// Get multiple path from args
		/// </summary>
		/// <param name="args">args</param>
		/// <returns>list of fullFilePaths</returns>
		/// <exception cref="FieldAccessException">_appSettings is missing</exception>
		[SuppressMessage("Usage", "S125:Remove this commented out code.", Justification = "Regex as comment")]
		public List<string> GetPathListFormArgs(IReadOnlyList<string> args)
		{
			if ( _appSettings == null ) throw new FieldAccessException("use with _appSettings");
			var path = GetUserInputPathFromArg(args);
			
			// To use only with -p or --path > current directory
			if ( (args.Contains("-p") || args.Contains("--path") ) && (path == string.Empty || path[0] == "-"[0]))
			{
				path = Directory.GetCurrentDirectory();
			}

			// Ignore quotes at beginning: unescaped ^"|"$
			path = new Regex("^\"|\"$", 
				RegexOptions.None, TimeSpan.FromMilliseconds(100))
				.Replace(path, string.Empty);
			
			// split every dot comma but ignore escaped
			// non escaped: (?<!\\);
			var dotCommaRegex = new Regex("(?<!\\\\);", 
				RegexOptions.None, TimeSpan.FromMilliseconds(100));
			return dotCommaRegex.Split(path).Where(p => !string.IsNullOrWhiteSpace(p)).ToList();
		}

		/// <summary>
		/// Get the user input from -p or --path
		/// </summary>
		/// <param name="args">arg list</param>
		/// <returns>path</returns>
		private static string GetUserInputPathFromArg(IReadOnlyList<string> args)
		{
			var path = string.Empty;
			for (var arg = 0; arg < args.Count; arg++)
			{
				if ((args[arg].Equals("--path", StringComparison.CurrentCultureIgnoreCase) || 
				     args[arg].Equals("-p", StringComparison.CurrentCultureIgnoreCase) ) && (arg + 1) != args.Count )
				{
					path = args[arg + 1];
				}
			}
			return path;
		}
		
		/// <summary>
		/// Get the user input from -p or --password
		/// </summary>
		/// <param name="args">arg list</param>
		/// <returns>path</returns>
		public static string GetUserInputPassword(IReadOnlyList<string> args)
		{
			var path = string.Empty;
			for (var arg = 0; arg < args.Count; arg++)
			{
				if ((args[arg].Equals("--password", StringComparison.CurrentCultureIgnoreCase) || 
				     args[arg].Equals("-p", StringComparison.CurrentCultureIgnoreCase) ) && (arg + 1) != args.Count )
				{
					path = args[arg + 1];
				}
			}
			return path;
		}
		
		/// <summary>
		/// Get output mode
		/// </summary>
		/// <param name="args">arg list</param>
		/// <returns>path</returns>
		public static ConsoleOutputMode GetConsoleOutputMode(IReadOnlyList<string> args)
		{
			var outputMode = ConsoleOutputMode.Default;
			for (var arg = 0; arg < args.Count; arg++)
			{
				if ( ( !args[arg].Equals("--output", StringComparison.CurrentCultureIgnoreCase) ) ||
				     ( arg + 1 ) == args.Count ) continue;
				var outputModeItem = args[arg + 1];
				Enum.TryParse(outputModeItem, true, out outputMode);
			}
			return outputMode;
		}
		
		/// <summary>
		/// Get the user input from -n or --name
		/// </summary>
		/// <param name="args">arg list</param>
		/// <returns>name</returns>
		public static string GetName(IReadOnlyList<string> args)
		{
			var name = string.Empty;
			for (var arg = 0; arg < args.Count; arg++)
			{
				if ((args[arg].Equals("--name", StringComparison.CurrentCultureIgnoreCase) || 
				     args[arg].Equals("-n", StringComparison.CurrentCultureIgnoreCase) ) && (arg + 1) != args.Count )
				{
					name = args[arg + 1];
				}
			}
			return name;
		}
		
		/// <summary>
		/// Get path from args
		/// </summary>
		/// <param name="args">args</param>
		/// <param name="dbStyle">convert to subPath style, default=true</param>
		/// <returns>string path</returns>
		/// <exception cref="FieldAccessException">appSettings is missing</exception>
		public string GetPathFormArgs(IReadOnlyList<string> args, bool dbStyle = true)
		{
			if ( _appSettings == null ) throw new FieldAccessException("use with _appSettings");

			var path = GetUserInputPathFromArg(args);
			
			// To use only with -p or --path > current directory
			if ( (args.Contains("-p") || args.Contains("--path") ) && (path == string.Empty || path[0] == "-"[0]))
			{
				var currentDirectory = Directory.GetCurrentDirectory();
				if ( currentDirectory != _appSettings.BaseDirectoryProject )
				{
					path = currentDirectory;
					if ( _appSettings.IsVerbose() ) Console.WriteLine($">> currentDirectory: {currentDirectory}");
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
		/// <returns>subPath string</returns>
		public static string GetSubPathFormArgs(IReadOnlyList<string> args)
		{
			var subPath = "/";
			
			for (var arg = 0; arg < args.Count; arg++)
			{
				if ((args[arg].Equals("--subpath", StringComparison.CurrentCultureIgnoreCase) || 
				     args[arg].Equals("-s", StringComparison.CurrentCultureIgnoreCase) ) && (arg + 1) != args.Count)
				{
					subPath = args[arg + 1];
				}
			}
			return subPath;
		}

		/// <summary>
		/// Get subPathRelative, so a structured url based on relative datetime
		/// </summary>
		/// <param name="args">args[]</param>
		/// <returns>relative subPath</returns>
		/// <exception cref="FieldAccessException">missing appSettings</exception>
		public int? GetRelativeValue(IReadOnlyList<string> args)
		{
			if (_appSettings == null) throw new FieldAccessException("use with _appSettings");
			var subPathRelative = string.Empty;
			
			for (int arg = 0; arg < args.Count; arg++)
			{
				if ((args[arg].Equals("--subpathrelative", StringComparison.InvariantCultureIgnoreCase) || 
					args[arg].Equals("-g", StringComparison.InvariantCultureIgnoreCase) ) && (arg + 1) != args.Count)
				{
					subPathRelative = args[arg + 1];
				}
			}
			
			if (string.IsNullOrWhiteSpace(subPathRelative)) return null; // null

			if ( int.TryParse(subPathRelative, out var subPathInt) && subPathInt >= 1 )
			{
				subPathInt *= -1; // always in the past
			}
			
			// Fallback for dates older than 24-11-1854 to avoid a exception.
			if ( subPathInt < -60000 ) return null;
			return subPathInt;
		}
		
		/// <summary>
		/// Know if --subpath or --path
		/// </summary>
		/// <param name="args">input[]</param>
		/// <returns>bool --path(true), false --subpath</returns>
		public static bool IsSubPathOrPath(IReadOnlyList<string> args)
		{
			// To use only with -p or --path > current directory
			if ( args.Any(arg => (arg.Equals("--path", StringComparison.CurrentCultureIgnoreCase) || 
			                      arg.Equals("-p", StringComparison.CurrentCultureIgnoreCase) )) )
			{
				return false;
			}
			
			// Detect if a input is a fullPath or a subPath.
			for (int arg = 0; arg < args.Count; arg++)
			{
				if ((args[arg].Equals("--subpath", StringComparison.CurrentCultureIgnoreCase) || 
				     args[arg].Equals("-s", StringComparison.CurrentCultureIgnoreCase) ) && (arg + 1) != args.Count)
				{
					return true;
				}
			}
			return true;
		}

		/// <summary>
		/// Using both options
		/// -s = if subPath || -p is path
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>
		public string SubPathOrPathValue(IReadOnlyList<string> args)
		{
			return IsSubPathOrPath(args) ? GetSubPathFormArgs(args) : GetPathFormArgs(args);
		}
	 
		/// <summary>
		/// --thumbnail bool
		/// </summary>
		/// <param name="args">args input</param>
		/// <returns>bool</returns>
		public static bool GetThumbnail(IReadOnlyList<string> args)
		{
			var isThumbnail = true;
			
			for (var arg = 0; arg < args.Count; arg++)
			{
				if ((args[arg].Equals("--thumbnail", StringComparison.CurrentCultureIgnoreCase) ||
				     args[arg].Equals("-t", StringComparison.CurrentCultureIgnoreCase) ) 
				    && (arg + 1) != args.Count && bool.TryParse(args[arg + 1], out var isThumbnail2))
				{
					isThumbnail = isThumbnail2;
				}
			}
			
			return isThumbnail;
		}
	 
		/// <summary>
		/// Check for parent/sub items feature
		/// </summary>
		/// <param name="args">args input</param>
		/// <returns>bool</returns>
		public bool GetOrphanFolderCheck(IReadOnlyList<string> args)
		{
			var isOrphanFolderCheck = false;
			
			for (var arg = 0; arg < args.Count; arg++)
			{
				if ((args[arg].Equals("--orphanfolder", StringComparison.CurrentCultureIgnoreCase) ||
				     args[arg].Equals("-o", StringComparison.CurrentCultureIgnoreCase) ) 
				    && (arg + 1) != args.Count && bool.TryParse(args[arg + 1], out var isOrphanFolderCheck2))
				{
					isOrphanFolderCheck = isOrphanFolderCheck2;
				}
			}
			
			if (_appSettings.IsVerbose()) Console.WriteLine(">> isOrphanFolderCheck " + isOrphanFolderCheck);
			return isOrphanFolderCheck;
		}
	 
		/// <summary>
		/// Move files
		/// </summary>
		/// <param name="args">args input</param>
		/// <returns>bool, true=move</returns>
		public static bool GetMove(IReadOnlyList<string> args)
		{
			var getMove = false;
			
			for (var arg = 0; arg < args.Count; arg++)
			{
				if ((args[arg].Equals("--move", StringComparison.CurrentCultureIgnoreCase)
					|| args[arg].Equals("-m", StringComparison.CurrentCultureIgnoreCase) ) 
					&& (arg + 1) != args.Count && bool.TryParse(args[arg + 1], out var getMove2))
				{
					getMove = getMove2;
					continue;
				}
				
				if ((args[arg].Equals("--move", StringComparison.CurrentCultureIgnoreCase) ||
				     args[arg].Equals("-m", StringComparison.CurrentCultureIgnoreCase) ) )
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
		public static bool GetAll(IReadOnlyList<string> args)
		{
			// default false
			var getAll = false;
			
			for (int arg = 0; arg < args.Count; arg++)
			{
				if ((args[arg].Equals("--all", StringComparison.CurrentCultureIgnoreCase) || 
				     args[arg].Equals("-a", StringComparison.CurrentCultureIgnoreCase) ) )
				{
					getAll = true;
				}

				if ( ( !args[arg].Equals("--all",
					       StringComparison.CurrentCultureIgnoreCase) &&
				       !args[arg].Equals("-a",
					       StringComparison.CurrentCultureIgnoreCase) ) ||
				     ( arg + 1 ) == args.Count )
				{
					continue;
				}

				if ( args[arg + 1].Equals("false",
					    StringComparison.CurrentCultureIgnoreCase) )
				{
					getAll = false;
				}
			}
			return getAll;
		}
	 
		/// <summary>
		/// Recursive scan for folders
		/// </summary>
		/// <param name="args">input args</param>
		/// <returns>bool</returns>
		public static bool NeedRecursive(IReadOnlyList<string> args)
		{
			bool needRecursive = false;
			
			foreach ( var arg in args )
			{
				if ((arg.Equals("--recursive", StringComparison.CurrentCultureIgnoreCase) || 
				     arg.Equals("-r", StringComparison.CurrentCultureIgnoreCase) ) )
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
		public static bool NeedCleanup(IReadOnlyList<string> args)
		{
			// -x --clean
			bool needCacheCleanup = false;
			
			foreach ( var arg in args )
			{
				if ((arg.Equals("--clean", StringComparison.CurrentCultureIgnoreCase) || 
				     arg.Equals("-x", StringComparison.CurrentCultureIgnoreCase) ) )
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
		public static int GetColorClass(IReadOnlyList<string> args)
		{	
			// --colorclass
			var colorClass = -1;
			
			for (var arg = 0; arg < args.Count; arg++)
			{
				if ( !args[arg].Equals("--colorclass",
					     StringComparison.CurrentCultureIgnoreCase) ||
				     ( arg + 1 ) == args.Count )
				{
					continue;
				}
				
				var colorClassString = args[arg + 1];
				var color =  ColorClassParser.GetColorClass(colorClassString);
				colorClass = (int) color;
			}
			return colorClass;
		}
	}
}
