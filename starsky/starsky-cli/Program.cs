﻿using System;
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

            if (NeedHelp(args))
            {
                Console.WriteLine("Starsky Help:");
                Console.WriteLine("--help or -h == help (this window)");
                Console.WriteLine("--subpath or -s == parameter: (string) ; path inside the index, default '/' ");
                Console.WriteLine("--index or -i == parameter: (bool) ; enable indexing, default true");
                Console.WriteLine("--thumbnail or -t == parameter: (bool) ; enable thumbnail, default false");
                return;
            }

            ConfigRead.SetAppSettingsProvider();

            var subpath = GetSubpathFormArgs(args);

            if (GetIndexMode(args))
            {
                Console.WriteLine("Start indexing");
                new SyncDatabase().SyncFiles(subpath);
                Console.WriteLine("Done SyncFiles!");
            }


            if (!GetThumbnail(args)) return;

            var allitems = new SyncDatabase().GetAll(subpath);

            foreach (var value in allitems)
            {
                Thumbnail.CreateThumb(value);
            }
            Console.WriteLine("Done!");


        }

    }
}
