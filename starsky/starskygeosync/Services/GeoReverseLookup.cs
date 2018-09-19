using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using NGeoNames;
using NGeoNames.Entities;
using starsky.Helpers;
using starsky.Models;

namespace starskygeosync.Services
{
    public class GeoReverseLookup
    {
        private readonly ReverseGeoCode<ExtendedGeoName> _reverseGeoCode;
        private IEnumerable<Admin1Code> _admin1CodesAscii;

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

        private string GetAdmin2Name(string countryCode, string[] admincodes)
        {
            if (admincodes.Length != 4) return null;

            var admin1Code = countryCode + "." + admincodes[0];
            
            var admin2Object = _admin1CodesAscii.FirstOrDefault(p => p.Code == admin1Code);
            return admin2Object?.NameASCII;
        }

        public void LoopFolderLookup(List<FileIndexItem> metaFilesInDirectory)
        {
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

                if (distanceTo > 40) continue; // 40 kilometers

                metaFileItem.LocationCity = nearestPlace.NameASCII;
                metaFileItem.LocationCountry = new RegionInfo(nearestPlace.CountryCode).EnglishName;
                metaFileItem.LocationState = GetAdmin2Name(nearestPlace.CountryCode, nearestPlace.Admincodes);

            }
        }
    }
}