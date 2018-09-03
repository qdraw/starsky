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
                appSettings.ApplicationType = AppSettings.StarskyAppType.Importer;
                new ArgsHelper(appSettings).NeedHelpShowDialog();
                return;
            }
            
            var inputPath = new ArgsHelper().GetPathFormArgs(args,false);
            
            if(appSettings.Verbose) Console.WriteLine("inputPath " + inputPath);

            var importSettings = new ImportSettingsModel
            {
                DeleteAfter = new ArgsHelper(appSettings).GetMove(args),
                AgeFileFilter = new ArgsHelper(appSettings).GetAll(args),
                RecursiveDirectory = new ArgsHelper().NeedRecruisive(args)
            };
            
            startupHelper.ImportService().Import(inputPath, importSettings);
           
            Console.WriteLine("Done Importing");
            
        }

        
    }
}