using System;
using System.Collections.Generic;
using System.Linq;
using starskycore.Helpers;
using starskycore.Interfaces;
using starskycore.Models;
using starskycore.Services;

namespace starskygeocore.Services
{
    public class GeoIndexGpx
    {
        private readonly IReadMeta _readMeta;
        private readonly AppSettings _appSettings;
	    private ReadMetaGpx _readMetaGpx;
	    private IStorage _iStorage;

	    public GeoIndexGpx(AppSettings appSettings, IReadMeta readMeta, IStorage iStorage)
        {
            _readMeta = readMeta;
	        _readMetaGpx = new ReadMetaGpx();
            _appSettings = appSettings;
	        _iStorage = iStorage;
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

            foreach (var metaFileItem in metaFilesInDirectory)
            {
                var dateTimeCameraUtc = TimeZoneInfo.ConvertTime(metaFileItem.DateTime, _appSettings.CameraTimeZoneInfo,
                    TimeZoneInfo.Utc); 
                
                var fileGeoData = gpxList.OrderBy(p => Math.Abs((p.DateTime - dateTimeCameraUtc).Ticks)).FirstOrDefault();
                if(fileGeoData == null) continue;

                var minutesDifference = (dateTimeCameraUtc - fileGeoData.DateTime).TotalMinutes;
                if(minutesDifference < -5 || minutesDifference > 5) continue;
                
                metaFileItem.Latitude = fileGeoData.Latitude;
                metaFileItem.Longitude = fileGeoData.Longitude;
                metaFileItem.LocationAltitude = fileGeoData.Altitude;
                
                toUpdateMetaFiles.Add(metaFileItem);
            }
            return toUpdateMetaFiles;
        }
    }
}
