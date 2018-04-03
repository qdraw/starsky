using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using starsky.Models;
using System.Runtime.InteropServices;
using Newtonsoft.Json.Linq;

namespace starsky.Services
{
    public static class ExifTool
    {
        // Write To Meta data using Exiftool.
        // This is a exiftool wrapper
        
        private static string _baseCommmand(string options,string fullFilePath)
        {
            fullFilePath = $"\"" + fullFilePath + $"\"";
            options = " " + options + " " + fullFilePath;

            Console.WriteLine(AppSettingsProvider.ExifToolPath);

            if (!File.Exists(AppSettingsProvider.ExifToolPath)) return null;

            var exifToolPath = AppSettingsProvider.ExifToolPath;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                exifToolPath = $"\"" + AppSettingsProvider.ExifToolPath + $"\"";
            }

            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = exifToolPath;
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;
            
            Console.WriteLine(options);
            
            psi.Arguments = options;
            Process p = Process.Start(psi);
            string strOutput = p.StandardOutput.ReadToEnd();
            p.WaitForExit();
            Console.WriteLine(strOutput);

            return strOutput;
        }
        
        private static string _fixingJsonKeywordString(string text)
        {
            //            [{
            //                "SourceFile": "fullPath",
            //                "Keywords": "singleword"
            //            }]

            // > need to be an array [""]

            // Without gives nice shiny 500 runtime errors :) :)

            var splitArray = text.Split("\n");

            var keywordsIndex = text.IndexOf("Keywords", StringComparison.InvariantCulture);
            if (keywordsIndex >= 0)
            {
                var updatedText = string.Empty;
                foreach (var item in splitArray)
                {
                    if (item.Contains("Keywords") && !item.Contains("["))
                    {
                        var key = item.Replace("\"Keywords\":", "");
                        key = key.Replace("\"", "");
                        key = key.Trim();
                        var newItem = "\"Keywords\": [\"" + key + "\"]";
                        updatedText += newItem + "\n";
                    }
                    else
                    {
                        updatedText += item + "\n";;
                    }
                }
                return updatedText;

            }

            return text;
//            else
//            {
//                var newupdatedText = string.Empty;
//
//                foreach (var item in splitArray)
//                {
//                    if (item.Contains("}]"))
//                    {
//                        newupdatedText += ", \"Keywords\": [\"" + "" + "\"]" + "\n";
//                    }
//                    newupdatedText += item + "\n";
//                }
//
//                Console.WriteLine("dsfdsfdfsdfdsfbdshfbhjsdkb");
//                Console.WriteLine(newupdatedText);
//                return newupdatedText;
//            }
        }

        private static ExifToolModel _parseJson(string text) {
            if (string.IsNullOrEmpty(text)) return null;

            text = text.Replace("\r\n", "");
            text = text.Replace($"\\", "");

            Console.WriteLine("apply fix");
            text = _fixingJsonKeywordString(text);
            
            Console.WriteLine("read from exiftool with fix applied");
            Console.WriteLine(text);

            var exifData = JsonConvert.DeserializeObject<IEnumerable<ExifToolModel>>(text).FirstOrDefault();

            if (exifData == null) return null;
            return exifData;

        }




        public static ExifToolModel Update(ExifToolModel updateModel, string fullFilePath)
            {
                var command = "-json -overwrite_original";
                var initCommand = command; // to check if nothing

                if (updateModel.Tags != null)
                {
                    command += " -sep \", \" -Keywords=\"" + updateModel.Tags + "\" ";
                }

                if (updateModel.ColorClass != FileIndexItem.Color.DoNotChange)
                {
                    var intColorClass = (int) updateModel.ColorClass;
                    command += " -Prefs=\"Tagged:0 ColorClass:" + intColorClass + " Rating:0 FrameNum:0\" ";
                }

                if (command != initCommand)
                {
                    _baseCommmand(command, fullFilePath);
                }

                // Also update class info
                return _parseJson(_baseCommmand("-Keywords -Prefs -json", fullFilePath));
            }

            public static ExifToolModel Info(string fullFilePath)
            {
                // Also update class 'Update'
                return _parseJson(_baseCommmand("-Keywords -Prefs -json", fullFilePath));
            }

        }

    }
