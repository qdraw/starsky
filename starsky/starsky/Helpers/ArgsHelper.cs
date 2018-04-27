using System;
using System.Collections.Generic;
using starsky.Models;

namespace starsky.Helpers
{
    public static class ArgsHelper
    {
        public static bool NeedVerbose(IReadOnlyList<string> args)
        {
            var needDebug = false;

            for (int arg = 0; arg < args.Count; arg++)
            {
                if ((args[arg].ToLower() == "--verbose" || args[arg].ToLower() == "-v") && (arg + 1) != args.Count)
                {
                    bool.TryParse(args[arg + 1], out needDebug);
                }
                if ((args[arg].ToLower() == "--verbose" || args[arg].ToLower() == "-v"))
                {
                    needDebug = true;
                }
            }

            return needDebug;
        }
        
        public static readonly List<string> ShortNameList = new List<string>{"-d","-c","-b","-f","-e"};
        public static readonly List<string> LongNameList = new List<string>{"--databasetype","--connection","--basepath","--thumbnailtempfolder","--exiftoolpath"};
        public static readonly List<string> EnvNameList = new List<string>{"DatabaseType","DefaultConnection","STARSKY_BASEPATH","ThumbnailTempFolder","ExifToolPath"};
        
        public static void SetEnvironmentByArgs(IReadOnlyList<string> args)
        {
            for (int i = 0; i < ShortNameList.Count; i++)
            {
                for (int arg = 0; arg < args.Count; arg++)
                {
                    if ((args[arg].ToLower() == LongNameList[i] || args[arg].ToLower() == ShortNameList[i]) && (arg + 1) != args.Count)
                    {
                        Environment.SetEnvironmentVariable(EnvNameList[i],args[arg+1]);
                    }
                }
            }
        }

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
        
        // Default On
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

        public static string GetPathFormArgs(IReadOnlyList<string> args, bool dbStyle = true)
        {
            var path = "";

            for (int arg = 0; arg < args.Count; arg++)
            {
                if ((args[arg].ToLower() == "--path" || args[arg].ToLower() == "-p") && (arg + 1) != args.Count)
                {
                    path = args[arg + 1];
                }
            }
            if (dbStyle)
            {
                path = FileIndexItem.FullPathToDatabaseStyle(path);
            }
            return path;
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
        
        public static bool IfSubpath(IReadOnlyList<string> args)
        {
            for (int arg = 0; arg < args.Count; arg++)
            {
                if ((args[arg].ToLower() == "--subpath" || args[arg].ToLower() == "-s") && (arg + 1) != args.Count)
                {
                    return true;
                }
                if ((args[arg].ToLower() == "--path" || args[arg].ToLower() == "-p") && (arg + 1) != args.Count)
                {
                    return false;
                }
            }
            return true;
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

            if (AppSettingsProvider.Verbose) Console.WriteLine(">> GetThumbnail " + isThumbnail);
            return isThumbnail;
        }

        public static bool GetOrphanFolderCheck(IReadOnlyList<string> args)
        {
            var isOrphanFolderCheck = false;

            for (int arg = 0; arg < args.Count; arg++)
            {
                if ((args[arg].ToLower() == "--orphanfolder" || args[arg].ToLower() == "-o") && (arg + 1) != args.Count)
                {
                    bool.TryParse(args[arg + 1], out isOrphanFolderCheck);
                }
            }

            if (AppSettingsProvider.Verbose) Console.WriteLine(">> isOrphanFolderCheck " + isOrphanFolderCheck);
            return isOrphanFolderCheck;
        }
        
        public static bool GetMove(IReadOnlyList<string> args)
        {
            var getMove = false;

            for (int arg = 0; arg < args.Count; arg++)
            {
                if ((args[arg].ToLower() == "--move" 
                     || args[arg].ToLower() == "-m") 
                     && (arg + 1) != args.Count)
                {
                    bool.TryParse(args[arg + 1], out getMove);
                }
                if ((args[arg].ToLower() == "--move" || args[arg].ToLower() == "-m"))
                {
                    getMove = true;
                }
            }
            return getMove;
        }

        
    }
}
