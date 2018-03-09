using System;
using System.IO;
using Newtonsoft.Json.Linq;
using starsky.Models;
using starsky.Services;

namespace starskyCli
{
    class Program
    {

        static void Main(string[] args)
        {
            ConfigRead.SetAppSettingsProvider();

            new SyncDatabase().SyncFiles();
            Console.WriteLine("Done!");
        }

    }
}
