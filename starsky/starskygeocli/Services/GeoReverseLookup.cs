using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using NGeoNames;
using NGeoNames.Entities;
using starskycore.Helpers;
using starskycore.Models;

namespace starskyGeoCli.Services
{
    public class GeoReverseLookup
    {
        private readonly ReverseGeoCode<ExtendedGeoName> _reverseGeoCode;
        private readonly IEnumerable<Admin1Code> _admin1CodesAscii;

        private const string CountryName = "cities1000";

        public GeoReverseLookup(AppSettings appSettings)
        {
            // The class for geodata
            var downloader = GeoFileDownloader.CreateGeoFileDownloader();
            downloader.DownloadFile(CountryName + ".zip", appSettings.TempFolder);    
            // Zipfile will be automatically extracted
            
            // code for the second administrative division, a county in the US, see file admin2Codes.txt; varchar(80)
            downloader.DownloadFile("admin1CodesASCII.txt", appSettings.TempFolder);

            _admin1CodesAscii = GeoFileReader.ReadAdmin1Codes(
                Path.Combine(appSettings.TempFolder, "admin1CodesASCII.txt"));
            
            // Create our ReverseGeoCode class and supply it with data
            _reverseGeoCode = new ReverseGeoCode<ExtendedGeoName>(
                GeoFileReader.ReadExtendedGeoNames(Path.Combine(appSettings.TempFolder, CountryName + ".txt"))
            );
            // end geodata
        }

        private string GetAdmin1Name(string countryCode, string[] admincodes)
        {
            if (admincodes.Length != 4) return null;

            var admin1Code = countryCode + "." + admincodes[0];
            
            var admin2Object = _admin1CodesAscii.FirstOrDefault(p => p.Code == admin1Code);
            return admin2Object?.NameASCII;
        }

        /// <summary>
        /// Checks for files that already done
        /// if latitude is not location 0,0, That's default
        /// If one of the meta items are missing, keep in list
        /// If extension in exifTool supported, so no gpx
        /// </summary>
        /// <param name="metaFilesInDirectory">List of files with metadata</param>
        /// <param name="overwriteLocationNames"></param>
        /// <returns>list that can be updated</returns>
        public List<FileIndexItem> RemoveNoUpdateItems(IEnumerable<FileIndexItem> metaFilesInDirectory, 
            bool overwriteLocationNames)
        {
            // this will overwrite the location names, that have a gps location 
            if (overwriteLocationNames) 
	            return metaFilesInDirectory.Where(
                    metaFileItem =>
                        Math.Abs(metaFileItem.Latitude) > 0.001 && Math.Abs(metaFileItem.Longitude) > 0.001)
                .ToList();
            
            // the default situation
            return metaFilesInDirectory.Where(
                metaFileItem => 
                    ((Math.Abs(metaFileItem.Latitude) > 0.001 && Math.Abs(metaFileItem.Longitude) > 0.001) 
                    && (string.IsNullOrEmpty(metaFileItem.LocationCity) 
                        || string.IsNullOrEmpty(metaFileItem.LocationState) 
                        || string.IsNullOrEmpty(metaFileItem.LocationCountry)))
                    && ExtensionRolesHelper.IsExtensionExifToolSupported(metaFileItem.FileName)
                    ).ToList();
        }


	    /// <summary>
	    /// 
	    /// </summary>
	    /// <param name="metaFilesInDirectory"></param>
	    /// <param name="overwriteLocationNames"></param>
	    /// <returns></returns>
	    public List<FileIndexItem> LoopFolderLookup(List<FileIndexItem> metaFilesInDirectory,
            bool overwriteLocationNames)
        {
            metaFilesInDirectory = RemoveNoUpdateItems(metaFilesInDirectory,overwriteLocationNames);
            
            foreach (var metaFiles in metaFilesInDirectory)
            {
                Console.WriteLine("~~~~ " + metaFiles.Latitude + " " +
                                  metaFiles.Longitude + " " +
                                  metaFiles.LocationCity + " " 
                                  + metaFiles.LocationCountry + " "  +metaFiles.LocationState);
            }  
            
            foreach (var metaFileItem in metaFilesInDirectory)
            {
                // Create a point from a lat/long pair from which we want to conduct our search(es) (center)
                var place = _reverseGeoCode.CreateFromLatLong(metaFileItem.Latitude, metaFileItem.Longitude);
            
                // Find nearest
                var nearestPlace = _reverseGeoCode.NearestNeighbourSearch(place, 1).FirstOrDefault();
            
                // Distance to avoid non logic locations
                var distanceTo = GeoDistanceTo.GetDistance(
                    nearestPlace.Latitude, 
                    nearestPlace.Longitude, 
                    metaFileItem.Latitude,
                    metaFileItem.Longitude);

                if (distanceTo > 40) continue; 
                // if less than 40 kilometers from that place add it to the object

                metaFileItem.LocationCity = nearestPlace.NameASCII;
                
                // Catch is used for example the region VA (Vatican City)
                try
                {
	                metaFileItem.LocationCountry = new RegionInfo(nearestPlace.CountryCode).NativeName;
                }
                catch ( ArgumentException e )
                {
	                Console.WriteLine(e);
                }
                metaFileItem.LocationState = GetAdmin1Name(nearestPlace.CountryCode, nearestPlace.Admincodes);
            }
            return metaFilesInDirectory;
        }
        
    }
}
