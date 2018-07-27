using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using starsky.Attributes;
using starsky.Helpers;
using starsky.Middleware;
using starsky.Models;
using starsky.Services;

namespace starskyimportercli
{
    static class Program
    {
        
        [ExcludeFromCoverage] // The ArgsHelper.cs is covered by unit tests
        static void Main(string[] args)
        {
            // Use args in application
            new ArgsHelper().SetEnvironmentByArgs(args);
            

            var startupHelper = new ConfigCliAppsStartupHelper();
            var appSettings = startupHelper.AppSettings();
            appSettings.Verbose = new ArgsHelper().NeedVerbose(args);


            if (new ArgsHelper().NeedHelp(args) || new ArgsHelper().GetPathFormArgs(args,false).Length <= 1)
            {
                // When this change please update ./readme.md
                Console.WriteLine("Starsky");
                Console.WriteLine("         Importer");
                Console.WriteLine("                  Help:");
                Console.WriteLine("--help or -h == help (this window)");
                Console.WriteLine("--path or -p == parameter: (string) ; fullpath");
                Console.WriteLine("                can be an folder or file");
                Console.WriteLine("--move or -m == delete file after importing (default false / copy file)");
                Console.WriteLine("--all or -a == import all files including files older than 2 years (default: false / ignore old files) ");
                Console.WriteLine("--recursive or -r == Import Directory recursive (default: false / only the selected folder) ");
                Console.WriteLine("--verbose or -v == verbose, more detailed info");
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
                return;
            }
            
            var inputPath = new ArgsHelper().GetPathFormArgs(args,false);
            
            if(appSettings.Verbose) Console.WriteLine("inputPath " + inputPath);
            
            startupHelper.ImportService().Import(inputPath, new ArgsHelper(appSettings).GetMove(args),new ArgsHelper(appSettings).GetAll(args),new ArgsHelper().NeedRecruisive(args));
           
            Console.WriteLine("Done Importing");
            
        }

        
    }
}