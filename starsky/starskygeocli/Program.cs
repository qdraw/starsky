using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using starsky.Helpers;
using starsky.Models;
using starsky.Services;
using starskyGeoCli.Services;

namespace starskyGeoCli
{
    
    public static class Program
    {
        public static CultureInfo GetCultureFromTwoLetterCountryCode(string twoLetterIsoCountryCode)
        {
            try
            {
                return CultureInfo
                    .GetCultures(CultureTypes.AllCultures 
                                               & ~ CultureTypes.NeutralCultures)
                    .FirstOrDefault(m => m.Name.EndsWith( "-" + twoLetterIsoCountryCode) );
            }
            catch
            {
                return null;
            }
        }
        
        public static void Main(string[] args)
        {
            
            new ArgsHelper().SetEnvironmentByArgs(args);
            
            var startupHelper = new ConfigCliAppsStartupHelper();
            var appSettings = startupHelper.AppSettings();
            
            // Use args in application
            appSettings.Verbose = new ArgsHelper().NeedVerbose(args);
            
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
            }
            
            // overwrite subpath with relative days
            // use -g or --SubpathRelative to use it.
            // envs are not supported
            var getSubpathRelative = new ArgsHelper(appSettings).GetSubpathRelative(args);
            if (getSubpathRelative != null)
            {
                inputPath = appSettings.DatabasePathToFilePath(getSubpathRelative);
            }

            if (new ArgsHelper().NeedHelp(args) || inputPath == null || 
                (new ArgsHelper().GetPathFormArgs(args,false).Length <= 1 
                && new ArgsHelper().GetSubpathFormArgs(args).Length <= 1 
                 && new ArgsHelper(appSettings).GetSubpathRelative(args).Length <= 1) )
            {
                appSettings.ApplicationType = AppSettings.StarskyAppType.Geo;
                new ArgsHelper(appSettings).NeedHelpShowDialog();
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
