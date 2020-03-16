using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Pomelo.EntityFrameworkCore.MySql.Storage;
using starsky.foundation.database.Data;
using starsky.foundation.injection;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starsky.foundation.thumbnailgeneration.Services;
using starskycore.Interfaces;
using starskycore.Middleware;
using starskycore.Models;
using starskycore.Services;
using Query = starsky.foundation.query.Services.Query;

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
	    private readonly UserManager _userManager;
	    private readonly IStorage _thumbnailStorage;
	    private readonly SelectorStorage _selectorStorage;
	    private readonly IStorage _hostFileSystemStorage;

	    /// <summary>
        /// Inject all services for the CLI applications
        /// </summary>
        public ConfigCliAppsStartupHelper()
        {
            // Only for CLI apps

            // dependency injection
            var services = new ServiceCollection();

            // Inject Config helper
            services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
            //             // Start using dependency injection

            var builder = AppSettingsToBuilder();
                        
            // build config
            var configuration = builder.Build();
            // inject config as object to a service
            services.ConfigurePoco<AppSettings>(configuration.GetSection("App"));
	        
            new RegisterDependencies().Configure(services);

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
                case starsky.foundation.platform.Models.AppSettings.DatabaseTypeList.Mysql:
                    builderDb.UseMySql(appSettings.DatabaseConnection, mySqlOptions =>
                    {
	                    mySqlOptions.CharSet(CharSet.Utf8Mb4);
	                    mySqlOptions.CharSetBehavior(CharSetBehavior.AppendToAllColumns);
	                    mySqlOptions.EnableRetryOnFailure(2);
                    });
                    break;
                case starsky.foundation.platform.Models.AppSettings.DatabaseTypeList.InMemoryDatabase:
                    builderDb.UseInMemoryDatabase("Starsky");
                    break;
                case starsky.foundation.platform.Models.AppSettings.DatabaseTypeList.Sqlite:
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
	        _thumbnailStorage = new StorageThumbnailFilesystem(appSettings);
	        
	        _selectorStorage = new SelectorStorage(_serviceProvider);
	        _hostFileSystemStorage =
		        _selectorStorage.Get(starsky.foundation.storage.Storage.SelectorStorage.StorageServices.HostFilesystem);
            _readmeta = new ReadMeta(_iStorage,appSettings);
            
            _userManager = new UserManager(context);
            
            _isync = new SyncService(query, appSettings, _selectorStorage);
            
            _import = new ImportService(context, _isync, _exifTool, appSettings, null, _selectorStorage);

	        _thumbnailCleaner = new ThumbnailCleaner(_thumbnailStorage, query, appSettings);
	        
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
	    public IStorage SubPathStorage()
	    {
		    return _iStorage;
	    }

	    public IStorage HostFileSystemStorage()
	    {
		    return _hostFileSystemStorage;
	    }
	    
	    /// <summary>
	    /// Returns an filled UserManager Interface
	    /// </summary>
	    /// <returns>UserManager</returns>
	    public IUserManager UserManager()
	    {
		    return _userManager;
	    }

	    /// <summary>
	    /// Thumbnail Storage
	    /// </summary>
	    /// <returns>IStorage component</returns>
	    public IStorage ThumbnailStorage()
	    {
		    return _thumbnailStorage;
	    }
	    /// <summary>
	    /// SelectorStorage
	    /// </summary>
	    /// <returns>SelectorStorage</returns>
	    public ISelectorStorage SelectorStorage()
	    {
		    return _selectorStorage;
	    }
	    
    }
}
