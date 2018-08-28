using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using starsky.Helpers;
using starsky.Middleware;
using starsky.Models;
using starsky.Services;
using starskywebhtmlcli.Services;

namespace starskywebhtmlcli
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            
            // Use args in application
            new ArgsHelper().SetEnvironmentByArgs(args);
            var startupHelper = new ConfigCliAppsStartupHelper();
            var appSettings = startupHelper.AppSettings();
            
            appSettings.Verbose = new ArgsHelper().NeedVerbose(args);
            
            if (new ArgsHelper().NeedHelp(args))
            {
                // Update Readme.md when this change!
                Console.WriteLine("Starsky WebHtml Cli Help:");
                Console.WriteLine("--help or -h == help (this window)");
                Console.WriteLine("--path or -p == parameter: (string) ; fullpath ");
                Console.WriteLine("--name or -n == parameter: (string) ; name of item ");
                Console.WriteLine("  use -v -help");
                return;
            }
            
            var inputPath = new ArgsHelper().GetPathFormArgs(args,false);

            if (string.IsNullOrWhiteSpace(inputPath))
            {
                Console.WriteLine("Please use the -p to add a path first");
                return;
            }
            
            if(Files.IsFolderOrFile(inputPath) != FolderOrFileModel.FolderOrFileTypeList.Folder)
                Console.WriteLine("Please add a valid folder: " + inputPath);

            if (appSettings.Name == new AppSettings().Name)
            {
                var suggestedInput = Path.GetFileName(inputPath);
                
                Console.WriteLine("\nWhat is the name of the item? (for: "+ suggestedInput +" press Enter)\n ");
                var name = Console.ReadLine();
                appSettings.Name = name;
                if (string.IsNullOrEmpty(name))
                {
                    appSettings.Name = suggestedInput;
                }
            }

            if(appSettings.Verbose) Console.WriteLine("Name: " + appSettings.Name);
            if(appSettings.Verbose) Console.WriteLine("inputPath " + inputPath);

            // used in this session to find the files back
            appSettings.StorageFolder = inputPath;
            
            var listOfFiles = Files.GetFilesInDirectory(inputPath);
            var fileIndexItemList = startupHelper.ReadMeta().ReadExifAndXmpFromFileAddFilePathHash(listOfFiles);
            

            
            // Create thumbnails from the source images 
            new ThumbnailByDirectory(appSettings).CreateThumb(inputPath);

//            foreach (var item in fileIndexItemList)
//            {
//                item.FileHash = Base64Helper
//                    .ToBase64(new Thumbnail(null).ResizeThumbnailToStream(item.FilePath, 4));
//            }
            new LoopPublications(appSettings).Render(fileIndexItemList);
            

        }

//        public static IServiceScopeFactory InitializeServices(string customApplicationBasePath = null)
//        {
//            // Initialize the necessary services
//            var services = new ServiceCollection();
//
//            // Inject Config helper
//            services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
//            var builder = new ConfigurationBuilder();
//            if (File.Exists(new AppSettings().BaseDirectoryProject + "appsettings.json"))
//            {
//                Console.WriteLine("loaded json > " +new AppSettings().BaseDirectoryProject  + "appsettings.json");
//                builder.AddJsonFile(
//                    new AppSettings().BaseDirectoryProject + "appsettings.json", optional: false);
//            }
//            // overwrite envs
//            builder.AddEnvironmentVariables();
//            // build config
//            var configuration = builder.Build();
//            // inject config as object to a service
//            services.ConfigurePoco<AppSettings>(configuration.GetSection("App"));
//            // end config
//            
//            var serviceProvider = services.BuildServiceProvider();
//            
//            appSettings = serviceProvider.GetRequiredService<AppSettings>();
//            
//            _readmeta = new ReadMeta(appSettings);
//
//            return serviceProvider.GetRequiredService<IServiceScopeFactory>();
//        }
        
    }
}
