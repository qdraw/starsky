﻿using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using starsky.Helpers;
using starsky.Models;
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
            
            if (new ArgsHelper().NeedHelp(args))
            {
                appSettings.ApplicationType = AppSettings.StarskyAppType.Sync;
                new ArgsHelper(appSettings).NeedHelpShowDialog();
                return;
            }
            
            var pathFormArgs = new ArgsHelper(appSettings).GetPathFormArgs(args,false);

            var filesInDirectory = Files.GetFilesInDirectory(pathFormArgs);
            var geoList = new List<GeoListItem>(); 
            foreach (var fullfilepath in filesInDirectory)
            {
                var imageFormat = Files.GetImageFormat(fullfilepath);
                if (imageFormat == Files.ImageFormat.gpx) startupHelper.ReadMeta().ReadGpxFile(fullfilepath, geoList);
            }
            
            var metaFilesInDirectory = startupHelper.ReadMeta().ReadExifAndXmpFromFileAddFilePathHash(filesInDirectory);
            
            
            metaFilesInDirectory = new GeoReverseLookup(appSettings).LoopFolderLookup(metaFilesInDirectory);
            new GeoReverseWrite(appSettings,startupHelper.ExifTool()).LoopFolder(metaFilesInDirectory);

            
//            foreach (var item in metaFilesInDirectory)
//            {
//                if(item.DateTime.Year < 2) continue; // skip no date
//                
//                var dateTime = item.DateTime.ToLocalTime();
//
//
//                var fileGeoData = new GeoListItem();
//                if (item.Latitude < 0.00001 && item.Longitude < 0.00001) // for files without GeoData
//                {
//                    if(!geoList.Any()) continue;
//                    fileGeoData = geoList.OrderBy(p => Math.Abs((p.DateTime - dateTime).Ticks)).FirstOrDefault();
//
//                    Console.WriteLine(dateTime + " " + fileGeoData.DateTime + " " + fileGeoData.Latitude + " " + fileGeoData.Longitude);
//                }
//                else
//                {
//                    fileGeoData.Latitude = item.Latitude;
//                    fileGeoData.Longitude = item.Longitude;
//                }
//
//                
//            }

            
            
            






//            
//            // Using both options
//            string subpath;
//            // -s = ifsubpath || -p is path
//            if (new ArgsHelper(appSettings).IfSubpathOrPath(args))
//            {
//                subpath = new ArgsHelper(appSettings).GetSubpathFormArgs(args);
//            }
//            else
//            {
//                subpath = new ArgsHelper(appSettings).GetPathFormArgs(args);
//            }
//            
//            // overwrite subpath with relative days
//            // use -g or --SubpathRelative to use it.
//            // envs are not supported
//            var getSubpathRelative = new ArgsHelper(appSettings).GetSubpathRelative(args);
//            if (getSubpathRelative != null)
//            {
//                subpath = getSubpathRelative;
//            }
//
//            if (new ArgsHelper().GetIndexMode(args))
//            {
//                Console.WriteLine("Start indexing");
//                startupHelper.SyncService().SyncFiles(subpath);
//                Console.WriteLine("Done SyncFiles!");
//            }
//
//            if (new ArgsHelper(appSettings).GetThumbnail(args))
//            {
//
//                var fullPath = appSettings.DatabasePathToFilePath(subpath);
//                var isFolderOrFile = Files.IsFolderOrFile(fullPath);
//
//                if (appSettings.Verbose) Console.WriteLine(isFolderOrFile);
//                var exiftool = startupHelper.ExifTool();
//                
//                if (isFolderOrFile == FolderOrFileModel.FolderOrFileTypeList.File)
//                {
//                    // If single file => create thumbnail
//                    new Thumbnail(appSettings,exiftool).CreateThumb(subpath); // <= this uses subpath
//                }
//                else
//                {
//                    new ThumbnailByDirectory(appSettings,exiftool).CreateThumb(fullPath); // <= this uses fullpath
//                }
//                
//                Console.WriteLine("Thumbnail Done!");
//            }
//            
//            if (new ArgsHelper(appSettings).GetOrphanFolderCheck(args))
//            {
//                Console.WriteLine(">>>>> Heavy CPU Feature => Use with care <<<<< ");
//                startupHelper.SyncService().OrphanFolder(subpath);
//            }
//            Console.WriteLine("Done!");

            
            
            
            
            //                string pattern = "yyyy:MM:dd HH:mm:ss";
//
//                DateTime.TryParseExact("2018:08:21 16:49:41", 
//                    pattern, 
//                    CultureInfo.InvariantCulture, 
//                    DateTimeStyles.None, 
//                    out var dateTime);
//

            
            
            
        }

    }
}
