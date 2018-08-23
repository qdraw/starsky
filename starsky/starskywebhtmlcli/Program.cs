using System;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using starsky.Helpers;
using starsky.Models;
using starsky.Services;
using starskywebhtmlcli.Models;
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
                Console.WriteLine("Starsky Indexer Help:");
                Console.WriteLine("--help or -h == help (this window)");
                Console.WriteLine("--path or -p == parameter: (string) ; fullpath ");
                Console.WriteLine("  use -v -help to show settings: ");
                if (!appSettings.Verbose) return;
                Console.WriteLine("");
                Console.WriteLine("AppSettings:");
                Console.WriteLine("Database Type (-d --databasetype) "+ appSettings.DatabaseType);
                Console.WriteLine("DatabaseConnection (-c --connection) " + appSettings.DatabaseConnection);
                Console.WriteLine("StorageFolder (-b --basepath) " + appSettings.StorageFolder);
                Console.WriteLine("ThumbnailTempFolder (-f --thumbnailtempfolder) "+ appSettings.ThumbnailTempFolder);
                Console.WriteLine("ExifToolPath  (-e --exiftoolpath) "+ appSettings.ExifToolPath);
                Console.WriteLine("Structure  (-u --structure) "+ appSettings.Structure);
                Console.WriteLine("BaseDirectoryProject (where the exe is located) " + appSettings.BaseDirectoryProject);
                return;
            }
            
            var inputPath = new ArgsHelper().GetPathFormArgs(args,false);
            if(appSettings.Verbose) Console.WriteLine("inputPath " + inputPath);
            
            if(Files.IsFolderOrFile(inputPath) != FolderOrFileModel.FolderOrFileTypeList.Folder)
                Console.WriteLine("Folders now are supported " + inputPath);

            var listOfFiles = Files.GetFilesInDirectory(inputPath);
            var fileIndexItemList = startupHelper.ReadMeta().ReadExifAndXmpFromFileAddBasics(listOfFiles);

            // used in this session to find the files back
            appSettings.StorageFolder = inputPath;
            
            new RenderConfig(appSettings).Render(fileIndexItemList);
            
//            new ViewRender(appSettings).Render(fileIndexItem);


        }

    }
}
