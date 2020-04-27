using System;
using System.Linq;
using starsky.feature.webhtmlpublish.Helpers;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Helpers;
using starsky.foundation.storage.Models;
using starsky.foundation.storage.Storage;
using starskycore.Helpers;
using starskycore.Models;
using starskycore.Services;
using starskywebftpcli.Services;

namespace starskywebftpcli
{
	static class Program
	{
		static void Main(string[] args)
		{
			// Use args in application
			new ArgsHelper().SetEnvironmentByArgs(args);
			
			// inject services + appsettings
			var startupHelper = new ConfigCliAppsStartupHelper();
			var appSettings = startupHelper.AppSettings();
            
			
			// verbose mode
			appSettings.Verbose = new ArgsHelper().NeedVerbose(args);
            
			// help window
			if (new ArgsHelper().NeedHelp(args))
			{
				appSettings.ApplicationType = AppSettings.StarskyAppType.WebFtp;
				new ArgsHelper(appSettings).NeedHelpShowDialog();
				return;
			}
			
			// inputPath
			var inputPath = new ArgsHelper(appSettings).GetPathFormArgs(args,false);

			if (string.IsNullOrWhiteSpace(inputPath))
			{
				Console.WriteLine("Please use the -p to add a path first");
				return;
			}
            
			// used in this session to find the files back
			appSettings.StorageFolder = inputPath;
			var storage = startupHelper.SubPathStorage();

			if ( storage.IsFolderOrFile("/") == FolderOrFileModel.FolderOrFileTypeList.Deleted )
			{
				Console.WriteLine($"Folder location {inputPath} is not found \nPlease try the `-h` command to get help ");
				return;
			}
			
			// check if settings is valid
			if ( string.IsNullOrEmpty(appSettings.WebFtp) )
			{
				Console.WriteLine($"Please update the WebFtp settings in appsettings.json"  );
				return;
			}

			// set storage folder !this is important!
			appSettings.StorageFolder = inputPath;
			
			// inject manifest
			if ( ! new PublishManifest(startupHelper.SelectorStorage().Get(SelectorStorage.StorageServices.HostFilesystem), 
				appSettings,new PlainTextFileHelper()).ImportManifest() )
			{
				// import false >
				Console.WriteLine($"Please run 'starskywebhtmlcli' first to generate a settings file"  );
				return;
			}

			//  now run the service
			var ftpService = new FtpService(appSettings,storage).Run();
			if ( !ftpService ) return;
			
			// get prepend path to show
			var prepend = appSettings.GetWebSafeReplacedName(
				appSettings.PublishProfiles
					.FirstOrDefault(p => !string.IsNullOrEmpty(p.Prepend))
					?.Prepend
			);
			
			// show prepend path!
			Console.WriteLine(prepend);

		}
	}
}
