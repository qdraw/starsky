using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
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

        private ImportService _import;
        private readonly Query _query;
        private readonly SyncService _isync;
        private readonly IExiftool _exiftool;
        private readonly AppSettings _appSettings;
        private ServiceProvider _serviceProvider;

        public ConfigCliAppsStartupHelper()
        {

            // Inject Fake Exiftool; dependency injection
            var services = new ServiceCollection();
            services.AddSingleton<IExiftool, ExifTool>();

            // Inject Config helper
            services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
            //             // Start using dependency injection

            Console.WriteLine(new AppSettings().BaseDirectoryProject  +  
                              Path.DirectorySeparatorChar + "appsettings.json");
            
            var builder = new ConfigurationBuilder()
                .AddJsonFile(
                    new AppSettings().BaseDirectoryProject  + 
                    Path.DirectorySeparatorChar + "appsettings.json", optional: false)
                .AddEnvironmentVariables();
                        
            // build config
            var configuration = builder.Build();
            // inject config as object to a service
            services.ConfigurePoco<AppSettings>(configuration.GetSection("App"));
            // build the service
            _serviceProvider = services.BuildServiceProvider();
            // get the service
            _appSettings = _serviceProvider.GetRequiredService<AppSettings>();

            // inject exiftool
            _exiftool = _serviceProvider.GetRequiredService<IExiftool>();

            // Build Datbase Context
            var builderDb = new DbContextOptionsBuilder<ApplicationDbContext>();
            
            if(_appSettings.Verbose) Console.WriteLine(_appSettings.DatabaseConnection);

            // Select database type
            switch (_appSettings.DatabaseType)
            {
                case Models.AppSettings.DatabaseTypeList.Mysql:
                    builderDb.UseMySql(_appSettings.DatabaseConnection);
                    break;
                case Models.AppSettings.DatabaseTypeList.InMemoryDatabase:
                    builderDb.UseInMemoryDatabase("Starsky");
                    break;
                case Models.AppSettings.DatabaseTypeList.Sqlite:
                    builderDb.UseSqlite(_appSettings.DatabaseConnection);
                    break;
                default:
                    builderDb.UseSqlite(_appSettings.DatabaseConnection);
                    break;
            }
            
            var options = builderDb.Options;
            var context = new ApplicationDbContext(options);
            _query = new Query(context);
            
            _isync = new SyncService(context, _query, _appSettings);

            // TOC:
            //   _context = context
            //   _isync = isync
            //   _exiftool = exiftool
            //   _appSettings = appSettings
            _import = new ImportService(context, _isync, _exiftool, _appSettings);
        }
        
        public AppSettings AppSettings()
        {
            return _serviceProvider.GetRequiredService<AppSettings>();
        }
        
        public ImportService ImportService()
        {
            return _import;
        }
        
        public SyncService SyncService()
        {
            return _isync;
        }
        
        // Only for CLI apps
        
//        public ConfigCliAppsStartupHelper()
//        {
//            // Depencency Injection for configuration
//            var services = new ServiceCollection();
//            services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
//
//            var builder = new ConfigurationBuilder()
//                .AddJsonFile(
//                    AppSettings().BaseDirectoryProject  + 
//                    Path.DirectorySeparatorChar + "appsettings.json", optional: false)
//                .AddEnvironmentVariables();
//            
//            var configuration = builder.Build();
//            services.ConfigurePoco<AppSettings>(configuration.GetSection("App"));
//            _serviceProvider = services.BuildServiceProvider();
//            // End of Depencency Injection for configuration
//        }
//        
//        private readonly IServiceProvider _serviceProvider;
//
//        public AppSettings AppSettings()
//        {
//            return _serviceProvider.GetRequiredService<AppSettings>();
//        }
            
    }
}