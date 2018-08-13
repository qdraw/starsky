using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using starsky.Models;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Caching.Memory;
using starsky.Helpers;
using starsky.Interfaces;

namespace starsky.Services
{
    public class ExifTool : IExiftool
    {
        // Write To Meta data using Exiftool.
        // This is a exiftool wrapper

        private readonly AppSettings _appSettings;
        private readonly IMemoryCache _cache;

        public ExifTool(AppSettings appSettings, IMemoryCache memoryCache = null)
        {
            _appSettings = appSettings;
            _cache = memoryCache;
        }
        
        private string BaseCommmand(string options, string fullFilePathSpaceSeperated)
        {
            options = " " + options + " " + fullFilePathSpaceSeperated;

            Console.WriteLine(_appSettings.ExifToolPath);

            if (!File.Exists(_appSettings.ExifToolPath)) return null;

            var exifToolPath = _appSettings.ExifToolPath;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                exifToolPath = $"\"" + _appSettings.ExifToolPath + $"\"";
            }

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = exifToolPath,
                UseShellExecute = false,
                RedirectStandardOutput = true
            };

            Console.WriteLine(options);
            
            psi.Arguments = options;
            Process p = Process.Start(psi);
            string strOutput = p.StandardOutput.ReadToEnd();
            p.WaitForExit();
            Console.WriteLine(strOutput);

            return strOutput;
        }
        
        public string FixingJsonKeywordString(string text, string nameInJson = "Keywords")
        {

            // Not Single Keyword
            // > need to be an array [""]
            // Without gives nice shiny 500 runtime errors :) :)

            var splitArray = text.Split("\n");

            var keywordsIndex = text.IndexOf(nameInJson, StringComparison.InvariantCulture);
            if (keywordsIndex >= 0)
            {
                var updatedTextStringBuilder = new StringBuilder();
                foreach (var item in splitArray)
                {
                    if (item.Contains(nameInJson) && !item.Contains("["))
                    {
                        var key = item.Replace("\"" + nameInJson + "\":", "");
                        key = key.Replace("\"", "");
                        
                        // Remove commas at end
                        Regex commaEndRegex = new Regex(",+$");
                        key = commaEndRegex.Replace(key, "");
                        
                        key = key.Trim();
                        var newItem = "\""+ nameInJson +"\": [\"" + key + "\"],"; 
                        // bug potential: Could give a bug if the next line does not contain any values
                        updatedTextStringBuilder.Append(newItem + "\n");
                    }
                    else
                    {
                        updatedTextStringBuilder.Append(item + "\n");
                    }
                }
                return updatedTextStringBuilder.ToString();

            }

            return text;
        }
        
        

        private ExifToolModel parseJson(string text) {
            if (string.IsNullOrEmpty(text)) return null;
            text = text.Replace("\r", string.Empty);

            Console.WriteLine("apply fix");
            text = FixingJsonKeywordString(text); // "Keywords"
            text = FixingJsonKeywordString(text,"Subject");

            Console.WriteLine("read from exiftool with fix applied");

            Console.WriteLine(text);
            Console.WriteLine("-----");

            var exifData = JsonConvert.DeserializeObject<IEnumerable<ExifToolModel>>(text).FirstOrDefault();

            if (exifData == null) return null;
            return exifData;

        }

        public void Update(ExifToolModel updateModel, string inputFullFilePath)
        {
            Update(updateModel, new List<string> {inputFullFilePath});
        }
        
        // Does not check in c# code if file exist
        public void Update(ExifToolModel updateModel, List<string> inputFullFilePaths)
            {
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
                            BaseCommmand(" -overwrite_original -TagsFromFile \""  + fullFilePath + "\"",  "\""+ xmpFullPath +  "\"");
                        }
                        // to continue as xmp file
                        fullFilePathsList.Add(xmpFullPath);
                        continue;
                    }
                    fullFilePathsList.Add(fullFilePath);
                }
                
                // Currently it does not allow emthy strings
                if (!string.IsNullOrWhiteSpace(updateModel.Tags))
                {
                    command += " -sep \", \" \"-xmp:subject\"=\"" + updateModel.Tags 
                                                                  + "\" -Keywords=\"" + updateModel.Tags + "\" ";
                }
                
  
                if (!string.IsNullOrWhiteSpace(updateModel.CaptionAbstract))
                {
                    command += " -Caption-Abstract=\"" + updateModel.CaptionAbstract 
                                                       + "\" -Description=\"" + updateModel.CaptionAbstract + "\"";
                }
               
                if (updateModel.ColorClass != FileIndexItem.Color.DoNotChange)
                {
                    var intColorClass = (int) updateModel.ColorClass;

                    var colorDisplayName = FileIndexItem.GetDisplayName(updateModel.ColorClass);
                    command += " \"-xmp:Label\"=" + "\"" + colorDisplayName + "\"" + " -ColorClass=\""+ intColorClass + 
                               "\" -Prefs=\"Tagged:0 ColorClass:" + intColorClass + " Rating:0 FrameNum:0\" ";
                }
                
                // // exiftool -Orientation#=5
                if (updateModel.Orientation != FileIndexItem.Rotation.DoNotChange)
                {
                    var intOrientation = (int) updateModel.Orientation;
                    command += " \"-Orientation#="+ intOrientation +"\" ";
                }

                if (updateModel.AllDatesDateTime.Year > 2)
                {
                    var exifToolString = updateModel.AllDatesDateTime.ToString("yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture);
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
                    
                    BaseCommmand(command, exifBaseInputStringBuilder.ToString());
                }

            }

            private StringBuilder Quoted(StringBuilder inputStringBuilder, string fullFilePath)
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

//            // Cached view >> IMemoryCache
//            // Short living cache Max 10. minutes
//            public ExifToolModel Info(string fullFilePath)
//            {
//                // The CLI programs uses no cache
//                if( _cache == null || _appSettings?.AddMemoryCache == false) return QueryInfo(fullFilePath);
//                
//                // Return values from IMemoryCache
//                var queryCacheName = "info_" + fullFilePath;
//                
//                // Return Cached object if it exist
//                if (_cache.TryGetValue(queryCacheName, out var objectExifToolModel))
//                    return objectExifToolModel as ExifToolModel;
//                
//                // Try to catch a new object
//                objectExifToolModel = QueryInfo(fullFilePath);
//                _cache.Set(queryCacheName, objectExifToolModel, new TimeSpan(0,10,0));
//                return (ExifToolModel) objectExifToolModel;
//            }

            // only for exiftool!
            // Why removing, The Update command does not update the entire object.
            // When you update tags, other tags will be null 
//            private void RemoveCache(string fullFilePath)
//            {
//                if (_cache == null || _appSettings?.AddMemoryCache == false) return;
//                var queryCacheName = "info_" + fullFilePath;
//    
//                if (!_cache.TryGetValue(queryCacheName, out var _)) return;
//                _cache.Remove(queryCacheName);
//            }



            // The actual query
            // will be removed very soon 
            public ExifToolModel Info(string fullFilePath)
            {
                // Add parentes around this file


                var xmpFullFilePath = Files.GetXmpSidecarFileWhenRequired(
                    fullFilePath,
                    _appSettings.ExifToolXmpPrefix);
                
                // only overwrite when a xmp file exist
                if (Files.IsFolderOrFile(xmpFullFilePath) == FolderOrFileModel.FolderOrFileTypeList.File)
                    fullFilePath = xmpFullFilePath;
                
                // When change also update class 'Update'
                // xmp:Subject == Keywords
                // Caption-Abstract == Description
                var fullFilePathStringBuilder = Quoted(null,fullFilePath);

                // -Orientation# <= hashtag is that exiftool must output a int and not a human readable string
                return parseJson(BaseCommmand("-Keywords \"-Orientation#\" -Description \"-xmp:subject\" -Caption-Abstract -Prefs -json", 
                    fullFilePathStringBuilder.ToString()));
            }

        }

    }
