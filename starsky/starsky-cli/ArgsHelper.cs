using System;
using System.Collections.Generic;
using System.Text;
using starsky.Models;

namespace starskycli
{
    public class ArgsHelper
    {
        public static bool NeedHelp(IReadOnlyList<string> args)
        {
            var needHelp = false;

            for (int arg = 0; arg < args.Count; arg++)
            {
                if ((args[arg].ToLower() == "--help" || args[arg].ToLower() == "-h") && (arg + 1) != args.Count)
                {
                    bool.TryParse(args[arg + 1], out needHelp);
                }
                if ((args[arg].ToLower() == "--help" || args[arg].ToLower() == "-h"))
                {
                    needHelp = true;
                }
            }

            return needHelp;
        }

        public static string GetPathFormArgs(IReadOnlyList<string> args)
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

        public static string GetSubpathFormArgs(IReadOnlyList<string> args)
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

        public static bool GetIndexMode(IReadOnlyList<string> args)
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

        public static bool GetThumbnail(IReadOnlyList<string> args)
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
    }
}
