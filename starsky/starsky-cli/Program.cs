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

            new SyncDatabase().SyncFiles(GetSubpathFormArgs(args));
            Console.WriteLine("Done SyncFiles!");

            if (!GetThumbnail(args)) return;

            var allitems = new SyncDatabase().GetAll(GetSubpathFormArgs(args));

            foreach (var value in allitems)
            {
                Thumbnail.CreateThumb(value);
            }
            Console.WriteLine("Done!");



        }

    }
}
