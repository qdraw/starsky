using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using NGeoNames;
using NGeoNames.Entities;
using starsky.corehelpers.Helpers;
using starsky.corehelpers.Models;
using starsky.Helpers;
using starsky.meta.Services;
using starsky.models.Models;

namespace starskygeosync
{
    
    public static class Program
    {
        public static CultureInfo GetCultureFromTwoLetterCountryCode(string twoLetterISOCountryCode)
        {
            try
            {
                return CultureInfo
                    .GetCultures(CultureTypes.AllCultures 
                                               & ~ CultureTypes.NeutralCultures)
                    .FirstOrDefault(m => m.Name.EndsWith( "-" + twoLetterISOCountryCode) );
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
            
            // The class for geodata
            var downloader = GeoFileDownloader.CreateGeoFileDownloader();
            downloader.DownloadFile("cities1000.zip", appSettings.TempFolder);    // Zipfile will be automatically extracted
            
            // Create our ReverseGeoCode class and supply it with data
            var reverseGeoCode = new ReverseGeoCode<ExtendedGeoName>(
                GeoFileReader.ReadExtendedGeoNames(Path.Combine(appSettings.TempFolder,"cities1000.txt"))
            );
            // end geodata
            
            
            foreach (var item in metaFilesInDirectory)
            {

                if(item.DateTime.Year < 2) continue; // skip no date
                
                var dateTime = item.DateTime.ToLocalTime();


                var fileGeoData = new GeoListItem();
                if (item.Latitude < 0.00001 && item.Longitude < 0.00001) // for files without GeoData
                {
                    if(!geoList.Any()) continue;
                    fileGeoData = geoList.OrderBy(p => Math.Abs((p.DateTime - dateTime).Ticks)).FirstOrDefault();

                    Console.WriteLine(dateTime + " " + fileGeoData.DateTime + " " + fileGeoData.Latitude + " " + fileGeoData.Longitude);
                }
                else
                {
                    fileGeoData.Latitude = item.Latitude;
                    fileGeoData.Longitude = item.Longitude;
                }

                // Create a point from a lat/long pair from which we want to conduct our search(es) (center)
                var place = reverseGeoCode.CreateFromLatLong(fileGeoData.Latitude, fileGeoData.Longitude);
            
                // Find nearest
                var nearestPlace = reverseGeoCode.NearestNeighbourSearch(place, 1).FirstOrDefault();
            
                var distanceTo = GeoDistanceTo.GetDistance(
                    nearestPlace.Latitude, 
                    nearestPlace.Longitude, 
                    fileGeoData.Latitude,
                    fileGeoData.Longitude);

                if (distanceTo > 50000) continue; // 50 kilometers

                Console.WriteLine(distanceTo);
                Console.WriteLine(nearestPlace.Latitude + " " + nearestPlace.Longitude + " " + nearestPlace.NameASCII + " " +  new RegionInfo(nearestPlace.CountryCode).EnglishName );
            }

            
            
            






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
