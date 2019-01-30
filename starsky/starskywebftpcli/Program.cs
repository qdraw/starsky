using System;
using System.Linq;
using starskycore.Helpers;
using starskycore.Models;
using starskywebftpcli.Services;

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
			
			if ( string.IsNullOrEmpty(appSettings.WebFtp) )
			{
				Console.WriteLine($"Please update the WebFtp settings in appsettings.json"  );
				return;
			}

			appSettings.StorageFolder = inputPath;

			if ( ! new ExportManifest(appSettings,new PlainTextFileHelper()).Import() )
			{
				// import false >
				Console.WriteLine($"Please run starskywebhtmlcli first to generate a settings file"  );
				return;
			}

			var ftpService = new FtpService(appSettings).Run();
			if ( !ftpService ) return;
			var prepend = appSettings.GetWebSafeReplacedName(
				appSettings.PublishProfiles
					.FirstOrDefault(p => !string.IsNullOrEmpty(p.Prepend))
					?.Prepend
			);
			Console.WriteLine(prepend);

		}
	}
}
