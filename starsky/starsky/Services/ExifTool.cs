﻿using System;
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
using starsky.Helpers;
using starsky.Interfaces;

namespace starsky.Services
{
    public class ExifTool : IExiftool
    {
        // Write To Meta data using Exiftool.
        // This is a exiftool wrapper

        private readonly AppSettings _appSettings;

        public ExifTool(AppSettings appSettings)
        {
            _appSettings = appSettings;
        }
        
        private string _baseCommmand(string options, string fullFilePath)
        {
            fullFilePath = $"\"" + fullFilePath + $"\"";
            options = " " + options + " " + fullFilePath;

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

        private ExifToolModel _parseJson(string text) {
            if (string.IsNullOrEmpty(text)) return null;
            text = text.Replace("\r", "");
            text = text.Replace($"\\", "");

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

        public ExifToolModel Update(ExifToolModel updateModel, string fullFilePath)
            {
                var command = "-json -overwrite_original";
                var initCommand = command; // to check if nothing

                // Create an XMP File -> as those files don't support those tags
                // Check first if it is needed
                
                if(Files.IsXmpSidecarRequired(fullFilePath))
                {
                    var xmpFullPath = Files.GetXmpSidecarFileWhenRequired(fullFilePath, _appSettings.ExifToolXmpPrefix);
                    
                    if (Files.IsFolderOrFile(xmpFullPath) == FolderOrFileModel.FolderOrFileTypeList.Deleted)
                    {
                        _baseCommmand(" -overwrite_original -TagsFromFile \""  + fullFilePath + "\"", xmpFullPath);
                    }
                    // to continue as xmp file
                    fullFilePath = xmpFullPath;
                }
                
                // Currently it does not allow emthy strings
                if (!string.IsNullOrWhiteSpace(updateModel.Tags))
                {
                    command += " -sep \", \" \"-xmp:subject\"=\"" + updateModel.Tags + "\" -Keywords=\"" + updateModel.Tags + "\" ";
                }
                
                if (!string.IsNullOrWhiteSpace(updateModel.CaptionAbstract))
                {
                    command += " -Caption-Abstract=\"" + updateModel.CaptionAbstract + "\" ";
                }

                if (updateModel.ColorClass != FileIndexItem.Color.DoNotChange)
                {
                    var intColorClass = (int) updateModel.ColorClass;
                    command += " -Prefs=\"Tagged:0 ColorClass:" + intColorClass + " Rating:0 FrameNum:0\" ";
                }

                if (updateModel.AllDatesDateTime.Year > 2)
                {
                    var exifToolString = updateModel.AllDatesDateTime.ToString("yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture);
                    command += " -AllDates=\""+ exifToolString + "\" ";
                }
                
                if (command != initCommand)
                {
                    _baseCommmand(command, fullFilePath);
                }

                // Also update class info
                return _parseJson(_baseCommmand("-Keywords \"-xmp:subject\" -Prefs -Caption-Abstract -json", fullFilePath));
            }

            public ExifToolModel Info(string fullFilePath)
            {
                var xmpFullFilePath = Files.GetXmpSidecarFileWhenRequired(fullFilePath, _appSettings.ExifToolXmpPrefix);
                
                // only overwrite when a xmp file exist
                if (Files.IsFolderOrFile(xmpFullFilePath) == FolderOrFileModel.FolderOrFileTypeList.File)
                    fullFilePath = xmpFullFilePath;
                
                // When change also update class 'Update'
                return _parseJson(_baseCommmand("-Keywords \"-xmp:subject\" -Caption-Abstract -Prefs -json", fullFilePath));
            }

        }

    }
