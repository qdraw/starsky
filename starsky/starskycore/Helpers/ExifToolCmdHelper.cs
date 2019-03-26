﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using starskycore.Interfaces;
using starskycore.Models;

namespace starskycore.Helpers
{
    public class ExifToolCmdHelper
    {
        private readonly IExifTool _exifTool;
	    private readonly IStorage _iStorage;

	    public ExifToolCmdHelper(IExifTool exifTool, IStorage iStorage)
        {
            _exifTool = exifTool;
	        _iStorage = iStorage;
        }

	    /// <summary>
	    /// To update Exiftool
	    /// </summary>
	    /// <param name="updateModel">update model</param>
	    /// <param name="comparedNames">list,string e.g. Tags</param>
	    /// <returns></returns>
        public string Update(FileIndexItem updateModel, List<string> comparedNames)
        {
            var exifUpdateFilePaths = new List<string>
            {
                updateModel.FilePath           
            };
	        return UpdateAsyncWrapper(updateModel, exifUpdateFilePaths, comparedNames).Result;
        }

	    
	    public string Update(FileIndexItem updateModel, List<string> inputSubPaths,
		    List<string> comparedNames)
	    {
		    return UpdateAsyncWrapper(updateModel, inputSubPaths, comparedNames).Result;
	    }

	    /// <summary>
	    /// Add a .xmp sidecar file
	    /// </summary>
	    /// <param name="subPath"></param>
	    /// <returns></returns>
	    public string XmpSync(string subPath)
	    {
		    // only for raw files
		    if ( !ExtensionRolesHelper.IsExtensionForceXmp(subPath) ) return subPath;

		    var withXmp = ExtensionRolesHelper.ReplaceExtensionWithXmp(subPath);
                
		    
		    if (_iStorage.IsFolderOrFile(withXmp) == FolderOrFileModel.FolderOrFileTypeList.Deleted)
		    {
			    throw new NotImplementedException();
//			    _exifTool.WriteTagsAsync(withXmp, "-TagsFromFile \""  + fullFilePath + "\"",  "\""+ xmpFullPath +  "\"");
		    }
		    return withXmp;
	    }
	    
	    

        /// <summary>
        /// For Raw files us an external .xmp sidecar file, and add this to the PathsList
        /// </summary>
        /// <param name="inputSubPaths">list of files to update</param>
        /// <returns>list of files, where needed for raw-files there are .xmp used</returns>
        private List<string> PathsListTagsFromFile(List<string> inputSubPaths)
        {
            var pathsList = new List<string>();
            foreach (var subPath in inputSubPaths)
            {
                if(ExtensionRolesHelper.IsExtensionForceXmp(subPath))
                {
	                var xmpPath = XmpSync(subPath);
                    // to continue as xmp file
                    pathsList.Add(xmpPath);
                    continue;
                }
                pathsList.Add(subPath);
            }
            return pathsList;
        }
	    
	    
	    // Wrapper to do Async tasks -- add variable to test make it in a unit test shorter
	    private async Task<string> UpdateAsyncWrapper(FileIndexItem updateModel, List<string> inputSubPaths, List<string> comparedNames)
	    {
		    var task = Task.Run(() => UpdateASync(updateModel,inputSubPaths,comparedNames));
		    return task.Wait(TimeSpan.FromSeconds(8)) ? task.Result : string.Empty;
	    }

        // Does not check in c# code if file exist
        public async Task<string> UpdateASync(FileIndexItem updateModel, List<string> inputSubPaths, List<string> comparedNames )
        {
	        Console.WriteLine("sdfsdf");
            var command = "-json -overwrite_original";
            var initCommand = command; // to check if nothing

            // Create an XMP File -> as those files don't support those tags
            // Check first if it is needed

            var subPathsList = PathsListTagsFromFile(inputSubPaths);

            command = UpdateKeywordsCommand(command, comparedNames, updateModel);
            command = UpdateDescriptionCommand(command, comparedNames, updateModel);

            command = UpdateGPSLatitudeCommand(command, comparedNames, updateModel);
            command = UpdateGPSLongitudeCommand(command, comparedNames, updateModel);
            command = UpdateLocationAltitudeCommand(command, comparedNames, updateModel);

            command = UpdateLocationCountryCommand(command, comparedNames, updateModel);
            command = UpdateLocationStateCommand(command, comparedNames, updateModel);
            command = UpdateLocationCityCommand(command, comparedNames, updateModel);
            
            if (comparedNames.Contains("Title"))
            {
                command += " -ObjectName=\"" + updateModel.Title + "\"" 
                           + " \"-title\"=" + "\"" + updateModel.Title  + "\"" ;
            }
           
            if (comparedNames.Contains(nameof(FileIndexItem.ColorClass)) && updateModel.ColorClass != FileIndexItem.Color.DoNotChange)
            {
                var intColorClass = (int) updateModel.ColorClass;

                var colorDisplayName = EnumHelper.GetDisplayName(updateModel.ColorClass);
                command += " \"-xmp:Label\"=" + "\"" + colorDisplayName + "\"" + " -ColorClass=\""+ intColorClass + 
                           "\" -Prefs=\"Tagged:0 ColorClass:" + intColorClass + " Rating:0 FrameNum:0\" ";
            }
            
            // // exiftool -Orientation#=5
            if (comparedNames.Contains( nameof(FileIndexItem.Orientation) ) && updateModel.Orientation != FileIndexItem.Rotation.DoNotChange)
            {
                var intOrientation = (int) updateModel.Orientation;
                command += " \"-Orientation#="+ intOrientation +"\" ";
            }

            if (comparedNames.Contains( nameof(FileIndexItem.DateTime) ) && updateModel.DateTime.Year > 2)
            {
                var exifToolString = updateModel.DateTime.ToString("yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture);
                command += " -AllDates=\""+ exifToolString + "\" ";
            }

	        if ( command == initCommand ) return command;
	        
	        foreach (var path in subPathsList)
	        {
		        if ( ! _iStorage.ExistFile(path) ) continue;
		        await _exifTool.WriteTagsAsync(path, command);
	        }

	        if (  _iStorage.ThumbnailExist(updateModel.FileHash) )
	        {
		        await _exifTool.WriteTagsThumbnailAsync(updateModel.FileHash, command);
	        }

	        return command;
        }

        private string UpdateLocationAltitudeCommand(
	        string command, List<string> comparedNames, FileIndexItem updateModel)
        {
            // -GPSAltitude="+160" -GPSAltitudeRef=above
            if (comparedNames.Contains("LocationAltitude"))
            {
                // 0 = "Above Sea Level"
                // 1 = Below Sea Level
                var gpsAltitudeRef = "0";
                var gpsAltitude = "+" + updateModel.LocationAltitude.ToString(CultureInfo.InvariantCulture);
                if (updateModel.LocationAltitude < 0)
                {
                    gpsAltitudeRef = "1";
                    gpsAltitude = "-" + (updateModel.LocationAltitude * -1).ToString(CultureInfo.InvariantCulture);
                } 
                command += " -GPSAltitude=\"" + gpsAltitude + "\" -gpsaltituderef#=\"" + gpsAltitudeRef + "\" ";
            }
            return command;
        }

        private string UpdateGPSLatitudeCommand(
	        string command, List<string> comparedNames, FileIndexItem updateModel)
        {
            // CultureInfo.InvariantCulture is used for systems where comma is the default seperator
            if (comparedNames.Contains("Latitude"))
            {
                command += " -GPSLatitude=\"" + updateModel.Latitude.ToString(CultureInfo.InvariantCulture) 
                                                              + "\" -GPSLatitudeRef=\"" 
                                              + updateModel.Latitude.ToString(CultureInfo.InvariantCulture) + "\" ";
            }
            return command;
        }
        
        private string UpdateGPSLongitudeCommand(string command, List<string> comparedNames, FileIndexItem updateModel)
        {
            if (comparedNames.Contains("Longitude"))
            {
                command += " -GPSLongitude=\"" + updateModel.Longitude.ToString(CultureInfo.InvariantCulture) 
                                              + "\" -GPSLongitudeRef=\"" 
                                               + updateModel.Longitude.ToString(CultureInfo.InvariantCulture) + "\" ";
            }
            return command;
        }

        private static string UpdateKeywordsCommand(string command, List<string> comparedNames, FileIndexItem updateModel)
        {
            if (comparedNames.Contains("Tags"))
            {
                command += " -sep \", \" \"-xmp:subject\"=\"" + updateModel.Tags 
                                                              + "\" -Keywords=\"" + updateModel.Tags + "\" ";
            }
            return command;
        }
        
        private static string UpdateLocationCityCommand(string command, List<string> comparedNames, FileIndexItem updateModel)
        {
            if (comparedNames.Contains("LocationCity"))
            {
                command += " -City=\"" + updateModel.LocationCity 
                                                   + "\" -xmp:City=\"" + updateModel.LocationCity + "\"";
            }
            return command;
        }
        
        private static string UpdateLocationStateCommand(string command, List<string> comparedNames, FileIndexItem updateModel)
        {
            if (comparedNames.Contains("LocationState"))
            {
                command += " -State=\"" + updateModel.LocationState 
                                       + "\" -Province-State=\"" + updateModel.LocationState + "\"";
            }
            return command;
        }
        
        private static string UpdateLocationCountryCommand(
	        string command, List<string> comparedNames, FileIndexItem updateModel)
        {
            if (comparedNames.Contains("LocationCountry"))
            {
                command += " -Country=\"" + updateModel.LocationCountry 
                                        + "\" -Country-PrimaryLocationName=\"" + updateModel.LocationCountry + "\"";
            }
            return command;
        }
        
        private static string UpdateDescriptionCommand(string command, List<string> comparedNames, FileIndexItem updateModel)
        {
            if (comparedNames.Contains("Description"))
            {
                command += " -Caption-Abstract=\"" + updateModel.Description 
                                                   + "\" -Description=\"" + updateModel.Description + "\"";
            }
            return command;
        }

//        public StringBuilder Quoted(StringBuilder inputStringBuilder, string fullFilePath)
//        {
//            if (inputStringBuilder == null)
//            {
//                inputStringBuilder = new StringBuilder();
//            }
//            inputStringBuilder.Append($"\"");
//            inputStringBuilder.Append(fullFilePath);
//            inputStringBuilder.Append($"\"");
//            return inputStringBuilder;
//        }

        public string CopyExifPublish(string fullSourceImage, string thumbPath)
        {
            // add space before command
            const string append = " -Software=\"Qdraw 1.0\" -CreatorTool=\"Qdraw 1.0\" " +
                                  "-HistorySoftwareAgent=\"Qdraw 1.0\" -HistoryParameters=\"Publish to Web\" " +
                                  "-PhotoshopQuality=\"\" -PMVersion=\"\" -Copyright=\"© Qdraw;Media www.qdraw.nl\"";
            CopyExif(fullSourceImage, thumbPath, append);
            return append;
        }

        public void CopyExif(string fullSourceImage, string thumbPath, string append = "")
        {
	        throw new NotImplementedException();
	        
//			// ignore files that are not exist
//			if(FilesHelper.IsFolderOrFile(fullSourceImage) != FolderOrFileModel.FolderOrFileTypeList.File) return;
//	        if(FilesHelper.IsFolderOrFile(thumbPath) != FolderOrFileModel.FolderOrFileTypeList.File) return;
//
//			// Reset Orientation on thumbpath
//			// Do an ExifTool exif sync for the file
//			if(_exifTool == null && _appSettings.Verbose) Console.WriteLine("Exiftool disabled");
//			_exifTool?.BaseCommmand(" -overwrite_original -TagsFromFile \"" + fullSourceImage + "\"",
//				"\"" + thumbPath + "\"" + " -Orientation=" + append);
//			// Reset orentation
        }
    }
}