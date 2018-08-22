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
            var fileIndexItem = startupHelper.ReadMeta().ReadExifAndXmpFromFileAddBasics(listOfFiles);

            new ViewRender(appSettings).Render(fileIndexItem);

            
            
//            new OverlayImage(appSettings).OverlayImageNow();
            
//            // Using both options
//            string subpath;
//            // -s = ifsubpath || -p is path
//            if (new ArgsHelper(appSettings).IfSubpathOrPath(args))
//            {
//                subpath = new ArgsHelper(appSettings).GetSubpathFormArgs(args);
//            }
//            else
//            {
//                subpath = new ArgsHelper(appSettings).GetPathFormArgs(args);
//            }
//            
//            // overwrite subpath with relative days
//            // use -n or --SubpathRelative to use it.
//            // envs are not supported
//            var getSubpathRelative = new ArgsHelper(appSettings).GetSubpathRelative(args);
//            if (getSubpathRelative != null)
//            {
//                subpath = getSubpathRelative;
//            }
//
//            if (new ArgsHelper().GetIndexMode(args))
//            {
//                Console.WriteLine("Start indexing");
//                startupHelper.SyncService().SyncFiles(subpath);
//                Console.WriteLine("Done SyncFiles!");
//            }
//
//            if (new ArgsHelper(appSettings).GetThumbnail(args))
//            {
//
//                var fullPath = appSettings.DatabasePathToFilePath(subpath);
//                var isFolderOrFile = Files.IsFolderOrFile(fullPath);
//
//                if (appSettings.Verbose) Console.WriteLine(isFolderOrFile);
//                
//                if (isFolderOrFile == FolderOrFileModel.FolderOrFileTypeList.File)
//                {
//                    // If single file => create thumbnail
//                    new Thumbnail(appSettings).CreateThumb(subpath); // <= this uses subpath
//                }
//                else
//                {
//                    new ThumbnailByDirectory(appSettings).CreateThumb(fullPath); // <= this uses fullpath
//                }
//                
//                Console.WriteLine("Thumbnail Done!");
//            }
//            
//            if (new ArgsHelper(appSettings).GetOrphanFolderCheck(args))
//            {
//                Console.WriteLine(">>>>> Heavy CPU Feature => Use with care <<<<< ");
//                startupHelper.SyncService().OrphanFolder(subpath);
//            }
//            Console.WriteLine("Done!");

        }

    }
}
