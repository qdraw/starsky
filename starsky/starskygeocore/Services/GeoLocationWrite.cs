using System;
using System.Collections.Generic;
using starskycore.Helpers;
using starskycore.Interfaces;
using starskycore.Models;
using starskycore.Services;

namespace starskygeocore.Services
{
    public class GeoLocationWrite
    {
        private readonly IExifTool _exifTool;
        private readonly AppSettings _appSettings;

        public GeoLocationWrite(AppSettings appSettings, IExifTool exifTool)
        {
            _exifTool = exifTool;
            _appSettings = appSettings;
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
                
				// todo: check if works
	            var iStorage = new StorageSubPathFilesystem(_appSettings);
	            
                new ExifToolCmdHelper(_exifTool, iStorage,new ReadMeta(iStorage))
	                .Update(metaFileItem, comparedNamesList);
	            
	            if ( _appSettings.Verbose ) Console.WriteLine(metaFileItem.FilePath);

                
            }

        }
    }
}
