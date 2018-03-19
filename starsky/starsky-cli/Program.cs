using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using starsky.Models;
using starsky.Services;

namespace starskyCli
{
    public class Program
    {
        private static bool NeedHelp(IReadOnlyList<string> args)
        {
            var needHelp = false;

            for (int arg = 0; arg < args.Count; arg++)
            {
                if ((args[arg].ToLower() == "--help" || args[arg].ToLower() == "-h") && (arg + 1) != args.Count)
                {
                    bool.TryParse(args[arg + 1], out needHelp);
                }
                if ((args[arg].ToLower() == "--help" || args[arg].ToLower() == "-h" ))
                {
                    needHelp = true;
                }
            }

            return needHelp;
        }

        private static string GetPathFormArgs(IReadOnlyList<string> args)
        {
            var path = "";

            for (int arg = 0; arg < args.Count; arg++)
            {
                if ((args[arg].ToLower() == "--path" || args[arg].ToLower() == "-p") && (arg + 1) != args.Count)
                {
                    path = args[arg + 1];
                }
            }

            var subpath = FileIndexItem.FullPathToDatabaseStyle(path);
            return subpath;
        }

        private static string GetSubpathFormArgs(IReadOnlyList<string> args)
        {
            var subpath = "/";

            for (int arg = 0; arg < args.Count; arg++)
            {
                if ((args[arg].ToLower() == "--subpath" || args[arg].ToLower() == "-s") && (arg + 1) != args.Count)
                {
                    subpath = args[arg + 1];
                }
            }

            return subpath;
        }

        private static bool GetIndexMode(IReadOnlyList<string> args)
        {
            var isIndexMode = true;

            for (int arg = 0; arg < args.Count; arg++)
            {
                if ((args[arg].ToLower() == "--index" || args[arg].ToLower() == "-i") && (arg + 1) != args.Count)
                {
                    bool.TryParse(args[arg + 1], out isIndexMode);
                }
            }

            return isIndexMode;
        }

        private static bool GetThumbnail(IReadOnlyList<string> args)
        {
            var isThumbnail = false;

            for (int arg = 0; arg < args.Count; arg++)
            {
                if ((args[arg].ToLower() == "--thumbnail" || args[arg].ToLower() == "-t") && (arg + 1) != args.Count)
                {
                    bool.TryParse(args[arg + 1], out isThumbnail);
                }
            }

            return isThumbnail;
        }


        public static void Main(string[] args)
        {
            ConfigRead.SetAppSettingsProvider();

            //var q = ExifTool.WriteExifToolKeywords("test1", "Z:\\data\\git\\starsky\\starsky\\starsky-cli\\bin\\Debug\\netcoreapp2.0\\20180101_000337.jpg");

            if (NeedHelp(args))
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
            if (GetPathFormArgs(args).Length >= 1)
            {
                subpath = GetPathFormArgs(args);
            }
            else
            {
                subpath = GetSubpathFormArgs(args);
            }


            if (GetIndexMode(args))
            {
                Console.WriteLine("Start indexing");
                new SyncDatabase().SyncFiles(subpath);
                Console.WriteLine("Done SyncFiles!");
            }


            if (!GetThumbnail(args)) return;

            // Thumb
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
