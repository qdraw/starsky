using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using starsky.Models;
using starsky.Services;
using starskycli;

namespace starskyCli
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ConfigRead.SetAppSettingsProvider();

            //var q = ExifTool.WriteExifToolKeywords("test1", "Z:\\data\\git\\starsky\\starsky\\starsky-cli\\bin\\Debug\\netcoreapp2.0\\20180101_000337.jpg");

            if (ArgsHelper.NeedHelp(args))
            {
                Console.WriteLine("Settings:");
                Console.WriteLine("Database Type "+ AppSettingsProvider.DatabaseType);
                Console.WriteLine("BasePath " + AppSettingsProvider.BasePath);
                Console.WriteLine("ThumbnailTempFolder " + AppSettingsProvider.ThumbnailTempFolder);
                Console.WriteLine("");
                Console.WriteLine("Starsky Help:");
                Console.WriteLine("--help or -h == help (this window)");
                Console.WriteLine("--subpath or -s == parameter: (string) ; path inside the index, default '/' ");
                Console.WriteLine("--path or -p == parameter: (string) ; fullpath, search and replace first part of the filename '/' ");
                Console.WriteLine("--index or -i == parameter: (bool) ; enable indexing, default true");
                Console.WriteLine("--thumbnail or -t == parameter: (bool) ; enable thumbnail, default false");
                return;
            }

            // Using both options
            var subpath = "/";
            if (ArgsHelper.GetPathFormArgs(args).Length >= 1)
            {
                subpath = ArgsHelper.GetPathFormArgs(args);
            }
            else
            {
                subpath = ArgsHelper.GetSubpathFormArgs(args);
            }


            if (ArgsHelper.GetIndexMode(args))
            {
                Console.WriteLine("Start indexing");
                new SyncDatabase().SyncFiles(subpath);
                Console.WriteLine("Done SyncFiles!");
            }


            if (!ArgsHelper.GetThumbnail(args)) return;

            // Thumbnail check service
            var subFoldersFullPath =  Files.GetAllFilesDirectory(subpath);

            foreach (var singleFolderFullPath in subFoldersFullPath)
            {
                string[] filesInDirectoryFullPath = Files.GetFilesInDirectory(singleFolderFullPath,false);
                var localFileListFileHash = FileHash.CalcHashCode(filesInDirectoryFullPath);

                for (int i = 0; i < filesInDirectoryFullPath.Length; i++)
                {
                    var value = new FileIndexItem()
                    {
                        FilePath = FileIndexItem.FullPathToDatabaseStyle(filesInDirectoryFullPath[i]),
                        FileHash = localFileListFileHash[i]
                    };
                    Thumbnail.CreateThumb(value);
                }

                if (filesInDirectoryFullPath.Length >= 1)
                {
                    Console.WriteLine("~ " + filesInDirectoryFullPath.Length + " ~ "+  FileIndexItem.FullPathToDatabaseStyle(singleFolderFullPath));
                }

            }

            Console.WriteLine("Done!");

        }

    }
}
