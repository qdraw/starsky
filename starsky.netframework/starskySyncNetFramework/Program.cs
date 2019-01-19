using System;
using starsky.Models;
using starsky.Services;
using starskycore.Helpers;
using starskycore.Models;
using starskyNetFrameworkShared;

namespace starskySyncNetFramework
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            // Use args in application
			new ArgsHelper().SetEnvironmentByArgs(args);
            
            var startupHelper = new ConfigCliAppsStartupHelperNetFramework();
            var appSettings = startupHelper.AppSettings();
            appSettings.Verbose = new ArgsHelper().NeedVerbose(args);
            
            if (new ArgsHelper().NeedHelp(args))
            {
                appSettings.ApplicationType = AppSettings.StarskyAppType.Sync;
                new ArgsHelper(appSettings).NeedHelpShowDialog();
                return;
            }
            
            // Using both options
            string subpath;
            // -s = ifsubpath || -p is path
            if (new ArgsHelper(appSettings).IfSubpathOrPath(args))
            {
                subpath = new ArgsHelper(appSettings).GetSubpathFormArgs(args);
            }
            else
            {
                subpath = new ArgsHelper(appSettings).GetPathFormArgs(args);
            }
            
            // overwrite subpath with relative days
            // use -g or --SubpathRelative to use it.
            // envs are not supported
            var getSubpathRelative = new ArgsHelper(appSettings).GetSubpathRelative(args);
			if (getSubpathRelative != string.Empty)
            {
                subpath = getSubpathRelative;
            }

            if (new ArgsHelper().GetIndexMode(args))
            {
                Console.WriteLine($"Start indexing {subpath}");
                startupHelper.SyncService().SyncFiles(subpath);
                Console.WriteLine("Done SyncFiles!");
            }

            if (new ArgsHelper(appSettings).GetThumbnail(args))
            {

                var fullPath = appSettings.DatabasePathToFilePath(subpath);
                var isFolderOrFile = Files.IsFolderOrFile(fullPath);

                if (appSettings.Verbose) Console.WriteLine(isFolderOrFile);
                var exiftool = startupHelper.ExifTool();
                
                if (isFolderOrFile == FolderOrFileModel.FolderOrFileTypeList.File)
                {
                    // If single file => create thumbnail
                    new Thumbnail(appSettings,exiftool).CreateThumb(subpath); // <= this uses subpath
                }
                else
                {
                    new ThumbnailByDirectory(appSettings,exiftool).CreateThumb(fullPath); // <= this uses fullpath
                }
                
                Console.WriteLine("Thumbnail Done!");
            }
            
            if (new ArgsHelper(appSettings).GetOrphanFolderCheck(args))
            {
                Console.WriteLine(">>>>> Heavy CPU Feature => Use with care <<<<< ");
                startupHelper.SyncService().OrphanFolder(subpath);
            }

	        if ( new ArgsHelper(appSettings).NeedCacheCleanup(args) )
	        {
		        Console.WriteLine(">>>>> Heavy CPU Feature => NeedCacheCleanup <<<<< ");
		        startupHelper.ThumbnailCleaner().CleanAllUnusedFiles();

	        }

	        Console.WriteLine("Done!");

        }

    }
}
