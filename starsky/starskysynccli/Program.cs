using System;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Models;
using starsky.foundation.storage.Services;
using starsky.foundation.thumbnailgeneration.Services;
using starskycore.Helpers;
using starskycore.Models;
using starskycore.Services;

namespace starskysynccli
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            // Use args in application
			new ArgsHelper().SetEnvironmentByArgs(args);
            
            var startupHelper = new ConfigCliAppsStartupHelper();
	        
	        // Todo: make feature of this -->
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
            if (new ArgsHelper(appSettings).IsSubPathOrPath(args))
            {
                subpath = new ArgsHelper(appSettings).GetSubpathFormArgs(args);
            }
            else
            {
                subpath = new ArgsHelper(appSettings).GetPathFormArgs(args);
            }
            
            // overwrite subPath with relative days
            // use -g or --SubPathRelative to use it.
            // envs are not supported
            var getSubPathRelative = new ArgsHelper(appSettings).GetRelativeValue(args);
            if (getSubPathRelative != null)
            {
	            var dateTime = DateTime.Now.AddDays(( double ) getSubPathRelative);
	            subpath = new StructureService(startupHelper.SubPathStorage(), appSettings.Structure)
	                .ParseSubfolders(dateTime);
            }

            if (new ArgsHelper().GetIndexMode(args))
            {
                Console.WriteLine($"Start indexing {subpath}");
                startupHelper.SyncService().SyncFiles(subpath);
                Console.WriteLine("Done SyncFiles!");
            }

            if (new ArgsHelper(appSettings).GetThumbnail(args))
            {
	            var storage = startupHelper.SubPathStorage();
	            var thumbnailStorage = startupHelper.ThumbnailStorage();

				var isFolderOrFile = storage.IsFolderOrFile(subpath);

                if (appSettings.Verbose) Console.WriteLine(isFolderOrFile);
                
                if (isFolderOrFile == FolderOrFileModel.FolderOrFileTypeList.File)
                {
                    // If single file => create thumbnail
	                var fileHash = new FileHash(storage).GetHashCode(subpath).Key;
                    new Thumbnail(storage,thumbnailStorage).CreateThumb(subpath,fileHash); // <= this uses subPath
                }
                else
                {
	                new Thumbnail(storage, thumbnailStorage).CreateThumb(subpath);
                }
                
                Console.WriteLine("Thumbnail Done!");
            }
            
            if (new ArgsHelper(appSettings).GetOrphanFolderCheck(args))
            {
                Console.WriteLine(">>>>> Heavy CPU Feature => Use with care <<<<< ");
                startupHelper.SyncService().OrphanFolder(subpath);
            }

	        if ( new ArgsHelper(appSettings).NeedCleanup(args) )
	        {
		        Console.WriteLine(">>>>> Heavy CPU Feature => NeedCacheCleanup <<<<< ");
		        startupHelper.ThumbnailCleaner().CleanAllUnusedFiles();

	        }

	        Console.WriteLine("Done!");

        }

    }
}
