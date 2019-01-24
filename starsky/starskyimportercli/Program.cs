﻿using System;
using starskycore.Attributes;
using starskycore.Helpers;
using starskycore.Models;

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
	        
	        // Copy for Net
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
                AgeFileFilterDisabled = new ArgsHelper(appSettings).GetAll(args),
                RecursiveDirectory = new ArgsHelper().NeedRecruisive(args),
	            IndexMode = new ArgsHelper().GetIndexMode(args)
            };
            
            startupHelper.ImportService().Import(inputPath, importSettings);
           
            Console.WriteLine("Done Importing");
            
        }

        
    }
}