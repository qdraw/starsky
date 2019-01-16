using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using starsky.Models;
using starsky.Services;
using starskycore.Data;
using starskycore.Interfaces;
using starskycore.Middleware;
using starskycore.Services;
using Query = starsky.core.Services.Query;
using ReadMeta = starsky.core.Services.ReadMeta;
using SyncService = starskycore.Services.SyncService;

namespace starskycore.Helpers
{
    public class ConfigCliAppsStartupHelperNetFramework
    {

        private readonly ImportService _import;
        private readonly SyncService _isync;
        private readonly ReadMeta _readmeta;
        private readonly IExiftool _exiftool;
	    private readonly ThumbnailCleaner _thumbnailCleaner;
	    private AppBase _appSettings;

	    public class AppBase
	    {
		    public AppSettings app { get; set; }
	    }
	    
	    /// <summary>
        /// Inject all services for the CLI applications
        /// </summary>
        public ConfigCliAppsStartupHelperNetFramework()
        {
	        var newappSettings = new AppSettings();

	        var appSettingsLocation =
		        Path.Combine(newappSettings.BaseDirectoryProject, "appsettings.json"); 
	        
	        var appSettingsString = new PlainTextFileHelper().ReadFile(appSettingsLocation);
	        	        
	        _appSettings = JsonConvert.DeserializeObject<AppBase>(appSettingsString);
	        
	        _exiftool = new ExifTool(_appSettings.app);
	        
	        

            // Build Datbase Context
            var builderDb = new DbContextOptionsBuilder<ApplicationDbContext>();
            
            if(_appSettings.app.Verbose) Console.WriteLine(_appSettings.app.DatabaseConnection);

            // Select database type
            switch (_appSettings.app.DatabaseType)
            {
                case starsky.Models.AppSettings.DatabaseTypeList.Mysql:
                    builderDb.UseMySql(_appSettings.app.DatabaseConnection);
                    break;
                case starsky.Models.AppSettings.DatabaseTypeList.InMemoryDatabase:
                    builderDb.UseInMemoryDatabase("Starsky");
                    break;
                case starsky.Models.AppSettings.DatabaseTypeList.Sqlite:
                    builderDb.UseSqlite(_appSettings.app.DatabaseConnection);
                    break;
                default:
                    builderDb.UseSqlite(_appSettings.app.DatabaseConnection);
                    break;
            }
            
            var options = builderDb.Options;
            var context = new ApplicationDbContext(options);
            var query = new Query(context);
            
            _readmeta = new ReadMeta(_appSettings.app);
            
            _isync = new SyncService(context, query, _appSettings.app,_readmeta);
            
            // TOC:
            //   _context = context
            //   _isync = isync
            //   _exiftool = exiftool
            //   _appSettings = appSettings
            //   _readmeta = readmeta
            _import = new ImportService(context, _isync, _exiftool, _appSettings.app, _readmeta,null);

	        _thumbnailCleaner = new ThumbnailCleaner(query, _appSettings.app);
        }

	    public static ConfigurationBuilder AppSettingsToBuilder()
	    {
		    var appSettings = new AppSettings();
		    var builder = new ConfigurationBuilder();
		    
		    // to remove spaces and other signs, check help to get your name
		    var appSettingsMachine =
			    $"appsettings.{Environment.MachineName.ToLowerInvariant()}.json";
		    
		    builder
			    .SetBasePath(appSettings.BaseDirectoryProject)
			    .AddJsonFile("appsettings.json",true)
			    .AddJsonFile(appSettingsMachine, optional: true)
			    // overwrite envs
			    // current dir gives problems on linux arm
			    .AddEnvironmentVariables();
		    return builder;
	    }
        
        /// <summary>
        /// Returns an filled AppSettings Interface
        /// </summary>
        /// <returns>AppSettings</returns>
        public AppSettings AppSettings()
        {
            return new AppSettings();
        }
        
        /// <summary>
        /// Returns an filled ImportService Interface
        /// </summary>
        /// <returns>ImportService</returns>
        public ImportService ImportService()
        {
            return _import;
        }
        
        /// <summary>
        /// Returns an filled SyncService Interface
        /// </summary>
        /// <returns>SyncService</returns>
        public SyncService SyncService()
        {
            return _isync;
        }

        /// <summary>
        /// Returns an filled ReadMeta Interface
        /// </summary>
        /// <returns>ReadMeta</returns>
        public ReadMeta ReadMeta()
        {
            return _readmeta;
        }
    
        /// <summary>
        /// Returns an filled ExifTool Interface
        /// </summary>
        /// <returns>ExifTool</returns>
        public IExiftool ExifTool()
        {
            return _exiftool;
        }
	    
	    /// <summary>
	    /// Returns an filled ThumbnailCleaner Interface
	    /// </summary>
	    /// <returns>ReadMeta</returns>
	    public ThumbnailCleaner ThumbnailCleaner()
	    {
		    return _thumbnailCleaner;
	    }
    }
}