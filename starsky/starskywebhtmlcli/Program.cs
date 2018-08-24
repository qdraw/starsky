// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.ObjectPool;
using starsky.Helpers;
using starsky.Middleware;
using starsky.Models;
using starsky.Services;
using starskywebhtmlcli.Services;

namespace starskywebhtmlcli
{
    public static class Program
    {
        private static AppSettings _appSettings;
        private static ReadMeta _readmeta;

        public static void Main(string[] args)
        {
            var startupHelper = InitializeServices(AppDomain.CurrentDomain.BaseDirectory); // AppDomain.CurrentDomain.BaseDirectory
            
            // Use args in application
             new ArgsHelper().SetEnvironmentByArgs(args);
            
            _appSettings.Verbose = new ArgsHelper().NeedVerbose(args);
            
            if (new ArgsHelper().NeedHelp(args))
            {
                // Update Readme.md when this change!
                Console.WriteLine("Starsky Indexer Help:");
                Console.WriteLine("--help or -h == help (this window)");
                Console.WriteLine("--path or -p == parameter: (string) ; fullpath ");
                Console.WriteLine("  use -v -help to show settings: ");
                if (!_appSettings.Verbose) return;
                Console.WriteLine("");
                Console.WriteLine("AppSettings:");
                Console.WriteLine("Database Type (-d --databasetype) "+ _appSettings.DatabaseType);
                Console.WriteLine("DatabaseConnection (-c --connection) " + _appSettings.DatabaseConnection);
                Console.WriteLine("StorageFolder (-b --basepath) " + _appSettings.StorageFolder);
                Console.WriteLine("ThumbnailTempFolder (-f --thumbnailtempfolder) "+ _appSettings.ThumbnailTempFolder);
                Console.WriteLine("ExifToolPath  (-e --exiftoolpath) "+ _appSettings.ExifToolPath);
                Console.WriteLine("Structure  (-u --structure) "+ _appSettings.Structure);
                Console.WriteLine("BaseDirectoryProject (where the exe is located) " + _appSettings.BaseDirectoryProject);
                return;
            }
            
            var inputPath = new ArgsHelper().GetPathFormArgs(args,false);
            if(_appSettings.Verbose) Console.WriteLine("inputPath " + inputPath);
            
            if(Files.IsFolderOrFile(inputPath) != FolderOrFileModel.FolderOrFileTypeList.Folder)
                Console.WriteLine("Folders now are supported " + inputPath);

            var listOfFiles = Files.GetFilesInDirectory(inputPath);
            var fileIndexItemList = _readmeta.ReadExifAndXmpFromFileAddBasics(listOfFiles);

            // used in this session to find the files back
            _appSettings.StorageFolder = inputPath;
            
            new LoopPublications(_appSettings,startupHelper).Render(fileIndexItemList);
            

        }

        public static IServiceScopeFactory InitializeServices(string customApplicationBasePath = null)
        {
            // Initialize the necessary services
            var services = new ServiceCollection();
            ConfigureDefaultServices(services, customApplicationBasePath);

            // Inject Config helper
            services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
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
            // end config
            
            var serviceProvider = services.BuildServiceProvider();
            
            _appSettings = serviceProvider.GetRequiredService<AppSettings>();
            
            _readmeta = new ReadMeta(_appSettings);

            return serviceProvider.GetRequiredService<IServiceScopeFactory>();
        }

        public static Task<string> RenderViewAsync(IServiceScopeFactory scopeFactory, List<FileIndexItem> model)
        {
            using (var serviceScope = scopeFactory.CreateScope())
            {
                var helper = serviceScope.ServiceProvider.GetRequiredService<IViewRenderService>();
                return helper.RenderToStringAsync("/Views/Autopost.cshtml", model.AsEnumerable());

//                var helper = serviceScope.ServiceProvider.GetRequiredService<RazorViewToStringRenderer>();
//                return helper.RenderViewToStringAsync("/Views/Autopost.cshtml", model.AsEnumerable());
            }
        }

        private static void ConfigureDefaultServices(IServiceCollection services, string customApplicationBasePath)
        {
            string applicationName;
            IFileProvider fileProvider;
            if (!string.IsNullOrEmpty(customApplicationBasePath))
            {
                applicationName = Path.GetFileName(customApplicationBasePath);
                fileProvider = new PhysicalFileProvider(customApplicationBasePath);
            }
            else
            {
                applicationName = Assembly.GetEntryAssembly().GetName().Name;
                fileProvider = new PhysicalFileProvider(Directory.GetCurrentDirectory());
            }

            services.AddSingleton<IHostingEnvironment>(new HostingEnvironment
            {
                ApplicationName =  applicationName,
                WebRootFileProvider = fileProvider,
            });
            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.FileProviders.Clear();
                options.FileProviders.Add(fileProvider);
            });
            var diagnosticSource = new DiagnosticListener("Microsoft.AspNetCore");
            services.AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();
            services.AddSingleton<DiagnosticSource>(diagnosticSource);
            services.AddLogging();
            services.AddMvc();
            services.AddTransient<RazorViewToStringRenderer>();
            services.AddScoped<IViewRenderService, ViewRenderService>();
        }
    }
}
