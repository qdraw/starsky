using System;
using System.Collections.Generic;
using CliWrap;
using CliWrap.Models;
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
                var input = new ExecutionInput(options + " " + fullFilePath);
                var output = cli.Execute(input);
                
                return output.StandardOutput;
            }

            //var escapedArgs = cmd.Replace("\"", "\\\"");

            //var process = new Process()
            //{
            //    StartInfo = new ProcessStartInfo
            //    {
            //        FileName = "exiftool(-k).exe",
            //        Arguments = $"-c \"{escapedArgs}\"",
            //        RedirectStandardOutput = true,
            //        UseShellExecute = false,
            //        CreateNoWindow = true,
            //    }
            //};

            //process.Start();
            //string result = process.StandardOutput.ReadToEnd();
            //process.WaitForExit();

            //return result;
        }

        public static string GetExitoolData(string filePathFull)
        {
            return _baseCommmand("-Keywords -json", filePathFull);
        }


    }

}
