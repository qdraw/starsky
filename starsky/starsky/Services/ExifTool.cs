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
            //text = text.Replace("\\\",\\\"", string.Empty); //-- \",\"
            text = text.Replace("\",\"", ", ");

            text = Regex.Replace(text, $"\\d*,\"", "\"");
            
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
      
        
        
//            // REMOVE ======>>
//        
//        
//        //public static ExifToolModel GetExitoolData(string filePathFull)
//        //{
//        //    return _parseJson(_baseCommmand("-Keywords -json", filePathFull));
//        //}
//
//        public static string ReadExifToolKeywords(string filePathFull)
//        {
//            var model = _parseJson(_baseCommmand("-Keywords -json", filePathFull));
//            return null;
////            return _duplicateKeywordCheck(model.Keywords);
//        }
//
//        // exiftool -Prefs="Tagged:0 ColorClass:0 Rating:2 FrameNum:0"
//
//
//
////        private static string _duplicateKeywordCheck(string keywords)
////        {
////            var hashSetKeywords = new HashSet<string>(keywords.Split(", "));
////            var toBeAddedKeywords = string.Empty;
////            foreach (var keyword in hashSetKeywords)
////            {
////
////                if (!string.IsNullOrWhiteSpace(keyword) && keyword != hashSetKeywords.LastOrDefault())
////                {
////                    toBeAddedKeywords += keyword + ", ";
////                }
////
////                if (!string.IsNullOrWhiteSpace(keyword) && keyword == hashSetKeywords.LastOrDefault())
////                {
////                    toBeAddedKeywords += keyword;
////                }
////
////            }
////
////            // Add everyting in lowercase
////            toBeAddedKeywords = toBeAddedKeywords.ToLower();
////
////            return toBeAddedKeywords;
////        }
//
//
//        public static string WriteExifToolKeywords(string keyWords, string filePathFull) // add or update
//        {
//            return null;
////            if (string.IsNullOrWhiteSpace(keyWords)) return null;
////
////            var currentKeywords = _parseJson(_baseCommmand("-Keywords -json", filePathFull));
////
////            var toBeAddedKeywords = _duplicateKeywordCheck(currentKeywords.Keywords + ", " + keyWords);
////
////            Console.WriteLine(toBeAddedKeywords);
////
////            _baseCommmand("-overwrite_original -sep \", \" -Keywords=\"" + toBeAddedKeywords + "\" -json ",
////                filePathFull);
////
////            return _parseJson(_baseCommmand("-Keywords -json", filePathFull)).Keywords;
//
//        }
//
//        // overwrite => keyword list
//        public static string SetExifToolKeywords(string keyWords, string filePathFull)
//        {
//            return null;
////            if (string.IsNullOrWhiteSpace(keyWords)) return null;
////
////            var toBeAddedKeywords = _duplicateKeywordCheck(keyWords);
////
////            _baseCommmand("-overwrite_original -sep \", \" -Keywords=\"" + toBeAddedKeywords + "\" -json ",
////                filePathFull);
////
////            return _parseJson(_baseCommmand("-Keywords -json", filePathFull)).Keywords;
//
//        }
//    // end




    }

}
