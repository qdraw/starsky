using System;
using System.IO;
using System.Linq;
using starskycore.Helpers;
using starskycore.Models;
using starskycore.Services;
using starskywebhtmlcli.Services;

namespace starskywebhtmlcli
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            
            // Use args in application
            new ArgsHelper().SetEnvironmentByArgs(args);
            var startupHelper = new ConfigCliAppsStartupHelper();
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
            
            if(FilesHelper.IsFolderOrFile(inputPath) != FolderOrFileModel.FolderOrFileTypeList.Folder)
                Console.WriteLine("Please add a valid folder: " + inputPath);

            if (appSettings.Name == new AppSettings().Name)
            {
                var suggestedInput = Path.GetFileName(inputPath);
                
                Console.WriteLine("\nWhat is the name of the item? (for: "+ suggestedInput +" press Enter)\n ");
                var name = Console.ReadLine();
                appSettings.Name = name;
                if (string.IsNullOrEmpty(name))
                {
                    appSettings.Name = suggestedInput;
                }
            }

            if(appSettings.Verbose) Console.WriteLine("Name: " + appSettings.Name);
            if(appSettings.Verbose) Console.WriteLine("inputPath " + inputPath);

            // used in this session to find the files back
            appSettings.StorageFolder = inputPath;

	        var iStorage = startupHelper.Storage();
			// use relative to StorageFolder
	        var listOfFiles = iStorage.GetAllFilesInDirectory("/")
		        .Where(ExtensionRolesHelper.IsExtensionExifToolSupported).ToList();
            var fileIndexList = startupHelper.ReadMeta().ReadExifAndXmpFromFileAddFilePathHash(listOfFiles);
            
            // Create thumbnails from the source images 
       
	        var thumbByDir = new Thumbnail(iStorage,startupHelper.ExifTool());
            thumbByDir.CreateThumb(inputPath);

	        var base64DataUri = new ToBase64DataUriList(iStorage).Create(fileIndexList);
	        
            new LoopPublications(appSettings,startupHelper.ExifTool())
                .Render(fileIndexList,base64DataUri );

			// Export all
			new ExportManifest(appSettings,new PlainTextFileHelper()).Export();

		}
        
    }
}
