using System;
using System.Collections.Generic;
using starsky.foundation.database.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starskycore.Helpers;
using starskycore.Interfaces;
using starskycore.Models;
using starskycore.Services;

namespace starsky.foundation.geo.Services
{
    public class GeoLocationWrite
    {
        private readonly IExifTool _exifTool;
        private readonly AppSettings _appSettings;
        private readonly IStorage _iStorage;
        private readonly IStorage _thumbnailStorage;

        public GeoLocationWrite(AppSettings appSettings, IExifTool exifTool, ISelectorStorage selectorStorage)
        {
            _exifTool = exifTool;
            _appSettings = appSettings;
            _thumbnailStorage = selectorStorage.Get(SelectorStorage.StorageServices.Thumbnail);
            _iStorage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
        }
        
        /// <summary>
        /// Write to ExifTool by list
        /// </summary>
        /// <param name="metaFilesInDirectory">list of files with data</param>
        /// <param name="syncLocationNames">Write city, state and country to exifTool (false > no)</param>
        public void LoopFolder(List<FileIndexItem> metaFilesInDirectory, bool syncLocationNames)
        {
            foreach (var metaFileItem in metaFilesInDirectory)
            {
                if (!ExtensionRolesHelper.IsExtensionExifToolSupported(metaFileItem.FileName)) continue;

				if ( _appSettings.Verbose ) Console.Write("*ExifSync*");

                var comparedNamesList = new List<string>
                {
                    nameof(FileIndexItem.Latitude),
                    nameof(FileIndexItem.Longitude),
                    nameof(FileIndexItem.LocationAltitude)
                };
                
                if(syncLocationNames) comparedNamesList.AddRange( new List<string>
                {
                    nameof(FileIndexItem.LocationCity),
                    nameof(FileIndexItem.LocationState),
                    nameof(FileIndexItem.LocationCountry),
                });
                
                new ExifToolCmdHelper(_exifTool, _iStorage, _thumbnailStorage, new ReadMeta(_iStorage))
	                .Update(metaFileItem, comparedNamesList);
	            
	            if ( _appSettings.Verbose ) Console.Write($"GeoLocationWrite: {metaFileItem.FilePath} ");
            }

        }
    }
}
