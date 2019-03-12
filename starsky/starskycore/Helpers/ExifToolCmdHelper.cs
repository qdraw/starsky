using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using starskycore.Interfaces;
using starskycore.Models;

namespace starskycore.Helpers
{
    public class ExifToolCmdHelper
    {
        private readonly IExiftool _exiftool;
        private readonly AppSettings _appSettings;
	    private readonly IStorage _iStorage;

	    public ExifToolCmdHelper(AppSettings appSettings = null, IExiftool exiftool = null, IStorage iStorage  = null)
        {
            _exiftool = exiftool;
            _appSettings = appSettings;
//	        _iStorage = iStorage;
        }

	    /// <summary>
	    /// To update Exiftool
	    /// </summary>
	    /// <param name="updateModel">update model</param>
	    /// <param name="inputFullFilePath">filepath</param>
	    /// <param name="comparedNames">list,string e.g. Tags</param>
	    /// <returns></returns>
        public string Update(FileIndexItem updateModel, string inputFullFilePath, List<string> comparedNames)
        {
            var exifUpdateFilePaths = new List<string>
            {
                inputFullFilePath           
            };
            return Update(updateModel, exifUpdateFilePaths, comparedNames);
        }

        /// <summary>
        /// For Raw files us an external .xmp sitecar file, and add this to the fullFilePathsList
        /// </summary>
        /// <param name="inputFullFilePaths">list of files to update</param>
        /// <returns>list of files, where needed for raw-files there are .xmp used</returns>
        private List<string> FullFilePathsListTagsFromFile(List<string> inputFullFilePaths)
        {
            var fullFilePathsList = new List<string>();
            foreach (var fullFilePath in inputFullFilePaths)
            {
                if(ExtensionRolesHelper.IsXmpSidecarRequired(fullFilePath))
                {
                    var xmpFullPath = ExtensionRolesHelper.GetXmpSidecarFileWhenRequired(fullFilePath, _appSettings.ExifToolXmpPrefix);
                
                    if (FilesHelper.IsFolderOrFile(xmpFullPath) == FolderOrFileModel.FolderOrFileTypeList.Deleted)
                    {
                        _exiftool.BaseCommmand(" -overwrite_original -TagsFromFile \""  
                                               + fullFilePath + "\"",  "\""+ xmpFullPath +  "\"");
                    }
                    // to continue as xmp file
                    fullFilePathsList.Add(xmpFullPath);
                    continue;
                }
                fullFilePathsList.Add(fullFilePath);
            }
            return fullFilePathsList;
        }

        // Does not check in c# code if file exist
        public string Update(FileIndexItem updateModel, List<string> inputFullFilePaths, List<string> comparedNames )
        {
            if(_exiftool == null) throw new ArgumentException("add exiftool please");
            if(_appSettings == null) throw new ArgumentException("add _appSettings please");

            var command = "-json -overwrite_original";
            var initCommand = command; // to check if nothing

            // Create an XMP File -> as those files don't support those tags
            // Check first if it is needed

            var fullFilePathsList = FullFilePathsListTagsFromFile(inputFullFilePaths);

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
           
            if (comparedNames.Contains("ColorClass") && updateModel.ColorClass != FileIndexItem.Color.DoNotChange)
            {
                var intColorClass = (int) updateModel.ColorClass;

                var colorDisplayName = EnumHelper.GetDisplayName(updateModel.ColorClass);
                command += " \"-xmp:Label\"=" + "\"" + colorDisplayName + "\"" + " -ColorClass=\""+ intColorClass + 
                           "\" -Prefs=\"Tagged:0 ColorClass:" + intColorClass + " Rating:0 FrameNum:0\" ";
            }
            
            // // exiftool -Orientation#=5
            if (comparedNames.Contains("Orientation") && updateModel.Orientation != FileIndexItem.Rotation.DoNotChange)
            {
                var intOrientation = (int) updateModel.Orientation;
                command += " \"-Orientation#="+ intOrientation +"\" ";
            }

            if (comparedNames.Contains("DateTime") && updateModel.DateTime.Year > 2)
            {
                var exifToolString = updateModel.DateTime.ToString("yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture);
                command += " -AllDates=\""+ exifToolString + "\" ";
            }
            
            if (command != initCommand)
            {
                var exifBaseInputStringBuilder = new StringBuilder();
                foreach (var fullFilePath in fullFilePathsList)
                {
                    exifBaseInputStringBuilder = Quoted(exifBaseInputStringBuilder,fullFilePath);
                    exifBaseInputStringBuilder.Append($" ");
                }
                
                _exiftool.BaseCommmand(command, exifBaseInputStringBuilder.ToString());
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

        public StringBuilder Quoted(StringBuilder inputStringBuilder, string fullFilePath)
        {
            if (inputStringBuilder == null)
            {
                inputStringBuilder = new StringBuilder();
            }
            inputStringBuilder.Append($"\"");
            inputStringBuilder.Append(fullFilePath);
            inputStringBuilder.Append($"\"");
            return inputStringBuilder;
        }

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
			// ignore files that are not exist
			if(FilesHelper.IsFolderOrFile(fullSourceImage) != FolderOrFileModel.FolderOrFileTypeList.File) return;
	        if(FilesHelper.IsFolderOrFile(thumbPath) != FolderOrFileModel.FolderOrFileTypeList.File) return;

			// Reset Orientation on thumbpath
			// Do an ExifTool exif sync for the file
			if(_exiftool == null && _appSettings.Verbose) Console.WriteLine("Exiftool disabled");
			_exiftool?.BaseCommmand(" -overwrite_original -TagsFromFile \"" + fullSourceImage + "\"",
				"\"" + thumbPath + "\"" + " -Orientation=" + append);
			// Reset orentation
        }
    }
}