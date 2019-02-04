using System;
using starskycore.Helpers;
using starskycore.Models;
using starskycore.Services;
using starskyGeoCli.Services;

namespace starskyGeoCli
{
    
    public static class Program
    {
        
        public static void Main(string[] args)
        {
            
            new ArgsHelper().SetEnvironmentByArgs(args);
            
            var startupHelper = new ConfigCliAppsStartupHelper();
            var appSettings = startupHelper.AppSettings();
            
            // Use args in application
            appSettings.Verbose = new ArgsHelper().NeedVerbose(args);

			// When there is no input show help
			if (new ArgsHelper().NeedHelp(args) || 
					(new ArgsHelper(appSettings).GetPathFormArgs(args,false).Length <= 1 
					&& new ArgsHelper().GetSubpathFormArgs(args).Length <= 1 
					&& new ArgsHelper(appSettings).GetSubpathRelative(args).Length <= 1))
			{
				appSettings.ApplicationType = AppSettings.StarskyAppType.Geo;
				new ArgsHelper(appSettings).NeedHelpShowDialog();
				return;
			}
	        
            // Using both options
            string inputPath;
            // -s = ifsubpath || -p is path
            if (new ArgsHelper(appSettings).IfSubpathOrPath(args))
            {
                inputPath = appSettings.DatabasePathToFilePath(
                    new ArgsHelper(appSettings).GetSubpathFormArgs(args)
                    );
            }
            else
            {
                inputPath = new ArgsHelper(appSettings).GetPathFormArgs(args,false);
	            // overwrite if folder not exist
	            if ( Files.IsFolderOrFile(inputPath) !=
	                 FolderOrFileModel.FolderOrFileTypeList.Folder ) inputPath = null;

            }
            
            // overwrite subpath with relative days
            // use -g or --SubpathRelative to use it.
            // envs are not supported
            var getSubpathRelative = new ArgsHelper(appSettings).GetSubpathRelative(args);
            if (getSubpathRelative != string.Empty)
            {
                inputPath = appSettings.DatabasePathToFilePath(getSubpathRelative);
            }

	        if ( inputPath == null )
	        {
		        Console.WriteLine($"Folder location is not found \nPlease try the `-h` command to get help ");
		        return;
	        }

            
            // used in this session to find the files back
            appSettings.StorageFolder = inputPath;

            var filesInDirectory = Files.GetFilesInDirectory(inputPath);
            var metaFilesInDirectory = startupHelper.ReadMeta()
                .ReadExifAndXmpFromFileAddFilePathHash(filesInDirectory);
            // FilePath is used as full
            
            var overwriteLocationNames = new ArgsHelper().GetAll(args);
            
            var gpxIndexMode = new ArgsHelper().GetIndexMode(args);

            if (gpxIndexMode)
            {
                Console.WriteLine("CameraTimeZone " + appSettings.CameraTimeZone);
                var toMetaFilesUpdate = new GeoIndexGpx(appSettings,startupHelper.ReadMeta()).LoopFolder(metaFilesInDirectory);
                new GeoLocationWrite(appSettings,startupHelper.ExifTool()).LoopFolder(toMetaFilesUpdate,false);
            }
            
            metaFilesInDirectory = new GeoReverseLookup(appSettings)
                .LoopFolderLookup(metaFilesInDirectory,overwriteLocationNames);
            new GeoLocationWrite(appSettings,startupHelper.ExifTool()).LoopFolder(metaFilesInDirectory,true);
            // update thumbs to avoid unnesseary re-generation
            new Thumbnail(appSettings).RenameThumb(metaFilesInDirectory);


        }

    }
}
