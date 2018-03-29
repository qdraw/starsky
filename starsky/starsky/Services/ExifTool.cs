using System;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;
using starsky.Models;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

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
            Console.WriteLine(options);

            if (!File.Exists(AppSettingsProvider.ExifToolPath)) return null;

            Console.WriteLine("options");
            Console.WriteLine(options);

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


            Console.WriteLine(AppSettingsProvider.ExifToolPath + options);

            return strOutput;
        }

        private static ExifToolModel _parseJson(string text) {
            if (string.IsNullOrEmpty(text)) return null;

            text = text.Replace("\r\n", "");
            text = text.Replace($"\\", "");
            text = text.Replace(@"[", "");
            text = text.Replace(@"]", "");
            text = text.Replace("\",\"", ", ");
            
//            text = Regex.Replace(text, $"(\",(\\d+))|((\\d+),\")", "");
            text = text.Replace($",\"", "");
            
            Regex isKeywordRegex = new Regex($"\"Keywords\": \"", RegexOptions.IgnoreCase);
            if (!isKeywordRegex.Match(text).Success)
            {
                text = text.Replace($"\"Keywords\": ", "\"Keywords\": \"" );
            }
            Regex isKeywordEndRegex = new Regex($"\",\n(\\s+\"Prefs|}})", RegexOptions.IgnoreCase);
            if (!isKeywordEndRegex.Match(text).Success)
            {
                Console.WriteLine("sdfdsfsdf");
                text = text.Replace($",\n", "\"," );
                text = text.Replace("\"\"", "\"");
                // ",0,1",
                Console.WriteLine(text);
                // > (",(\d+))|((\d+),")   --> single numbers will be removed
                text = Regex.Replace(text, $"(\",(\\d+))|((\\d+),\")", "");
            }
           
            Console.WriteLine(text);
            
            var exifData = JsonConvert.DeserializeObject<ExifToolModel>(text);
            return exifData;
        }


        public static ExifToolModel Update(ExifToolModel updateModel, string fullFilePath)
        {
            var command = "-json -overwrite_original";
            var initCommand = command; // to check if nothing
            
            if(updateModel.Keywords != null)
            {
                command += " -sep \", \" -Keywords=\"" + updateModel.Keywords + "\" ";
            }
            if(updateModel.ColorClass != FileIndexItem.Color.DoNotChange)
            {
                var intColorClass = (int) updateModel.ColorClass;
                command += " -Prefs=\"Tagged:0 ColorClass:"+ intColorClass +" Rating:0 FrameNum:0\" ";
            }

            if (command != initCommand)
            {
                _baseCommmand(command, fullFilePath);
            };
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
