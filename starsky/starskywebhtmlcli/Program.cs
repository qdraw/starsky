using System;
using System.IO;
using starsky.Models;
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
            
            var inputPath = new ArgsHelper().GetPathFormArgs(args,false);

            if (string.IsNullOrWhiteSpace(inputPath))
            {
                Console.WriteLine("Please use the -p to add a path first");
                return;
            }
            
            if(Files.IsFolderOrFile(inputPath) != FolderOrFileModel.FolderOrFileTypeList.Folder)
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
            
            var listOfFiles = Files.GetFilesInDirectory(inputPath);
            var fileIndexList = startupHelper.ReadMeta().ReadExifAndXmpFromFileAddFilePathHash(listOfFiles);
            
            // Create thumbnails from the source images 
            var thumbByDir = new ThumbnailByDirectory(appSettings,startupHelper.ExifTool());
            thumbByDir.CreateThumb(inputPath);
            new LoopPublications(appSettings,startupHelper.ExifTool())
                .Render(fileIndexList,thumbByDir.ToBase64DataUriList(fileIndexList));
        }
        
    }
}
