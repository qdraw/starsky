﻿using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using starsky.Data;
using starsky.Interfaces;
using starsky.Middleware;
using starsky.Models;
using starsky.Services;

namespace starsky.Helpers
{
    public class ConfigCliAppsStartupHelper
    {

        private readonly ImportService _import;
        private readonly SyncService _isync;
        private readonly ServiceProvider _serviceProvider;
        private readonly ReadMeta _readmeta;
        private readonly IExiftool _exiftool;
	    private readonly ThumbnailCleaner _thumbnailCleaner;

	    /// <summary>
        /// Inject all services for the CLI applications
        /// </summary>
        public ConfigCliAppsStartupHelper()
        {
            // Only for CLI apps

            // Inject Fake Exiftool; dependency injection
            var services = new ServiceCollection();
            services.AddSingleton<IExiftool, ExifTool>();

            // Inject Config helper
            services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
            //             // Start using dependency injection

            var builder = AppSettingsToBuilder();
                        
            // build config
            var configuration = builder.Build();
            // inject config as object to a service
            services.ConfigurePoco<AppSettings>(configuration.GetSection("App"));
            // build the service
            _serviceProvider = services.BuildServiceProvider();
            // get the service
            var appSettings = _serviceProvider.GetRequiredService<AppSettings>();

            // inject exiftool
            _exiftool = _serviceProvider.GetRequiredService<IExiftool>();

            // Build Datbase Context
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
            
            _readmeta = new ReadMeta(appSettings);
            
            _isync = new SyncService(context, query, appSettings,_readmeta);
            
            // TOC:
            //   _context = context
            //   _isync = isync
            //   _exiftool = exiftool
            //   _appSettings = appSettings
            //   _readmeta = readmeta
            _import = new ImportService(context, _isync, _exiftool, appSettings, _readmeta,null);

	        _thumbnailCleaner = new ThumbnailCleaner(query, appSettings);
        }

	    public static ConfigurationBuilder AppSettingsToBuilder()
	    {
		    Console.WriteLine(Environment.MachineName.ToLower());
		    var appSettings = new AppSettings();
		    var builder = new ConfigurationBuilder();
		    
		    // to remove spaces and other signs, check help to get your name
		    var machineName = Environment.MachineName.ToLowerInvariant();
		    builder
			    .SetBasePath(appSettings.BaseDirectoryProject)
			    .AddJsonFile("appsettings.json",true)
			    .AddJsonFile($"appsettings.{machineName}.json",true)
			    .SetBasePath(Directory.GetCurrentDirectory())
			    .AddJsonFile("appsettings.json",true)
			    .AddJsonFile($"appsettings.{machineName}.json",true)
			    // overwrite envs
			    .AddEnvironmentVariables();
		    return builder;
	    }

//	    private ConfigurationBuilder AppSettingsToBuilder2()
//	    {
//		    var appSettings = new AppSettings();
//		    var builder = new ConfigurationBuilder();
//
//		    var addFileOnPrio = new List<string>
//		    {
//			    Path.Join(appSettings.BaseDirectoryProject,$"appsettings.{Environment.MachineName.ToLower()}.json"),
//			    Path.Join(appSettings.BaseDirectoryProject,"appsettings.json"),
//			    Path.Join(Directory.GetCurrentDirectory(),$"appsettings.{Environment.MachineName.ToLower()}.json"),
//			    Path.Join(Directory.GetCurrentDirectory(),"appsettings.json"),
//		    };
//
//		    foreach ( var filePath in addFileOnPrio )
//		    {
//			    Console.WriteLine(filePath);
//			    if ( !File.Exists(filePath) ) continue;
//			    builder.AddJsonFile(filePath);
//			    builder.AddEnvironmentVariables();
//			    return  builder;
//		    }
//		    // overwrite envs			    
//		    builder.AddEnvironmentVariables();
//		    return builder;
//	    }
        
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