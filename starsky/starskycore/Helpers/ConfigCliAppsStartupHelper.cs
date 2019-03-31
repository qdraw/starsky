using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using starskycore.Data;
using starskycore.Interfaces;
using starskycore.Middleware;
using starskycore.Models;
using starskycore.Services;
using Query = starskycore.Services.Query;
using ReadMeta = starskycore.Services.ReadMeta;
using SyncService = starskycore.Services.SyncService;

namespace starskycore.Helpers
{
    public class ConfigCliAppsStartupHelper
    {

        private readonly ImportService _import;
        private readonly SyncService _isync;
        private readonly ServiceProvider _serviceProvider;
        private readonly ReadMeta _readmeta;
        private readonly IExifTool _exifTool;
	    private readonly ThumbnailCleaner _thumbnailCleaner;
	    private readonly IStorage _iStorage;

	    /// <summary>
        /// Inject all services for the CLI applications
        /// </summary>
        public ConfigCliAppsStartupHelper()
        {
            // Only for CLI apps

            // Inject Fake Exiftool; dependency injection
            var services = new ServiceCollection();

            // Inject Config helper
            services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
            //             // Start using dependency injection

            var builder = AppSettingsToBuilder();
                        
            // build config
            var configuration = builder.Build();
            // inject config as object to a service
            services.ConfigurePoco<AppSettings>(configuration.GetSection("App"));
	        
	        // Inject Filesystem backend
	        services.AddSingleton<IStorage, StorageSubPathFilesystem>();

	        // Inject ExifTool
	        services.AddSingleton<IExifTool, ExifTool>();
	        
	        
            // build the service
            _serviceProvider = services.BuildServiceProvider();
            // get the service
            var appSettings = _serviceProvider.GetRequiredService<AppSettings>();

            // inject exifTool
            _exifTool = _serviceProvider.GetRequiredService<IExifTool>();

            // Build Database Context
            var builderDb = new DbContextOptionsBuilder<ApplicationDbContext>();
            
            if(appSettings.Verbose) Console.WriteLine(appSettings.DatabaseConnection);

            // Select database type
            switch (appSettings.DatabaseType)
            {
                case Models.AppSettings.DatabaseTypeList.Mysql:
                    builderDb.UseMySql(appSettings.DatabaseConnection);
                    break;
                case Models.AppSettings.DatabaseTypeList.InMemoryDatabase:
                    builderDb.UseInMemoryDatabase("Starsky");
                    break;
                case Models.AppSettings.DatabaseTypeList.Sqlite:
                    builderDb.UseSqlite(appSettings.DatabaseConnection);
                    break;
                default:
                    builderDb.UseSqlite(appSettings.DatabaseConnection);
                    break;
            }
            
            var options = builderDb.Options;
            var context = new ApplicationDbContext(options);
            var query = new Query(context);

	        _iStorage = new StorageSubPathFilesystem(appSettings);
            
            _readmeta = new ReadMeta(_iStorage,appSettings);
            
            _isync = new SyncService(query, appSettings,_readmeta, _iStorage);
            
            // TOC:
            //   _context = context
            //   _isync = isync
            //   _exiftool = exiftool
            //   _appSettings = appSettings
            //   _readmeta = readmeta
            _import = new ImportService(context, _isync, _exifTool, appSettings, null, _iStorage, true);

	        _thumbnailCleaner = new ThumbnailCleaner(query, appSettings);
	        
        }

	    /// <summary>
	    /// Default appsettings.json to builder
	    /// </summary>
	    /// <returns>Configbuilder</returns>
	    public static ConfigurationBuilder AppSettingsToBuilder()
	    {
		    var appSettings = new AppSettings();
		    var builder = new ConfigurationBuilder();
		    
		    // to remove spaces and other signs, check help to get your name
		    var appSettingsMachine =
			    $"appsettings.{Environment.MachineName.ToLowerInvariant()}."; // dot here
		    
		    builder
			    .SetBasePath(appSettings.BaseDirectoryProject)
			    .AddJsonFile("appsettings.patch.json",true)
			    .AddJsonFile(appSettingsMachine + "patch.json", optional: true)
			    .AddJsonFile("appsettings.json",true)
			    .AddJsonFile(appSettingsMachine + "json", optional: true)
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
            return _serviceProvider.GetRequiredService<AppSettings>();
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
        public IExifTool ExifTool()
        {
            return _exifTool;
        }
	    
	    /// <summary>
	    /// Returns an filled ThumbnailCleaner Interface
	    /// </summary>
	    /// <returns>ReadMeta</returns>
	    public ThumbnailCleaner ThumbnailCleaner()
	    {
		    return _thumbnailCleaner;
	    }
	    
	    /// <summary>
	    /// Storage Container
	    /// </summary>
	    /// <returns>IStorage</returns>
	    public IStorage Storage()
	    {
		    return _iStorage;
	    }
	    
    }
}