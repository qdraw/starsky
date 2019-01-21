using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using starskycore.Helpers;
using starskycore.Interfaces;
using starskycore.Models;

namespace starskycore.Services
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
        
        public string BaseCommmand(string options, string fullFilePathSpaceSeperated)
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

	        if ( !p.HasExited )
	        {
		        p.CloseMainWindow();
		        p.Close();
		        return null;
	        }
	        
	        // make sure that there nothing left
	        p.Dispose();

			Console.WriteLine(strOutput);
			return strOutput;
        }
        
        public string FixingJsonKeywordString(string text, string nameInJson = "Keywords")
        {

            // Not Single Keyword
            // > need to be an array [""]
            // Without gives nice shiny 500 runtime errors :) :)

            var splitArray = text.Split("\n".ToCharArray());

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


		/// <summary>
		/// Parses the json to a ExifToolModel object
		/// </summary>
		/// <param name="text">the json from exiftool.</param>
		/// <returns>ExifToolModel object</returns>
		public ExifToolModel ParseJson(string text) {
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

        
        // The actual query
        // will be removed very soon 
        // Only used by DownloadPhoto
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
            var fullFilePathStringBuilder = new ExifToolCmdHelper().Quoted(null,fullFilePath);

            // -Orientation# <= hashtag is that exiftool must output a int and not a human readable string
            return ParseJson(BaseCommmand("-Keywords \"-xmp:title\" -ObjectName \"-Orientation#\" -Description \"-xmp:subject\" -Caption-Abstract -Prefs -json", 
                fullFilePathStringBuilder.ToString()));
        }

    }

}
