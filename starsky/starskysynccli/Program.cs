using System;
using starsky.Helpers;
using starsky.Models;
using starsky.Services;

namespace starskysynccli
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
                Console.WriteLine("--subpath or -s == parameter: (string) ; path inside the index, default '/' ");
                Console.WriteLine("--path or -p == parameter: (string) ; fullpath, search and replace first part of the filename '/' ");
                Console.WriteLine("--index or -i == parameter: (bool) ; enable indexing, default true");
                Console.WriteLine("--thumbnail or -t == parameter: (bool) ; enable thumbnail, default false");
                Console.WriteLine("--orphanfolder or -o == To delete files without a parent folder (heavy cpu usage), default false");
                Console.WriteLine("--verbose or -v == verbose, more detailed info");
                Console.WriteLine("--databasetype or -d == Overwrite EnvironmentVariable for DatabaseType");
                Console.WriteLine("--basepath or -b == Overwrite EnvironmentVariable for StorageFolder");
                Console.WriteLine("--connection or -c == Overwrite EnvironmentVariable for DatabaseConnection");
                Console.WriteLine("--thumbnailtempfolder or -f == Overwrite EnvironmentVariable for ThumbnailTempFolder");
                Console.WriteLine("--exiftoolpath or -e == Overwrite EnvironmentVariable for ExifToolPath");
                Console.WriteLine("--subpathrelative or -n == Overwrite subpath to use relative days to select a folder" +
                                  ", use for example '1' to select yesterday. (structure is required)");
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
            if (getSubpathRelative != null)
            {
                subpath = getSubpathRelative;
            }

            if (new ArgsHelper().GetIndexMode(args))
            {
                Console.WriteLine("Start indexing");
                startupHelper.SyncService().SyncFiles(subpath);
                Console.WriteLine("Done SyncFiles!");
            }

            if (new ArgsHelper(appSettings).GetThumbnail(args))
            {

                var fullPath = appSettings.DatabasePathToFilePath(subpath);
                var isFolderOrFile = Files.IsFolderOrFile(fullPath);

                if (appSettings.Verbose) Console.WriteLine(isFolderOrFile);
                
                if (isFolderOrFile == FolderOrFileModel.FolderOrFileTypeList.File)
                {
                    // If single file => create thumbnail
                    new Thumbnail(appSettings).CreateThumb(subpath); // <= this uses subpath
                }
                else
                {
                    new ThumbnailByDirectory(appSettings).CreateThumb(fullPath); // <= this uses fullpath
                }
                
                Console.WriteLine("Thumbnail Done!");
            }
            
            if (new ArgsHelper(appSettings).GetOrphanFolderCheck(args))
            {
                Console.WriteLine(">>>>> Heavy CPU Feature => Use with care <<<<< ");
                startupHelper.SyncService().OrphanFolder(subpath);
            }
            Console.WriteLine("Done!");

        }

    }
}
