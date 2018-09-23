using System;
using System.Collections.Generic;
using starsky.Helpers;
using starsky.Interfaces;
using starsky.Models;

namespace starskyGeoCli.Services
{
    public class GeoLocationWrite
    {
        private readonly IExiftool _exiftool;
        private readonly AppSettings _appSettings;

        public GeoLocationWrite(AppSettings appSettings, IExiftool exiftool)
        {
            _exiftool = exiftool;
            _appSettings = appSettings;
        }
        
        public void LoopFolder(List<FileIndexItem> metaFilesInDirectory, bool syncLocationNames)
        {
            foreach (var metaFileItem in metaFilesInDirectory)
            {
                if (!Files.IsExtensionExifToolSupported(metaFileItem.FileName)) continue;

                Console.WriteLine("Do a exiftoolSync");
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
                

                var destinationFullPath = _appSettings.DatabasePathToFilePath(metaFileItem.FilePath);

                new ExifToolCmdHelper(_appSettings, _exiftool).Update(metaFileItem, destinationFullPath,
                    comparedNamesList);
                
                Console.WriteLine(metaFileItem.FilePath);
            }

        }
    }
}