using System;
using System.IO;
using starsky.Models;
using starsky.Services;

namespace starskyCli
{
    class Program
    {

        static void Main(string[] args)
        {
            ConfigRead.SetAppSettingsProvider();

            new SyncDatabase().SyncFiles("/2018/");

            //var q = new FileIndexItem();
            //q.FilePath = "/2018/01/20180101_130000_imc.jpg";
            //q.FileHash = "LV57Mb1fOCYgkOhMmx1t6Q==";
            //Thumbnail.CreateThumb(q);


            Console.WriteLine("Done!");
        }

    }
}
