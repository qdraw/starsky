using System;
using System.Collections.Generic;
using System.Linq;
using starsky.Helpers;
using starsky.Models;
using starsky.Services;
using starskycore.Helpers;
using starskycore.Models;
using ReadMeta = starsky.core.Services.ReadMeta;

namespace starskyGeoCli.Services
{
    public class GeoIndexGpx
    {
        private readonly ReadMeta _readMeta;
        private readonly AppSettings _appSettings;

        public GeoIndexGpx(AppSettings appSettings, ReadMeta readMeta)
        {
            _readMeta = readMeta;
            _appSettings = appSettings;
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
                if(metaFileItem.ImageFormat != Files.ImageFormat.gpx) continue;
                var fullfilepath = _appSettings.DatabasePathToFilePath(metaFileItem.FilePath);
                _readMeta.ReadGpxFile(fullfilepath, geoList);
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