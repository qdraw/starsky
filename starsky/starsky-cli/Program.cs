using System;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using starsky.Attributes;
using starsky.Helpers;
using starsky.Models;
using starsky.Services;

namespace starskyCli
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var provider = new ServiceCollection()
                .AddMemoryCache()
                .BuildServiceProvider();
            var cache = provider.GetService<IMemoryCache>();
            
            // Check if user want more info
            AppSettingsProvider.Verbose = ArgsHelper.NeedVerbose(args);

            // If used '-d inmemorydatabase'
            ArgsHelper.SetEnvironmentByArgs(args);

            ConfigRead.SetAppSettingsProvider();
            
            if (ArgsHelper.NeedHelp(args))
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
                Console.WriteLine("--basepath or -b == Overwrite EnvironmentVariable for STARSKY_BASEPATH");
                Console.WriteLine("--connection or -c == Overwrite EnvironmentVariable for DefaultConnection");
                Console.WriteLine("--thumbnailtempfolder or -f == Overwrite EnvironmentVariable for ThumbnailTempFolder");
                Console.WriteLine("--exiftoolpath or -e == Overwrite EnvironmentVariable for ExifToolPath");
                Console.WriteLine("  use -v -help to show settings: ");
                if (!AppSettingsProvider.Verbose) return;
                Console.WriteLine("");
                Console.WriteLine("Settings:");
                Console.WriteLine("Database Type "+ AppSettingsProvider.DatabaseType);
                Console.WriteLine("BasePath " + AppSettingsProvider.BasePath);
                Console.WriteLine("ThumbnailTempFolder " + AppSettingsProvider.ThumbnailTempFolder);
                Console.WriteLine("DbConnectionString " + AppSettingsProvider.DbConnectionString);
                return;
            }

            // Using both options
            string subpath;
            // -s = ifsubpath || -p is path
            if (ArgsHelper.IfSubpath(args))
            {
                subpath = ArgsHelper.GetSubpathFormArgs(args);
            }
            else
            {
                subpath = ArgsHelper.GetPathFormArgs(args);
            }
           

            if (ArgsHelper.GetIndexMode(args))
            {
                Console.WriteLine("Start indexing");
                new SyncDatabase(cache).SyncFiles(subpath);
                Console.WriteLine("Done SyncFiles!");
            }

            if (ArgsHelper.GetThumbnail(args)) {

                // If single file => create thumbnail
                Thumbnail.CreateThumb(subpath);
    
                if (Files.IsFolderOrFile(subpath) 
                    == FolderOrFileModel.FolderOrFileTypeList.Folder)
                {
                    ThumbnailByDirectory.CreateThumb(subpath);
                }
                Console.WriteLine("Thumbnail Done!");
            }
            
            if (ArgsHelper.GetOrphanFolderCheck(args))
            {
                Console.WriteLine(">>>>> Heavy CPU Feature => Use with care <<<<< ");
                new SyncDatabase(cache).OrphanFolder(subpath);
            }

            Console.WriteLine("Done!");

        }

    }
}
