using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Caching.Memory;
using starsky.feature.geolookup.Models;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starsky.foundation.readmeta.Models;
using starsky.foundation.readmeta.Services;
using starsky.foundation.storage.Interfaces;

[assembly: InternalsVisibleTo("starskytest")]
namespace starsky.feature.geolookup.Services
{
    public class GeoIndexGpx
    {
	    private readonly AppSettings _appSettings;
	    private readonly ReadMetaGpx _readMetaGpx;
	    private readonly IStorage _iStorage;
	    private readonly IMemoryCache _cache;

	    public GeoIndexGpx(AppSettings appSettings, IStorage iStorage, IMemoryCache memoryCache = null )
        {
	        _readMetaGpx = new ReadMetaGpx();
            _appSettings = appSettings;
	        _iStorage = iStorage;
	        _cache = memoryCache;
        }
        
        private List<FileIndexItem> GetNoLocationItems(IEnumerable<FileIndexItem> metaFilesInDirectory,
            bool overwriteLocations = false)
        {
            return metaFilesInDirectory.Where(
                    metaFileItem =>
                        (Math.Abs(metaFileItem.Latitude) < 0.001 && Math.Abs(metaFileItem.Longitude) < 0.001)
                        && metaFileItem.DateTime.Year > 2) // ignore files without a date
                .ToList();
        }

        private List<GeoListItem> GetGpxFile(List<FileIndexItem> metaFilesInDirectory)
        {
            var geoList = new List<GeoListItem>(); 
            foreach (var metaFileItem in metaFilesInDirectory)
            {
	            
                if( !ExtensionRolesHelper.IsExtensionForceGpx(metaFileItem.FileName) ) continue;
	            
	            if ( !_iStorage.ExistFile(metaFileItem.FilePath) ) continue;
	            
	            using ( var stream = _iStorage.ReadStream(metaFileItem.FilePath) )
	            {
		            geoList.AddRange(_readMetaGpx.ReadGpxFile(stream, geoList));
	            }
            }
            return geoList;
        }

        /// <summary>
        /// Convert to the appSettings timezone setting
        /// </summary>
        /// <param name="valueDateTime"></param>
        /// <returns>The time in the specified timezone</returns>
        /// <exception cref="ArgumentException">DateTime Kind should not be Local</exception>
        internal DateTime ConvertTimeZone(DateTime valueDateTime)
        {
	        // Not supported by TimeZoneInfo convert
	        if ( valueDateTime.Kind != DateTimeKind.Unspecified ) 
	        {
		        throw new ArgumentException("DateTime Kind should be Unspecified", nameof(DateTime));
	        }
	        
	        return TimeZoneInfo.ConvertTime(valueDateTime, _appSettings.CameraTimeZoneInfo, TimeZoneInfo.Utc); 
        }

        public List<FileIndexItem> LoopFolder(List<FileIndexItem> metaFilesInDirectory)
        {
            var toUpdateMetaFiles = new List<FileIndexItem>();

            var gpxList = GetGpxFile(metaFilesInDirectory);
            if(!gpxList.Any()) return toUpdateMetaFiles;

            metaFilesInDirectory = GetNoLocationItems(metaFilesInDirectory);

            var subPath = metaFilesInDirectory.FirstOrDefault()?.ParentDirectory;
            new GeoCacheStatusService(_cache).Update(subPath,
	            metaFilesInDirectory.Count, StatusType.Total);
            
            foreach (var metaFileItem in metaFilesInDirectory.Select((value, index) => new { value, index }))
            {
	            var dateTimeCameraUtc = ConvertTimeZone(metaFileItem.value.DateTime);
                
                var fileGeoData = gpxList.OrderBy(p => Math.Abs((p.DateTime - dateTimeCameraUtc).Ticks)).FirstOrDefault();
                if(fileGeoData == null) continue;

                var minutesDifference = (dateTimeCameraUtc - fileGeoData.DateTime).TotalMinutes;
                if(minutesDifference < -5 || minutesDifference > 5) continue;
                
                metaFileItem.value.Latitude = fileGeoData.Latitude;
                metaFileItem.value.Longitude = fileGeoData.Longitude;
                metaFileItem.value.LocationAltitude = fileGeoData.Altitude;
                
                toUpdateMetaFiles.Add(metaFileItem.value);
                
                // status update
                new GeoCacheStatusService(_cache).Update(metaFileItem.value.ParentDirectory, 
	                metaFileItem.index, StatusType.Current);
            }
            
            // Ready signal
            new GeoCacheStatusService(_cache).Update(subPath,
	            metaFilesInDirectory.Count, StatusType.Current);
            
            return toUpdateMetaFiles;
        }
    }
}
