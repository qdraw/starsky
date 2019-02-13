using System;
using starskycore.Attributes;
using starskycore.Helpers;
using starskycore.Models;
using starskyNetFrameworkShared;

namespace starskyimportercliNetFramework
{
	public static class Program
	{

		[ExcludeFromCoverage] // The ArgsHelper.cs is covered by unit tests
		public static void Main(string[] args)
		{
			// Use args in application
			new ArgsHelper().SetEnvironmentByArgs(args);

			var startupHelper = new ConfigCliAppsStartupHelperNetFramework();

			// Copy for Net
			var appSettings = startupHelper.AppSettings();
			appSettings.Verbose = new ArgsHelper().NeedVerbose(args);


			if ( new ArgsHelper().NeedHelp(args) || new ArgsHelper(appSettings).GetPathFormArgs(args, false).Length <= 1 )
			{
				appSettings.ApplicationType = AppSettings.StarskyAppType.Importer;
				new ArgsHelper(appSettings).NeedHelpShowDialog();
				return;
			}

			var inputPath = new ArgsHelper(appSettings).GetPathFormArgs(args, false);

			if ( appSettings.Verbose ) Console.WriteLine("inputPath " + inputPath);

			var importSettings = new ImportSettingsModel
			{
				DeleteAfter = new ArgsHelper(appSettings).GetMove(args),
				AgeFileFilterDisabled = new ArgsHelper(appSettings).GetAll(args),
				RecursiveDirectory = new ArgsHelper().NeedRecruisive(args),
				IndexMode = new ArgsHelper().GetIndexMode(args)
			};
			
			if ( appSettings.Verbose ) 
			{
				Console.WriteLine($"Options: DeleteAfter: {importSettings.DeleteAfter}, " +
				                  $"AgeFileFilterDisabled: {importSettings.AgeFileFilterDisabled},  " +
				                  $"RecursiveDirectory {importSettings.RecursiveDirectory}, " +
				                  $"IndexMode {importSettings.IndexMode}");
			}

			

			startupHelper.ImportService().Import(inputPath, importSettings);

			Console.WriteLine("Done Importing");

		}


	}
}
