using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using starskycore.Data;
using starskycore.Helpers;
using starskycore.Interfaces;
using starskycore.Models;
using starskycore.Services;

namespace starskyNetFrameworkShared
{
    public class ConfigCliAppsStartupHelperNetFramework
    {

        private readonly ImportService _import;
        private readonly SyncService _isync;
        private readonly ReadMeta _readmeta;
        private readonly IExiftool _exiftool;
	    private readonly ThumbnailCleaner _thumbnailCleaner;
	    private AppSettings _appSettings;

	    public class AppSettingsJsonBase
	    {
		    public AppSettings app { get; set; }
	    }

	    
	    /// <summary>
        /// Inject all services for the CLI applications
        /// </summary>
        public ConfigCliAppsStartupHelperNetFramework()
	    {
		    
			var baseDirectoryProject = new AppSettings().BaseDirectoryProject;
	        var filePaths =  new List<string>
	        {
		        Path.Combine(baseDirectoryProject, $"appsettings.{Environment.MachineName.ToLowerInvariant()}.patch.json"),
		        Path.Combine(baseDirectoryProject, "appsettings.patch.json"),
		        Path.Combine(baseDirectoryProject, $"appsettings.{Environment.MachineName.ToLowerInvariant()}.json"),
		        Path.Combine(baseDirectoryProject, "appsettings.json"),
	        };

	        
	        var appSettingsString = new PlainTextFileHelper().ReadFirstFile(filePaths);

		    if ( !string.IsNullOrEmpty(appSettingsString) )
		    {
			    var appSettingsJsonBase = JsonConvert.DeserializeObject<AppSettingsJsonBase>(appSettingsString);
			    _appSettings = appSettingsJsonBase.app;    
		    }
		    else
		    {
			    _appSettings = new AppSettings();
		    }

			// Used to import Environment variables
			new ArgsHelper(_appSettings).SetEnvironmentToAppSettings();

		    
		    // for running sqlite
		    if ( _appSettings.DatabaseType == starskycore.Models.AppSettings.DatabaseTypeList.Sqlite )
		    {
			    SQLitePCL.Batteries.Init();
		    }
		    
		    
	        _exiftool = new ExifTool(_appSettings);


            // Build Datbase Context
            var builderDb = new DbContextOptionsBuilder<ApplicationDbContext>();
            
            if( _appSettings.Verbose) Console.WriteLine(_appSettings.DatabaseConnection);

            // Select database type
            switch ( _appSettings.DatabaseType)
            {
                case starskycore.Models.AppSettings.DatabaseTypeList.Mysql:
                    builderDb.UseMySql(_appSettings.DatabaseConnection);
                    break;
                case starskycore.Models.AppSettings.DatabaseTypeList.InMemoryDatabase:
                    builderDb.UseInMemoryDatabase("Starsky");
                    break;
                case starskycore.Models.AppSettings.DatabaseTypeList.Sqlite:
                    builderDb.UseSqlite(_appSettings.DatabaseConnection);
                    break;
                default:
                    builderDb.UseSqlite(_appSettings.DatabaseConnection);
                    break;
            }
            
            var options = builderDb.Options;
            var context = new ApplicationDbContext(options);
            var query = new Query(context);
            
            _readmeta = new ReadMeta(_appSettings);
            
            _isync = new SyncService(context, query, _appSettings,_readmeta);
            
            // TOC:
            //   _context = context
            //   _isync = isync
            //   _exiftool = exiftool
            //   _appSettingsJsonSettings = appSettings
            //   _readmeta = readmeta
			_import = new ImportService(context, _isync, _exiftool, _appSettings, _readmeta, null);

	        _thumbnailCleaner = new ThumbnailCleaner(query, _appSettings);
        }

        /// <summary>
        /// Returns an filled AppSettings Interface
        /// </summary>
        /// <returns>AppSettings</returns>
        public AppSettings AppSettings()
        {
	        return _appSettings;
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
