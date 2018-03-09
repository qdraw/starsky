using System;
using System.IO;
using Newtonsoft.Json.Linq;
using starsky.Models;

namespace starskyCli
{
    class Program
    {
        private static void ReadConfig()
        {
            string basePath;
            string defaultConnection;

            try
            {
                string text = System.IO.File.ReadAllText("../starsky/appsettings.json");

                JObject obj = JObject.Parse(text);

                basePath = (string)obj["ConnectionStrings"]["STARSKY_BASEPATH"];
                defaultConnection = (string)obj["ConnectionStrings"]["DefaultConnection"];

            }
            catch (FileNotFoundException e)
            {
                basePath = null;
                defaultConnection = null;
            }

            if (basePath != null || defaultConnection != null)
            {
                AppSettingsProvider.BasePath = basePath;
                AppSettingsProvider.DbConnectionString = defaultConnection;

                //new SettingsCli().BasePath = basePath;
                //new SettingsCli().DefaultConnection = defaultConnection;
                //AppSettingsProvider.BasePath = "Z:\\data\\isight\\2018";
                //AppSettingsProvider.DbConnectionString = "Data Source=../starsky/data.db";
            }
            else
            {
                throw new System.ArgumentException("BasePath or ConnectionStrings not readed");
            }

        }

        static void Main(string[] args)
        {
            ReadConfig();

            var q = new SyncDatabase().SyncFiles();
            Console.WriteLine("Done!");
        }

    }
}
