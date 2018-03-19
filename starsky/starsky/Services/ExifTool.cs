using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using starsky.Models;

namespace starsky.Services
{
    public class ExifTool
    {

        private static string _baseCommmand(string options,string fullFilePath)
        {
            fullFilePath = $"\"" + fullFilePath + $"\"";
            options = " " + options + " " + fullFilePath;

            if (!File.Exists(AppSettingsProvider.ExifToolPath)) return null;
            

            var exifToolPath = $"\"" + AppSettingsProvider.ExifToolPath + $"\"";

            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = exifToolPath;
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;

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

            Console.WriteLine(text);

            ExifToolModel exifData = JsonConvert.DeserializeObject<ExifToolModel>(text);
            return exifData;
        }


        //public static ExifToolModel GetExitoolData(string filePathFull)
        //{
        //    return _parseJson(_baseCommmand("-Keywords -json", filePathFull));
        //}

        public static string ReadExifToolKeywords(string keyWords, string filePathFull)
        {
            var model = _parseJson(_baseCommmand("-Keywords -json", filePathFull));
            return _duplicateKeywordCheck(model.Keywords);
        }




        private static string _duplicateKeywordCheck(string keywords)
        {
            var hashSetKeywords = new HashSet<string>(keywords.Split(", "));
            var toBeAddedKeywords = string.Empty;
            foreach (var keyword in hashSetKeywords)
            {
                if (!string.IsNullOrWhiteSpace(keyword) && keyword != hashSetKeywords.LastOrDefault())
                {
                    toBeAddedKeywords += keyword + ", ";
                }

                if (!string.IsNullOrWhiteSpace(keyword) && keyword == hashSetKeywords.LastOrDefault())
                {
                    toBeAddedKeywords += keyword;
                }
            }

            return toBeAddedKeywords;

        }


        public static string WriteExifToolKeywords(string keyWords, string filePathFull) // add or update
        {
            if (string.IsNullOrWhiteSpace(keyWords)) return null;

            var currentKeywords = _parseJson(_baseCommmand("-Keywords -json", filePathFull));

            var toBeAddedKeywords = _duplicateKeywordCheck(currentKeywords.Keywords + ", " + keyWords);

            Console.WriteLine(toBeAddedKeywords);

            _baseCommmand("-overwrite_original -sep \", \" -Keywords=\"" + toBeAddedKeywords + "\" -json ",
                filePathFull);

            return _parseJson(_baseCommmand("-Keywords -json", filePathFull)).Keywords;

        }

        // overwrite => keyword list
        public static string SetExifToolKeywords(string keyWords, string filePathFull)
        {
            if (string.IsNullOrWhiteSpace(keyWords)) return null;

            var toBeAddedKeywords = _duplicateKeywordCheck(keyWords);

            _baseCommmand("-overwrite_original -sep \", \" -Keywords=\"" + toBeAddedKeywords + "\" -json ",
                filePathFull);

            return _parseJson(_baseCommmand("-Keywords -json", filePathFull)).Keywords;

        }





    }

}
