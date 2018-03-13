using System;
using System.Collections.Generic;
using System.IO;
using starsky.Models;
using starsky.Services;

namespace starskyCli
{
    public class Program
    {
        private static string GetSubpathFormArgs(IReadOnlyList<string> args)
        {
            var subpath = "/";

            for (int arg = 0; arg < args.Count; arg++)
            {
                if ((args[arg] == "--subpath" || args[arg] == "-s") && (arg + 1) != args.Count)
                {
                    subpath = args[arg + 1];
                }
            }

            return subpath;
        }


        public static void Main(string[] args)
        {
            ConfigRead.SetAppSettingsProvider();

            new SyncDatabase().SyncFiles(GetSubpathFormArgs(args));

            //var q = new FileIndexItem();
            //q.FilePath = "/2018/01/20180101_130000_imc.jpg";
            //q.FileHash = "LV57Mb1fOCYgkOhMmx1t6Q==";
            //Thumbnail.CreateThumb(q);


            Console.WriteLine("Done!");
        }

    }
}
