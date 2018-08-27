using System;
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
        private ReadMeta _readmeta;

        public ConfigCliAppsStartupHelper()
        {
            // Only for CLI apps

            // Inject Fake Exiftool; dependency injection
            var services = new ServiceCollection();
            services.AddSingleton<IExiftool, ExifTool>();

            // Inject Config helper
            services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
            //             // Start using dependency injection


            var builder = new ConfigurationBuilder();
            if (File.Exists(new AppSettings().BaseDirectoryProject + "appsettings.json"))
            {
                Console.WriteLine("loaded json > " + new AppSettings().BaseDirectoryProject  + "appsettings.json");
                builder.AddJsonFile(
                    new AppSettings().BaseDirectoryProject + "appsettings.json", optional: false);
            }
            
            // overwrite envs
            builder.AddEnvironmentVariables();
                        
            // build config
            var configuration = builder.Build();
            // inject config as object to a service
            services.ConfigurePoco<AppSettings>(configuration.GetSection("App"));
            // build the service
            _serviceProvider = services.BuildServiceProvider();
            // get the service
            var appSettings = _serviceProvider.GetRequiredService<AppSettings>();

            // inject exiftool
            var exiftool = _serviceProvider.GetRequiredService<IExiftool>();

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
            _import = new ImportService(context, _isync, exiftool, appSettings, _readmeta);
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

        public ReadMeta ReadMeta()
        {
            return _readmeta;
        }
    }
}