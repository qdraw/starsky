using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.AspNetCore.Mvc.Razor.Internal;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.Language;
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

        private readonly ImportService _import;
        private readonly SyncService _isync;
        private readonly ServiceProvider _serviceProvider;
//        private ViewRenderService _viewRender;


        public ConfigCliAppsStartupHelper(bool isViewRender = false)
        {
            // Only for CLI apps

            // Inject Fake Exiftool; dependency injection
            var services = new ServiceCollection();
            services.AddSingleton<IExiftool, ExifTool>();

//            if (isViewRender)
//            {
//                services.AddSingleton<IRazorPageFactoryProvider,DefaultRazorPageFactoryProvider>();
//                services.AddSingleton<IViewCompilerProvider,RazorViewCompilerProvider>();
//                services.AddSingleton<IRazorViewEngine,RazorViewEngine>();
//                services.AddSingleton<ApplicationPartManager>();
//                services.AddSingleton<RazorTemplateEngine>();
//                services.AddSingleton<Microsoft.AspNetCore.Razor.Language.RazorEngine>();
//
//                
//                services.AddSingleton<ITempDataProvider,SessionStateTempDataProvider>();
//                services.AddSingleton<IViewRenderService, ViewRenderService>();
//            }

            // Inject Config helper
            services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
            //             // Start using dependency injection


            var builder = new ConfigurationBuilder();
            if (File.Exists(new AppSettings().BaseDirectoryProject + "appsettings.json"))
            {
                Console.WriteLine("loaded json > " +new AppSettings().BaseDirectoryProject  + "appsettings.json");
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
            
            var readmeta = new ReadMeta(appSettings);
            
            _isync = new SyncService(context, query, appSettings,readmeta);

//            if (isViewRender)
//            {
//                var razorViewEngine = _serviceProvider.GetRequiredService<IRazorViewEngine>();
//                var tempDataProvider = _serviceProvider.GetRequiredService<ITempDataProvider>();
//                _viewRender = new ViewRenderService(razorViewEngine,tempDataProvider,_serviceProvider);
//            }
            
            
            // TOC:
            //   _context = context
            //   _isync = isync
            //   _exiftool = exiftool
            //   _appSettings = appSettings
            //   _readmeta = readmeta
            _import = new ImportService(context, _isync, exiftool, appSettings, readmeta);
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
    }
}