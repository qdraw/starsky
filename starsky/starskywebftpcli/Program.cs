using System;
using starskycore.Helpers;
using starskycore.Models;

namespace starskywebftpcli
{
	static class Program
	{
		static void Main(string[] args)
		{
			// Use args in application
			new ArgsHelper().SetEnvironmentByArgs(args);
			var startupHelper = new ConfigCliAppsStartupHelper();
			var appSettings = startupHelper.AppSettings();
            
			appSettings.Verbose = new ArgsHelper().NeedVerbose(args);
            
			if (new ArgsHelper().NeedHelp(args))
			{
				appSettings.ApplicationType = AppSettings.StarskyAppType.WebFtp;
				new ArgsHelper(appSettings).NeedHelpShowDialog();
				return;
			}
			
			var inputPath = new ArgsHelper().GetPathFormArgs(args,false);

			if (string.IsNullOrWhiteSpace(inputPath))
			{
				Console.WriteLine("Please use the -p to add a path first");
				return;
			}
            
			if(Files.IsFolderOrFile(inputPath) != FolderOrFileModel.FolderOrFileTypeList.Folder)
				Console.WriteLine("Please add a valid folder: " + inputPath);
			
			
			Console.WriteLine("Hello World!");
		}
	}
}
