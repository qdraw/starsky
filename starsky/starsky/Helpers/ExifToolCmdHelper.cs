using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using starsky.Interfaces;
using starsky.Models;

namespace starsky.Helpers
{
    public class ExifToolCmdHelper
    {
        private readonly IExiftool _exiftool;
        private readonly AppSettings _appSettings;

        public ExifToolCmdHelper(AppSettings appSettings = null, IExiftool exiftool = null)
        {
            _exiftool = exiftool;
            _appSettings = appSettings;
        }

        public string Update(FileIndexItem updateModel, string inputFullFilePath, List<string> comparedNames)
        {
            var exifUpdateFilePaths = new List<string>
            {
                inputFullFilePath           
            };
            return Update(updateModel, exifUpdateFilePaths, comparedNames);
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

            var fullFilePathsList = new List<string>();
            foreach (var fullFilePath in inputFullFilePaths)
            {
                if(Files.IsXmpSidecarRequired(fullFilePath))
                {
                    var xmpFullPath = Files.GetXmpSidecarFileWhenRequired(fullFilePath, _appSettings.ExifToolXmpPrefix);
                
                    if (Files.IsFolderOrFile(xmpFullPath) == FolderOrFileModel.FolderOrFileTypeList.Deleted)
                    {
                        _exiftool.BaseCommmand(" -overwrite_original -TagsFromFile \""  + fullFilePath + "\"",  "\""+ xmpFullPath +  "\"");
                    }
                    // to continue as xmp file
                    fullFilePathsList.Add(xmpFullPath);
                    continue;
                }
                fullFilePathsList.Add(fullFilePath);
            }

            if (comparedNames.Contains("Tags"))
            {
                command += " -sep \", \" \"-xmp:subject\"=\"" + updateModel.Tags 
                                                              + "\" -Keywords=\"" + updateModel.Tags + "\" ";
            }
         
            if (comparedNames.Contains("Description"))
            {
                command += " -Caption-Abstract=\"" + updateModel.Description 
                                                   + "\" -Description=\"" + updateModel.Description + "\"";
            }
            
            if (comparedNames.Contains("Title"))
            {
                command += " -ObjectName=\"" + updateModel.Title + "\"" 
                           + " \"-title\"=" + "\"" + updateModel.Title  + "\"" ;
            }
           
            if (comparedNames.Contains("ColorClass") && updateModel.ColorClass != FileIndexItem.Color.DoNotChange)
            {
                var intColorClass = (int) updateModel.ColorClass;

                var colorDisplayName = FileIndexItem.GetDisplayName(updateModel.ColorClass);
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
        
    }
}