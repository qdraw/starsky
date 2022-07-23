using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using NGeoNames;
using NGeoNames.Entities;
using NGeoNames.Parsers;
using starsky.feature.geolookup.Interfaces;
using starsky.feature.geolookup.Models;
using starsky.foundation.database.Models;
using starsky.foundation.injection;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.readmeta.Helpers;

namespace starsky.feature.geolookup.Services
{
	[Service(typeof(IGeoReverseLookup), InjectionLifetime = InjectionLifetime.Scoped)]
	[SuppressMessage("Performance", "CA1822:Mark members as static")]
	public class GeoReverseLookup : IGeoReverseLookup
    {
        private ReverseGeoCode<ExtendedGeoName> _reverseGeoCode;
        private readonly IEnumerable<Admin1Code> _admin1CodesAscii;
        private readonly IMemoryCache _cache;
        private readonly AppSettings _appSettings;
        private readonly IWebLogger _logger;

        /// <summary>
        /// Getting GeoData
        /// </summary>
        /// <param name="appSettings">to know where to store the deps files</param>
        /// <param name="geoFileDownload">Abstraction to download Geo Data</param>
        /// <param name="memoryCache">for keeping status</param>
        public GeoReverseLookup(AppSettings appSettings, IGeoFileDownload geoFileDownload, IMemoryCache memoryCache = null, IWebLogger logger = null)
        {
	        // Needed when not having this, application will fail
	        geoFileDownload.Download().ConfigureAwait(false).GetAwaiter();
	        
	        _appSettings = appSettings;
	        _logger = logger;
            _admin1CodesAscii = GeoFileReader.ReadAdmin1Codes(
                Path.Combine(appSettings.DependenciesFolder, "admin1CodesASCII.txt"));
            
            // Create our ReverseGeoCode class and supply it with data
            InitReverseGeoCode();

            _cache = memoryCache;
        }

        private void InitReverseGeoCode()
        {
	        if ( _reverseGeoCode != null ) return;
	        try
	        {
		        _reverseGeoCode = new ReverseGeoCode<ExtendedGeoName>(
			        GeoFileReader.ReadExtendedGeoNames(
				        Path.Combine(_appSettings.DependenciesFolder, GeoFileDownload.CountryName + ".txt")));
	        }
	        catch ( ParserException e )
	        {
		        _logger?.LogError(e,"catch-ed GeoFileDownload GeoReverseLookup error");
	        }
	        catch ( FileNotFoundException e )
	        {
		        _logger?.LogError(e,"catch-ed GeoFileDownload GeoReverseLookup error");
	        }
        }

        private string GetAdmin1Name(string countryCode, string[] adminCodes)
        {
            if (adminCodes.Length != 4) return null;

            var admin1Code = countryCode + "." + adminCodes[0];
            
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
        /// <param name="overwriteLocationNames">true = overwrite the location names, that have a gps location </param>
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
	    /// Reverse Geo Syncing for a folder
	    /// </summary>
	    /// <param name="metaFilesInDirectory">list of files to lookup</param>
	    /// <param name="overwriteLocationNames">true = overwrite the location names, that have a gps location</param>
	    /// <returns></returns>
	    public List<FileIndexItem> LoopFolderLookup(List<FileIndexItem> metaFilesInDirectory,
            bool overwriteLocationNames)
        {
	        InitReverseGeoCode();
	        
	        metaFilesInDirectory = RemoveNoUpdateItems(metaFilesInDirectory,overwriteLocationNames);

            var subPath = metaFilesInDirectory.FirstOrDefault()?.ParentDirectory;
            
	        new GeoCacheStatusService(_cache).StatusUpdate(subPath, metaFilesInDirectory.Count, StatusType.Total);

            foreach (var metaFileItem in metaFilesInDirectory.Select(
	            (value, index) => new { value, index }))
            {
                // Create a point from a lat/long pair from which we want to conduct our search(es) (center)
                var place = _reverseGeoCode.CreateFromLatLong(
	                metaFileItem.value.Latitude, metaFileItem.value.Longitude);
            
                // Find nearest
                var nearestPlace = _reverseGeoCode.NearestNeighbourSearch(place, 1).FirstOrDefault();
            
                // Distance to avoid non logic locations
                var distanceTo = GeoDistanceTo.GetDistance(
                    nearestPlace.Latitude, 
                    nearestPlace.Longitude, 
                    metaFileItem.value.Latitude,
                    metaFileItem.value.Longitude);

                new GeoCacheStatusService(_cache).StatusUpdate(metaFileItem.value.ParentDirectory, 
	                metaFileItem.index, StatusType.Current);
	                
                if (distanceTo > 35) continue; 
                // if less than 35 kilometers from that place add it to the object

                metaFileItem.value.LocationCity = nearestPlace.NameASCII;
                
                // Catch is used for example the region VA (Vatican City)
                try
                {
	                metaFileItem.value.LocationCountry = new RegionInfo(nearestPlace.CountryCode).NativeName;
                }
                catch ( ArgumentException e )
                {
	                Console.WriteLine(e);
                }
                metaFileItem.value.LocationState = GetAdmin1Name(nearestPlace.CountryCode, nearestPlace.Admincodes);
            }
            
            // Ready signal
            new GeoCacheStatusService(_cache).StatusUpdate(subPath,
	            metaFilesInDirectory.Count, StatusType.Current);
            
            return metaFilesInDirectory;
        }
        
    }
}
