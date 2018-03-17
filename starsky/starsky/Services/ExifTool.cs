using System;
using System.Collections.Generic;
using System.IO;
using CliWrap;
using CliWrap.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using starsky.Models;

namespace starsky.Services
{
    public class ExifTool
    {
        private static string _baseCommmand(string options,string fullFilePath)
        {
            Console.WriteLine("eT: " +  AppSettingsProvider.ExifToolPath);
            using (var cli = new Cli(AppSettingsProvider.ExifToolPath))
            {
                Console.WriteLine("options " + options);
                var input = new ExecutionInput(options + " " + fullFilePath);
                var output = cli.Execute(input);
                
                return output.StandardOutput;
            }
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


        public static ExifToolModel GetExitoolData(string filePathFull)
        {
            return _parseJson(_baseCommmand("-Keywords -json", filePathFull));
        }


        public static ExifToolModel WriteExifToolKeywords(string keyWords, string filePathFull)
        {
            var currentKeywords = _parseJson(_baseCommmand("-Keywords -json", filePathFull));
            var stringArrayKeywords = currentKeywords.Keywords.Split(", ");
            var hashSetKeywords = new HashSet<string>(currentKeywords.Keywords.Split(", "));
            var toBeAddedKeywords = string.Empty;
            foreach (var keyword in hashSetKeywords)
            {
                toBeAddedKeywords += keyword + ", ";
            }
       
            return _parseJson(_baseCommmand(" -sep \", \" -Keywords=\"" + toBeAddedKeywords + "\" -json", filePathFull)); ;
        }



    }

}
