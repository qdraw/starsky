using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using starsky.feature.webhtmlpublish.Helpers;
using starsky.feature.webhtmlpublish.Services;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Services;
using starsky.foundation.storage.Helpers;
using starsky.foundation.storage.Models;
using starsky.foundation.storage.Storage;
using starsky.foundation.thumbnailgeneration.Services;
using starskycore.Helpers;
using starskycore.Models;
using starskycore.Services;

namespace starskywebhtmlcli
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            new ArgsHelper().SetEnvironmentByArgs(args);
            var startupHelper = new ConfigCliAppsStartupHelper();
	        
	        // Run feature:
            var appSettings = startupHelper.AppSettings();
            
            appSettings.Verbose = new ArgsHelper().NeedVerbose(args);
            
            if (new ArgsHelper().NeedHelp(args))
            {
                appSettings.ApplicationType = AppSettings.StarskyAppType.WebHtml;
                new ArgsHelper(appSettings).NeedHelpShowDialog();
                return;
            }
            
            var inputPath = new ArgsHelper(appSettings).GetPathFormArgs(args,false);

            if (string.IsNullOrWhiteSpace(inputPath))
            {
                Console.WriteLine("Please use the -p to add a path first");
                return;
            }
            
            if(startupHelper.HostFileSystemStorage().IsFolderOrFile(inputPath) != FolderOrFileModel.FolderOrFileTypeList.Folder)
                Console.WriteLine("Please add a valid folder: " + inputPath);

            var name = new ArgsHelper().GetName(args);
            if ( string.IsNullOrWhiteSpace(name))
            {
	            var suggestedInput = Path.GetFileName(inputPath);
                
	            Console.WriteLine("\nWhat is the name of the item? (for: "+ suggestedInput +" press Enter)\n ");
	            name = Console.ReadLine();
	            if (string.IsNullOrEmpty(name))
	            {
		            name = suggestedInput;
	            }
            }

            if(appSettings.Verbose) Console.WriteLine("Name: " + name);
            if(appSettings.Verbose) Console.WriteLine("inputPath " + inputPath);

            // used in this session to find the files back
            appSettings.StorageFolder = inputPath;

	        var iStorage = startupHelper.SubPathStorage();
			// use relative to StorageFolder
	        var listOfFiles = iStorage.GetAllFilesInDirectory("/")
		        .Where(ExtensionRolesHelper.IsExtensionExifToolSupported).ToList();
	        
            var fileIndexList = startupHelper.ReadMeta().ReadExifAndXmpFromFileAddFilePathHash(listOfFiles);
            
            // Create thumbnails from the source images 
			new Thumbnail(iStorage, startupHelper.ThumbnailStorage()).CreateThumb("/"); // <= subPath style
	        
	        var base64DataUri = new ToBase64DataUriList(iStorage, startupHelper.ThumbnailStorage()).Create(fileIndexList);

	        var profileName = new PublishPreflight(appSettings).GetPublishProfileNameByIndex(0);
			await new WebHtmlPublishService(startupHelper.SelectorStorage(), appSettings, 
					startupHelper.ExifTool(), startupHelper.ReadMeta(), new ConsoleWrapper())
				.Render(fileIndexList, base64DataUri, profileName);

			// Copy all items in the subFolder content for example JavaScripts
			new Content(iStorage).CopyContent();

			// Export all
			new PublishManifest( startupHelper.SelectorStorage().Get(SelectorStorage.StorageServices.HostFilesystem),appSettings,
				new PlainTextFileHelper()).ExportManifest();

		}
        
    }
}
