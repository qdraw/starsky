﻿using System;
using System.Collections.Generic;
using System.Linq;
using starsky.Models;

namespace starsky.Helpers
{
    public class ArgsHelper
    {
        // Table of Content
        
        // --verbose -v
        // --databasetype -d
        // --connection -c
        // --basepath -b
        // --thumbnailtempfolder -f
        // --exiftoolpath -e
        // --help -h
        // --index -i
        // --path -p
        // --subpath -s
        // --thumbnail -t
        // --orphanfolder -o
        // --move -m
        // --all -a
        // --recruisive -r 
        // -rf --readonlyfolders // no need to use in cli/importercli
        // -u --structure

        public ArgsHelper(AppSettings appSettings)
        {
            _appSettings = appSettings;
        }

        private readonly AppSettings _appSettings;

        public bool NeedVerbose(IReadOnlyList<string> args)
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
        
        public readonly IEnumerable<string> ShortNameList = new List<string>
            {
                "-d","-c","-b","-f","-e","-u"
            }.AsReadOnly();
        
        public readonly IEnumerable<string> LongNameList = new List<string>
            {
            "--databasetype","--connection","--basepath","--thumbnailtempfolder","--exiftoolpath","--structure"
            }
            .AsReadOnly();
        
        public readonly IEnumerable<string> EnvNameList = new List<string>
            {
                "DatabaseType","DefaultConnection","STARSKY_BASEPATH","ThumbnailTempFolder","ExifToolPath", "Structure"
            }.AsReadOnly();


        public void SetEnvironmentByArgs(IReadOnlyList<string> args)
        {
            var shortNameList = ShortNameList.ToArray();
            var longNameList = LongNameList.ToArray();
            var envNameList = EnvNameList.ToArray();

            for (int i = 0; i < ShortNameList.Count(); i++)
            {
                for (int arg = 0; arg < args.Count; arg++)
                {
                    if ((args[arg].ToLower() == longNameList[i] || 
                         args[arg].ToLower() == shortNameList[i]) && (arg + 1) != args.Count)
                    {
                        Environment.SetEnvironmentVariable(envNameList[i],args[arg+1]);
                    }
                }
            }
        }

       public bool NeedHelp(IReadOnlyList<string> args)
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
        public bool GetIndexMode(IReadOnlyList<string> args)
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
        
        public string GetPathFormArgs(IReadOnlyList<string> args, bool dbStyle = true)
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
                path = _appSettings.FullPathToDatabaseStyle(path);
            }
            return path;
        }
        
        public string GetSubpathFormArgs(IReadOnlyList<string> args)
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
        
        public bool IfSubpathOrPath(IReadOnlyList<string> args)
        {
            // Detect if a input is a fullpath or a subpath.
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
        
        public bool GetThumbnail(IReadOnlyList<string> args)
        {
            var isThumbnail = false;
        
            for (int arg = 0; arg < args.Count; arg++)
            {
                if ((args[arg].ToLower() == "--thumbnail" || args[arg].ToLower() == "-t") && (arg + 1) != args.Count)
                {
                    bool.TryParse(args[arg + 1], out isThumbnail);
                }
            }
        
            if (_appSettings.Verbose) Console.WriteLine(">> GetThumbnail " + isThumbnail);
            return isThumbnail;
        }
        
        public bool GetOrphanFolderCheck(IReadOnlyList<string> args)
        {
            var isOrphanFolderCheck = false;
        
            for (int arg = 0; arg < args.Count; arg++)
            {
                if ((args[arg].ToLower() == "--orphanfolder" || args[arg].ToLower() == "-o") && (arg + 1) != args.Count)
                {
                    bool.TryParse(args[arg + 1], out isOrphanFolderCheck);
                }
            }
        
            if (_appSettings.Verbose) Console.WriteLine(">> isOrphanFolderCheck " + isOrphanFolderCheck);
            return isOrphanFolderCheck;
        }
        
        public bool GetMove(IReadOnlyList<string> args)
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
        
        public bool GetAll(IReadOnlyList<string> args)
        {
            // default false
            var getAll = true;
        
            for (int arg = 0; arg < args.Count; arg++)
            {
                if ((args[arg].ToLower() == "--all" 
                     || args[arg].ToLower() == "-a") 
                    && (arg + 1) != args.Count)
                {
                    bool.TryParse(args[arg + 1], out getAll);
                }
                if ((args[arg].ToLower() == "--all" || args[arg].ToLower() == "-a"))
                {
                    getAll = false;
                }
            }
            return getAll;
        }
        
        public bool NeedRecruisive(IReadOnlyList<string> args)
        {
            bool needRecruisive = false;
            
            for (int arg = 0; arg < args.Count; arg++)
            {
                if ((args[arg].ToLower() == "--recruisive" || args[arg].ToLower() == "-r"))
                {
                    needRecruisive = true;
                }
            }

            return needRecruisive;
        }
        
        
    }
}
