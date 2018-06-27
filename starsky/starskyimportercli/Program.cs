using System;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using starsky.Attributes;
using starsky.Helpers;
using starsky.Models;
using starsky.Services;

namespace starskyimportercli
{
    static class Program
    {
        
        [ExcludeFromCoverage] // The ArgsHelper.cs is covered by unit tests
        static void Main(string[] args)
        {
            var provider = new ServiceCollection()
                .AddMemoryCache()
                .BuildServiceProvider();
            var cache = provider.GetService<IMemoryCache>();
            
            // Check if user want more info
            AppSettingsProvider.Verbose = ArgsHelper.NeedVerbose(args);
            ConfigRead.SetAppSettingsProvider();

            if (ArgsHelper.NeedHelp(args) || ArgsHelper.GetPathFormArgs(args,false).Length <= 1)
            {
                Console.WriteLine("Starsky");
                Console.WriteLine("         Importer");
                Console.WriteLine("                  Help:");
                Console.WriteLine("--help or -h == help (this window)");
                Console.WriteLine("--path or -p == parameter: (string) ; fullpath");
                Console.WriteLine("                can be an folder or file");
                Console.WriteLine("--move or -m == move file after importing (default false)");
                Console.WriteLine("--all or -a == import all files including files older than 2 years (default: false) ");
                Console.WriteLine("--verbose or -v == verbose, more detailed info");
                Console.WriteLine("  use -v -help to show settings: ");
                if (!AppSettingsProvider.Verbose) return;
                Console.WriteLine("");
                Console.WriteLine("Settings:");
                Console.WriteLine("Database Type "+ AppSettingsProvider.DatabaseType);
                Console.WriteLine("BasePath " + AppSettingsProvider.BasePath);
                return;
            }
            
            var inputPath = ArgsHelper.GetPathFormArgs(args,false);
            
            if(AppSettingsProvider.Verbose) Console.WriteLine("inputPath " + inputPath);
            
            new ImportDatabase(cache).Import(inputPath, ArgsHelper.GetMove(args),ArgsHelper.GetAll(args));
           
            Console.WriteLine("Done Importing");
            
        }

        
    }
}