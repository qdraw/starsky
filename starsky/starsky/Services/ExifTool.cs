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
        
        private string _baseCommmand(string options,string fullFilePath)
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
        
        public string FixingJsonKeywordString(string text)
        {

            // Not Single Keyword
            // > need to be an array [""]
            // Without gives nice shiny 500 runtime errors :) :)

            var splitArray = text.Split("\n");

            var keywordsIndex = text.IndexOf("Keywords", StringComparison.InvariantCulture);
            if (keywordsIndex >= 0)
            {
                var updatedTextStringBuilder = new StringBuilder();
                foreach (var item in splitArray)
                {
                    if (item.Contains("Keywords") && !item.Contains("["))
                    {
                        var key = item.Replace("\"Keywords\":", "");
                        key = key.Replace("\"", "");
                        
                        // Remove commas at end
                        Regex commaEndRegex = new Regex(",+$");
                        key = commaEndRegex.Replace(key, "");
                        
                        key = key.Trim();
                        var newItem = "\"Keywords\": [\"" + key + "\"],"; 
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
            text = FixingJsonKeywordString(text);
            
            Console.WriteLine("read from exiftool with fix applied");

            var exifData = JsonConvert.DeserializeObject<IEnumerable<ExifToolModel>>(text).FirstOrDefault();

            if (exifData == null) return null;
            return exifData;

        }

        public ExifToolModel Update(ExifToolModel updateModel, string fullFilePath)
            {
                var command = "-json -overwrite_original";
                var initCommand = command; // to check if nothing

                // Currently it does not allow emty strings
                if (!string.IsNullOrWhiteSpace(updateModel.Tags))
                {
                    command += " -sep \", \" -Keywords=\"" + updateModel.Tags + "\" ";
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
                return _parseJson(_baseCommmand("-Keywords -Prefs -Caption-Abstract -json", fullFilePath));
            }

            public ExifToolModel Info(string fullFilePath)
            {
                // Also update class 'Update'
                return _parseJson(_baseCommmand("-Keywords -Caption-Abstract -Prefs -json", fullFilePath));
            }

        }

    }
