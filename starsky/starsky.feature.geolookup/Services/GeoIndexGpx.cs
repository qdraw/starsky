using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using starsky.foundation.database.Models;
using starsky.foundation.geo.Models;
using starskycore.Helpers;
using starskycore.Interfaces;
using starskycore.Models;
using starskycore.Services;

namespace starsky.foundation.geo.Services
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
                var dateTimeCameraUtc = TimeZoneInfo.ConvertTime(metaFileItem.value.DateTime, _appSettings.CameraTimeZoneInfo,
                    TimeZoneInfo.Utc); 
                
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
