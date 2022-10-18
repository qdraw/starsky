#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using NGeoNames;
using NGeoNames.Entities;
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
        private ReverseGeoCode<ExtendedGeoName>? _reverseGeoCode;
        private IEnumerable<Admin1Code>? _admin1CodesAscii;
        private readonly IMemoryCache? _cache;
        private readonly AppSettings _appSettings;
        private readonly IWebLogger _logger;
        private readonly IGeoFileDownload _geoFileDownload;

        /// <summary>
        /// Getting GeoData
        /// </summary>
        /// <param name="appSettings">to know where to store the deps files</param>
        /// <param name="geoFileDownload">Abstraction to download Geo Data</param>
        /// <param name="memoryCache">for keeping status</param>
        /// <param name="logger">debug logger</param>
        public GeoReverseLookup(AppSettings appSettings, IGeoFileDownload geoFileDownload, IWebLogger logger, IMemoryCache? memoryCache = null)
        {
	        _appSettings = appSettings;
	        _logger = logger;
	        _geoFileDownload = geoFileDownload;
	        _reverseGeoCode = null;
	        _admin1CodesAscii = null;
	        _cache = memoryCache;
        }
        
        internal async Task<(IEnumerable<Admin1Code>, ReverseGeoCode<ExtendedGeoName>)> SetupAsync()
        {
	        await _geoFileDownload.DownloadAsync();
	        
			_admin1CodesAscii = GeoFileReader.ReadAdmin1Codes(
				Path.Combine(_appSettings.DependenciesFolder, "admin1CodesASCII.txt"));
			
			_reverseGeoCode = new ReverseGeoCode<ExtendedGeoName>(
				GeoFileReader.ReadExtendedGeoNames(
					Path.Combine(_appSettings.DependenciesFolder, GeoFileDownload.CountryName + ".txt")));

			return (_admin1CodesAscii, _reverseGeoCode );
        }

        internal string? GetAdmin1Name(string countryCode, string[] adminCodes)
        {
            if (_admin1CodesAscii == null || adminCodes.Length != 4) return null;

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
	    public async Task<List<FileIndexItem>> LoopFolderLookup(List<FileIndexItem> metaFilesInDirectory,
            bool overwriteLocationNames)
	    {
		    if ( _reverseGeoCode == null )
		    {
			    (_, _reverseGeoCode) = await SetupAsync();
		    }

		    metaFilesInDirectory = RemoveNoUpdateItems(metaFilesInDirectory,overwriteLocationNames);

            var subPath = metaFilesInDirectory.FirstOrDefault()?.ParentDirectory;
            
	        new GeoCacheStatusService(_cache).StatusUpdate(subPath, metaFilesInDirectory.Count*2, StatusType.Total);

            foreach (var metaFileItem in metaFilesInDirectory.Select(
	            (value, index) => new { value, index }))
            {
                // Create a point from a lat/long pair from which we want to conduct our search(es) (center)
                var place = _reverseGeoCode.CreateFromLatLong(
	                metaFileItem.value.Latitude, metaFileItem.value.Longitude);
            
                // Find nearest
                var nearestPlace = _reverseGeoCode.NearestNeighbourSearch(place, 1).FirstOrDefault();

                if ( nearestPlace == null ) continue;
                
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
	                var region = new RegionInfo(nearestPlace.CountryCode);
	                metaFileItem.value.LocationCountry = region.NativeName;
	                metaFileItem.value.LocationCountryCode = region.ThreeLetterISORegionName;
                }
                catch ( ArgumentException e )
                {
	                _logger.LogInformation("[GeoReverseLookup] " + e.Message);
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
